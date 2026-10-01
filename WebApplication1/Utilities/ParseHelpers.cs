using System;
using System.Globalization;

namespace WebApplication1.Utilities
{
    public class ParseHelpers
    {
        //Nedan metoder används inte för tillfället, planen är att de ska det i framtiden


        //ParseToDecimal
        public static decimal DoDecimal(string input, decimal defaultValue = 0)
        {

            input = input.Replace(',', '.');

            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            // NumberStyles.Any allows currency symbols, exponents, thousands separators, and signs
            // CultureInfo.InvariantCulture enforces standard '.' for decimal points and ',' for thousands
            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            JsHelpers.Alert("Fel!");

            return defaultValue;
        }





        // Safely converts an Eval object directly to an HTML5 compatible string
        public static string DoHtmlInputNumber(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";

            // Convert the data source value safely to a decimal
            decimal num = Convert.ToDecimal(value);

            // Forces a dot decimal separator so HTML5 <input type="number"> understands it
            return num.ToString(CultureInfo.InvariantCulture);
        }

    }
}