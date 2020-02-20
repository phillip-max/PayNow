using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Simple;
using System;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using PayNowReceiptsGeneration.Helpers;

namespace PayNowReceiptsGeneration
{
    public class Account : SimpleBase<Account>
    {
        #region private members
        private string _accno;
        private string _accsvctype;
        private string _aerefno;
        private string _aecd;
        private string _aename;
        private bool _hasledger;
        private bool _isMultiCurrency;
        private string[] _ledgerCurrencies = null;
        private string _addressRefNo;      

        private ItemList _miscentry = new ItemList();
        private ItemList _utContracts = new ItemList();
        private ItemList _deposits = new ItemList();
        private ItemList _ContraLoss = new ItemList(); // Venkat RPS00037 18/08/2010
        private ItemList _NOMTransactions = new ItemList(); // Venkat RPS00036 10/11/2010
        private ItemList _caContracts = new ItemList();//WAI RPS00055 22/02/2013

        #endregion

        #region properties
        public string AccNo
        {
            get
            {
                return _accno;
            }
            set
            {
                _accno = value;
            }
        }
        public string AccSvcType
        {
            get
            {
                return _accsvctype;
            }
            set
            {
                _accsvctype = value;
            }
        }
        public string AeRefNo
        {
            get
            {
                return _aerefno;
            }
            set
            {
                _aerefno = value;
            }
        }
        public string AeCd
        {
            get
            {
                return _aecd;
            }
            set
            {
                _aecd = value;
            }
        }
        public string AeName
        {
            get
            {
                return _aename;
            }
            set
            {
                _aename = value;
            }
        }
        public bool HasLedger
        {
            get
            {
                return _hasledger;
            }
            set
            {
                _hasledger = value;
            }
        }
        public bool IsMultiCurrency
        {
            get
            {
                return _isMultiCurrency;
            }
            set
            {
                _isMultiCurrency = value;
            }
        }
        public string AddressRefNo
        {
            get
            {
                return _addressRefNo;
            }
            set
            {
                _addressRefNo = value;
            }
        }

        public ItemList Deposits
        {
            get
            {
                return _deposits;
            }
        }

        public ItemList MiscEntry
        {
            get
            {
                return _miscentry;
            }
        }

        public ItemList UTContracts
        {
            get
            {
                return _utContracts;
            }
        }

        public ItemList CAContracts//WAI RPS00055 22/02/2013
        {
            get
            {
                return _caContracts;
            }
        }

        public ItemList ContraLoss// Venkat RPS00037 18/08/2010
        {
            get
            {
                return _ContraLoss;
            }
        }
        public ItemList NOMTransactions// Venkat RPS00036 10/11/2010
        {
            get
            {
                return _NOMTransactions;
            }
        }
        public string[] LedgerCurrencies
        {
            get
            {
                return _ledgerCurrencies.Clone() as string[];
            }
        }

        public ItemList[] Items
        {
            get
            {
                return new ItemList[] 
                {
                    _miscentry,
                    _utContracts,
                    _deposits,
                    _ContraLoss,
                    _NOMTransactions,
                    _caContracts
                };// Venkat RPS00037 18/08/2010
                // Venkat RPS00036 10/11/2010
            }
        }       

        #endregion

        #region overrides
        public override string ToString()
        {
            return string.Format("{0}/{1}({2})", _aecd, _accno, _accsvctype);
        }
        #endregion

        #region constructor
        public Account()
        {

        }

        public Account(Account ac)
            : this()
        {
            _aerefno = ac.AeRefNo;
            _aecd = ac.AeCd;
            _aename = ac.AeName;
            _accno = ac.AccNo;
            _addressRefNo = ac.AddressRefNo;            
            _accsvctype = ac.AccSvcType;            
            while (_deposits.Count < 1)
                AddDeposit();
        }
    

    /*
     * Save Items
     */
    public void SaveItems(SqlDatabase db, SqlTransaction transaction, Receipt receipt, ref int itemNumber)
        {
            foreach (ItemList itemlist in Items)
            {
                itemlist.Save(db, transaction, receipt, this, ref itemNumber);
            }
        }

        #endregion

        #region Validation
        [SelfValidation]
        public void DoValidate(ValidationResults results)
        {
            foreach (ItemList itemlist in Items)
            {
                if (!itemlist.IsValid)
                {
                    ValidationResult result = new ValidationResult("Invalid Deposits", typeof(ItemList), "", "", null);
                    results.AddResult(result);
                }
            }
        }
        #endregion       

        public Deposit AddDeposit(string currcd, decimal amount)
        {
            Deposit newDeposit = (Deposit)this._deposits.Add(new Deposit(currcd, amount));
            return newDeposit;
        }

        public Deposit AddDeposit()
        {
            return AddDeposit(FixedCodes.BaseCurrencyCode, decimal.Zero);
        }        
    }

    [Serializable]
    [HasSelfValidation]
    public class AccountList : SimpleCollection<Account>
    {
        public AccountList()
        {

        }

        public Account AddAccount(Account acc)
        {
            Account ac = new Account(acc);            
            this.Add(ac);
            return ac;
        }

        public Account Search(string accno, string accsvctype)
        {
            foreach (Account ac in this)
                if (ac.AccNo.Equals(accno) && ac.AccSvcType.Equals(accsvctype))
                    return ac;

            return null;
        }

        public bool IsSingleAE
        {
            get
            {
                if (this.Count <= 0) return false;
                else
                {
                    bool issingleae = true;
                    string prevAeRefNo = this[0].AeRefNo;
                    for (int i = 1; i < this.Count; i++)
                    {
                        if (!this[i].AeRefNo.Equals(prevAeRefNo))
                        {
                            issingleae = false;
                            break;
                        }
                    }
                    return issingleae;
                }
            }
        }

        public string SingleAeRefNo
        {
            get
            {
                if (IsSingleAE)
                    return this[0].AeRefNo;
                else
                    return string.Empty;
            }
        }

        public void Save(SqlDatabase db, SqlTransaction transaction, Receipt receipt)
        {
            int itemnumber = 0;
            foreach (Account account in this)
                account.SaveItems(db, transaction, receipt, ref itemnumber);
        }

        public void SaveWithDrawal(SqlDatabase db, SqlTransaction transaction, Receipt receipt)
        {
            bool isDeposit = false;
            foreach (Account account in this)
            {
                if (account.AccSvcType == "CA")
                {
                    for (int i = 0; i < account.Items.Length; i++)
                    {
                        for (int j = 0; j < account.Items[i].Count; j++)
                        {
                            if (account.Items[i][j] != null)
                            {
                                if (account.Items[i][j].ItemType == "D")
                                {
                                    isDeposit = true;
                                    SaveAW(db, transaction, receipt.ReceiptID, "RPSAW", account.AccSvcType);
                                    break;
                                }
                            }
                        }
                        if (isDeposit == true)
                        {
                            break;
                        }

                    }
                }
            }
        }


        public void SaveAW(SqlDatabase db, SqlTransaction transaction, long receiptID, string source, string accServiceType)
        {
            using (SqlCommand cm = db.GetStoredProcCommand("Usp_Payment_PIProcess_Insert") as SqlCommand)
            {
                //Add Parameters 
                cm.Parameters.AddWithValue("@StrSource", source);
                cm.Parameters.AddWithValue("@iIntReceiptID", receiptID);
                cm.Parameters.AddWithValue("@istrAccservicetype", accServiceType);

                db.ExecuteNonQuery(cm, transaction);
            }
        }
        #region Validation
        [SelfValidation]
        public void DoValidate(ValidationResults results)
        {
            if (this.Count == 0)
            {
                ValidationResult result = new ValidationResult("There is no depoist/item to save.", typeof(AccountList), "", "", null);
                results.AddResult(result);
            }
        }
        #endregion

        /// <summary>
        /// Sort Accounts according the order from lookup. The values from lookup comes first.
        /// [WAI]+[RPS00055]+[20140205]
        /// </summary>
        public void Sort()
        {
            /*New Comment*/
            string accSvcTypeOrder = "";// LookupCodeInfo.GetAccServiceTypeOrderInfo("RECEIPT_UI").Value1;
            if (!string.IsNullOrEmpty(accSvcTypeOrder))
            {
                string[] orders = accSvcTypeOrder.Split(',');
                AccountList accountList = new AccountList();//List to maintain final result
                AccountList accountsAtLast = this;//List which acc svc type is not in the lookup. Need to append at last.
                //Sort the list based on accsvc order from lookup
                foreach (string order in orders)
                {
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (this[i].AccSvcType == order)//if in lookup add in final result list and remove from at last.
                        {
                            accountList.Add(this[i]);
                            accountsAtLast.Remove(this[i]);
                            i = 0;
                        }
                    }
                }
                //add acc from at last list to final result
                foreach (Account acc in accountsAtLast)
                {
                    accountList.Add(acc);
                }
                this.Clear();
                //transfer final result list to this.
                foreach (Account acc in accountList)
                {
                    this.Add(acc);
                }
            }
        }
    }
}
