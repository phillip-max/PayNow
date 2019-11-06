//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AccessRPSService
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

        //        #endregion

        //        #region Properties

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

        public bool isMarginAcct { get; set; } = false;


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

        /*
         * GET Excess Amounts Collection
         */
        //public ExcessAmountList ExcessAmounts
        //{
        //    get
        //    {
        //        return _excessAmts;
        //    }
        //}



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

        //public bool ValidWithSubsystems(out List<string> errors)
        //{
        //    errors = new List<string>();
        //    foreach (Account ac in this.Accounts)
        //        foreach (Item item in ac.UTContracts)
        //            if (!((UTContract)item).IsValidInSubSystem)
        //                errors.Add(string.Format("Contract: {0} has been settled already.", item.ItemRef));
        //    return (errors.Count == 0);
        //}

        /*public Dictionary<string, ExchRateInfo> DictExchangeRate
        {
            get
            {
                return _jsExchRates;
            }
            set
            {
                _jsExchRates = value;
            }
        }*/

        /*New comment*/
        //public decimal GetExchangeRateToBase(string currency, Dictionary<string, ExchRateInfo> exchRates)
        //{
        //    //ExchRateInfo rate = null;
        //    decimal exchRate = 1M;
        //    try
        //    {
        //        ////if (currency.Equals(FixedCodes.USDCurrencyCode))
        //        ////    exchRate = Util.Truncate(1M / exchRates[FixedCodes.BaseCurrencyCode].USDRateLow, FixedCodes.ExchRateDecimalPoints);
        //        ////else
        //        exchRate = exchRates[currency].BaseRateHigh;
        //        //exchRate = rate.BaseRateHigh;
        //    }
        //    catch { }
        //    return (exchRate);
        //}

        /*New comment*/
        //public SortedList<string, Balance> GetBalances(Dictionary<string, ExchRateInfo> exchRates)
        //{
        //    SortedList<string, Balance> list = new SortedList<string, Balance>();
        //    //Calculate Item Totals from Accounts Collection                
        //    foreach (Account ac in _accounts)
        //    {
        //        SortedList<string, decimal> itemTotals = ac.TotalAmounts;   //Read Amounts for each account
        //        for (int i = 0; i < itemTotals.Count; i++)                  //loop through amounts
        //        {
        //            //Look for existing balance object with currency code key
        //            if (list.ContainsKey(itemTotals.Keys[i]))               //If Found
        //            {
        //                Balance b = list[itemTotals.Keys[i]];
        //                b.ItemTotal += itemTotals.Values[i];                //Sum up the balance                           

        //            }
        //            else
        //            {                                                   //If Not Found
        //                Balance b = new Balance();
        //                b.CurrCd = itemTotals.Keys[i];
        //                b.ItemTotal = itemTotals.Values[i];
        //                list.Add(b.CurrCd, b);                              //add new Balance object with Currncy Code key
        //            }
        //        }

        //        SortedList<string, decimal> osTotals = ac.OSAmounts;   //Read OS Amounts for each account
        //        for (int i = 0; i < osTotals.Count; i++)                  //loop through amounts
        //        {
        //            //Look for existing balance object with currency code key
        //            if (list.ContainsKey(osTotals.Keys[i]))               //If Found
        //            {
        //                Balance b = list[osTotals.Keys[i]];
        //                b.OSAmount += osTotals.Values[i];                //Sum up the balance                           

        //                if (b.CurrCd.Equals(FixedCodes.BaseCurrencyCode))
        //                {
        //                    b.OSAmountSGD += Util.Truncate(osTotals.Values[i], 2);

        //                }
        //                else
        //                {
        //                    b.OSAmountSGD += Util.Truncate(osTotals.Values[i] * GetExchangeRateToBase(b.CurrCd, exchRates), 2);
        //                }
        //            }
        //            else
        //            {                                                   //If Not Found
        //                Balance b = new Balance();
        //                b.CurrCd = osTotals.Keys[i];
        //                b.ItemTotal = osTotals.Values[i];
        //                list.Add(b.CurrCd, b);                              //add new Balance object with Currncy Code key
        //            }
        //        }
        //    }

        //    //Calculate Payment Totals from Payments Collection
        //    SortedList<string, decimal> paymentTotals = _paymentlist.TotalAmounts; //Read total amounts from payments collection
        //    for (int i = 0; i < paymentTotals.Count; i++)       //loop through amounts
        //    {
        //        //Look for existing balance object with currency code key
        //        if (list.ContainsKey(paymentTotals.Keys[i]))    // If Found
        //        {
        //            Balance b = list[paymentTotals.Keys[i]];
        //            b.PaymentTotal += paymentTotals.Values[i];  // Sum up the balance                          
        //        }
        //        else
        //        {                                           // If Not Found
        //            Balance b = new Balance();
        //            b.CurrCd = paymentTotals.Keys[i];
        //            b.PaymentTotal = paymentTotals.Values[i];
        //            list.Add(b.CurrCd, b);                      // Add new Balance object with Currency Code key
        //        }
        //    }

        //    //Fetch Payment Total
        //    SortedList<string, decimal> usedtotal = _paymentlist.UsedAmounts;
        //    for (int i = 0; i < usedtotal.Count; i++)
        //    {
        //        if (list.ContainsKey(usedtotal.Keys[i]))
        //        {
        //            Balance b = list[usedtotal.Keys[i]];
        //            b.UsedTotal += usedtotal.Values[i];
        //        }
        //        else
        //        {
        //            Balance b = new Balance();
        //            b.CurrCd = usedtotal.Keys[i];
        //            b.UsedTotal = usedtotal.Values[i];
        //            list.Add(b.CurrCd, b);
        //        }
        //    }

        //    //Set item total amount in payment currency code
        //    SortedList<string, decimal> itemTotalsInPaymentCurrency = _paymentlist.ItemTotalsInPaymentCurr;
        //    for (int i = 0; i < itemTotalsInPaymentCurrency.Count; i++)
        //    {
        //        if (list.ContainsKey(itemTotalsInPaymentCurrency.Keys[i]))
        //        {
        //            Balance b = list[itemTotalsInPaymentCurrency.Keys[i]];
        //            b.ItemTotalAmtPaymentCurr += itemTotalsInPaymentCurrency.Values[i];
        //        }
        //        else
        //        {
        //            Balance b = new Balance();
        //            b.CurrCd = itemTotalsInPaymentCurrency.Keys[i];
        //            b.ItemTotalAmtPaymentCurr = itemTotalsInPaymentCurrency.Values[i];
        //            list.Add(b.CurrCd, b);
        //        }
        //    }

        //    return list;
        //}

        /*public SortedList<string, Balance> BalancesToConfirm
        {
            get
            {
                SortedList<string, Balance> list = new SortedList<string, Balance>();

                //Fetach Items Total
                foreach (Account ac in _accounts)
                {
                    SortedList<string, decimal> sublist = ac.TotalAmounts;
                    for (int i = 0; i < sublist.Count; i++)
                    {
                        if (list.ContainsKey(sublist.Keys[i]))
                        {
                            Balance b = list[sublist.Keys[i]];
                            b.ItemTotal += sublist.Values[i];
                        }
                        else
                        {
                            Balance b = new Balance();
                            b.CurrCd = sublist.Keys[i];
                            b.ItemTotal = sublist.Values[i];
                            list.Add(b.CurrCd, b);
                        }
                    }
                }

                //Fetch Excess Amount
                foreach (ExcessAmount excess in _excessAmts)
                {
                    if (list.ContainsKey(excess.CurrCd))
                    {
                        Balance b = list[excess.CurrCd];
                        b.ItemTotal += excess.Amount;
                    }
                    else
                    {
                        Balance b = new Balance();
                        b.CurrCd = excess.CurrCd;
                        b.ItemTotal = excess.Amount;
                        list.Add(b.CurrCd, b);
                    }
                }
                return list;
            }
        } */



        #endregion

        //        #region Constructor
        //        public Receipt(string bizdate, string companyCode, string clientname, string clientID1, string clientID2, string clientRefNo1, string clientRefNo2)
        //        {
        //            _businessDate = bizdate;
        //            _companyCode = companyCode;
        //            _clientID1 = clientID1;
        //            _clientID2 = clientID2;
        //            _clientName = clientname;
        //            _clientRefNo1 = clientRefNo1;
        //            _clientRefNo2 = clientRefNo2;
        //            _accounts = new AccountList();
        //            _paymentlist = new PaymentList();
        //            _excessAmts = new ExcessAmountList();
        //        }
        //        #endregion

        //        #region Public Methods

        //        // Settle valid deposits with valid payments
        //        public void SettleItems(Dictionary<string, ExchRateInfo> exchRates)
        //        {
        //            //Reset previous settlements
        //            this._paymentlist.ResetUsedAmounts();

        //            //Settle for same currency payments
        //            foreach (Account ac in _accounts)
        //                foreach (Item item in ac.MiscEntry)
        //                    item.SettleDirect(this, _paymentlist);

        //            foreach (Account ac in _accounts)
        //                foreach (Item item in ac.MiscEntry)
        //                    item.SettleWithConversion(this, _paymentlist, exchRates);

        //            //Settle for same currency payments
        //            //foreach (Account ac in _accounts)
        //            //    ac.UTContracts.SettleDirect(this, _paymentlist);

        //            // RPS00037 23/08/2010 Venkat
        //            //foreach (Account ac in _accounts)
        //            //{
        //            //    ac.ContraLoss.ClearPartialSettlements();

        //            //}
        //            foreach (Account ac in _accounts)
        //            {
        //                foreach (Item item in ac.ContraLoss)
        //                {
        //                    //item.Amount = item.
        //                    item.SettleDirect(this, _paymentlist);
        //                    item.SettleWithConversion(this, _paymentlist, exchRates);

        //                    //item.ClearPartialSettlements();
        //                    // item.Amount = item.SettlementList.TotalSetlAmount;
        //                }
        //                //ac.
        //            }
        //            /*foreach (Account ac in _accounts)
        //                foreach (Item item in ac.UTContracts)
        //                    item.SettleWithConversion(this, _paymentlist, exchRates);*/

        //            // WAI RPS00055 20130225
        //            foreach (Account ac in _accounts)
        //            {
        //                //In order to sort by SetlAmount of each CAContract.
        //                List<CAContract> caList = new List<CAContract>();
        //                List<CAContract> caUnSetlList = new List<CAContract>();

        //                if (_IsUserOverrideSelection)
        //                {
        //                    foreach (Item item in ac.CAContracts)
        //                    {
        //                        CAContract caItem = item as CAContract;

        //                        if (caItem.IsSelected)
        //                        {
        //                            caList.Add(caItem);
        //                        }
        //                        else
        //                        {
        //                            caUnSetlList.Add(caItem);
        //                        }
        //                    }
        //                    caList.Sort(CAContract.SetlAmountComparisonAsc);
        //                    caList.AddRange(caUnSetlList);
        //                    foreach (CAContract item in caList)
        //                    {
        //                        item.SettleDirect(this, _paymentlist);
        //                        item.SettleWithConversion(this, _paymentlist, exchRates);
        //                    }
        //                }
        //                else
        //                {
        //                    if (!_isMarginAcct)
        //                        AutoSettleCAItems(exchRates, ac);
        //                }
        //            }
        //            //End of CA settlement

        //            foreach (Account ac in _accounts)
        //            {
        //                ac.UTContracts.ClearPartialSettlements();
        //                ac.NOMTransactions.ClearPartialSettlements();// RPS00036 10/11/2010
        //            }

        //            foreach (Account ac in _accounts)
        //                foreach (Item item in ac.UTContracts)
        //                {
        //                    item.SettleDirect(this, _paymentlist);
        //                    item.SettleWithConversion(this, _paymentlist, exchRates);
        //                    item.ClearPartialSettlements();
        //                }

        //            // RPS00036 10/11/2010
        //            foreach (Account ac in _accounts)
        //            {
        //                foreach (Item item in ac.NOMTransactions)
        //                {
        //                    item.SettleDirect(this, _paymentlist);
        //                    item.SettleWithConversion(this, _paymentlist, exchRates);
        //                    item.ClearPartialSettlements();
        //                }
        //            }

        //            bool hasAutoDeposit = AutoDeposit(exchRates);

        //            //Settle for diff currency payments
        //            foreach (Account ac in _accounts)
        //                foreach (Item item in ac.Deposits)
        //                    item.SettleDirect(this, _paymentlist);

        //            foreach (Account ac in _accounts)
        //                foreach (Item item in ac.Deposits)
        //                    item.SettleWithConversion(this, _paymentlist, exchRates);

        //            if (hasAutoDeposit)
        //            {
        //                foreach (Account ac in _accounts)       //Added by Gowri 20081210
        //                    foreach (Deposit deposit in ac.Deposits)      //Modified by Gowri 20081210  Changed the index from fixed value of 0 to loop thru all accounts
        //                        deposit.AdjustAutoDepositAmount();
        //            }
        //        }

        //        private void AutoSettleCAItems(Dictionary<string, ExchRateInfo> exchRates, Account ac)
        //        {
        //            foreach (CAContract caItem in ac.CAContracts)
        //            {
        //                caItem.IsSelected = false;
        //                caItem.Amount = caItem.TrnAmount;
        //            }
        //            PaymentList matchPaymentList = new PaymentList();
        //            List<Payment> matchPayments = new List<Payment>();
        //            List<CAContract> matchCAItems = new List<CAContract>();

        //            //Find exact matching amount from payment and item. If there is exact amount settle that item first.
        //            foreach (Payment payment in _paymentlist)
        //            {
        //                foreach (CAContract caItem in ac.CAContracts)
        //                {
        //                    if (caItem.CurrCd == payment.CurrCd && caItem.TrnAmount == payment.Amount && !caItem.IsSelected
        //                        && !matchCAItems.Contains(caItem) && !matchPayments.Contains(payment))
        //                    {
        //                        matchCAItems.Add(caItem);
        //                        matchPayments.Add(payment);
        //                    }
        //                }
        //            }

        //            matchCAItems.Sort(CAContract.TrnAmountComparisonAsc);
        //            matchPayments.Sort(Payment.PaymentAmountComparisonAsc);
        //            foreach (Payment payment in matchPayments)
        //            {
        //                matchPaymentList.Add(payment);
        //            }

        //            foreach (Payment payment in matchPayments)
        //            {
        //                foreach (CAContract item in matchCAItems)
        //                {
        //                    if (!item.IsSelected && !item.IsPartialContra)
        //                    {
        //                        item.IsSelected = item.CanSettle(this, matchPaymentList);
        //                        item.SettleDirect(this, matchPaymentList);
        //                        item.SettleWithConversion(this, matchPaymentList, exchRates);
        //                    }
        //                }
        //            }


        //            //After exact amount matching settle remaing item.
        //            foreach (CAContract item in ac.CAContracts)
        //            {
        //                if (!item.IsSelected && !item.IsPartialContra)
        //                {
        //                    item.IsSelected = item.CanSettle(this, _paymentlist);
        //                    item.SettleDirect(this, _paymentlist);
        //                    item.SettleWithConversion(this, _paymentlist, exchRates);
        //                }
        //            }
        //        }

        //        // Generate New Receipt Object from existing object
        //        public Receipt GetFinalReceiptObject(Dictionary<string, ExchRateInfo> exchRates)
        //        {
        //            //Create new object
        //            Receipt newReceipt = new Receipt(_businessDate, _companyCode, _clientName, _clientID1, _clientID2, _clientRefNo1, _clientRefNo2);

        //            //Loop through Payments
        //            foreach (Payment payment in this.Payments)
        //            {
        //                //Add valid payments to new object
        //                if (payment.IsValid)
        //                    newReceipt.Payments.Add(payment.Clone());
        //            }

        //            //Settle Valid Items with Valid Payments before checking each deposit/item            
        //            this.SettleItems(exchRates);

        //            //Loop through Accounts to look for invalid or unsettled deposits
        //            foreach (Account account in this.Accounts)
        //            {
        //                //Create a clone with existing Deposits and Items
        //                Account newAc = account.Clone();

        //                //Prepare list of invalid deposit entries
        //                List<Item> deletedItems = new List<Item>();
        //                //List<MiscChargeItem> deletedMiscChargeItem = new List<MiscChargeItem>();                

        //                foreach (ItemList itemlist in newAc.Items)
        //                {
        //                    foreach (Item item in itemlist)
        //                    {
        //                        if (!item.IsValid)
        //                        {
        //                            deletedItems.Add(item);
        //                            continue;
        //                        }

        //                        bool bdelete = false;
        //                        if (item is Deposit)
        //                        {
        //                            Deposit deposit = item as Deposit;
        //                            if (deposit.Amount == deposit.OutstandingAmount)
        //                                bdelete = true;
        //                        }
        //                        else if (item is MiscChargeItem)
        //                        {
        //                            MiscChargeItem misc = item as MiscChargeItem;
        //                            if (misc.OutstandingAmount > 0)
        //                                bdelete = true;
        //                        }
        //                        else if (item is UTContract)
        //                        {
        //                            UTContract ut = item as UTContract;
        //                            if (ut.OutstandingAmount > 0)
        //                                bdelete = true;
        //                        }
        //                        else if (item is NOMTransactions)
        //                        {
        //                            NOMTransactions NOMTrans = item as NOMTransactions;
        //                            if (NOMTrans.OutstandingAmount > 0)
        //                                bdelete = true;
        //                        }
        //                        //Take out unsettle items [WAI] + [20130805] + RPS00055
        //                        else if (item is CAContract)
        //                        {
        //                            CAContract caItem = item as CAContract;
        //                            if (caItem.Amount.Equals(caItem.OutstandingAmount))
        //                            {
        //                                bdelete = true;
        //                            }
        //                        }
        //                        else if (item is CLossTrans)
        //                        {
        //                            CLossTrans CLossItem = item as CLossTrans;
        //                            if (CLossItem.Amount.Equals(CLossItem.OutstandingAmount))
        //                            {
        //                                bdelete = true;
        //                            }
        //                        }
        //                        if (bdelete)
        //                            deletedItems.Add(item);
        //                    }
        //                    foreach (Item item in deletedItems)
        //                    {
        //                        itemlist.Remove(item);
        //                    }
        //                    deletedItems.Clear();
        //                }

        //                //foreach (Deposit deposit in newAc.Deposits)
        //                //{
        //                //    if (deposit.IsValid == false ||            //Invalid Entry
        //                //         deposit.Amount == deposit.OutstandingAmount)    //Unsettled Deposit
        //                //        deletedDeposits.Add(deposit);
        //                //}

        //                //foreach (MiscChargeItem misChargeItem in newAc.MiscEntry)
        //                //{
        //                //    if (misChargeItem.IsValid == false ||            //Invalid Entry
        //                //         misChargeItem.OutstandingAmount>0)         //Unsettled Amt
        //                //        deletedMiscChargeItem.Add(misChargeItem);
        //                //}


        //                ////Remove invalid deposits/items from new object
        //                //foreach (Deposit deposit in deletedDeposits)
        //                //{
        //                //    newAc.Deposits.Remove(deposit);
        //                //}

        //                ////Remove invalid misc entries/items from new object
        //                //foreach (MiscChargeItem misChargeItem in deletedMiscChargeItem)
        //                //{
        //                //    newAc.MiscEntry.Remove(misChargeItem);
        //                //}


        //                //Update deposit amounts as settled
        //                foreach (Deposit deposit in newAc.Deposits)
        //                {
        //                    deposit.Amount = deposit.SettlementList.TotalSetlAmount;
        //                }

        //                //Update CA amounts as settled [WAI] + [20130805] + RPS00055
        //                foreach (CAContract caItem in newAc.CAContracts)
        //                {
        //                    caItem.Amount = caItem.SettlementList.TotalSetlAmount;
        //                }

        //                //Update miscentry amounts as settled
        //                //foreach (MiscChargeItem misChargeItem in newAc.MiscEntry)
        //                //{
        //                //    misChargeItem.Amount = misChargeItem.SettlementList.TotalSetlAmount;
        //                //}


        //                //Clear list of invalid deposits
        //                //deletedDeposits.Clear();

        //                //Clear list of invalid misc Entries
        //                //deletedMiscChargeItem.Clear();

        //                //Add this account if it contains valid deposits / Misc Entries
        //                int totItemsCount = 0;
        //                foreach (ItemList itemlist in newAc.Items)
        //                    totItemsCount += itemlist.Count;
        //                //if ((newAc.Deposits.Count > 0) || (newAc.MiscEntry.Count > 0))
        //                if (totItemsCount > 0)
        //                {
        //                    //[Wai]+ [20131113] + new receipt also need have same value in order to have same settlement order.
        //                    //Why:If this flag is not set the same value as this.IsUserOverrideSelection, 
        //                    //it will reorder the sequence of receipt items because it is called from newReceipt.SettleItems(exchRates) in below.
        //                    newReceipt.IsUserOverrideSelection = this.IsUserOverrideSelection;
        //                    newReceipt.Accounts.Add(newAc);
        //                }
        //            }
        //            newReceipt.SettleItems(exchRates);
        //            foreach (ExcessAmount e in this.ExcessAmounts)
        //                newReceipt.ExcessAmounts.Add(e.Clone());
        //            return newReceipt;
        //        }

        //        /*
        //         * Save Receipt
        //         */
        //        public void Save(int locationID, int terminalID, string userID)
        //        {
        //            foreach (Payment payment in this.Payments)
        //            {
        //                payment._dbExcessAmt = payment.UsableAmount - payment.UsedAmount;
        //                if (payment._dbExcessAmt > 0)
        //                {
        //                    foreach (ExcessAmount excessAmt in this.ExcessAmounts)
        //                    {
        //                        if (excessAmt.CurrCd.Equals(payment.CurrCd))
        //                        {
        //                            payment._dbRefundCurrCd = excessAmt.RefundCurrCd;
        //                            payment._dbRefundMethod = excessAmt.RefundMethodCode;
        //                            break;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    payment._dbRefundCurrCd = string.Empty;
        //                    payment._dbRefundMethod = string.Empty;
        //                }
        //            }

        //            SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        //            using (SqlConnection connection = db.CreateConnection() as SqlConnection)
        //            {
        //                connection.Open();
        //                SqlTransaction transaction = connection.BeginTransaction();
        //                try
        //                {
        //                    SaveMe(db, transaction, locationID, terminalID, userID);
        //                    // Commit the transaction.
        //                    transaction.Commit();
        //                }
        //                catch (Exception ex)
        //                {
        //                    ResetAutoIDs();

        //                    // Roll back the transaction. 
        //                    transaction.Rollback();
        //                    throw ex;
        //                }
        //                finally
        //                {
        //                    connection.Close();
        //                }
        //            }
        //            // Update Paid Amount in ContrLoss
        //            try
        //            {
        //                UpdatePaidAmount(2, _receiptID.ToString());
        //            }
        //            catch (Exception ex)
        //            {
        //                bool rethrow = ExceptionPolicy.HandleException(ex, "RPS Alert Policy with Email");
        //                if (rethrow)
        //                    throw;
        //            }
        //            try
        //            {
        //                //Send Data To Sub Systems
        //                Receipt.SendNewDataToSubSystems(_receiptID, userID);
        //            }
        //            catch { }
        //        }

        //        internal void ResetAutoIDs()
        //        {
        //            _receiptID = 0;
        //            _receiptNo = string.Empty;
        //            _transactionID = 0;
        //        }


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

                //Save Payments
                _paymentlist.SaveMe(db, transaction, this);

                //Save Items
                _accounts.Save(db, transaction, this);

                //Save Excess Amounts

                //_excessAmts.SaveMe(db, transaction, ReceiptID, userID);

                //UpdateCAContracts(db, transaction, 1, ReceiptID);//WAI RPS00055 25/02/2013

                //Auto WD insert
                _accounts.SaveWithDrawal(db, transaction, this);
            }
        }
        //        #endregion

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
                cm.Parameters.AddWithValue("@iStrClientID1", ClientID1);
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

        //        public decimal GetTotalCashPaymentsInSGD(System.Collections.Generic.Dictionary<string, ExchRateInfo> exchRates)
        //        {
        //            decimal memTotal = _paymentlist.GetTotalCashPaymentsInSGD(exchRates);
        //            decimal dbTotal = decimal.Zero;
        //            SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        //            using (SqlCommand cm = db.GetStoredProcCommand("Usp_Receipt_ReceiptPayments_FetchCashPaymentTotalInSGD") as SqlCommand)
        //            {
        //                cm.Parameters.AddWithValue("@iStrBusinessDate", DateTime.Now.ToString(FixedCodes.DateFormatDB));
        //                cm.Parameters.AddWithValue("@iStrClientID1", _clientID1);
        //                cm.Parameters.AddWithValue("@iStrClientID2", _clientID2);
        //                dbTotal = decimal.Parse(db.ExecuteScalar(cm).ToString());
        //            }
        //            return memTotal + dbTotal;
        //        }

        //        #region Validation
        //        [SelfValidation]
        //        public void DoValidate(ValidationResults results)
        //        {
        //            if (!_paymentlist.IsValid)
        //            {
        //                ValidationResult result = new ValidationResult("Invalid Payments", typeof(PaymentList), "", "", null);
        //                results.AddResult(result);
        //            }

        //            if (!_accounts.IsValid)
        //            {
        //                ValidationResult result = new ValidationResult("Invalid Deposits/Items", typeof(AccountList), "", "", null);
        //                results.AddResult(result);
        //            }
        //        }
        //        #endregion

        //        public static bool SendNewDataToSubSystems(long receiptID, string userID)
        //        {
        //            return SendDataToSubSystems(receiptID, userID, 1);
        //        }

        //        public static bool SendReverseDataToSubSystems(long receiptID, string userID)
        //        {
        //            return SendDataToSubSystems(receiptID, userID, 2);
        //        }

        //        public static bool SendDataToSubSystems(long receiptID, string userID, int mode)
        //        {
        //            SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;

        //            //Send Data To Sub Systems
        //            using (SqlConnection connection = db.CreateConnection() as SqlConnection)
        //            {
        //                connection.Open();
        //                SqlTransaction transaction = connection.BeginTransaction();
        //                try
        //                {
        //                    using (SqlCommand cm = db.GetStoredProcCommand("Usp_Receipt_SendSettlementsToSubSystems_TestLive") as SqlCommand)
        //                    {
        //                        cm.CommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ForceTimeout2"].ToString());
        //                        DateTime createdDateTime = DateTime.Now;
        //                        //Add Parameters
        //                        cm.Parameters.AddWithValue("@iIntMode", mode); //Mode 1 --> New Receipts, Mode 2 --> Reverse receipt
        //                        cm.Parameters.AddWithValue("@iIntReceiptID", receiptID);
        //                        cm.Parameters.AddWithValue("@iStrSendBy", userID);
        //                        //db.ExecuteNonQuery(cm, transaction);
        //                        int x = db.ExecuteNonQuery(cm);
        //                    }
        //                    // Commit the transaction.
        //                    transaction.Commit();
        //                    return true;
        //                }
        //                catch (Exception ex)
        //                {
        //                    // Roll back the transaction. 
        //                    transaction.Rollback();
        //                    bool rethrow = ExceptionPolicy.HandleException(ex, "RPS Alert Policy with Email");
        //                    if (rethrow)
        //                        throw;
        //                    return false;
        //                }
        //                finally
        //                {
        //                    connection.Close();
        //                }
        //            }
        //        }

        //        public static void ReverseReceipts(long receiptID, string remark, string userID)
        //        {
        //            SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        //            using (SqlConnection connection = db.CreateConnection() as SqlConnection)
        //            {
        //                connection.Open();
        //                SqlTransaction transaction = connection.BeginTransaction();
        //                try
        //                {
        //                    using (SqlCommand cm = db.GetStoredProcCommand("Usp_Receipt_Receipts_Reverse") as SqlCommand)
        //                    {
        //                        cm.CommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ForceTimeout2"].ToString());
        //                        cm.Parameters.AddWithValue("@iIntReceiptID", receiptID);
        //                        cm.Parameters.AddWithValue("@iStrReverseRemark", remark.Trim());
        //                        cm.Parameters.AddWithValue("@iStrReversedBy", userID);
        //                        db.ExecuteNonQuery(cm, transaction);
        //                    }
        //                    // Commit the transaction.
        //                    transaction.Commit();
        //                }
        //                catch (Exception ex)
        //                {
        //                    // Roll back the transaction. 
        //                    transaction.Rollback();
        //                    throw ex;
        //                }
        //                finally
        //                {
        //                    connection.Close();
        //                }
        //            }
        //            // To update Contra Loss Paid amount RPS00037 24/08/2010
        //            UpdatePaidAmount(3, receiptID.ToString());
        //            //Send Data To Sub Systems                                    
        //            Receipt.SendReverseDataToSubSystems(receiptID, userID);
        //        }


        //        private bool AutoDeposit(System.Collections.Generic.Dictionary<string, ExchRateInfo> exchangeRates)
        //        {
        //            bool hasAutoDeposit = false;
        //            int ledgerAcIndex = -1;
        //            int ledgerAcCount = 0;

        //            for (int i = 0; i < this.Accounts.Count; i++)
        //            {
        //                if (this.Accounts[i].HasLedger)
        //                {
        //                    ledgerAcCount++;
        //                    ledgerAcIndex = i;
        //                }
        //            }

        //            //if (this.Accounts.Count == 1)
        //            if (ledgerAcCount == 1)
        //            {
        //                hasAutoDeposit = true;
        //                Account account = this.Accounts[ledgerAcIndex];
        //                int bkDepositsCount = account.Deposits.Count;

        //                ClearSystemAutoDeposit();

        //                if (account.HasLedger //ledger based acc
        //                    && account.MiscEntry.TotalAmounts.Count == 0 //zero misc charge
        //                    && account.Deposits.TotalAmounts.Count == 0 //zero custom deposit
        //                                                                /*[WAI]+ [20130614]+ [RPS00055]
        //                                                                 *To handle CA auto deposit. For CA no need auto deposit.
        //                                                                 * User is able to choose deposit or refund.
        //                                                                 */
        //                    && account.CAContracts.Count == 0
        //                    && Payments.TotalAmounts.Count > 0 //having payment
        //                    )
        //                {
        //                    account.Deposits.Clear();
        //                    foreach (Payment payment in this.Payments)
        //                    {
        //                        if (payment.IsValid && payment.OSAmount > decimal.Zero)
        //                        {
        //                            account.AddAutoDeposit(payment);
        //                        }
        //                    }
        //                }
        //                while (account.Deposits.Count < bkDepositsCount)
        //                    account.AddDeposit();
        //            }
        //            return hasAutoDeposit;
        //        }
        //        //? Move to Account Collection?
        //        public void ClearSystemAutoDeposit()
        //        {
        //            foreach (Account account in _accounts)
        //            {
        //                int i = 0;
        //                while (i < account.Deposits.Count)
        //                {
        //                    if (((Deposit)account.Deposits[i]).IsSystemAutoDeposit)
        //                        account.Deposits.Remove(account.Deposits[i]);
        //                    else
        //                        i++;
        //                }
        //            }
        //        }

        //        public static void UpdatePaidAmount(int intMode, string receiptId)
        //        {
        //            ContraLoss objContraLoss = new ContraLoss();
        //            objContraLoss.UpdateClossData(intMode, string.Empty, receiptId);
        //        }

        //        /// <summary>
        //        /// Update CAContracts
        //        /// </summary>
        //        /// <param name="db">database object</param>
        //        /// <param name="transaction">Current connection transaction object</param>
        //        /// <param name="intMode">Mode of SP</param>
        //        /// <param name="ReceiptId">ReceiptID to be updated for CAContracts</param>
        //        public void UpdateCAContracts(SqlDatabase db, SqlTransaction transaction, int intMode, long ReceiptId)
        //        {
        //            foreach (Account objAccount in this.Accounts)
        //            {
        //                foreach (CAContract objCA in objAccount.CAContracts)
        //                {
        //                    objCA.UpdateCAContracts(db, transaction, intMode, ReceiptId.ToString());
        //                }
        //            }
        //        }
        //    }

        //    public class Balance
        //    {
        //        public string CurrCd;
        //        public decimal ItemTotal;
        //        public decimal PaymentTotal;
        //        public decimal UsedTotal;
        //        public decimal OSAmount;
        //        public decimal OSAmountSGD;
        //        public decimal ItemTotalAmtPaymentCurr;
        //    }

        //    [Serializable]
        //    public class ReceiptList : SimpleCollection<Receipt>
        //    {
        //        public void Save(int locationID, int terminalID, string userID)
        //        {
        //            for (int i = 0; i < this.Count; i++)
        //            {
        //                Receipt currentReceipt = this[i];
        //                foreach (Payment currPayment in currentReceipt.Payments)
        //                {
        //                    if (!currPayment.IsSharePayment)
        //                    {
        //                        decimal totalUsedAmt = currPayment.UsedAmount;
        //                        for (int j = i + 1; j < this.Count; j++)
        //                        {
        //                            Receipt nextReceipt = this[j];
        //                            foreach (Payment nextPayment in nextReceipt.Payments)
        //                            {
        //                                if (nextPayment.IsSharePayment && object.ReferenceEquals(nextPayment.OrgPayment, currPayment))
        //                                    totalUsedAmt += nextPayment.UsedAmount;
        //                            }
        //                        }
        //                        currPayment._dbExcessAmt = currPayment.Amount - totalUsedAmt;
        //                        if (currPayment._dbExcessAmt > 0)
        //                        {
        //                            foreach (ExcessAmount excessAmt in this[this.Count - 1].ExcessAmounts)
        //                            {
        //                                if (excessAmt.CurrCd.Equals(currPayment.CurrCd))
        //                                {
        //                                    currPayment._dbRefundCurrCd = excessAmt.RefundCurrCd;
        //                                    currPayment._dbRefundMethod = excessAmt.RefundMethodCode;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        currPayment._dbExcessAmt = decimal.Zero;
        //                        currPayment._dbRefundCurrCd = string.Empty;
        //                        currPayment._dbRefundMethod = string.Empty;
        //                    }
        //                }
        //            }

        //            SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        //            using (SqlConnection connection = db.CreateConnection() as SqlConnection)
        //            {
        //                connection.Open();
        //                SqlTransaction transaction = connection.BeginTransaction();
        //                try
        //                {
        //                    for (int i = 0; i < this.Count; i++)
        //                    {
        //                        Receipt receipt = this[i];
        //                        if (i > 0)
        //                            receipt.TransactionID = this[i - 1].TransactionID;
        //                        receipt.SaveMe(db, transaction, locationID, terminalID, userID);
        //                    }
        //                    // Commit the transaction.
        //                    transaction.Commit();
        //                }
        //                catch (Exception ex)
        //                {
        //                    // Roll back the transaction. 
        //                    transaction.Rollback();

        //                    foreach (Receipt receipt in this)
        //                    {
        //                        receipt.ResetAutoIDs();
        //                    }

        //                    throw ex;
        //                }
        //                finally
        //                {
        //                    connection.Close();
        //                }
        //            }

        //            //Send Data To Sub Systems            
        //            if (this.Count > 0)
        //            {
        //                long receiptID = this[0].ReceiptID;
        //                try
        //                {

        //                    Receipt.SendNewDataToSubSystems(receiptID, userID);
        //                }
        //                catch { }
        //            }
        //        }
    }
}
