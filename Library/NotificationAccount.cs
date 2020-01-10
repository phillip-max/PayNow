namespace PayNowReceiptsGeneration
{
    public class NotificationAccount
    {
        public string Event { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }

        public string AccountNumber { get; set; }

        public string AccountCurrency { get; set; }
        public string Amount { get; set; }

        public string TransactionType { get; set; }
        public string OurReference { get; set; }

        public string YourReference { get; set; }

        public string TransactionText { get; set;  }

        public bool IsPiggy { get; set; }

        public string AccountServiceType { get; set; }

        public string UsedAmount { get; set; }

        public string PayNowIndicator { get; set; }
    }
}
