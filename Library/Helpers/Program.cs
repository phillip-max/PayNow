using AccessRPSService.itsd.dev.backoffice;
using Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessRPSService
{
    class Program
    {
        
        static void Main(string[] args)
        {  
            // Right time for insert the Receipt
            if (WebServiceHelper.CheckReceiptInsertTime(1, "PayNowUser"))
            {
                string[] marginAccountBlockingTime = new string[]{ };
                string[] piggyAccountBlockingTime =  new string[] { };
                string[] ledgerAccountBlockingTime = new string[] { };

                string[] MarginAccountServices = WebServiceHelper.AccountServices("MGNSVCTYPE", "MGN", "PSPL", out marginAccountBlockingTime);
                string[] PiggyAccountServices = WebServiceHelper.AccountServices("PayNow", "PiggyBank", "PSPL", out piggyAccountBlockingTime);
                string[] LedgerAccountServices = WebServiceHelper.AccountServices("LEDSVCTYPE", "LED", "PSPL", out ledgerAccountBlockingTime);

                List<NotificationAccount> accountDetails = WebServiceHelper.GetUOBPaidAccounts(PiggyAccountServices);
                AccountDetails acc = new AccountDetails();
                acc.InsertReceiptDetails(accountDetails, MarginAccountServices, PiggyAccountServices, LedgerAccountServices,
                                        ledgerAccountBlockingTime);
            }
        }
    }
}
