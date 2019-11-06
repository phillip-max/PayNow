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
        public static List<string> GetUOBPaidAccounts()
        {
            try
            {
                
                string ConnectionString = "Data Source =10.30.11.41; Initial Catalog = ReceiptsNPayments; Integrated Security = True";
                SqlConnection connection = new SqlConnection(ConnectionString);

                SqlCommand command = new SqlCommand("dbo.Usp_Receipt_UOBPaidAccount_Fetch", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                List<string> AccList = new List<string>();              

                while (reader.Read())
                {
                    AccList.Add(reader["AccountNumber"].ToString());                   
                }

                return AccList;
            }
            catch (Exception ex)
            {
                    throw ex;
            }            
        }
    }
}

