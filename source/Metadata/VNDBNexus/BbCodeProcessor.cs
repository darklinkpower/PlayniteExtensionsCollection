﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VNDBNexus
{
    public class BbCodeProcessor
    {

        private static readonly Dictionary<string, string> replaceFormatters;
        private static readonly Dictionary<string, string> regexFormatters;

        static BbCodeProcessor()
        {
            replaceFormatters = new Dictionary<string, string>
            {
                {"\r\n\n", "<br>" },
                {"\n", "<br>" },
                {"\r", "" },
                {"\t", "" },
                {"[hr][/hr]", "<hr>" }
            };

            regexFormatters = new Dictionary<string, string>
            {
                //Tags
                {@"\[b\]((.|\n)*?)\[\/b\]", "<strong>$1</strong>" },
                {@"\[u\]((.|\n)*?)\[\/u\]", "<u>$1</u>" },
                {@"\[s\]((.|\n)*?)\[\/s\]", "<strike>$1</strike>" },
                {@"\[i\]((.|\n)*?)\[\/i\]", "<em>$1</em>" },
                {@"\[code\]((.|\n)*?)\[\/code\]", "<code>$1</code>" },
                {@"\[h1\]((.|\n)*?)\[\/h1\]", @"<h1 class=""bb_tag"">$1</h1>" },
                {@"\[h2\]((.|\n)*?)\[\/h2\]", @"<h2 class=""bb_tag"">$1</h2>" },
                {@"\[h3\]((.|\n)*?)\[\/h3\]", @"<h3 class=""bb_tag"">$1</h3>" },

                //Urls
                {@"\[url=([^\]]+)]\s*((.|\n)*?)\[\/url\]", @"<a href=""$1"" target=""_blank"">$2</a>" },

                //Lists
                {@"\[list\]((.|\n)*?)\[\/list\]", @"<ul>$1</ul>" },
                {@"\[olist\]((.|\n)*?)\[\/olist\]", @"<ol>$1</ol>" },
                {@"\[\*\]((.|\n)*?)<br>", @"<li>$1</li>" },

                //Tables
                {@"\[table\]((.|\n)*?)\[\/table\]", "<table>$1</table>" },
                {@"\[th\]((.|\n)*?)\[\/th\]<br>", "<th>$1</th>" },
                {@"\[tr\]((.|\n)*?)\[\/tr\]<br>", "<tr>$1</tr>" },
                {@"\[td\]((.|\n)*?)\[\/td\]<br>", "<td>$1</td>" },
                {@"<tr><br>", "<tr>" },

                //Spoiler
                {@"\[spoiler\]((.|\n)*?)\[\/spoiler\]", "<spoiler>$1</spoiler>" },

                //Quote
                {@"\[quote\]((.|\n)*?)\[\/quote\]", "<blockquote>$1</blockquote><br>" },
                {@"\[quote=([^\]]+)]((.|\n)*?)\[\/quote\]", "<blockquote>$1:</blockquote><br><blockquote>$2</blockquote><br>" }
            };
        }

        public string ToHtml(string bbCodeString)
        {
            if (string.IsNullOrEmpty(bbCodeString))
            {
                return string.Empty;
            }

            foreach (var item in replaceFormatters)
            {
                bbCodeString = bbCodeString.Replace(item.Key, item.Value);
            }

            foreach (var item in regexFormatters)
            {
                bbCodeString = Regex.Replace(bbCodeString, item.Key, item.Value, RegexOptions.IgnoreCase);
            }

            return bbCodeString;
        }

    }
}