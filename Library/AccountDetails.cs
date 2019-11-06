using AccessRPSService.itsd.dev.backoffice;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessRPSService
{
    public class AccountDetails
    {
        //AccNo,ClientName,ClientID,ClientType,ClientTypecDesc,AccServiceType,AccServiceTypecDesc,AccType,AccTypecDesc,ClientRefNo1,ClientRefNo2,
        //JointAcName,AddressRefNo,AeRefNo,AeCd,AeName,AeFirmId,StatusInd,StatusBy,StatusDate,AltAeRefNo,GSASubAccNo,AccNoCreationDate,EmailRefNo,
        //CDSAccNo,KlseAccNo,W8BenSerialNo,W8SubDate,Gst,SpelCd,MultiCurrency

        public string AccountNo { get; set; }
        public string ClientName { get; set; }

        public string ClientID { get; set; }

        public string ClientType { get; set; }

        public string ClientTypeDesc { get; set; }

        public string AccServiceType { get; set; }

        public string AccServiceTypecDesc { get; set; }

        public string AccType { get; set; }

        public string AccTypecDesc { get; set; }

        public string ClientRefNo1 { get; set; }

        public string ClientRefNo2 { get; set; }
        public string JointAcName { get; set; }
        public string AddressRefNo { get; set; }
        public string AeRefNo { get; set; }
        public string AeCd { get; set; }

        public string AeName { get; set; }

        public string AeFirmId { get; set; }
        public string StatusInd { get; set; }
        public string StatusBy { get; set; }
        public string StatusDate { get; set; }
        public string AltAeRefNo { get; set; }
        public string GSASubAccNo { get; set; }

        public string AccNoCreationDate { get; set; }
        public string EmailRefNo { get; set; }

        public string CDSAccNo { get; set; }
        public string KlseAccNo { get; set; }
        public string W8BenSerialNo { get; set; }

        public string Gst { get; set; }
        public string SpelCd { get; set; }

        public string MultiCurrency { get; set; }



        public void InsertReceiptDetails(List<string> AccountNums)
        {
            //SqlDatabase dbq = DatabaseFactory.CreateDatabase() as SqlDatabase;




            COREInfo cOREInfo = new COREInfo();

            AuthHeader authHeader = new AuthHeader();
            authHeader.UserName = "rps";
            authHeader.Password = "f29c6d292c424991aca2c82277e3d42c==";

            cOREInfo.AuthHeaderValue = authHeader;

            if (AccountNums != null && AccountNums.Count > 0)
            {

                AccountDetails accountDetails = new AccountDetails();
                ClientDetails clientDetail = new ClientDetails();
                foreach (var acc in AccountNums)
                {
                    var accountInfo = cOREInfo.GetAccountMasterInfo(38, "ACCNO", new string[] { acc }, new string[] { acc });

                    var accTb = accountInfo.Tables[0];

                    accountDetails.AccountNo = accTb.Columns["AccNo"].ToString();
                    accountDetails.AccServiceType = accTb.Columns["AccServiceType"].ToString();
                    accountDetails.ClientRefNo1 = accTb.Columns["ClientRefNo1"].ToString();
                    accountDetails.ClientRefNo2 = accTb.Columns["ClientRefNo2"].ToString();
                    accountDetails.ClientName = accTb.Columns["ClientName"].ToString();


                    //var cc = cOREInfo.GetClientInfo("I", "CLIENTREFNO", new string[] { acc });

                    var Receipt = new Receipt(DateTime.Today.ToString(FixedCodes.DateFormatDB), "PSPL", accountDetails.ClientName, accountDetails.ClientID,
                                  "", accountDetails.ClientRefNo1, accountDetails.ClientRefNo2);

                    //Receipt.Payments.Add(new Payment { });
                    //Receipt.Accounts.Add(new Account());
                    try
                    {
                        SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
                        using (SqlConnection connection = db.CreateConnection() as SqlConnection)
                        {
                            connection.Open();
                            SqlTransaction transaction = connection.BeginTransaction();
                            try
                            {
                                Receipt.SaveMe(db, transaction, 0, 0, "Test");
                            }
                            catch (Exception ex)
                            {
                                //  Roll back the transaction. 
                                transaction.Rollback();

                                //foreach (Receipt receipt in this)
                                //{
                                //    // receipt.ResetAutoIDs();
                                //}

                                throw ex;
                            }
                            finally
                            {
                                connection.Close();
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        var rr = ex.Message;
                    }
                    
                }
            }
        }

    }
}
