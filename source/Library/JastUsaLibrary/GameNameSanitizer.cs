using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary
{
    public static class GameNameSanitizer
    {
        public static string Satinize(string gameName)
        {
            return gameName.RemoveTrademarks()
                .Replace("[PRE-ORDER]", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                // Typos with extra spaces exist in the store
                .Replace("( PRE-ORDER) ", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                .Replace("(PRE-ORDER) ", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                .Trim();
        }
    }
}
