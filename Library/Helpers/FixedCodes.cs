using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayNowReceiptsGeneration
{
    public static class FixedCodes
    {
        public const string AS400BANKCODE = "AS400BANKCODE";
        public const string RECEIPTINSTRUCTION = "ITEMSOURCE";
        public const string LookUpCodeType = "TYPE";
        public const string LookUpCodeCompany = "COMPANY";
        public const string LookUpCodeAccSvcType = "ACCSVCTYPE";
        public const string LookUpCodePaymentType = "PAYMENTMETHOD";
        public const string LookUpDiagnostic = "DIAGNOSTIC";
        //public const string LookUpCodePaymentCurrencies = "PAYMENTMETHODCUR";
        public const string LookUpCodeRefBankCode = "REFBANKCODE";
        public const string LookUpCodeCISList = "CISLIST";
        public const string LookUpCodeBankCode = "BANKCODE";
        public const string LookUpCodeSubSystem = "SUBSYSTEM";
        public const string LookUpCodeAMLRemarks = "AMLREMARKS";
        public const string LookUpCodeGSTPercentage = "GSTPERCENT";
        public const string LookUpCodeAMLAmounts = "AMLAMTS";
        public const string LookUpCodeAMLAmountsAlertAmount = "ALERTAMT";
        public const string LookUpCodeRefundType = "REFUNDTYPE";
        //Swapna -billPayments 25/05/09
        public const string LookUpCodeBPBankCode = "BPBANKCODE";
        public const string LookUpCodeBPFileType = "BPFILETYPE";
        public const string LookUpCodeBPFileFormatType = "BPFORMATTYPE";
        public const string LookUpCodeBPFileSections = "BPFILESECTIONS";
        public const string LookUpCodeBPFileFormat = "BPFORMAT";
        public const string LookUpCodeBPCHNLPMTTYPE = "BPCHNLPMTTYPE";
        public const string LookUpCodeEquityAccSrvType = "EQUITYACCSVCTYPE";
        public const string LookUpCodeNonEquityAccSrvType = "NONEQUITYACCSVCTYPE";
        public const string BPFileTXTType = "TXT";


        public const string LookUpCodePaymentTypeCheque = "CHQ";
        public const string LookUpCodePaymentTypeHouseCheque = "HCHQ";  //++Chithra
        public const string LookUpCodePaymentTypeCash = "CASH";
        public const string LookUpCodePaymentTypeGBOLGR = "GBOLGR";

        //[Shilpa]-20131110 - Added below statement which is used by FormatContractNo function in entry.js file
        public const string LookUpCodeCustomContractNoCdType = "CUSTOMCONNO";
        public const string LookUpCodeCustomContractNoCode = "BUY";

        public const string BaseCurrencyCode = "SGD";
        public const string BaseCurrencySymbol = "SGD";
        public const string USDCurrencyCode = "USD";
        public const string USDCurrencySymbol = "USD";

        public const string DateFormat = "dd/MM/yyyy";
        public const string DateFormatDataBind = "{0:dd/MM/yyyy}";
        public const string DateTimeFormatDataBind = "{0:dd/MM/yyyy hh:mm tt}";
        public const string DateFormatDB = "yyyyMMdd";

        public const string ExchRateFormat = "F9";
        public const string ExchRateFormatDataBind = "{0:F9}";
        public const string ExchRateFormatJS = "0.000000000";
        public const int ExchRateDecimalPoints = 9;

        public const string ExchRateMarginFormat = "F4";
        public const string ExchRateMarginFormatDataBind = "{0:F4}";
        public const string ExchRateMarginFormatJS = "0.0000";

        public const string QtyFormatTruncate = "N0";
        public const string AmountFormat = "N2";
        public const string AmountFormatDataBind = "{0:N2}";
        public const string AmountFormatJS = "0.00";

        public const string AccNoFormat = "0000000";
        public const string AccNoFormatDataBind = "{0:0000000}";
        public const string AccNoFormatJS = "0000000";

        public const string SubSystemSynergy = "SYN";
        //Added CFD to Enable Multicurrency by default -by Swapna 09/03/2009
        public const string SubSystemCFD = "CFD";
        public const string SubSystemUT = "UT";
        public const string AccServiceTypeUTW = "UTW";   //silgia 20091202  RPS00015
        public const string AccServiceTypeCA = "CA";//WAI 20130227 RPS00055
        public const string AccServiceTypeKC = "KC";
        public const string AccServiceTypeUT = "UT";
        public const string AccServiceTypeCFD = "CFD";
        public const string AccServiceTypeOPT = "OPT";
        public const string AccServiceTypeSBP = "SBP";


        public const string RefundCodeType = "GBO";//WAI 20130227 RPS00055


        public const string GSTCode = "GST";

        public const string ExchMarginTypeSpread = "S";
        public const string ExchMarginTypePercentage = "%";

        public const string FELTypePercentage = "%";
        public const string FELTypeAmount = "";

        public const string CompanyCodePSPL = "PSPL";
        public const string CompanyCodePFPL = "PFPL";


        //Added By Swapna 22/12/2008 for Payments
        public const string LookUpCodePMTType = "PMTTYPE";
        public const string LookUpCodeRemarks = "PMTREMARKS";
        public const string LookUpCodeCPIUserSystems = "PMTCPIUSERSYSTEMS";

        public const string LookUpCodeBankBranchUOB = "PMTBankBranchUOB";  //silgia added on 20090930
        public const string LookUpCodePMTBankDetails = "PMTBankDetails";     //silgia added on 20091109
        public const string LookUpCodePMTBankDetailsMap = "PMTBankDetailsMap";

        public const string LookUpCodePMTApprovalLevel = "PMTAPPROVALLEVEL";  //silgia added on 20100209 RPS00023
        public const string LookupCodePMTBankCode = "PMTBANKCODE"; // Added by Jansi on 20131024
        public const string LookupCodePMTCompanyCode = "GIROOUT"; // Added by Jansi on 20131024
        public const string LookUpCodeAccSvcTypeOrder = "AccSvcTypeOrder";
        public const string LookupCodeAccType = "ACCTYPE";

        //Added By Gowri 20081212;
        public const string LookUpCodeChequeIssuingBank = "PMTCHQBANK";
        public const string ChequeNoFormat = "000000";
        public const string ChequeNoFormatDataBind = "{0:000000}";

        // venkat 20100616
        public const string FELAccSvcType1 = "UTW";
        public const string FELAccSvcType2 = "S2";

        //Venkat 02/09/2010 RPS00037
        public const string LookUpCodeMthStmtType = "MSTMTTYPE";
        public const string MailRPTFooter = "RPTFOOTER";
        public const string MailType = "MTHSTMT";
        public const string PSEUDOEXCEPT = "PSEUDOEXCEPT"; // 10/11/2010

        //Venkat 20/09/2010 RPS00034 
        public const string LookUpcodeGIROSetup = "PMTGIROFileSetup";
        public const string LookUpcodeGIROReject = "PMTGIROREJECTCODE";
        public const string LookUpcodeGIROSequence = "PMTGIROFileSequence";
        public const string LookUpcodePseudoSystem = "PSEUDOSYSTEM";
        public const string NomineeSubsystem = "NOM";
        public const string NomineeVoucher = "NOMVOC";
        public const string GBOCAVoucher = "CAVOU";
        public const string GBOCASubsystem = "GBOCA";

        //sedney 16/09/2011 RPS00047
        public const string LookUpcodeEmailSendSubs = "PMTEMAILSENDSUBS";
        public const string CodeTypeGIROResultUpload = "GIROResultUpload";
        public const string GiroFileSequenceCombined = "COMBINED";

        public const string LookUpCodeCurrency = "CURRENCY";
        public class ItemDesc
        {
            public const string Deposit = "Deposit";
        }

        public class ItemTypes
        {
            public const string Deposit = "D";
            public const string MiscCharge = "M";
            public const string UTContract = "UT";
            public const string CLossTrans = "C"; // Venkat RPS00037 18/08/2010
            public const string NOMTransactions = "NM"; // Venkat RPS00036 18/08/2010
            public const string CAContract = "CA";//WAI RPS00055 20130222
        }

        //Gowri 23-02-2009
        //FormIDs for setting permissions to buttons
        public const int NewPseudoAccountFormID = 130;
        public const int PaymentInstructionsFormID = 211;
        public const int ChequePrintingFormID = 212;
        public const int UpdateChequeNumbersFormID = 213;
        public const int SendDataToSubSystemsFormID = 214;
        public const int PostToReceiptsFormID = 215;
        public const int SplitInstructionsFormID = 216;
        public const int btnChequePrintFormID = 217;
        public const int btnUpdateChequenumbersFormID = 218;
        public const int btnSendToSubSystemsFormID = 219;
        //silgia 20091027
        public const int GIROPaymentFormID = 219;
        public const int btnGIROGIROPaymentFormID = 221;
        public const int btnViewGIROGIROPaymentFormID = 222;
        public const int AdhocPaymentFormID = 220;
        public const int btnUploadExcelAdhocPaymentFormID = 223;
        //Venkat RPS00037 23/09/2010
        public const int ClossTransFormId = 151;
        public const int CLossStatementFormId = 152;
        public const int btnGenerateStatementFormId = 154;
        public const int btnSendSatementFormId = 155;

        //20180831
        public const int ReverseReceiptFormId = 120;
        public const int EditReverseReceipt = 3;

        //irfan 20150420
        public const string LookUpcodeEPSTIME = "EPSAPPTIME";
        public const string LookUpcodeGIROTIME = "GIROAPPTIME";
        public const string LookUpcodeEPSGIROTIME = "EPSGIROAPPTIME";
        public const string LookUpcodeEPSGIROENDTIME = "ENDTIME";

        //Mai 20161026
        public const string lookUpcodePaymentMode = "PaymentMode"; // to use in Pull payment UI 

        //Mai + 20180810
        public const string lookUpcodeBalanceType = "BalanceType"; //To fetch Balance Type from setup lookup table

        public class TransTypes
        {
            public const string Loss = "LOSS";
            public const string SADR = "SADR";
            public const string BUY = "BUY";
            public const string CHDN = "CHDN";
            public const string CLDN = "CLDN";
            public const string CHAO = "CHAO";
            public const string CHCAO = "CHCAO"; //20190801
            public const string CF = "CF";
            public const string OPCR = "OPCR"; //20180712
            public const string SELL = "SELL"; //20180712
        }
    }
}
