using Library;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessRPSService
{
    public class WebServiceHelper
    {
        /// <summary>
        /// Get list of accounts for inserting the receipt records in RPS core system.
        /// </summary>
        /// <returns></returns>
        public static List<NotificationAccount> GetUOBPaidAccounts(string[] PiggyAccServices)
        {
            try
            {
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Connection String"].ConnectionString;
                
                SqlConnection connection = new SqlConnection(ConnectionString);

                SqlCommand command = new SqlCommand("dbo.Usp_Receipt_UOBPaidAccount_Fetch", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                List<NotificationAccount> notificationAccounts = new List<NotificationAccount>();

                while (reader.Read())
                {
                    NotificationAccount notificationAccount = new NotificationAccount();

                    notificationAccount.IsPiggy = Util.IsPiggy(reader["TransactionText"].ToString(), PiggyAccServices);
                    notificationAccount.AccountNumber = Util.FormatAccountNumber(reader["TransactionText"].ToString(), PiggyAccServices);
                    notificationAccount.AccountServiceType = Util.GetAccountServiceType(reader["TransactionText"].ToString(), PiggyAccServices);
                    notificationAccount.AccountName = reader["AccountName"].ToString();
                    notificationAccount.AccountCurrency = reader["AccountCurrency"].ToString();
                    notificationAccount.Amount = reader["Amount"].ToString();

                    notificationAccounts.Add(notificationAccount);
                }
                return notificationAccounts;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        protected static SqlCommand GetInsertCommand(SqlDatabase db, string accNumber, string isCreated, string rejectReason)
        { 
            SqlCommand insertCmd = db.GetStoredProcCommand("Usp_Receipt_UpdateUOBAccoutStatus") as SqlCommand;
            //Add Parameters
            insertCmd.Parameters.AddWithValue("@iStrAccNo", accNumber);            
            insertCmd.Parameters.AddWithValue("@iStrCreated", isCreated);
            insertCmd.Parameters.AddWithValue("@iStrRejectReason", rejectReason);
            
            return insertCmd;
        }
        
        public static void UpdateUOBAccountStatus(string accountNo, string isCreated, string rejectReason)
        {
            try
            {
                SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;               
                using (SqlCommand insertCmd = GetInsertCommand(db, accountNo, isCreated, rejectReason))
                {
                    db.ExecuteNonQuery(insertCmd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static bool CheckReceiptInsertTime(int initMode, string createdBy)
        {
            try
            {
                bool IsInsertable = false;
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Connection String"].ConnectionString;

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("Usp_Receipt_CheckInsert", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@iintMode", initMode);
                        cmd.Parameters.AddWithValue("@iStrCreatedBy", createdBy ?? string.Empty);
                        cmd.Parameters.Add("@IsInsertable", SqlDbType.Bit);
                        cmd.Parameters["@IsInsertable"].Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        IsInsertable = Convert.ToBoolean(cmd.Parameters["@IsInsertable"].Value);
                    }
                }
                return IsInsertable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string[] AccountServices(string codeType, string code, string companyCode)
        {
            try
            {
                string[] accountServices = null;
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Connection String"].ConnectionString;

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("Usp_Setup_LookupCodes_Fetch", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@iStrCodeType", codeType);
                        cmd.Parameters.AddWithValue("@iStrCode", code);
                        cmd.Parameters.AddWithValue("@iStrCompanyCode", companyCode);                        
                        con.Open();
                        //cmd.ExecuteNonQuery();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while(reader.Read())
                        {
                           accountServices = reader["Value1"].ToString().Split(',').ToArray();
                        }
                        con.Close();                      
                    }
                }
                return accountServices;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

