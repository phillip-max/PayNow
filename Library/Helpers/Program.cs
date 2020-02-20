using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using PayNow.Cryptography;

namespace PayNowReceiptsGeneration
{
    public class Program
    {

        static void Main(string[] args)
        {
           // Right time for insert the Receipt
            if (WebServiceHelper.CheckReceiptInsertTime(1, "PayNowUser"))
            {
                    string[] MarginAccountBlockingTime = new string[] { };
                    string[] PiggyAccountBlockingTime = new string[] { };
                    string[] LedgerAccountBlockingTime = new string[] { };
                    string[] NonEquityAccountBlockingTime = new string[] { };
                    string[] ExcludingAccountTypesBlockingTime = new string[] { };

                    string[] MarginAccountServices = WebServiceHelper.AccountServices("MGNSVCTYPE", "MGN", "PSPL", out MarginAccountBlockingTime);
                    string[] PiggyAccountServices = WebServiceHelper.AccountServices("PayNow", "PiggyBank", "PSPL", out PiggyAccountBlockingTime);
                    string[] LedgerAccountServices = WebServiceHelper.AccountServices("LEDSVCTYPE", "LED", "PSPL", out LedgerAccountBlockingTime);
                    string[] NoNEquityAccountServices = WebServiceHelper.AccountServices("NONEQUITYACCSVCTYPE", "NONEQUITYACCSRVTYPE", "", out NonEquityAccountBlockingTime);
                    string[] ExcludingAccountServices = WebServiceHelper.AccountServices("EXCLUDEACCTYPESUOB", "EXCLUDEACCTYPES", "PSPL", out ExcludingAccountTypesBlockingTime);


                    List<NotificationAccount> accountDetails = WebServiceHelper.GetUOBPaidAccounts(PiggyAccountServices);
                    AccountDetails acc = new AccountDetails();

                    acc.InsertReceiptDetails(accountDetails, MarginAccountServices, PiggyAccountServices, LedgerAccountServices,
                                             NoNEquityAccountServices, ExcludingAccountServices, LedgerAccountBlockingTime);
            }          
        }        
    }
}
