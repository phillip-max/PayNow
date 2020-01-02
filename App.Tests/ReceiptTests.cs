using AccessRPSService;
using Library;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Tests
{
    [TestFixture]
    public class ReceiptTests
    {
        [Test]
        public void MarginAccountsReceipt()
        {
           if(WebServiceHelper.CheckReceiptInsertTime(1, "PayNowUser"))
           {
                string[] marginAccountBlockingTime = new string[] { };
                string[] piggyAccountBlockingTime = new string[] { };
                string[] ledgerAccountBlockingTime = new string[] { };
                string[] nonEquityAccountBlockingTime = new string[] { };
                string[] ExcludingAccountTypesBlockingTime = new string[] { };

                string[] MarginAccountServices = WebServiceHelper.AccountServices("MGNSVCTYPE", "MGN", "PSPL", out marginAccountBlockingTime);
                string[] PiggyAccountServices = WebServiceHelper.AccountServices("PayNow", "PiggyBank", "PSPL", out piggyAccountBlockingTime);
                string[] LedgerAccountServices = WebServiceHelper.AccountServices("LEDSVCTYPE", "LED", "PSPL", out ledgerAccountBlockingTime);
                string[] NoNEquityAccountServices = WebServiceHelper.AccountServices("NONEQUITYACCSVCTYPE", "NONEQUITYACCSRVTYPE", "", out nonEquityAccountBlockingTime);
                string[] ExcludingAccountServices = WebServiceHelper.AccountServices("EXCLUDEACCTYPESUOB", "EXCLUDEACCTYPES", "PSPL", out ExcludingAccountTypesBlockingTime);


                List<NotificationAccount> accountDetails = WebServiceHelper.GetUOBPaidAccounts(PiggyAccountServices);
                AccountDetails acc = new AccountDetails();
                acc.InsertReceiptDetails(accountDetails, MarginAccountServices, PiggyAccountServices, LedgerAccountServices,
                                         NoNEquityAccountServices, ExcludingAccountServices, ledgerAccountBlockingTime);
            }
        }        
    }
}
