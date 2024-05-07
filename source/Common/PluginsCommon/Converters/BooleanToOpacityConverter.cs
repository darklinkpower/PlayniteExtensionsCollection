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
    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double falseOpacity = 0.5;
            if (parameter != null && double.TryParse(parameter.ToString(), out double paramOpacity))
            {
                falseOpacity = paramOpacity;
            }

            if (value is bool boolValue)
            {
                return boolValue ? 1.0 : falseOpacity;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}