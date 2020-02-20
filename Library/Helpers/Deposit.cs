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
        public void AdjustAutoDepositAmount()
        {
            if (_isSystemAutoDeposit)
            {
                Amount = SettlementList.TotalSetlAmount;
                _isSystemAutoDeposit = true;
            }
        }       
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

