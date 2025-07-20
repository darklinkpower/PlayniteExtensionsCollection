using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VNDBNexus
{
    public static class HtmlDescriptionCleaner
    {
        private static readonly Regex DescriptionSourceRegex = new Regex(
            @"(?:<br\s*/?>\s*)+\[\s*[^]]+?\s*\]",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex TrailingBrRegex = new Regex(
            @"(\s*<br\s*/?>\s*)+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string CleanHtml(string html)
        {
            if (html.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            html = TrailingBrRegex.Replace(html, string.Empty);
            html = DescriptionSourceRegex.Replace(html, string.Empty);
            html = TrailingBrRegex.Replace(html, string.Empty);
            return html.Trim();
        }
    }
}