using AccessRPSService.itsd.dev.backoffice;
using Library;
using Library.Helpers;
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


        /// <summary>
        /// Get all the accounts that is inserted in the table Tb_UOB_Notification(while UOB calling our API).
        /// from that account number get the account details from core system and created the new receipt object.
        /// </summary>
        /// <param name="notificationAccounts"></param>
        public void InsertReceiptDetails(List<NotificationAccount> notificationAccounts, string[] MarginAccountServices, string[] PiggyAccountServices)
        {
            COREInfo cOREInfo = new COREInfo();
            AuthHeader authHeader = new AuthHeader
            {
                UserName = System.Configuration.ConfigurationManager.AppSettings["User"],
                Password = System.Configuration.ConfigurationManager.AppSettings["Password"]
            };
            cOREInfo.AuthHeaderValue = authHeader;

            if (notificationAccounts != null && notificationAccounts.Count > 0)
            {
                foreach (var notiAcc in notificationAccounts)
                {
                    AccountDetails accountDetails = new AccountDetails();
                    ClientDetails clientDetail = new ClientDetails();

                    var clientAccountInfo = cOREInfo.GetAccountMasterInfo(38, "ACCNO",
                                       new string[] { notiAcc.AccountNumber },
                                       new string[] { notiAcc.AccountNumber });

                    if (!this.ValidateClientAccount(clientAccountInfo))
                    {
                        WebServiceHelper.UpdateUOBAccountStatus(notiAcc.AccountNumber, "N", "Account Number Not found");
                        continue;
                    }                  

                    this.GetAccountDetails(accountDetails, clientAccountInfo, notiAcc.AccountNumber, notiAcc.IsPiggy, notiAcc.AccountServiceType);                    

                    if (MarginAccountServices.Contains(accountDetails.AccServiceType) 
                       || PiggyAccountServices.Contains(accountDetails.AccServiceType))
                    {
                        var Receipt = new Receipt(DateTime.Today.ToString(FixedCodes.DateFormatDB),
                                  "PSPL", accountDetails.ClientName, accountDetails.ClientID, "",
                                  accountDetails.ClientRefNo1, accountDetails.ClientRefNo2);

                        Receipt.Payments.Add(new Payment
                        {
                            BankCode = "UOB SGD",
                            Amount = Convert.ToDecimal(notiAcc.Amount),
                            CurrCd = "SGD",
                            PaymentType = Enum.GetName(typeof(PaymentMode), 0),
                            SetlOption = "SGD",
                            RefBankCode = "UOB",
                            UsedAmount = Convert.ToDecimal(notiAcc.Amount),
                            RefText = "",
                            Is3rdPartyPayment = false,
                            IsRemisierPayment = false,
                            ThirdPartyName = string.Empty,
                            ThirdPartyNRIC = string.Empty,
                            ThridPartyRemark = string.Empty,
                            _dbExcessAmt = 0,
                            _dbRefundCurrCd = string.Empty,
                            _dbRefundMethod = string.Empty

                        });
                        Receipt.Accounts.AddAccount(new Account
                        {
                            AccNo = accountDetails.AccountNo,
                            AccSvcType = accountDetails.AccServiceType,
                            AddressRefNo = accountDetails.AddressRefNo,
                            AeCd = accountDetails.AeCd,
                        });

                        //Set posted amount
                        if (Receipt.Accounts.Count > 0)
                        {
                            if (Receipt.Accounts[0].Deposits.Count > 0)
                            {
                                Receipt.Accounts[0].Deposits[0].Amount = Convert.ToDecimal(notiAcc.Amount);
                            }
                        }

                        foreach (Account ac in Receipt.Accounts)
                           foreach (Item item in ac.Deposits)
                                item.SettleDirect(Receipt, Receipt.Payments);

                        try
                        {
                            SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
                            using (SqlConnection connection = db.CreateConnection() as SqlConnection)
                            {
                                connection.Open();
                                SqlTransaction transaction = connection.BeginTransaction();
                                try
                                {
                                    Receipt.SaveMe(db, transaction, 0, 0, "PayNowUser");
                                    transaction.Commit();
                                    WebServiceHelper.UpdateUOBAccountStatus(notiAcc.AccountNumber, "Y", string.Empty);
                                }
                                catch (Exception ex)
                                {
                                    //Roll back the transaction. 
                                    transaction.Rollback();
                                    throw ex;
                                }
                                finally
                                {
                                    connection.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        //Send Data To Sub Systems    
                        // Need to send subsystem to batch not single receipt.
                        //if (Receipt.TransactionID > 0)
                        //{
                        //    long receiptID = Receipt.TransactionID;
                        //    try
                        //    {

                        //        Receipt.SendNewDataToSubSystems(receiptID, "PayNowUser");
                        //    }
                        //    catch { }
                        //}
                    }

                }
            }
        }

        private bool ValidateClientAccount(DataSet clientAccountInfo)
        {
            return clientAccountInfo.Tables[0].Rows.Count > 0;
        }

        private void GetAccountDetails(AccountDetails accountDetails, System.Data.DataSet clientAccountInfo, string accNumber, bool isPiggy, string accountServiceType)
        {
            var accRow = clientAccountInfo.Tables[0].Rows[0];

            accountDetails.AccServiceType = isPiggy ? accountServiceType : accRow.IsNull("AccServiceType") ? string.Empty : Convert.ToString(accRow["AccServiceType"]);

            accountDetails.ClientID = accRow.IsNull("ClientID") ? string.Empty : Convert.ToString(accRow["ClientID"]);
            accountDetails.AccountNo = accRow.IsNull("AccNo") ? string.Empty : Convert.ToString(accRow["AccNo"]);            
            accountDetails.ClientRefNo1 = accRow.IsNull("ClientRefNo1") ? string.Empty : Convert.ToString(accRow["ClientRefNo1"]);
            accountDetails.ClientRefNo2 = accRow.IsNull("ClientRefNo2") ? string.Empty : Convert.ToString(accRow["ClientRefNo2"]);
            accountDetails.ClientName = accRow.IsNull("ClientName") ? string.Empty : Convert.ToString(accRow["ClientName"]);
            accountDetails.AddressRefNo = accRow.IsNull("AddressRefNo") ? string.Empty : Convert.ToString(accRow["AddressRefNo"]);
            accountDetails.AeCd = accRow.IsNull("AeCd") ? string.Empty : Convert.ToString(accRow["AeCd"]);
            accountDetails.AeName = accRow.IsNull("AeName") ? string.Empty : Convert.ToString(accRow["AeName"]);
            accountDetails.AeRefNo = accRow.IsNull("AeRefNo") ? string.Empty : Convert.ToString(accRow["AeRefNo"]);
        }
    }
}
