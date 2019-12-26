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
                    NotificationAccount notificationAccount = new NotificationAccount
                    {
                        IsPiggy = Util.IsPiggy(reader["TransactionText"].ToString(), PiggyAccServices),
                        AccountNumber = Util.FormatAccountNumber(reader["TransactionText"].ToString(), PiggyAccServices),
                        AccountServiceType = Util.GetAccountServiceType(reader["TransactionText"].ToString(), PiggyAccServices),
                        AccountName = reader["AccountName"].ToString(),
                        AccountCurrency = reader["AccountCurrency"].ToString(),
                        Amount = reader["Amount"] != DBNull.Value ? reader["Amount"].ToString() : decimal.Zero.ToString(),
                        UsedAmount = reader["UsedAmount"] != DBNull.Value ? reader["UsedAmount"].ToString() : decimal.Zero.ToString(),
                        PayNowIndicator = reader["PayNowIndicator"] != DBNull.Value ? reader["PayNowIndicator"].ToString() : string.Empty,
                        TransactionText = reader["TransactionText"] != DBNull.Value ? reader["TransactionText"].ToString() : string.Empty
                    };

                    notificationAccounts.Add(notificationAccount);
                }
                return notificationAccounts;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        protected static SqlCommand GetInsertCommand(SqlDatabase db, string transactionText, string isCreated, 
                                                     string rejectReason, long receiptID, string receiptNo)
        { 
            SqlCommand insertCmd = db.GetStoredProcCommand("dbo.Usp_Receipt_UpdateUOBAccoutStatus") as SqlCommand;
            //Add Parameters
            insertCmd.Parameters.AddWithValue("@iStrTranTxt", transactionText);            
            insertCmd.Parameters.AddWithValue("@iStrCreated", isCreated);
            insertCmd.Parameters.AddWithValue("@iStrRejectReason", rejectReason);
            insertCmd.Parameters.AddWithValue("@iIntReceiptID", receiptID);
            insertCmd.Parameters.AddWithValue("@iStrReceiptNo", receiptNo);
            
            return insertCmd;
        }
        
        /// <summary>
        /// Update the account status.
        /// </summary>
        /// <param name="accountNo">Account Number</param>
        /// <param name="isCreated">Receipt created or not</param>
        /// <param name="rejectReason">Receipt reject reason</param>
        public static void UpdateUOBAccountStatus(string accountNo, string isCreated, string rejectReason, long receiptID, string receiptNo)
        {
            try
            {
                SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;               
                using (SqlCommand insertCmd = GetInsertCommand(db, accountNo, isCreated, rejectReason, receiptID, receiptNo))
                {
                    db.ExecuteNonQuery(insertCmd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Check the RPS receipt  blocking time, that blocking period we should not create the receipt.
        /// </summary>
        /// <param name="initMode">Init mode 1</param>
        /// <param name="createdBy">Created by</param>
        /// <returns></returns>
        public static bool CheckReceiptInsertTime(int initMode, string createdBy)
        {
            try
            {
                bool IsInsertable = false;
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Connection String"].ConnectionString;

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo.Usp_Receipt_CheckInsert", con))
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

        /// <summary>
        /// Get the account services based on the code type,code and company code.
        /// </summary>
        /// <param name="codeType">code type(Margin- MGNSVCTYPE)</param>
        /// <param name="code">code(Margin- MGN)</param>
        /// <param name="companyCode">PSPL</param>
        /// <returns></returns>
        public static string[] AccountServices(string codeType, string code, string companyCode, out string[] blockingTime )
        {
            try
            {
                string[] accountServices = new string[] { };
                blockingTime = new string[] { };
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Connection String"].ConnectionString;

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo.Usp_Setup_LookupCodes_Fetch", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@iStrCodeType", codeType);
                        cmd.Parameters.AddWithValue("@iStrCode", code);
                        cmd.Parameters.AddWithValue("@iStrCompanyCode", companyCode);                        
                        con.Open();                       
                        SqlDataReader reader = cmd.ExecuteReader();
                        while(reader.Read())
                        {
                           accountServices = reader.IsDBNull(3) ? new string[] { } : reader["Value1"].ToString().Split(',').ToArray();
                           blockingTime = reader.IsDBNull(4) ? new string[] { } : reader["Value2"].ToString().Split(',').ToArray();
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


        /// <summary>
        /// Get the account services based on the code type,code and company code.
        /// </summary>
        /// <param name="codeType">code type(Margin- MGNSVCTYPE)</param>
        /// <param name="code">code(Margin- MGN)</param>
        /// <param name="companyCode">PSPL</param>
        /// <returns></returns>
        public static bool IsPublicHoliday(string currentDate)
        {
            try
            {
                
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Connection String"].ConnectionString;
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo.Udf_CheckPublicHoliday", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@strDateInput", currentDate);
                        SqlParameter returnValue = cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Bit);
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        return Convert.ToBoolean(returnValue.Value);
                    }
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

