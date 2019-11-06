using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Simple;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessRPSService
{
    public abstract class Item : SimpleBase<Item>
    {
        #region Private Data

        //Item No
        private int _itemNo = 0;

        //Item Ref
        private string _itemRef = string.Empty;

        //ItemDescription
        private string _itemDesc = string.Empty;

        //Item CurrCd
        private string _currCd = FixedCodes.BaseCurrencyCode;

        //Item Amount
        private decimal _amount = decimal.Zero;
        private Dictionary<string, decimal> _customExchRates = new Dictionary<string, decimal>();
        private Dictionary<string, decimal> _customShowExchRates = new Dictionary<string, decimal>();

        #endregion
        public Item() { }

        public Item(string itemRef, string itemDesc, string currcd, decimal amount)
        {
            _itemRef = itemRef;
            _itemDesc = itemDesc;
            _currCd = currcd;
            _amount = amount;
        }

        #region Public Properties

        //Item No
        public int ItemNo
        {
            get
            {
                return _itemNo;
            }
            set
            {
                _itemNo = value;
            }
        }

        //Item Ref
        public string ItemRef
        {
            get
            {
                return _itemRef;
            }
            set
            {
                _itemRef = value.Trim();
            }
        }

        //Item Description
        public string ItemDescription
        {
            get
            {
                return _itemDesc;
            }
            set
            {
                _itemDesc = value.Trim();
            }
        }

        //CurrCd
      //  [StringLengthValidator(0, RangeBoundaryType.Exclusive, 0, RangeBoundaryType.Ignore, MessageTemplateResourceName = "MandatoryFieldMsg", Tag = "Currency Code", MessageTemplateResourceType = typeof(ErrorMessages))]
        public virtual string CurrCd
        {
            get
            {
                return _currCd;
            }
            set
            {
                _currCd = value.Trim();
            }
        }

        //Amount
       // [RangeValidator(typeof(Decimal), "0", RangeBoundaryType.Exclusive, "0", RangeBoundaryType.Ignore, MessageTemplateResourceName = "MinRangeMsg", Tag = "Amount", MessageTemplateResourceType = typeof(ErrorMessages))]
        public virtual decimal Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
            }
        }

        //Settled Amount
        public decimal SetlAmount
        {
            get
            {
                return SettlementList.TotalSetlAmount;//_setlList
            }
        }

        //Outstanding Amount
        public decimal OutstandingAmount
        {
            get
            {
                return _amount - SettlementList.TotalSetlAmount;
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

        public virtual void SettleDirect(Receipt receipt, PaymentList payments)
        {
            //clear prev settled
            SettlementList.Clear();

            //Is this deposit is valid
            /*New comment*/
            if (/*IsValid &&*/ this.OutstandingAmount > 0)
            {
                //Go through all payments
                foreach (Payment payment in payments)
                {
                    //Stop when there is no OS amount
                    if (this.OutstandingAmount <= decimal.Zero)
                        break;

                    //Is the payment valid
                    /*New comment*/
                    if (/*payment.IsValid &&*/ payment.IsSharePayment)
                        if (!payment.IsValidSharePayment(receipt))
                            continue;

                    /*New comment*/
                    if (/*payment.IsValid &&*/  //is valid
                        payment.OSAmount > 0 && //any oustanding amt to use
                        payment.CurrCd.Equals(this._currCd))//same currency?                        
                    {
                        Settlement settlement = GetNewSettlementObject();
                        if (settlement.SettleDirect(this, payment))
                            SettlementList.Add(settlement);
                    }
                }
            }

        }

        /*New comment*/
        //public virtual void SettleWithConversion(Receipt receipt, PaymentList payments, Dictionary<string, ExchRateInfo> exchRates)
        //{
        //    /*New comment*/
        //    if (/*IsValid &&*/ this.OutstandingAmount > decimal.Zero)
        //    {
        //        foreach (Payment payment in payments)
        //        {
        //            //Stop when there is no OS amount
        //            if (this.OutstandingAmount <= decimal.Zero)
        //                break;
        //            if (!string.IsNullOrEmpty(payment.CurrCd))
        //            {
        //                Settlement settlement = GetNewSettlementObject();
        //                //WAI+20130917+RPS00055+ calculate itemos in paymen currency
        //                settlement.SetItemOSInPaymentCurr(this, payment, exchRates);

        //                /*New comment*/
        //                if (/*payment.IsValid*/ true)
        //                {

        //                    if (/*payment.IsValid &&*/  //is valid
        //                    payment.OSAmount > 0 && //any oustanding amt to use
        //                    !payment.CurrCd.Equals(this._currCd) &&
        //                    (payment.SetlOption.Equals(this._currCd) || string.IsNullOrEmpty(payment.SetlOption)))//Is Setl Option blank OR same with currency
        //                    {
        //                        if (settlement.SettleWithConversion(this, payment, exchRates))
        //                            SettlementList.Add(settlement);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #region Abstract Properties/Methods

        //Item Type
        public abstract string ItemType
        {
            get;
        }

        //SettlementList
        public SettlementList SettlementList { get; } = new SettlementList();

        public abstract Settlement GetNewSettlementObject();

        #endregion

        #region Protected Method

        //Generate Insert SqlCommand
        protected virtual SqlCommand GetInsertCommand(SqlDatabase db, Receipt receipt, Account account)
        {
            /*--------------------------------------------------------------------------------------------
            * DATE             Author        Comment
            * -------------------------------------------------------------------------------------------
            *  06/02/08        JAW        Created 
            *  02/02/08         SWAPNA      Added parameter to send Pseudo receipt information-RPS00004
            * -----------------------------------------------------------------------------------------*/

            SqlCommand insertCmd = db.GetStoredProcCommand("Usp_Receipt_ReceiptItems_Insert") as SqlCommand;
            //Add Parameters
            insertCmd.Parameters.AddWithValue("@iIntReceiptID", receipt.ReceiptID);
            insertCmd.Parameters.AddWithValue("@iIntItemNo", _itemNo);
            insertCmd.Parameters.AddWithValue("@iStrAccNo", account.AccNo);
            insertCmd.Parameters.AddWithValue("@iStrAccServiceType", account.AccSvcType);
            insertCmd.Parameters.AddWithValue("@iStrAeCd", account.AeCd);
            insertCmd.Parameters.AddWithValue("@iStrAeRefNo", account.AeRefNo);
            insertCmd.Parameters.AddWithValue("@iStrItemType", ItemType);
            insertCmd.Parameters.AddWithValue("@iStrItemRef", _itemRef);
            insertCmd.Parameters.AddWithValue("@iStrItemDesc", _itemDesc);
            insertCmd.Parameters.AddWithValue("@iStrCurrCd", _currCd);
            insertCmd.Parameters.AddWithValue("@iDecAmount", _amount);
            insertCmd.Parameters.AddWithValue("@ibIsPseudoReceipt", receipt.IsPseudoReceipt);
            return insertCmd;
        }

        //Desc: Save Item to DB
        //Return: Item No
        internal int Save(SqlDatabase db, SqlTransaction trans, Receipt receipt, Account account)
        {
            using (SqlCommand insertCmd = GetInsertCommand(db, receipt, account))
            {
                db.ExecuteNonQuery(insertCmd, trans);
            }
            SettlementList.Save(db, trans, receipt, this);
            return 0;
        }

        #endregion        

        internal void ClearSettlements()
        {
            foreach (Settlement setl in SettlementList)
                setl.UndoSettle();
            SettlementList.Clear();
        }

        public void ClearPartialSettlements()
        {
            if (this.OutstandingAmount > 0M)
                ClearSettlements();
        }
    }

    [Serializable]
    public class ItemList : SimpleCollection<Item>
    {
        #region Public Member

        //
        //Desc:   Save all objects in the collection
        //
        public void Save(SqlDatabase db, SqlTransaction trans, Receipt receipt, Account account, ref int lastItemNo)
        {
            foreach (Item item in this)
            {
                item.ItemNo = ++lastItemNo;
                item.Save(db, trans, receipt, account);
            }
        }

        public Item Search(int itemNo)
        {
            foreach (Item item in this)
                if (item.ItemNo == itemNo) return item;

            return null;
        }

        public new void Remove(Item item)
        {
            base.Remove(item);
            for (int i = 0; i < this.Count; i++)
                this[i].ItemNo = i + 1;
        }

        public new Item Add(Item item)
        {
            item.ItemNo = this.Count + 1;
            return base.Add(item);
        }

        public void ClearPartialSettlements()
        {
            foreach (Item item in this)
                if (item.OutstandingAmount > 0)
                    item.ClearSettlements();
        }

        #endregion
    }
}
