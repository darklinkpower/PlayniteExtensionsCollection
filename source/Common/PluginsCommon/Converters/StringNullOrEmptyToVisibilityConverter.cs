using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PluginsCommon.Converters
{
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var invertResult = parameter != null && System.Convert.ToBoolean(parameter);
            if (value is string stringValue)
            {
                if (invertResult)
                {
                    return !string.IsNullOrEmpty(stringValue) ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return string.IsNullOrEmpty(stringValue) ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}