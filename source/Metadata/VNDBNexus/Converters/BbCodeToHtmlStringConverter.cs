using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VNDBNexus.Converters
{
    public class BbCodeToHtmlStringConverter : IValueConverter
    {
        private static readonly BbCodeProcessor _bbcodeProcessor = new BbCodeProcessor();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return _bbcodeProcessor.ToHtml(str);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
