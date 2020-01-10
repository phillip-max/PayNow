using System;
using System.Linq;

namespace PayNowReceiptsGeneration
{
    public class Util
    {
        public static decimal Truncate(decimal value, int decimalpoints)
        {
            decimal points = value - decimal.Truncate(value);
            value = decimal.Truncate(value);

            decimal multiplyer = decimal.Parse(Math.Pow(10, decimalpoints).ToString());
            points = decimal.Multiply(points, multiplyer);
            points = decimal.Truncate(points);
            points = decimal.Divide(points, multiplyer);
            return value + points;
        }

        public static string FormatAccountNumber(string accNo, string[] piggyAccServices)
        {
            if (piggyAccServices.Any(accNo.Contains))
            {
                foreach (var accservice in piggyAccServices)
                {
                    if (accNo.Contains(accservice))
                    {
                        accNo = accNo.Replace(accservice, "");
                        break;
                    }
                }
            }      

            string NewAccNo = accNo;

            if (accNo.Length < 7)
            {
                NewAccNo = accNo.PadLeft(7, '0');
            }
            return NewAccNo;
        }

        public static bool IsPiggy(string accNo, string[] piggyAccServices)
        {
            return piggyAccServices.Any(accNo.Contains);
        }


        public static string GetAccountServiceType(string accNo, string[] piggyAccServices)
        { 
            if (piggyAccServices.Any(accNo.Contains))
            {
                foreach (var accservice in piggyAccServices)
                {
                    if (accNo.Contains(accservice))
                    {
                        return accservice;
                    }
                }
            }
            return string.Empty;

        }
    }
}
