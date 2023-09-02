using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPresetsQuickLauncher
{
    public static class TextComparer
    {
        // Based on https://github.com/JosefNemec/Playnite
        private static readonly char[] textMatchSplitter = new char[] { ' ' };

        public static bool MatchTextFilter(string str, string searchString)
        {
            if (searchString.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (!searchString.IsNullOrWhiteSpace() && str.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (searchString.IsNullOrWhiteSpace() && str.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (searchString.Length > str.Length)
            {
                return false;
            }

            var toMatchSplit = str.Split(textMatchSplitter, StringSplitOptions.RemoveEmptyEntries);
            var searchStringSplit = searchString.Split(textMatchSplitter, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in searchStringSplit)
            {
                if (!toMatchSplit.Any(a => a.ContainsInvariantCulture(word, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}