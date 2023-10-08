using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayNotes.Converters
{
    public class MarkdownUnescapeConverter : IValueConverter
    {
        private static readonly Regex MarkdownEscapeRegex = new Regex(@"\\(.)", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string markdownText)
            {
                //markdownText = MarkdownEscapeRegex.Replace(markdownText, "$1"); // For some reason the \ escape character is displayed
                markdownText = Regex.Replace(markdownText, "\n", "\n\n"); // For some reason single line breaks sometimes don't result in a new line being displayed
                return markdownText;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}