using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data.SqlClient;
using Simple;
namespace PayNowReceiptsGeneration
{
    public class Settlement : SimpleBase<Settlement>
    {
        #region Protected Data
        protected decimal _setlAmount = decimal.Zero;
        protected decimal _exchRate1 = decimal.One;
        protected decimal _exchRate2 = decimal.One;
        protected string _exchRate1Key = string.Empty;
        protected string _exchRate2Key = string.Empty;
        protected string _paymentCurrCd = string.Empty;
        protected decimal _usedPaymentAmount = decimal.Zero;

        [NonSerialized]
        protected Payment _linkPayment = null;

        #endregion

        #region Public Properties

        public string PaymentCurrCd
        {
            get
            {
                return _paymentCurrCd;
            }
        }

        public decimal UsedPaymentAmount
        {
            get
            {
                return _usedPaymentAmount;
            }
        }

        public string ExchRate1Key
        {
            get
            {
                return _exchRate1Key;
            }
        }

        public string ExchRate2Key
        {
            get
            {
                return _exchRate2Key;
            }
        }

        //Settled Amount of Item Amount
        public decimal SetlAmount
        {
            get
            {
                return _setlAmount;
            }
        }

        //Exchange Rate 1
        public decimal ExchangeRate1
        {
            get
            {
                return _exchRate1;
            }
        }

        //Exchange Rate 2
        public decimal ExchangeRate2
        {
            get
            {
                return _exchRate2;
            }
        }

        //Effective Rate
        public decimal EffectiveRate
        {
            get
            {
                //[WAI] + [RPS00055] + to show the exchange rates from setup in UI, change "*" operator to "/"                
                return decimal.Zero;//Util.Truncate(_exchRate1 / _exchRate2, FixedCodes.ExchRateDecimalPoints);
            }
        }

        public int PaymentRowNo
        {
            get
            {
                if (_linkPayment != null)
                    return _linkPayment.RowNo;
                else
                    return 0;
            }
        }

        public string PaymentRef
        {
            get
            {
                if (_linkPayment.PaymentType.Equals(FixedCodes.LookUpCodePaymentTypeCash))
                    return "CASH";
                else
                    return string.Format("{0} {1}", _linkPayment.RefBankCode, _linkPayment.RefText);
            }
        }

        #endregion

        #region Portected Methods

        protected virtual SqlCommand GetInsertCommand(SqlDatabase db, Receipt receipt, Item item)
        {
            SqlCommand insertCmd = db.GetStoredProcCommand("Usp_Receipt_ReceiptSettlements_Insert") as SqlCommand;
            insertCmd.CommandTimeout = 10000;// Convert.ToInt32(ConfigurationManager.AppSettings["ForceTimeout2"].ToString());
            insertCmd.Parameters.AddWithValue("@iIntReceiptID", receipt.ReceiptID);
            insertCmd.Parameters.AddWithValue("@iIntItemNo", item.ItemNo);
            insertCmd.Parameters.AddWithValue("@iIntPaymentNo", /*_linkPayment.RowNo*/ 1);
            insertCmd.Parameters.AddWithValue("@iStrSetlCurrCd", item.CurrCd);
            insertCmd.Parameters.AddWithValue("@iDecSetlAmount", _setlAmount);
            insertCmd.Parameters.AddWithValue("@iStrUsedPaymentCurrCd", item.CurrCd /*_paymentCurrCd*/);
            insertCmd.Parameters.AddWithValue("@iDecUsedPaymentAmount", _usedPaymentAmount);
            insertCmd.Parameters.AddWithValue("@iDecExchRate1", _exchRate1);
            insertCmd.Parameters.AddWithValue("@iDecExchRate2", _exchRate2);
            insertCmd.Parameters.AddWithValue("@iDecEffectiveExchRate", EffectiveRate);
            return insertCmd;
        }

        internal void Save(SqlDatabase db, SqlTransaction trans, Receipt receipt, Item item)
        {
            using (SqlCommand insertCmd = GetInsertCommand(db, receipt, item))
            {
                db.ExecuteNonQuery(insertCmd, trans);
            }
        }

        #endregion

        internal Settlement() { }

        public void UndoSettle()
        {
            _linkPayment.UsedAmount -= _usedPaymentAmount;
            _linkPayment = null;
            _setlAmount = decimal.Zero;
            _exchRate1 = decimal.One;
            _exchRate2 = decimal.One;
            _exchRate1Key = string.Empty;
            _exchRate2Key = string.Empty;
            _paymentCurrCd = string.Empty;
            _usedPaymentAmount = decimal.Zero;
        }

        public virtual bool SettleDirect(Item item, Payment payment)
        {
            _setlAmount = payment.Amount;
            _usedPaymentAmount = payment.Amount;
            //if (item.CurrCd.Equals(payment.CurrCd) && payment.OSAmount > 0M)
            //{
            //    _linkPayment = payment;
            //    _paymentCurrCd = payment.CurrCd;

            //    decimal itemOSAmount = item.OutstandingAmount;
            //    decimal paymentOSAmount = payment.OSAmount;

            //    //Payment's OS is more than item amount
            //    if (itemOSAmount < paymentOSAmount)
            //    {
            //        _setlAmount = _usedPaymentAmount = itemOSAmount;
            //        payment.UsedAmount += _usedPaymentAmount;
            //    }
            //    //Payment's OS is less than or equal to item amount
            //    else
            //    {
            //        _setlAmount = _usedPaymentAmount = paymentOSAmount;
            //        payment.UsedAmount = payment.UsableAmount;
            //    }
            //}
            return (_setlAmount > 0);
           
        }

        /// <summary>
        /// This method should call after SetExchangeRate
        /// [WAI] + [RPS00055]
        /// </summary>
        /// <param name="amount">Amount to be converted</param>
        /// <param name="isPayment">Flag to indentify receipt item or payment item</param>
        /// <returns></returns>
        private decimal ConvertRates(decimal amount, bool isPayment)
        {
            decimal result = amount;
            decimal inverseRate = decimal.One;

            //Conversion for ExchRate1 (Same logic apply for ExchRate2 as well)
            if (!string.IsNullOrEmpty(_exchRate1Key))//Check exchratekey is empty (SGD:USD)
            {
                inverseRate = _exchRate1;
                if (!isPayment)//Check payment flag to indentify payment item or receipt item
                {
                    //Payment item need to use inverse rate because for payment SGD:USD need to calculate as USD:SGD
                    inverseRate = decimal.Divide(1, _exchRate1);
                }
                if (_exchRate1Key.Contains("SGD:"))//If from currency is SGD use "/" to convert
                {
                    result = amount / inverseRate;
                }
                else// If from currency is is foreign currency use * to convert
                {
                    result = amount * inverseRate;
                }
            }

            //Conversion for ExchRate2
            if (!string.IsNullOrEmpty(_exchRate2Key))
            {
                inverseRate = _exchRate2;
                if (!isPayment)
                {
                    inverseRate = decimal.Divide(1, _exchRate2);
                }
                if (_exchRate2Key.Contains("SGD:"))
                {
                    result = result / inverseRate;
                }
                else
                {
                    result = result * inverseRate;
                }
            }
            return result;
        }       
    }

    [Serializable]
    public class SettlementList : SimpleCollection<Settlement>
    {
        #region Public Member

        public decimal TotalSetlAmount
        {
            get
            {
                decimal total = decimal.Zero;
                foreach (Settlement setl in this)
                    total += setl.SetlAmount;
                return total;
            }
        }

        public void Save(SqlDatabase db, SqlTransaction trans, Receipt receipt, Item item)
        {
            foreach (Settlement setl in this)
                setl.Save(db, trans, receipt, item);
        }

        #endregion
    }
}
