using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AccessRPSService
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
            string AccService1 = string.Empty;
            string AccService2 = string.Empty;           

             AccService1 = piggyAccServices.Length > 0 ? piggyAccServices[0] : string.Empty;
             AccService2 = piggyAccServices.Length > 1 ? piggyAccServices[1] : string.Empty;

            if (accNo.Contains(AccService1))
                accNo = accNo.Replace(AccService1, "");
            else if (accNo.Contains(AccService2))
                accNo = accNo.Replace(AccService2, "");
            
            string NewAccNo = accNo;

            if(accNo.Length < 7)
            {
                NewAccNo = accNo.PadLeft(7, '0');
            }
            return NewAccNo;
        }
    }
}
