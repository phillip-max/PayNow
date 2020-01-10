namespace PayNowReceiptsGeneration.Helpers
{
    public class Deposit : Item
    {
        #region private members

        //Specific members for Deposit
        private bool _isSystemAutoDeposit = false;
        private decimal _felamt = decimal.Zero;
        private string _felamttype = "%";

        #endregion

        #region properties

       // [StringLengthValidator(0, RangeBoundaryType.Exclusive, 0, RangeBoundaryType.Ignore, MessageTemplateResourceName = "MandatoryFieldMsg", Tag = "Currency Code", MessageTemplateResourceType = typeof(ErrorMessages))]
        public override string CurrCd
        {
            get
            {
                return base.CurrCd;
            }
            set
            {
                if (!value.Equals(base.CurrCd))
                    _isSystemAutoDeposit = false;
                base.CurrCd = value;
            }
        }

       // [RangeValidator(typeof(Decimal), "0", RangeBoundaryType.Exclusive, "0", RangeBoundaryType.Ignore, MessageTemplateResourceName = "MinRangeMsg", Tag = "Amount", MessageTemplateResourceType = typeof(ErrorMessages))]
        public override decimal Amount
        {
            get
            {
                return base.Amount;
            }
            set
            {
                if (value != base.Amount)
                {
                    _isSystemAutoDeposit = false;
                    base.Amount = value;
                }
            }
        }

        public decimal FELAmount
        {
            get
            {
                return _felamt;
            }
            set
            {
                if (value != _felamt)
                {
                    _isSystemAutoDeposit = false;
                    _felamt = value;
                }
            }
        }

        public string FELAmountType
        {
            get
            {
                return _felamttype;
            }
            set
            {
                if (!value.Equals(_felamttype))
                {
                    _isSystemAutoDeposit = false;
                    _felamttype = value;
                }
            }
        }

        public bool IsSystemAutoDeposit
        {
            get
            {
                return _isSystemAutoDeposit;
            }
        }

        #endregion

        #region constructor

        protected Deposit() : base()
        {
            ItemDescription = FixedCodes.ItemDesc.Deposit;
        }

        internal Deposit(string currcd, decimal amount)
            : this()
        {
            CurrCd = currcd;
            Amount = amount;
        }

        internal Deposit(string currcd, decimal amount, bool autoDeposit)
            : this(currcd, amount)
        {
            _isSystemAutoDeposit = true;
        }

        #endregion

        #region Public Methods

        /*
         * Do settlements for same currency payments
         */
        /*
        public void Settle1stLevel(Receipt receipt, PaymentList payments)
        {
            //ItemPaymentList oldItemPayments = _setlPayments;
            _setlPayments = new ItemPaymentList();
            if (this.IsValid)
            {
                foreach (Payment p in payments)
                {
                    if (this._amount == _setlPayments.SettledTotal)
                        break;

                    if (p.IsValid && p.IsSharePayment)
                        if (!p.IsValidSharePayment(receipt))
                            continue;

                    if (p.IsValid &&  //is valid
                        p.OSAmount > 0 && //any oustanding amt to use
                        p.CurrCd.Equals(this._currcd))//same currency?                        
                    {
                        _setlPayments.Add(new ItemPayment(this, p, null));
                    }
                }
            }
        }
         */

        /*
         * Do settlements for different currency, but having speicific or All currenceies settlement option payments
         */
        /*
        public void Settle2ndLevel(Receipt receipt, PaymentList payments, Dictionary<string, ExchRateInfo> exchRates)
        {
            //ItemPaymentList oldItemPayments = _setlPayments;
            //_setlPayments = new ItemPaymentList();
            if (this.IsValid && this.OSAmount > 0)
            {
                foreach (Payment p in payments)
                {
                    if (this._amount == _setlPayments.SettledTotal)
                        break;

                    if (p.IsValid &&  //is valid
                        p.OSAmount > 0 && //any oustanding amt to use
                        !p.CurrCd.Equals(this._currcd) &&
                        (p.SetlOption.Equals(this._currcd) || string.IsNullOrEmpty(p.SetlOption)))//Is Setl Option blank OR same with currency
                    {
                        _setlPayments.Add(new ItemPayment(this, p, exchRates));
                    }
                }
            }
        }*/



        public void AdjustAutoDepositAmount()
        {
            if (_isSystemAutoDeposit)
            {
                Amount = SettlementList.TotalSetlAmount;
                _isSystemAutoDeposit = true;
            }
        }

        //protected override SqlCommand GetInsertCommand(SqlDatabase db, Receipt receipt, Account account)
        //{
        //    //decimal totFelAmt = decimal.Zero;

        //    //if (_felamttype.Equals("%"))
        //    //{
        //    //    totFelAmt = decimal.Round(Amount * _felamt / decimal.Parse("100"), 2);
        //    //    foreach (DepositSettlement setl in SettlementList)
        //    //    {
        //    //        setl.FELAmount = decimal.Round(setl.SetlAmount * _felamt / decimal.Parse("100"), 2);
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    totFelAmt = _felamt;
        //    //    foreach (DepositSettlement setl in SettlementList)
        //    //    {
        //    //        setl.FELAmount = decimal.Round(_felamt / Amount * setl.SetlAmount, 2);
        //    //    }
        //    //}

        //    //decimal collectedFel = decimal.Zero;
        //    //foreach (DepositSettlement setl in SettlementList)
        //    //{
        //    //    if (collectedFel + setl.FELAmount > totFelAmt)
        //    //    {
        //    //        setl.FELAmount = totFelAmt - collectedFel;
        //    //        break;
        //    //    }
        //    //    collectedFel += setl.FELAmount;
        //    //}

        //    //for (int i = SettlementList.Count - 1; i >= 0 && collectedFel < totFelAmt; i--)
        //    //{
        //    //    DepositSettlement setl = SettlementList[i] as DepositSettlement;
        //    //    if (setl.SetlAmount - setl.FELAmount > totFelAmt - collectedFel)
        //    //    {
        //    //        setl.FELAmount += (totFelAmt - collectedFel);
        //    //        collectedFel += (totFelAmt - collectedFel);
        //    //    }
        //    //    else
        //    //    {
        //    //        collectedFel += (setl.SetlAmount - setl.FELAmount);
        //    //        setl.FELAmount += (setl.SetlAmount - setl.FELAmount);
        //    //    }
        //    //}

        //    //SqlCommand insertCmd = base.GetInsertCommand(db, receipt, account);
        //    //insertCmd.Parameters.AddWithValue("@iStrFELAmount", string.Format("{0:F2}{1}", _felamt, _felamttype));
        //    //return insertCmd;
        //}

        /*
         * Save Deposit
         */
        /*
        public void SaveMe(SqlDatabase db, SqlTransaction transaction, long receiptID, Account account, int itemNo)
        {
            _rowno = itemNo;
            using (SqlCommand cm = db.GetStoredProcCommand("Usp_Receipt_ReceiptDeposits_Insert") as SqlCommand)
            {
                //Add Parameters
                cm.Parameters.AddWithValue("@iIntReceiptID", receiptID);
                cm.Parameters.AddWithValue("@iIntItemNo", _rowno);
                cm.Parameters.AddWithValue("@iStrAccNo", account.AccNo);
                cm.Parameters.AddWithValue("@iStrAccServiceType", account.AccSvcType);
                cm.Parameters.AddWithValue("@iStrAeCd", account.AeCd);
                cm.Parameters.AddWithValue("@iStrAeRefNo", account.AeRefNo);
                cm.Parameters.AddWithValue("@iStrCurrCd", _currcd);
                cm.Parameters.AddWithValue("@iDecAmount", _amount);
                cm.Parameters.AddWithValue("@iStrFELAmount", string.Format("{0:F2}{1}", _felamt, _felamttype));

                db.ExecuteNonQuery(cm, transaction);
            }

            decimal totFelAmt = decimal.Zero;

            if (_felamttype.Equals("%"))
            {

                totFelAmt = decimal.Round(_amount * _felamt / decimal.Parse("100"), 2);
                foreach (ItemPayment p in _setlPayments)
                {
                    p.FELAmount = decimal.Round(p.SettledAmount * _felamt / decimal.Parse("100"), 2);
                }
            }
            else
            {
                totFelAmt = FELAmount;
                foreach (ItemPayment p in _setlPayments)
                {
                    p.FELAmount = decimal.Round(_felamt / _amount * p.SettledAmount, 2);
                }
            }

            decimal collectedFel = decimal.Zero;
            foreach (ItemPayment p in _setlPayments)
            {
                if (collectedFel + p.FELAmount > totFelAmt)
                {
                    p.FELAmount = totFelAmt - collectedFel;
                }
                collectedFel += p.FELAmount;
            }

            for (int i = _setlPayments.Count - 1; i >= 0 && collectedFel < totFelAmt; i--)
            {
                ItemPayment p = _setlPayments[i];
                if (p.SettledAmount - p.FELAmount > totFelAmt - collectedFel)
                {
                    p.FELAmount += (totFelAmt - collectedFel);
                    collectedFel += (totFelAmt - collectedFel);
                }
                else
                {
                    collectedFel += (p.SettledAmount - p.FELAmount);
                    p.FELAmount += (p.SettledAmount - p.FELAmount);
                }
            }

            _setlPayments.SaveMe(db, transaction, receiptID, this);
        }
        */
        #endregion

        public override string ItemType
        {
            get { return FixedCodes.ItemTypes.Deposit; }
        }

        public override Settlement GetNewSettlementObject()
        {
            return new Settlement();
        }
    }
}
/*
[Serializable]
public class DepositList : SimpleCollection<Deposit>
{
    #region Constructors
    public DepositList()
    {
    }
    #endregion

    #region Public Methods

    public void ClearSystemAutoDeposit()
    {
        int i=0;
        while (i < this.Count)
        {
            if (this[i].IsSystemAutoDeposit)
                this.Remove(this[i]);
            else
                i++;
        }
        for (i = 0; i < this.Count; i++)
            this[i].RowNo = i + 1;
    }

    public Deposit AddDeposit()
    {
        this.Add(new Deposit(this.Count + 1));
        return this[this.Count - 1];
    }

    public Deposit AddDeposit(string currcd, decimal amount)
    {
        this.Add(new Deposit(this.Count + 1, currcd, amount));
        return this[this.Count - 1];
    }

    public Deposit AddAutoDeposit(string currcd)//, decimal amount)
    {
        bool isExist = false;

        foreach (Deposit deposit in this)
        {
            if (deposit.CurrCd.Equals(currcd))
            {
                isExist = true;
                break;
            }
        }
        if (!isExist)
        {
            this.Add(new Deposit(this.Count + 1, currcd, decimal.MaxValue, true));
        }
 */
/*
int i=0;
while(i<this.Count)
{
    if(this[i].IsSystemAutoDeposit && this[i].CurrCd.Equals(currcd))
    {
        amount+=this[i].Amount;
        this.Remove(this[i]);
    }
    else
        i++;
}
this.Add(new Deposit(this.Count + 1, currcd, amount, true));
*/

    //    return this[this.Count - 1];
    //}


    //public Deposit Search(int rowno)
    //{
    //    foreach (Deposit d in this)
    //        if (d.RowNo == rowno) return d;

    //    return null;
    //}

    //public void SaveMe(SqlDatabase db, SqlTransaction transaction, long receiptID, Account account, ref int itemNumber)
    //{
    //    for (int i = 0; i < this.Count; i++)
    //    {
    //        itemNumber++; //Increment Item number
    //        this[i].SaveMe(db, transaction, receiptID, account, itemNumber);
    //    }
    //}
    //#endregion
