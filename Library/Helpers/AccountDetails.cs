using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using PayNowReceiptsGeneration.CISAccountService;
using PayNowReceiptsGeneration.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PayNowReceiptsGeneration
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
        public void InsertReceiptDetails(List<NotificationAccount> notificationAccounts, string[] MarginAccountServices,
                                        string[] PiggyAccountServices, string[] LedgerAccountServices, string[] NoNEquityAccountServices,
                                        string[] ExcludingAccountServices,string[] LedgerBlockingTime)
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
                    string AccountNumber = notiAcc.AccountNumber.Trim() + notiAcc.AccountServiceType.Trim();

                    AccountDetails accountDetails = new AccountDetails();
                    ClientDetails clientDetail = new ClientDetails();
                    var clientAccountInfo = new DataSet();


                    if (!PiggyAccountServices.Contains(notiAcc.AccountServiceType))
                    {
                       clientAccountInfo = cOREInfo.GetAccountMasterInfo(38, "ACCNO",
                                            new string[] { notiAcc.AccountNumber },
                                            new string[] { notiAcc.AccountNumber });
                    }
                    else 
                    {
                        clientAccountInfo = cOREInfo.GetAccountMasterInfo(1, notiAcc.AccountServiceType.Trim(),
                                            new string[] { notiAcc.AccountNumber },
                                            new string[] { notiAcc.AccountNumber });

                    }
                    if (!this.ValidateClientAccount(clientAccountInfo,  NoNEquityAccountServices, notiAcc.AccountServiceType))
                    {
                        WebServiceHelper.UpdateUOBAccountStatus(notiAcc.TransactionText.Trim(), "ERR", "Account number not found in CIS",
                                                               0, string.Empty);
                        continue;
                    }
                    this.GetAccountDetails(accountDetails, clientAccountInfo, notiAcc.AccountNumber, notiAcc.IsPiggy, notiAcc.AccountServiceType, NoNEquityAccountServices);

                    if (LedgerAccountServices.Contains(accountDetails.AccServiceType) && !CanCreateLedgerReceipt(LedgerBlockingTime))
                    {
                        continue;
                    }
                    

                    if (MarginAccountServices.Contains(accountDetails.AccServiceType)
                       //|| PiggyAccountServices.Contains(accountDetails.AccServiceType) 
                       || LedgerAccountServices.Contains(accountDetails.AccServiceType))
                    {
                        var Receipt = new Receipt(DateTime.Today.ToString(FixedCodes.DateFormatDB),
                                  "PSPL", accountDetails.ClientName, accountDetails.ClientID, "",
                                  accountDetails.ClientRefNo1, accountDetails.ClientRefNo2);

                        Receipt.Payments.Add(new Payment
                        {
                            BankCode = "UOB SGD",
                            Amount = Convert.ToDecimal(notiAcc.Amount) - Convert.ToDecimal(notiAcc.UsedAmount),
                            CurrCd = "SGD",
                            PaymentType = notiAcc.PayNowIndicator.ToUpper() == "Y" ? Enum.GetName(typeof(PaymentMode), 0) : Enum.GetName(typeof(PaymentMode), 1),
                            SetlOption = "SGD",
                            RefBankCode = "UOB",
                            UsedAmount = Convert.ToDecimal(notiAcc.Amount) - Convert.ToDecimal(notiAcc.UsedAmount),
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
                               // Convert.ToDecimal(notiAcc.Amount) -
                                Receipt.Accounts[0].Deposits[0].Amount = Convert.ToDecimal(notiAcc.Amount) - Convert.ToDecimal(notiAcc.UsedAmount);
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
                                    WebServiceHelper.UpdateUOBAccountStatus(notiAcc.TransactionText.Trim(), "CMP",
                                                         string.Empty, Receipt.ReceiptID, Receipt.ReceiptNumber);
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
                    }
                    else if(ExcludingAccountServices.Contains(accountDetails.AccServiceType))
                    {
                        WebServiceHelper.UpdateUOBAccountStatus(notiAcc.TransactionText.Trim(), "BPM",
                                                         string.Empty, 0, "");
                    }

                }
                //Send receipt data to subsystems as batch. 
                //Let's say each batch we will have 100 receipts created, but we will only call the send to subsystem SP once
                try
                {

                   Receipt.SendNewDataToSubSystems(0, "PayNowUser");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private bool ValidateClientAccount(DataSet clientAccountInfo, string[] nonEquityAccountServices, string AccServiceType)
        {
           var dt = GetCorrectAccount(clientAccountInfo, nonEquityAccountServices, AccServiceType);
           return dt.Rows.Count > 0;
        }

        public DataTable GetCorrectAccount(DataSet clientAccountInfo, string[] nonEquityAccountServices, string AccServiceType)
        {
            DataTable dt = clientAccountInfo.Tables[0];
            if(string.IsNullOrEmpty(AccServiceType))
            {
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow dr = dt.Rows[i];
                    if (nonEquityAccountServices.Contains((string)dr["AccServicetype"]))
                        dr.Delete();
                }
                dt.AcceptChanges();
            }                      
            return dt;
        }

        private void GetAccountDetails(AccountDetails accountDetails, System.Data.DataSet clientAccountInfo,
                                     string accNumber, bool isPiggy, string accountServiceType, 
                                     string[] nonEquityAccountServices)
        {
            var accRow =  GetCorrectAccount(clientAccountInfo, nonEquityAccountServices, accountServiceType).Rows[0];//clientAccountInfo.Tables[0].Rows[0];

            accountDetails.AccServiceType = isPiggy ? accountServiceType : accRow.IsNull("AccServiceType") ? string.Empty
                                            : Convert.ToString(accRow["AccServiceType"]);

            if(accRow.Table.Columns.Contains("ClientID"))
            {
                accountDetails.ClientID = accRow.IsNull("ClientID") ? string.Empty : Convert.ToString(accRow["ClientID"]);
            } 
            else
            {
                accountDetails.ClientID = accRow.IsNull("ClientID1") ? string.Empty : Convert.ToString(accRow["ClientID1"]);
            }
            if(accRow.Table.Columns.Contains("ClientName"))
            {
                accountDetails.ClientName = accRow.IsNull("ClientName") ? string.Empty : Convert.ToString(accRow["ClientName"]);
            }
            else
            {
                accountDetails.ClientName = accRow.IsNull("ClientName1") ? string.Empty : Convert.ToString(accRow["ClientName1"]);
            }
            accountDetails.AccountNo = accRow.IsNull("AccNo") ? string.Empty : Convert.ToString(accRow["AccNo"]);
            accountDetails.ClientRefNo1 = accRow.IsNull("ClientRefNo1") ? string.Empty : Convert.ToString(accRow["ClientRefNo1"]);
            accountDetails.ClientRefNo2 = accRow.IsNull("ClientRefNo2") ? string.Empty : Convert.ToString(accRow["ClientRefNo2"]);            
            accountDetails.AddressRefNo = accRow.IsNull("AddressRefNo") ? string.Empty : Convert.ToString(accRow["AddressRefNo"]);
            accountDetails.AeCd = accRow.IsNull("AeCd") ? string.Empty : Convert.ToString(accRow["AeCd"]);
            accountDetails.AeName = accRow.IsNull("AeName") ? string.Empty : Convert.ToString(accRow["AeName"]);
            accountDetails.AeRefNo = accRow.IsNull("AeRefNo") ? string.Empty : Convert.ToString(accRow["AeRefNo"]);
        }

        /// <summary>
        /// Check the account service type is ledger.
        /// If it is ledger we can't create receipt for if the date if fall in public holidays or weekends.
        /// If bill payment is not yet paid still can't create receipt for account.
        /// </summary>
        /// <returns></returns>
        public bool CanCreateLedgerReceipt(string[] LedgerBlockingTime)
        {

            //TimeSpan start = new TimeSpan(17, 0, 0); //10 pm
            //TimeSpan end = new TimeSpan(24, 0, 0); //12 midnight

            bool createReceipt = true;
            bool isPublicHoliDay = WebServiceHelper.IsPublicHoliday(DateTime.Now.ToShortDateString());
            TimeSpan start = TimeSpan.Parse(LedgerBlockingTime[0]);  // 5PM
            TimeSpan end =  TimeSpan.Parse(LedgerBlockingTime[1]);    // 12 AM
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (isPublicHoliDay || DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {                
               createReceipt = false;
            }
            else
            {
                bool isBillPaymentCompleted = WebServiceHelper.CheckReceiptInsertTime(2, "PayNowUser");

                if ((now >= start && now <= end) || !isBillPaymentCompleted)
                    createReceipt = false;
            }
            return createReceipt;
        }
    }
}
