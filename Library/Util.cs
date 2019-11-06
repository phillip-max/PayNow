//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AccessRPSService
//{
//    public class Util
//    {
//        public static decimal Truncate(decimal value, int decimalpoints)
//        {
//            decimal points = value - decimal.Truncate(value);
//            value = decimal.Truncate(value);

//            decimal multiplyer = decimal.Parse(Math.Pow(10, decimalpoints).ToString());
//            points = decimal.Multiply(points, multiplyer);
//            points = decimal.Truncate(points);
//            points = decimal.Divide(points, multiplyer);
//            return value + points;
//        }

//        public static decimal GetAmoutInBase(string currencycode, decimal amount, System.Collections.Generic.Dictionary<string, ExchRateInfo> exchangeRates)
//        {
//            if (currencycode.Equals(FixedCodes.BaseCurrencyCode))
//                return amount;
//            else
//            {
//                decimal amt = decimal.Zero;
//                ExchRateInfo exchInfo = exchangeRates[currencycode];
//                if (exchInfo != null)
//                {
//                    amt = Util.Truncate(amount * exchInfo.BaseRateLow, 2);
//                }
//                return amt;
//            }
//        }

//        public bool IsEmptyDataSet(DataSet ds)
//        {
//            if (ds.Tables.Count == 0)
//                return true;
//            else
//            {
//                bool empty = true;
//                foreach (DataTable t in ds.Tables)
//                    if (t.Rows.Count != 0)
//                    {
//                        empty = false;
//                        break;
//                    }
//                return empty;
//            }
//        }

//        public static string DataTable2HTMLTable(DataTable dt)
//        {
//            string sTableStart = @"<HTML><BODY><TABLE Border=1>";
//            string sTableEnd = @"</TABLE></BODY></HTML>";
//            string sTHead = "<TR>";
//            StringBuilder sTableData = new StringBuilder();

//            foreach (DataColumn col in dt.Columns)
//            {
//                sTHead += @"<TH>" + col.ColumnName + @"</TH>";
//            }

//            sTHead += @"</TR>";

//            foreach (DataRow row in dt.Rows)
//            {
//                sTableData.Append(@"<TR>");
//                for (int i = 0; i < dt.Columns.Count; i++)
//                {
//                    sTableData.Append(@"<TD>" + row[i].ToString() + @"</TD>");
//                }
//                sTableData.Append(@"</TR>");
//            }

//            string sTable = sTableStart + sTHead + sTableData.ToString() + sTableEnd;
//            return sTable;
//        }
//    }
//}
