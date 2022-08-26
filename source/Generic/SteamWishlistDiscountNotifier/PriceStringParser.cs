using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier
{
    public static class PriceStringParser
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly char[] numberChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static void GetPriceValues(string parsedBlock, out string currencyCode, out double? currencyValue)
        {
            parsedBlock = parsedBlock.Trim();
            var firstNumberIndex = parsedBlock.IndexOfAny(numberChars);
            var lastNumberIndex = parsedBlock.LastIndexOfAny(numberChars);

            if (firstNumberIndex == -1 || lastNumberIndex == -1)
            {
                logger.Error($"Failed to parsed money parsed block \"{parsedBlock}\", firstNumberIndex {firstNumberIndex}, lastNumberIndex {lastNumberIndex}");
                currencyCode = null;
                currencyValue = null;
                return;
            }

            var currencyValueStr = parsedBlock.Substring(firstNumberIndex, lastNumberIndex - firstNumberIndex + 1);
            currencyValue = GetParsedPrice(currencyValueStr);
            currencyCode = parsedBlock.Remove(firstNumberIndex, lastNumberIndex - firstNumberIndex + 1).Trim();
        }

        private static double GetParsedPrice(string str)
        {
            var pointIndex = str.LastIndexOf('.');
            var commaIndex = str.LastIndexOf(',');

            if (commaIndex < pointIndex)
            {
                // Point is decimal separator
                return double.Parse(str, CultureInfo.InvariantCulture);
            }
            else
            {
                // Comma is decimal separator
                return double.Parse(str, CultureInfo.GetCultureInfo("es-ES"));
            }
        }
    }
}