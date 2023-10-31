using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayNotes.Converters
{
    public class MarkdownUnescapeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string markdownText && !string.IsNullOrEmpty(markdownText))
            {
                markdownText = AddSpacesBeforeNewline(markdownText); // For some reason single line breaks sometimes don't result in a new line being displayed
                return markdownText;
            }

            return value;
        }

        static string AddSpacesBeforeNewline(string input)
        {
            var sb = new StringBuilder();
            var precededByPipe = false;
            foreach (char c in input)
            {
                if (c == '|') // In case of tables, don't add double spaces to not break them
                {
                    precededByPipe = true;
                }
                else if (c == '\n')
                {
                    if (!precededByPipe)
                    {
                        sb.Append("  ");
                    }

                    precededByPipe = false;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}