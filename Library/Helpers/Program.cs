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
            /// Right time for insert the Receipt
            if (WebServiceHelper.CheckReceiptInsertTime(1, "PayNowUser"))
            {
                string[] MarginAccountServices = WebServiceHelper.AccountServices("MGNSVCTYPE", "MGN", "PSPL");
                string[] PiggyAccountServices = WebServiceHelper.AccountServices("PayNow", "PiggyBank", "PSPL");

                List<NotificationAccount> accountDetails = WebServiceHelper.GetUOBPaidAccounts(PiggyAccountServices);
                AccountDetails acc = new AccountDetails();
                acc.InsertReceiptDetails(accountDetails, MarginAccountServices, PiggyAccountServices);
            }
        }
    }
}
