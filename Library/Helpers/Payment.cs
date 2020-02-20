using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Simple;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PayNowReceiptsGeneration
{
    public class Payment : SimpleBase<Payment>
    {
        #region private variables
        private int _rowno = 0;
        private string _paymenttypedesc = string.Empty;
        private string _currency = string.Empty;
        private string _bankcode = string.Empty;
        //removed default value to amount-swapna 12/11/2008
        private decimal _amount;
        private string _refbankcode = string.Empty;
        private string _ref = string.Empty;
        private string _setlOption = string.Empty;
        private decimal _usedAmount = decimal.Zero;
        private bool _is3rdPartyPayment = false;
        private bool _isRemisierPayment = false;
        private string _3rdName = string.Empty;
        private string _3rdRemark = string.Empty;
        private string _3rdNRIC = string.Empty;

        private string _aeRefNo = string.Empty;
        private string _ownerClientID1 = string.Empty;
        private string _ownerClientID2 = string.Empty;
        private string _ownerClientName = string.Empty;

        private decimal _usableAmount = decimal.Zero;
        private bool _isSharePayment = false;
        public decimal _dbExcessAmt = decimal.Zero;
        public string _dbRefundCurrCd = string.Empty;
        public string _dbRefundMethod = string.Empty;
        private long _receiptID = 0;
        private Dictionary<string, decimal> _customExchRates = new Dictionary<string, decimal>();
        private Dictionary<string, decimal> _customShowExchRates = new Dictionary<string, decimal>();
        private decimal _itemTotalAmtInPayCurrCd = decimal.Zero;//[WAI]+[20130917]+RPS0005+Item total amount in payment currency
        private bool _isCreditLedger = false;//[WAI] + [20131202] + RPS0005 + Flag that indicate credit ledger payment.
        private decimal _creditLedgerAmount = decimal.Zero;//[WAI] + [20131202] + RPS0005 + Flag that indicate credit ledger payment.


        [NonSerialized]
        private Payment _orgPayment = null;

        #endregion

        public new Payment Clone()
        {
            Payment newPayment = base.Clone();
            newPayment._orgPayment = this._orgPayment;
            return newPayment;
        }


        #region Accessors
        [System.ComponentModel.DataObjectField(true, false)]
        public int RowNo
        {
            get
            {
                return _rowno;
            }
        }

       // [StringLengthValidator(0, RangeBoundaryType.Exclusive, 0, RangeBoundaryType.Ignore, MessageTemplateResourceName = "MandatoryFieldMsg", Tag = "Payment Type", MessageTemplateResourceType = typeof(ErrorMessages))]
        public string PaymentType { get; set; } = string.Empty;

       // [StringLengthValidator(0, RangeBoundaryType.Exclusive, 0, RangeBoundaryType.Ignore, MessageTemplateResourceName = "MandatoryFieldMsg", Tag = "Currency Code", MessageTemplateResourceType = typeof(ErrorMessages))]
        public string CurrCd
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
            }
        }

       // [StringLengthValidator(0, RangeBoundaryType.Exclusive, 0, RangeBoundaryType.Ignore, MessageTemplateResourceName = "MandatoryFieldMsg", Tag = "Bank Code", MessageTemplateResourceType = typeof(ErrorMessages))]
        public string BankCode
        {
            get
            {
                return _bankcode;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _bankcode = string.Empty;
                else _bankcode = value;
            }
        }

      //  [RangeValidator(typeof(Decimal), "0", RangeBoundaryType.Exclusive, "0", RangeBoundaryType.Ignore, MessageTemplateResourceName = "MinRangeMsg", Tag = "Amount", MessageTemplateResourceType = typeof(ErrorMessages))]
        public decimal Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                if (!_isSharePayment)
                {
                    _amount = value;
                    _usableAmount = value;
                }
                else
                    throw new ApplicationException("This is a share payment");
            }
        }

        public decimal UsableAmount
        {
            get
            {
                return _usableAmount;
            }
        }

        public bool IsSharePayment
        {
            get
            {
                return _isSharePayment;
            }
        }

        public Payment OrgPayment
        {
            get
            {
                return _orgPayment;
            }
        }

        public string RefBankCode
        {
            get
            {
                return _refbankcode;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _refbankcode = string.Empty;
                else _refbankcode = value;
            }
        }

        public string RefText
        {
            get
            {
                return _ref;
            }
            set
            {
                _ref = value.Trim();
            }
        }

        public string SetlOption
        {
            get
            {
                return _setlOption;
            }
            set
            {
                _setlOption = value;
            }
        }

        public decimal UsedAmount
        {
            get
            {
                return _usedAmount;
            }
            set
            {
                _usedAmount = value;
            }
        }

        public bool Is3rdPartyPayment
        {
            get
            {
                return _is3rdPartyPayment;
            }
            set
            {
                _is3rdPartyPayment = value;
                if (value == false)
                {
                    _3rdName = string.Empty;
                    _3rdRemark = string.Empty;
                    _3rdNRIC = string.Empty;
                }
            }
        }

        public bool IsRemisierPayment
        {
            get
            {
                return _isRemisierPayment;
            }
            set
            {
                _isRemisierPayment = value;
                /*if (_isRemisierPayment)
                {
                    _is3rdPartyPayment = true;
                }*/
            }
        }

        public string ThirdPartyName
        {
            get
            {
                return _3rdName;
            }
            set
            {
                _3rdName = value.Trim();
            }
        }

        public string ThridPartyRemark
        {
            get
            {
                return _3rdRemark;
            }
            set
            {
                _3rdRemark = value.Trim();
            }
        }

        public string ThirdPartyNRIC
        {
            get
            {
                return _3rdNRIC;
            }
            set
            {
                _3rdNRIC = value.Trim();
            }
        }

        public decimal OSAmount
        {
            get
            {
                decimal v = _usableAmount - _usedAmount;
                if (v > decimal.Zero)
                    return v;
                else return decimal.Zero;
            }
        }

        public string PaymentTypeDesc
        {
            get
            {
                return _paymenttypedesc;
            }
            set
            {
                _paymenttypedesc = value;
            }
        }

        public Dictionary<string, decimal> CustomeExchangeRates
        {
            get
            {
                return _customExchRates;
            }
        }

        public Dictionary<string, decimal> CustomeShowExchangeRates
        {
            get
            {
                return _customShowExchRates;
            }
        }

        /// <summary>
        /// [WAI]+[20130917]+Item total amount in payment currency
        /// </summary>
        public decimal ItemTotalAmtInPayCurr
        {
            get { return _itemTotalAmtInPayCurrCd; }
            set { _itemTotalAmtInPayCurrCd = value; }
        }

        public bool IsCreditLedger
        {
            get
            {
                return _isCreditLedger;
            }
        }

        public decimal CreditLedgerAmount
        {
            get
            {
                return _creditLedgerAmount;
            }
        }
        #endregion

        #region constructor
        public Payment()
        {

        }

        public Payment(int rowno)
            : this()
        {
            _rowno = rowno;
        }

        public Payment CloneForSharePayment(Receipt receipt, Receipt newReceipt, int paymentRowNo)
        {
            if (!_isSharePayment && !_is3rdPartyPayment && !_isRemisierPayment)
            {
                _ownerClientID1 = receipt.ClientID1;
                _ownerClientID2 = receipt.ClientID2;
                _ownerClientName = receipt.ClientName;
            }
            if (!_isSharePayment && _isRemisierPayment && receipt.Accounts.IsSingleAE)
            {
                _aeRefNo = receipt.Accounts.SingleAeRefNo;
            }

            Payment newPayment = this.Clone();
            newPayment._isSharePayment = true;
            if (this.IsSharePayment)
                newPayment._orgPayment = this.OrgPayment;
            else
                newPayment._orgPayment = this;

            newPayment._rowno = paymentRowNo;
            newPayment._usableAmount = this.OSAmount;
            newPayment._usedAmount = 0;
            newPayment._setlOption = _currency;

            if (!_isRemisierPayment)
            {
                bool isOwnPayment = false;
                isOwnPayment = (!string.IsNullOrEmpty(_ownerClientID1) &&
                                (_ownerClientID1.Equals(newReceipt.ClientID1) ||
                                _ownerClientID1.Equals(newReceipt.ClientID2))) ||
                                (!string.IsNullOrEmpty(_ownerClientID2) &&
                                (_ownerClientID2.Equals(newReceipt.ClientID1) ||
                                _ownerClientID2.Equals(newReceipt.ClientID2)));

                newPayment._is3rdPartyPayment = !isOwnPayment;
                newPayment._3rdName = (newPayment._is3rdPartyPayment) ? ((this._is3rdPartyPayment) ? _3rdName : this._ownerClientName) : string.Empty;
                newPayment._3rdRemark = (newPayment._is3rdPartyPayment) ? _3rdRemark : string.Empty;
                newPayment._3rdNRIC = (newPayment._is3rdPartyPayment) ? ((this._is3rdPartyPayment) ? _3rdNRIC : this._ownerClientID1 + " " + this._ownerClientID2) : string.Empty;
            }
            return newPayment;
        }      
        #endregion

        #region Public Method

        /*
         * Keep user entred exchange rates in memory, to be used in settlement
         */
        public void AddCustomExchangeRate(string exchKey, decimal rate)
        {
            if (_customExchRates.ContainsKey(exchKey))
                _customExchRates[exchKey] = rate;
            else
                _customExchRates.Add(exchKey, rate);
        }

        public void AddCustomShowExchangeRate(string exchKey, decimal rate)
        {
            if (_customShowExchRates.ContainsKey(exchKey))
                _customShowExchRates[exchKey] = rate;
            else
                _customShowExchRates.Add(exchKey, rate);
        }

        public void SaveMe(SqlDatabase db, SqlTransaction transaction, Receipt receipt, int paymentNo)
        {
            _rowno = paymentNo;
            _receiptID = receipt.ReceiptID; //Keep a copy for later use

            using (SqlCommand cm = db.GetStoredProcCommand("Usp_Receipt_ReceiptPayments_Insert") as SqlCommand)
            {
                //Add Parameters
                cm.Parameters.AddWithValue("@iIntReceiptID", receipt.ReceiptID);
                cm.Parameters.AddWithValue("@iIntPaymentNo", _rowno);
                cm.Parameters.AddWithValue("@iStrPaymentType", PaymentType);
                cm.Parameters.AddWithValue("@iStrBankCode", _bankcode);
                cm.Parameters.AddWithValue("@iStrCurrCd", _currency);
                cm.Parameters.AddWithValue("@iDecAmount", _amount);
                cm.Parameters.AddWithValue("@iStrSetlOption", _setlOption);
                cm.Parameters.AddWithValue("@iStrRefBankCode", _refbankcode);
                cm.Parameters.AddWithValue("@iStrRefText", _ref);
                cm.Parameters.AddWithValue("@iBitThirdPartyFlag", _is3rdPartyPayment);
                cm.Parameters.AddWithValue("@iStrThirdPartyName", _3rdName);
                cm.Parameters.AddWithValue("@iStrThirdPartyRemark", _3rdRemark);
                cm.Parameters.AddWithValue("@iStrThirdPartyNRIC", _3rdNRIC);

                _isRemisierPayment = _isRemisierPayment && receipt.Accounts.IsSingleAE;

                cm.Parameters.AddWithValue("@iBitAeFlag", _isRemisierPayment);

                if (_isSharePayment)
                    cm.Parameters.AddWithValue("@iStrAeRefNo", (_isRemisierPayment) ? _aeRefNo : string.Empty);
                else
                    cm.Parameters.AddWithValue("@iStrAeRefNo", (_isRemisierPayment) ? receipt.Accounts.SingleAeRefNo : string.Empty);

                cm.Parameters.AddWithValue("@iDecUseAmount", _usedAmount);
                cm.Parameters.AddWithValue("@iBitIsSharePayment", _isSharePayment);
                cm.Parameters.AddWithValue("@iDecExcessAmount", _dbExcessAmt);
                cm.Parameters.AddWithValue("@iStrRefundCurrCd", _dbRefundCurrCd);
                cm.Parameters.AddWithValue("@iStrRefundType", _dbRefundMethod);

                if (_isSharePayment)
                {
                    cm.Parameters.AddWithValue("@iIntShareReceiptID", _orgPayment._receiptID); //Use cache receiptID
                    cm.Parameters.AddWithValue("@iIntSharePaymentNo", _orgPayment._rowno);
                    cm.Parameters.AddWithValue("@iStrReceiptNo", receipt.ReceiptNumber);
                }
                db.ExecuteNonQuery(cm, transaction);
            }//using
        }

        public bool IsValidSharePayment(Receipt receipt)
        {
            return (_isSharePayment && !_isRemisierPayment) ||
                    (_isSharePayment && _isRemisierPayment && receipt.Accounts.IsSingleAE && receipt.Accounts.SingleAeRefNo == _aeRefNo);
        }

        /// <summary>
        /// Sort Payment object by TrnAmount asc [WAI] + [20131001] + Compare two payment objects by Payment Amount.
        /// </summary>
        public static Comparison<Payment> PaymentAmountComparisonAsc =
              delegate (Payment Payment1, Payment Payment2)
              {
                  return Payment1.Amount.CompareTo(Payment2.Amount);
              };
        #endregion
    }

    [Serializable]
    // [HasSelfValidation]
    public class PaymentList : SimpleCollection<Payment>
    {
        #region Constructor
        public PaymentList()
        {
        }
        #endregion

        #region Public Methods

        /*
         * Reset previous settlement informations
         * [WAI]+20130917+[RPS00055]+Reset zero to item outstanding amount of payment currcd.
         */
        public void ResetUsedAmounts()
        {
            foreach (Payment p in this)
            {
                p.UsedAmount = decimal.Zero;
                p.ItemTotalAmtInPayCurr = decimal.Zero;
            }
        }

        /*
         * Add Blank Payment Rows for entry screen
         */
        public void AddNewRows(int rowcount)
        {
            for (int i = 0; i < rowcount; i++)
                this.Add(new Payment(this.Count + 1));
        }

        /*
         * Search Payment Object by Row No
         */
        public Payment Search(int rowno)
        {
            foreach (Payment p in this)
                if (p.RowNo == rowno) return p;

            return null;
        }

        /*
         * Calculate Total Valid Payment Amounts, Group by CurrCd
         */
        public SortedList<string, decimal> TotalAmounts
        {
            get
            {
                SortedList<string, decimal> list = new SortedList<string, decimal>();
                foreach (Payment p in this)
                {
                   // if (p.IsValid)
                        if (list.ContainsKey(p.CurrCd))
                            list[p.CurrCd] += p.UsableAmount;
                        else
                            list.Add(p.CurrCd, p.UsableAmount);
                }
                return list;
            }
        }

        /*
         * Calculate Total Amounts Used for Settlements, Group by Currcd
         */
        public SortedList<string, decimal> UsedAmounts
        {
            get
            {
                SortedList<string, decimal> list = new SortedList<string, decimal>();
                foreach (Payment p in this)
                {
                   // if (p.IsValid)
                        if (list.ContainsKey(p.CurrCd))
                            list[p.CurrCd] += p.UsedAmount;
                        else
                            list.Add(p.CurrCd, p.UsedAmount);
                }
                return list;
            }
        }

        /// <summary>
        /// [WAI]+[20130917]+[RPS0005]+ Get All Items Total amount in payment currency
        /// </summary>
        public SortedList<string, decimal> ItemTotalsInPaymentCurr
        {
            get
            {
                SortedList<string, decimal> list = new SortedList<string, decimal>();
                foreach (Payment p in this)
                {
                    if (!string.IsNullOrEmpty(p.CurrCd))
                        if (list.ContainsKey(p.CurrCd))
                            list[p.CurrCd] += p.ItemTotalAmtInPayCurr;
                        else
                            list.Add(p.CurrCd, p.ItemTotalAmtInPayCurr);
                }
                return list;
            }
        }

        /*
         * Save Payments
         */
        public void SaveMe(SqlDatabase db, SqlTransaction transaction, Receipt receipt)
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].SaveMe(db, transaction, receipt, i + 1);
            }
        }

        #endregion

        /*
         * Copy payments with excess amount from source
         */
        public void CopySharePayments(Receipt prevReceipt, Receipt receipt)
        {
            foreach (Payment prevPayment in prevReceipt.Payments)
            {
                if (prevPayment.OSAmount > 0)
                    this.Add(prevPayment.CloneForSharePayment(prevReceipt, receipt, this.Count + 1));
            }
        }        

        #region Validation
        [SelfValidation]
        public void DoValidate(ValidationResults results)
        {
            if (this.Count == 0)
            {
                ValidationResult result = new ValidationResult("There is no payment to save.", typeof(PaymentList), "", "", null);
                results.AddResult(result);
            }
        }
        #endregion
    }
}
