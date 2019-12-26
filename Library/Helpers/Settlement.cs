using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
namespace AccessRPSService
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

            /*Contract contractItem = item as Contract;
            if (contractItem != null && (!contractItem.CurrCd.Equals(payment.CurrCd)))
            {
                if (contractItem.TrnSetlCurrCd.Equals(payment.CurrCd) && payment.OSAmount > 0M)
                {
                    _linkPayment = payment;
                    _paymentCurrCd = payment.CurrCd;

                    decimal itemOSAmount = contractItem.OutstandingAmount;
                    decimal itemOSAmtInSetlCurrCd = contractItem.TrnSetlAmount - Util.Truncate(contractItem.SetlAmount * contractItem.TrnSetlExchRate, 2);
                    decimal paymentOSAmount = payment.OSAmount;

                    //Payment's OS is more than item amount
                    if (itemOSAmtInSetlCurrCd < paymentOSAmount)
                    {
                        _setlAmount = itemOSAmount;
                        _usedPaymentAmount = itemOSAmtInSetlCurrCd;
                        payment.UsedAmount += _usedPaymentAmount;
                    }
                    //Payment's OS is less than or equal to item amount
                    else
                    {
                        _setlAmount = Util.Truncate(paymentOSAmount / contractItem.TrnSetlExchRate, 2);
                        _usedPaymentAmount = paymentOSAmount;
                        payment.UsedAmount = payment.UsableAmount;
                    }
                }
                return (_setlAmount > 0);
            }
            else
            {*/
            if (item.CurrCd.Equals(payment.CurrCd) && payment.OSAmount > 0M)
            {
                _linkPayment = payment;
                _paymentCurrCd = payment.CurrCd;

                decimal itemOSAmount = item.OutstandingAmount;
                decimal paymentOSAmount = payment.OSAmount;

                //Payment's OS is more than item amount
                if (itemOSAmount < paymentOSAmount)
                {
                    _setlAmount = _usedPaymentAmount = itemOSAmount;
                    payment.UsedAmount += _usedPaymentAmount;
                }
                //Payment's OS is less than or equal to item amount
                else
                {
                    _setlAmount = _usedPaymentAmount = paymentOSAmount;
                    payment.UsedAmount = payment.UsableAmount;
                }
            }
            return (_setlAmount > 0);
            //}
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

        //public virtual bool SettleWithConversion(Item item, Payment payment, Dictionary<string, ExchRateInfo> exchRates)
        //{
        //    /*--------------------------------------------------------------------------------------------
        //     * DATE             Author        Comment
        //     * -------------------------------------------------------------------------------------------
        //     *                  JAW           Created
        //     *  07-Jul-08       SWAP          Modofied to romove the validation for setled currency
        //     * -----------------------------------------------------------------------------------------*/

        //    if (payment.OSAmount > 0)
        //    {
        //        if (!item.CurrCd.Equals(payment.CurrCd))
        //        {
        //            _linkPayment = payment;
        //            _paymentCurrCd = payment.CurrCd;

        //            Contract contract = item as Contract;

        //            // If it is contract
        //            //
        //            ////Swapna 07/Jul/08-Removed to use SetlAmt instead of AmtOs as OutStanding amount
        //            //
        //            ////if (contract != null && contract.TrnSetlCurrCd.Equals(payment.CurrCd))
        //            ////{
        //            ////    _exchRate1Key = string.Format("{0}:{1}", payment.CurrCd, item.CurrCd);
        //            ////    _exchRate2Key = string.Empty;
        //            ////    //Exchange Rate 2 is one
        //            ////    _exchRate1 = Util.Truncate(decimal.Divide(decimal.One, contract.TrnSetlExchRate), FixedCodes.ExchRateDecimalPoints);
        //            ////    _exchRate2 = decimal.One;
        //            ////}
        //            //
        //            SetExchangeRate(item, payment, exchRates);

        //            decimal paymentOSAmount = payment.OSAmount;
        //            decimal paymentOSAmountInPaymentCurr = payment.OSAmount;

        //            Deposit deposit = item as Deposit;
        //            if (deposit != null && deposit.IsSystemAutoDeposit)
        //            {
        //                //[WAI] + [RPS00055]+ Converts rates as per setup exchange rates.
        //                //Payment Item
        //                decimal paymentOSinItemCurrcd = Util.Truncate(ConvertRates(paymentOSAmount, true), 2);
        //                _setlAmount = paymentOSinItemCurrcd;
        //                _usedPaymentAmount = paymentOSAmount;
        //                payment.UsedAmount += paymentOSAmount;
        //            }
        //            else
        //            {
        //                decimal itemOSAmount = item.OutstandingAmount;

        //                //Calculate OS amount in payment currcd
        //                //[WAI] + [RPS00055]+ Converts rates as per setup exchange rates.
        //                //Receipt Item
        //                decimal itemOSinPaymentCurrCd = Util.Truncate(ConvertRates(itemOSAmount, false), 2);

        //                //If payment is enough
        //                if (itemOSinPaymentCurrCd <= paymentOSAmount)
        //                {
        //                    _setlAmount = itemOSAmount;
        //                    _usedPaymentAmount = itemOSinPaymentCurrCd;
        //                    payment.UsedAmount += itemOSinPaymentCurrCd;
        //                }
        //                //If paymnet isn't enough
        //                else if (itemOSinPaymentCurrCd > paymentOSAmount)
        //                {
        //                    //[WAI] + [RPS00055]+ Converts rates as per setup exchange rates.
        //                    //PaymentItem Item
        //                    decimal paymentOSinItemCurrcd = Util.Truncate(ConvertRates(paymentOSAmount, true), 2);
        //                    _setlAmount = paymentOSinItemCurrcd;
        //                    _usedPaymentAmount = paymentOSAmount;
        //                    payment.UsedAmount += paymentOSAmount;
        //                }
        //            }
        //        }
        //    }
        //    return (_setlAmount > 0);
        //}

        //private void SetExchangeRate(Item item, Payment payment, Dictionary<string, ExchRateInfo> exchRates)
        //{
        //    // 1 Rate conversion
        //    if (payment.CurrCd.Equals(FixedCodes.BaseCurrencyCode) ||
        //        item.CurrCd.Equals(FixedCodes.BaseCurrencyCode))/* ||
        //        payment.CurrCd.Equals(FixedCodes.USDCurrencyCode) ||
        //        item.CurrCd.Equals(FixedCodes.USDCurrencyCode)) */
        //                                                        //Conversion to USD not required
        //    {
        //        //Key of the exchange rate for later validation purpose
        //        _exchRate1Key = string.Format("{0}:{1}", payment.CurrCd, item.CurrCd);
        //        _exchRate2Key = string.Empty;

        //        //Exchange Rate 2 is one
        //        _exchRate2 = decimal.One;

        //        //Check for existance of user edited rate
        //        //If there is one, make use it
        //        if (item.CustomeExchangeRates.ContainsKey(_exchRate1Key))
        //            _exchRate1 = item.CustomeExchangeRates[_exchRate1Key];
        //        else if (item is Deposit && payment.CustomeExchangeRates.ContainsKey(_exchRate1Key))
        //            _exchRate1 = payment.CustomeExchangeRates[_exchRate1Key];
        //        //Make use default rates
        //        else
        //        {
        //            ExchRateInfo rate = null;

        //            //Find direct XX$->SGD/USD rate
        //            if (item.CurrCd.Equals(FixedCodes.BaseCurrencyCode))// || item.CurrCd.Equals(FixedCodes.USDCurrencyCode))
        //            {
        //                rate = exchRates[payment.CurrCd];
        //                //_exchRate1 = (item.CurrCd.Equals(FixedCodes.BaseCurrencyCode)) ? rate.BaseRateLow : rate.USDRateLow;
        //                _exchRate1 = rate.BaseRateLow;
        //            }

        //            //If there is no direct conversion
        //            //Use reverse rate
        //            if (rate == null)
        //            {
        //                //Get the rate of XX$ -> SGD/USD
        //                rate = exchRates[item.CurrCd];
        //                //_exchRate1 = (payment.CurrCd.Equals(FixedCodes.BaseCurrencyCode)) ? rate.BaseRateHigh : rate.USDRateHigh;
        //                _exchRate1 = rate.BaseRateHigh;

        //                //Convert to SGD/USD -> XX$ rate
        //                //[WAI] + [RPS00055] + Commented below line not to use inverse rate in order to use rate from setup
        //                //_exchRate1 = decimal.Divide(decimal.One, _exchRate1);

        //                //Truncate 9 decimal places
        //                _exchRate1 = Util.Truncate(_exchRate1, FixedCodes.ExchRateDecimalPoints);
        //            }
        //        }
        //    }//End of 1Rate conversion

            //decimal paymentOSAmount = payment.OSAmount;
            //decimal itemOSAmount = item.OutstandingAmount;

            ////Calculate OS amount in payment currcd
            //decimal itemOSinPaymentCurrCd = Util.Truncate(itemOSAmount / _exchRate1, 2);                        

            ////If payment is enough
            //if (itemOSinPaymentCurrCd <= paymentOSAmount)
            //{
            //    _setlAmount = itemOSAmount;
            //    _usedPaymentAmount = itemOSinPaymentCurrCd;
            //    payment.UsedAmount += itemOSinPaymentCurrCd;
            //}
            ////If paymnet isn't enough
            //else if(itemOSinPaymentCurrCd > paymentOSAmount)
            //{
            //    decimal paymentOSinItemCurrcd = Util.Truncate(paymentOSAmount * _exchRate1, 2);
            //    _setlAmount = paymentOSinItemCurrcd;
            //    _usedPaymentAmount = paymentOSAmount;
            //    payment.UsedAmount += paymentOSAmount;
            //}


        //    // 2 Rates conversion
        //    else
        //    {
        //        //Key of the exchange rate for later validation purpose
        //        //_exchRate1Key = string.Format("{0}:{1}", payment.CurrCd, FixedCodes.USDCurrencyCode);
        //        //Cross currency - Use SGD instead of USD
        //        _exchRate1Key = string.Format("{0}:{1}", payment.CurrCd, FixedCodes.BaseCurrencyCode);

        //        //Check in user edited rates
        //        if (payment.CustomeExchangeRates.ContainsKey(_exchRate1Key))
        //            _exchRate1 = payment.CustomeExchangeRates[_exchRate1Key];
        //        //Fetch from setup rates
        //        else
        //        {
        //            //Get the rate of XX$ -> USD
        //            ExchRateInfo rate = exchRates[payment.CurrCd];
        //            //_exchRate1 = rate.USDRateLow;
        //            _exchRate1 = rate.BaseRateLow;
        //        }

        //        //Key of the exchange rate for later validation purpose
        //        //_exchRate2Key = string.Format("{0}:{1}", FixedCodes.USDCurrencyCode, item.CurrCd);
        //        //Cross currency - Use SGD instead of USD
        //        _exchRate2Key = string.Format("{0}:{1}", FixedCodes.BaseCurrencyCode, item.CurrCd);

        //        //Check in user edited rates
        //        if (payment.CustomeExchangeRates.ContainsKey(_exchRate2Key))
        //            _exchRate2 = payment.CustomeExchangeRates[_exchRate2Key];
        //        //Fetch from setup rates
        //        else
        //        {
        //            //Get the rate of XX$ -> USD
        //            ExchRateInfo rate = exchRates[item.CurrCd];
        //            //_exchRate2 = rate.USDRateHigh;
        //            _exchRate2 = rate.BaseRateHigh;

        //            //Convert to SGD/USD -> XX$ rate
        //            //[WAI] + [RPS00055] + Commented below line not to use inverse rate in order to use rate from setup
        //            //_exchRate2 = decimal.Divide(decimal.One, _exchRate2);

        //            //Truncate 9 decimal places
        //            _exchRate2 = Util.Truncate(_exchRate2, FixedCodes.ExchRateDecimalPoints);
        //        }
        //    }//End of 2Rates conversion
        //}

        //public void SetItemOSInPaymentCurr(Item item, Payment payment, Dictionary<string, ExchRateInfo> exchRates)
        //{
        //    /*--------------------------------------------------------------------------------------------
        //     * DATE             Author        Comment
        //     * -------------------------------------------------------------------------------------------
        //     *  17-Sep-13       WAI          Calculate Payment Balance in Payment Currency (Exchange rate conversion logic copy from SettleWithConversion method)
        //     * -----------------------------------------------------------------------------------------*/


        //    if (!item.CurrCd.Equals(payment.CurrCd) && !string.IsNullOrEmpty(payment.CurrCd))
        //    {
        //        Contract contract = item as Contract;
        //        if (contract != null)
        //        {
        //            SetExchangeRate(item, payment, exchRates);

        //            if (item.CurrCd == payment.SetlOption || string.IsNullOrEmpty(payment.SetlOption))
        //            {
        //                //Calculate OS amount in payment currcd
        //                //[WAI] + [RPS00055]+ Converts rates as per setup exchange rates.
        //                //Receipt Item
        //                decimal itemOSinPaymentCurrCd = Util.Truncate(ConvertRates(contract.TrnAmount, false), 2);
        //                payment.ItemTotalAmtInPayCurr += itemOSinPaymentCurrCd;
        //            }
        //        }
        //    }
        //}
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
