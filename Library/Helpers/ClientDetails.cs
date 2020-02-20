using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayNowReceiptsGeneration
{
    public class ClientDetails
    {      

        public string ClientID { get; set; }
        public string ClientType { get; set; }
        public string ClientName { get; set; }
        public string ClientRefNo { get; set; }
        public string StatusInd { get; set; }
        public string FamilyName { get; set; }
        public string Title { get; set; }
        public string Sex { get; set; }
        public string Gst { get; set; }
        public string DOB { get; set; }
        public string Nationality { get; set; }
        public string PRStatus { get; set; }
        public string Occupation { get; set; }
        public string EmployerName { get; set; }
        public string ModifiedDate { get; set; }
        public string AccCreationDate { get; set; }
        public string BankCd { get; set; }
        public string BankName { get; set; }
        public string BnkAccNo { get; set; }

        public string BnkBranchCd { get; set; }
        public string BankBranchName { get; set; }

        public string AnnualIncome { get; set; }



        public ClientDetails GetClientDetails()
        {
            return new ClientDetails();
        }
    }
}
