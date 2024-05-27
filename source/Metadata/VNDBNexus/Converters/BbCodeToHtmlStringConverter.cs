using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using VndbApiDomain.SharedKernel;

namespace VNDBNexus.Converters
{
    public class BbCodeToHtmlStringConverter :  IMultiValueConverter
    {
        private static readonly BbCodeProcessor _bbcodeProcessor = new BbCodeProcessor();
        private const string _spoilerTextReplacement = "<code>Hidden by spoiler settings</code>";
        private static readonly Regex _spoilerRegex = new Regex(@"<spoiler>(.|\s)+?<\/spoiler>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is string str)
            {
                if (str.IsNullOrEmpty())
                {
                    return str;
                }

                // Some titles like The Great Ace Attorney only have the spoiler opening
                // tag but not the closing one.
                if (str.Contains("[spoiler]", StringComparison.InvariantCultureIgnoreCase) &&
                    !str.Contains("[/spoiler]", StringComparison.InvariantCultureIgnoreCase))
                {
                    str += "[/spoiler]";
                }

                var htmlString = _bbcodeProcessor.ToHtml(str);
                if (!htmlString.Contains("<spoiler>", StringComparison.InvariantCultureIgnoreCase))
                {
                    return htmlString;
                }
                
                if (values.Length > 1 &&
                    values[1] is SpoilerLevelEnum maxSpoilerLevel &&
                    maxSpoilerLevel == SpoilerLevelEnum.Major)
                {
                    return htmlString;
                }
                else
                {
                    return _spoilerRegex.Replace(htmlString, _spoilerTextReplacement);
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
