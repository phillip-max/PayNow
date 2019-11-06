//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AccessRPSService
//{
//    public class ExchRateInfo : Csla.ReadOnlyBase<ExchRateInfo>
//    {
//        #region Business Properties and Methods

//        //declare members
//        private string _currCd = string.Empty;
//        private decimal _baseRateLow = 0;
//        private decimal _baseRateHigh = 0;
//        private decimal _uSDRateLow = 0;
//        private decimal _uSDRateHigh = 0;
//        private decimal _baseMarginRate = 0;
//        private decimal _uSDMarginRate = 0;
//        private string _lastUpdatedBy = string.Empty;
//        private SmartDate _lastUpdatedDateTime = new SmartDate(false);
//        private byte[] _lastUpdatedTimeStamp;
//        private string _baseMarginRateType = string.Empty;
//        private string _uSDMarginRateType = string.Empty;

//        [System.ComponentModel.DataObjectField(true, false)]
//        public string CurrCd
//        {
//            get
//            {
//                return _currCd;
//            }
//        }

//        public decimal BaseRateLow
//        {
//            get
//            {
//                return _baseRateLow;
//            }
//        }

//        public decimal BaseRateHigh
//        {
//            get
//            {
//                return _baseRateHigh;
//            }
//        }

//        public decimal USDRateLow
//        {
//            get
//            {
//                return _uSDRateLow;
//            }
//        }

//        public decimal USDRateHigh
//        {
//            get
//            {
//                return _uSDRateHigh;
//            }
//        }

//        public decimal BaseMarginRate
//        {
//            get
//            {
//                return _baseMarginRate;
//            }
//        }

//        public decimal USDMarginRate
//        {
//            get
//            {
//                return _uSDMarginRate;
//            }
//        }

//        public string LastUpdatedBy
//        {
//            get
//            {
//                return _lastUpdatedBy;
//            }
//        }

//        public DateTime LastUpdatedDateTime
//        {
//            get
//            {
//                return _lastUpdatedDateTime.Date;
//            }
//        }

//        public string LastUpdatedDateTimeString
//        {
//            get
//            {
//                return _lastUpdatedDateTime.Text;
//            }
//        }

//        public string BaseMarginRateType
//        {
//            get
//            {
//                return _baseMarginRateType;
//            }
//        }

//        public string USDMarginRateType
//        {
//            get
//            {
//                return _uSDMarginRateType;
//            }
//        }

//        protected override object GetIdValue()
//        {
//            return _currCd;
//        }

//        #endregion //Business Properties and Methods

//        #region Factory Methods
//        private ExchRateInfo()
//        { /* require use of factory method */
//        }

//        internal ExchRateInfo(SafeDataReader dr)
//        {
//            FetchObjectFromReader(dr);
//        }
//        public static ExchRateInfo GetExchRateInfo(string currCd)
//        {
//            return DataPortal.Fetch<ExchRateInfo>(new Criteria(currCd));
//        }
//        #endregion //Factory Methods

//        #region Data Access

//        #region Criteria

//        [Serializable()]
//        private class Criteria
//        {
//            public string CurrCd;

//            public Criteria(string currCd)
//            {
//                this.CurrCd = currCd;
//            }
//        }

//        #endregion //Criteria

//        #region Data Access - Fetch
//        /*
//         * *-------------------------------------------------------------*
//         * 12/11/2008  Swapna    Modified by swapna Enabled Caching the data based on configuration
//         * ----------------------------------------------------------*/
//        private void DataPortal_Fetch(Criteria criteria)
//        {

//            try
//            {
//                if (HttpContext.Current.Session["ExchRates"] == null)
//                {
//                    SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
//                    using (SqlCommand cm = db.GetStoredProcCommand("Usp_Setup_ExchRates_FetchList") as SqlCommand)
//                    {
//                        DataTable dt = new DataTable();
//                        dt.Load(db.ExecuteReader(cm));
//                        HttpContext.Current.Session["ExchRates"] = dt;
//                        dt = null;
//                    }
//                }
//                DataTable dtExchRatesTable = ((DataTable)HttpContext.Current.Session["ExchRates"]);
//                string strExpr;
//                strExpr = "CurrCd='" + criteria.CurrCd + "'";
//                DataRow[] drExchRatesRows = dtExchRatesTable.Select(strExpr);

//                DataTable dtExchRates = new DataTable();
//                dtExchRates = dtExchRatesTable.Clone();
//                foreach (DataRow drExchRates in drExchRatesRows)
//                {
//                    dtExchRates.ImportRow(drExchRates);
//                }

//                using (DataTableReader dtr = dtExchRates.CreateDataReader())
//                {
//                    using (SafeDataReader dr = new SafeDataReader(dtr))
//                    {
//                        FetchObject(dr);
//                        //load child object(s)
//                        FetchChildren(dr);
//                    }
//                }
//                dtExchRatesTable = null;
//                drExchRatesRows = null;
//                dtExchRates = null;


//            }

//            catch (Exception ex)
//            {
//                // Alert Policy and Email
//                bool rethrow = ExceptionPolicy.HandleException(ex, "RPS Alert Policy with Email");
//                if (rethrow)
//                    throw;

//            }
//        }

//        private void ExecuteFetch(SqlDatabase db, Criteria criteria)
//        {
//            try
//            {
//                using (SqlCommand cm = db.GetStoredProcCommand("Usp_Setup_ExchRates_Fetch") as SqlCommand)
//                {
//                    cm.Parameters.AddWithValue("@iStrCurrCd", criteria.CurrCd);

//                    using (SafeDataReader dr = new SafeDataReader(db.ExecuteReader(cm)))
//                    {

//                        FetchObject(dr);

//                        //load child object(s)
//                        FetchChildren(dr);
//                    }
//                }//using
//            }
//            catch (Exception ex)
//            {
//                // Alert Policy and Email
//                bool rethrow = ExceptionPolicy.HandleException(ex, "RPS Alert Policy with Email");
//                if (rethrow)
//                    throw;
//            }
//        }

//        private void FetchObject(SafeDataReader dr)
//        {
//            try
//            {
//                if (dr.Read())
//                    FetchObjectFromReader(dr);
//            }
//            catch (Exception ex)
//            {
//                // Alert Policy and Email
//                bool rethrow = ExceptionPolicy.HandleException(ex, "RPS Alert Policy with Email");
//                if (rethrow)
//                    throw;
//            }
//        }

//        private void FetchObjectFromReader(SafeDataReader dr)
//        {
//            _currCd = dr.GetString("CurrCd");
//            _baseRateLow = dr.GetDecimal("BaseRateLow");
//            _baseRateHigh = dr.GetDecimal("BaseRateHigh");
//            _uSDRateLow = dr.GetDecimal("USDRateLow");
//            _uSDRateHigh = dr.GetDecimal("USDRateHigh");
//            _baseMarginRate = dr.GetDecimal("BaseMarginRate");
//            _uSDMarginRate = dr.GetDecimal("USDMarginRate");
//            _lastUpdatedBy = dr.GetString("LastUpdatedBy");
//            _lastUpdatedDateTime = dr.GetSmartDate("LastUpdatedDateTime", _lastUpdatedDateTime.EmptyIsMin);
//            _lastUpdatedTimeStamp = (byte[])dr["LastUpdatedTimeStamp"];
//            _baseMarginRateType = dr.GetString("BaseMarginRateType");
//            _uSDMarginRateType = dr.GetString("USDMarginRateType");
//        }
//        private void FetchChildren(SafeDataReader dr)
//        {
//        }
//        #endregion //Data Access - Fetch
//        #endregion //Data Access

//        public static bool RangeCheck(System.Collections.Generic.Dictionary<string, ExchRateInfo> editingExchRates, string exchKey, decimal rate, out decimal minRate, out decimal maxRate)
//        {
//            decimal marginrate = decimal.Zero,
//                      margin = decimal.Zero;
//            minRate = decimal.Zero;
//            maxRate = decimal.Zero;
//            string from, to;
//            string[] keys = exchKey.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
//            try
//            {
//                if (keys.Length == 2)
//                {
//                    from = keys[0];
//                    to = keys[1];
//                }
//                else
//                    return false;

//                ExchRateInfo exchrateInfo = null;
//                if (to.Equals(FixedCodes.BaseCurrencyCode)) //|| to.Equals(FixedCodes.USDCurrencyCode))
//                    exchrateInfo = editingExchRates[from];
//                else
//                    exchrateInfo = editingExchRates[to];

//                if (to.Equals(FixedCodes.BaseCurrencyCode))
//                {
//                    marginrate = exchrateInfo.BaseMarginRate;
//                    if (exchrateInfo.BaseMarginRateType.Equals(FixedCodes.ExchMarginTypePercentage))
//                        margin = exchrateInfo.BaseRateLow * marginrate / 100M;
//                    else if (exchrateInfo.BaseMarginRateType.Equals(FixedCodes.ExchMarginTypeSpread))
//                        margin = marginrate;
//                    minRate = exchrateInfo.BaseRateLow - margin;
//                    maxRate = exchrateInfo.BaseRateHigh + margin;
//                }
//                //else if (to.Equals(FixedCodes.USDCurrencyCode))
//                //{
//                //    marginrate = exchrateInfo.USDMarginRate;
//                //    if (exchrateInfo.USDMarginRateType.Equals(FixedCodes.ExchMarginTypePercentage))
//                //        margin = exchrateInfo.USDRateLow * marginrate / 100M;
//                //    else if (exchrateInfo.USDMarginRateType.Equals(FixedCodes.ExchMarginTypeSpread))
//                //        margin = marginrate;
//                //    minRate = exchrateInfo.USDRateLow - margin;
//                //    maxRate = exchrateInfo.USDRateHigh + margin;
//                //}
//                else if (from.Equals(FixedCodes.BaseCurrencyCode))
//                {
//                    //[WAI] + [RPS00055] + [20141008] + Remove 1/BaseRate to handle follow the same currency rate logic as Settlement.cs
//                    //In Settlement.cs, currency rates are calucated and shown to Receipt UI as Exchange Rate Setup.

//                    decimal minReverseRate = Util.Truncate(exchrateInfo.BaseRateHigh, FixedCodes.ExchRateDecimalPoints);
//                    decimal maxReverseRate = Util.Truncate(exchrateInfo.BaseRateLow, FixedCodes.ExchRateDecimalPoints);

//                    marginrate = exchrateInfo.BaseMarginRate;
//                    if (exchrateInfo.BaseMarginRateType.Equals(FixedCodes.ExchMarginTypePercentage))
//                        margin = minReverseRate * marginrate / 100M;
//                    else if (exchrateInfo.BaseMarginRateType.Equals(FixedCodes.ExchMarginTypeSpread))
//                        margin = marginrate;
//                    minRate = minReverseRate - margin;
//                    maxRate = maxReverseRate + margin;
//                }
//                //else if (from.Equals(FixedCodes.USDCurrencyCode))
//                //{
//                //    decimal minReverseRate = Util.Truncate(decimal.One / exchrateInfo.USDRateHigh, FixedCodes.ExchRateDecimalPoints);
//                //    decimal maxReverseRate = Util.Truncate(decimal.One / exchrateInfo.USDRateLow, FixedCodes.ExchRateDecimalPoints);

//                //    marginrate = exchrateInfo.USDMarginRate;
//                //    if (exchrateInfo.USDMarginRateType.Equals(FixedCodes.ExchMarginTypePercentage))
//                //        margin = minReverseRate * marginrate / 100M;
//                //    else if (exchrateInfo.USDMarginRateType.Equals(FixedCodes.ExchMarginTypeSpread))
//                //        margin = marginrate;
//                //    minRate = minReverseRate - margin;
//                //    maxRate = maxReverseRate + margin;
//                //}
//                minRate = Util.Truncate(minRate, FixedCodes.ExchRateDecimalPoints);
//                maxRate = Util.Truncate(maxRate, FixedCodes.ExchRateDecimalPoints);

//            }
//            catch (Exception ex)
//            {
//                // Alert Policy and Email
//                bool rethrow = ExceptionPolicy.HandleException(ex, "RPS Alert Policy with Email");
//                if (rethrow)
//                    throw;
//            }
//            return (rate >= minRate) && (rate <= maxRate);
//        }
//    }
//}
