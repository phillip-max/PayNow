using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using PayNowReceiptsGeneration.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;

namespace PayNowReceiptsGeneration
{
    public class Receipt
    {

        #region Private Members
        private PaymentList _paymentlist = null;
        private AccountList _accounts = null;
        /*New comment*/
        //private ExcessAmountList _excessAmts = null;
        private string _amlAlertRemark = string.Empty;
        //private Dictionary<string, ExchRateInfo> _jsExchRates;
        private string _businessDate = DateTime.Today.ToString(FixedCodes.DateFormatDB);
        private bool _IsPseudoReceipt = false;
        private bool _IsUserOverrideSelection = false;

       #endregion

       #region Properties

        public long ReceiptID { get; set; } = 0;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string CompanyCode { get; } = string.Empty;
        public string ClientName { get; } = string.Empty;
        public string ClientID1 { get; } = string.Empty;

        public string ClientID2 { get; } = string.Empty;

        public string ClientRefNo1 { get; } = string.Empty;

        public string ClientRefNo2 { get; } = string.Empty;

        public long TransactionID { get; set; } = 0;

        public decimal AMLAlertAmount { get; set; } = decimal.Zero;

        public bool IsMarginAcct { get; set; } = false;


        public PaymentList Payments
        {
            get
            {
                return _paymentlist;
            }
        }

        /*
         * GET Accounts Collection
         */
        public AccountList Accounts
        {
            get
            {
                return _accounts;
            }
        } 

        public string AMLAlertRemark
        {
            get
            {
                return _amlAlertRemark;
            }
            set
            {
                _amlAlertRemark = value.Trim();
            }
        }

        public bool IsPseudoReceipt
        {
            get { return _IsPseudoReceipt; }
            set { _IsPseudoReceipt = value; }
        }

        public bool IsUserOverrideSelection
        {
            get { return _IsUserOverrideSelection; }
            set { _IsUserOverrideSelection = value; }
        }

        public string BusinessDate
        {
            get
            {
                return _businessDate;
            }
            set
            {
                DateTime newdate;
                if (DateTime.TryParseExact(value, FixedCodes.DateFormatDB, null, System.Globalization.DateTimeStyles.None, out newdate))
                {
                    if (newdate.CompareTo(DateTime.Today) <= 0)
                        _businessDate = value.Trim();
                    else
                        throw new ApplicationException("Business Date mustn't greater than system date.");
                }
            }
        }

        public string BusinessDateFormatted
        {
            get
            {
                return DateTime.ParseExact(_businessDate, FixedCodes.DateFormatDB, null).ToString(FixedCodes.DateFormat);
            }
            set
            {
                DateTime newdate;
                if (DateTime.TryParseExact(value, FixedCodes.DateFormat, null, System.Globalization.DateTimeStyles.None, out newdate))
                {
                    if (newdate.CompareTo(DateTime.Today) <= 0)
                        _businessDate = newdate.ToString(FixedCodes.DateFormatDB);
                    else
                        throw new ApplicationException("Business Date mustn't greater than system date.");
                }
            }
        }

        public bool HasAnythingToSave
        {
            get
            {
                bool hasanything = false;
                foreach (Account ac in this.Accounts)
                {
                    foreach (ItemList items in ac.Items)
                    {
                        foreach (Item item in items)
                            /*New Comment*/
                            if (/*item.IsValid &&*/ item.SetlAmount > 0)
                            {
                                hasanything = true;
                                break;
                            }
                    }
                }
                return hasanything;
            }
        }
        #endregion


        public Receipt(string bizdate, string companyCode, string clientname, string clientID1, string clientID2, string clientRefNo1, string clientRefNo2)
        {
            BusinessDate = bizdate;
            CompanyCode = companyCode;
            ClientID1 = clientID1;
            ClientID2 = clientID2;
            ClientName = clientname;
            ClientRefNo1 = clientRefNo1;
            ClientRefNo2 = clientRefNo2;
            _accounts = new AccountList();
            _paymentlist = new PaymentList();
            //_excessAmts = new ExcessAmountList();
        }

        internal void SaveMe(SqlDatabase db, SqlTransaction transaction, int locationID, int terminalID, string userID)
        {
            bool isvalid = true;// IsValid;
            if (!isvalid)
                throw new ApplicationException("The system couldn't save invalid receipt.");
            else if (ReceiptID > 0)
                throw new ApplicationException("The system couldn't save already saved receipt.");
            else
            {
                //Save Receipt
                ExecuteInsert(db, transaction, locationID, terminalID, userID);

                if(this.TransactionID > 0)
                {
                    //Save Payments
                    _paymentlist.SaveMe(db, transaction, this);

                    //Save Items
                    _accounts.Save(db, transaction, this);
                   
                }

            }
        }       

        protected void ExecuteInsert(SqlDatabase db, SqlTransaction transaction, int locationID, int terminalID, string userID)
        {
            using (SqlCommand cm = db.GetStoredProcCommand("Usp_Receipt_Receipts_Insert") as SqlCommand)
            {
                DateTime createdDateTime = DateTime.Now;
                //Add Parameters
                //cm.Parameters.AddWithValue("@iStrBusinessDate", createdDateTime.ToString(FixedCodes.DateFormatDB));
                cm.Parameters.AddWithValue("@iStrBusinessDate", _businessDate);
                cm.Parameters.AddWithValue("@iStrCompanyCode", CompanyCode);
                cm.Parameters.AddWithValue("@iStrClientName", ClientName);
                cm.Parameters.AddWithValue("@iStrClientID1", ClientID1 ?? "f");
                cm.Parameters.AddWithValue("@iStrClientID2", ClientID2);
                cm.Parameters.AddWithValue("@iStrClientRefNo1", ClientRefNo1);
                cm.Parameters.AddWithValue("@iStrClientRefNo2", ClientRefNo2);

                string addressRefNo = string.Empty;

                if (_accounts.Count > 0)
                    addressRefNo = _accounts[0].AddressRefNo;

                cm.Parameters.AddWithValue("@iStrAddressRefNo", addressRefNo);
                cm.Parameters.AddWithValue("@iIntLocationID", locationID);
                cm.Parameters.AddWithValue("@iIntTerminalID", terminalID);
                cm.Parameters.AddWithValue("@iDecAMLAlertAmount", AMLAlertAmount);
                cm.Parameters.AddWithValue("@iStrAMLAlertRemark", _amlAlertRemark);

                if (TransactionID != 0)
                    cm.Parameters.AddWithValue("@iIntTransactionID", TransactionID);

                cm.Parameters.AddWithValue("@iStrCreatedBy", userID);
                cm.Parameters.AddWithValue("@iDteCreatedDateTime", createdDateTime);
                cm.Parameters.Add("@oIntReceiptID", SqlDbType.BigInt);
                cm.Parameters["@oIntReceiptID"].Direction = ParameterDirection.Output;
                cm.Parameters.Add("@oStrReceiptNo", SqlDbType.VarChar, 20);
                cm.Parameters["@oStrReceiptNo"].Direction = ParameterDirection.Output;

                db.ExecuteNonQuery(cm, transaction);                

                ReceiptID = long.Parse(cm.Parameters["@oIntReceiptID"].Value.ToString());
                ReceiptNumber = cm.Parameters["@oStrReceiptNo"].Value.ToString();

                if (TransactionID == 0)
                    TransactionID = ReceiptID;
            }
        }       

        public static bool SendNewDataToSubSystems(long receiptID, string userID)
        {
            return SendDataToSubSystems(receiptID, userID, 1);
        }
        public static bool SendDataToSubSystems(long receiptID, string userID, int mode)
        {
            SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;

            //Send Data To Sub Systems
            using (SqlConnection connection = db.CreateConnection() as SqlConnection)
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (SqlCommand cm = db.GetStoredProcCommand("Usp_Receipt_SendSettlementsToSubSystems_Paynow") as SqlCommand)
                    {
                        cm.CommandTimeout = 300;//Convert.ToInt32(ConfigurationManager.AppSettings["ForceTimeout2"].ToString());
                        DateTime createdDateTime = DateTime.Now;
                        //Add Parameters
                        cm.Parameters.AddWithValue("@iIntMode", mode); //Mode 1 --> New Receipts, Mode 2 --> Reverse receipt
                        cm.Parameters.AddWithValue("@iIntReceiptID", receiptID);
                        cm.Parameters.AddWithValue("@iStrSendBy", userID);
                        cm.Parameters.AddWithValue("@FetchFor", Enum.GetName(typeof(PaymentMode), 0));                        
                        int x = db.ExecuteNonQuery(cm);
                    }
                    // Commit the transaction.
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    // Roll back the transaction. 
                    transaction.Rollback();
                    //bool rethrow = ExceptionPolicy.HandleException(ex, "RPS Alert Policy with Email");
                    //if (rethrow)
                    //    throw;
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        } 
    }
}
