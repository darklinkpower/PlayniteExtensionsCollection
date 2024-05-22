using CodeKicker.BBCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VNDBFuze
{
    public class BbCodeProcessor
    {

        private static readonly Dictionary<string, string> replaceFormatters;
        private static readonly Dictionary<string, string> regexFormatters;
        private static readonly BBCodeParser bbcodeParser;

        static BbCodeProcessor()
        {
            replaceFormatters = new Dictionary<string, string>
            {
                {"\r\n", "<br>" },
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

            var bbTags = new List<BBTag>()
            {
                new BBTag("b", "<strong>", "</strong>"),
                new BBTag("u", "<u>", "</u>"),
                new BBTag("s", "<strike>", "</strike>"),
                new BBTag("i", "<em>", "</em>"),
                new BBTag("h1", "<h1>", "</h1>"),
                new BBTag("h2", "<h2>", "</h2>"),
                new BBTag("h3", "<h3>", "</h3>"),
                new BBTag("hr", "<hr>", "</hr>"),

                //Lists
                new BBTag("list", "<ul>", "</ul>") { SuppressFirstNewlineAfter = true },
                new BBTag("li", "<li>", "</li>", true, false),
                new BBTag("*", "<li>", "</li>", true, false),
                new BBTag("olist", "<ol>", "</ol>"),
                new BBTag("url", "<a href=\"${href}\">", "</a>",
                    new BBAttribute("href", ""),
                    new BBAttribute("href", "href"),
                    new BBAttribute("target", "target")),
                new BBTag("code", "<code>", "</code>")
                {
                    StopProcessing = true,
                    SuppressFirstNewlineAfter = true
                },
                //Tables
                new BBTag("table", "<table>", "</table>"),
                new BBTag("th", "<th>", "</th>"),
                new BBTag("tr", "<tr>", "</tr>"),
                new BBTag("td", "<td>", "</td>"),

                // Spoiler
                new BBTag("spoiler", "<spoiler>", "</spoiler>"),

                // quote
                new BBTag("quote", "<blockquote>", "</blockquote>"),
                // image
                new BBTag("img", "<img src=\"${content}\"/>", "", false, true)
            };

            bbcodeParser = new BBCodeParser(ErrorMode.TryErrorCorrection, null, bbTags);
        }

        public string ToHtml(string bbCodeString)
        {
            try
            {
                return bbcodeParser.ToHtml(bbCodeString);
            }
            catch
            {
                // Fallback when parser fails
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
                    bbCodeString = Regex.Replace(bbCodeString, item.Key, item.Value);
                }

                return bbCodeString;
            }
        }

    }
}