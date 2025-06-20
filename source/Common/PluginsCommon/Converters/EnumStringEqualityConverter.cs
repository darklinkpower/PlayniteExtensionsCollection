using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PluginsCommon.Converters
{
    public class EnumStringEqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            string enumParam = parameter.ToString();
            if (value.GetType().IsEnum && Enum.IsDefined(value.GetType(), enumParam))
            {
                var paramValue = Enum.Parse(value.GetType(), enumParam);
                return value.Equals(paramValue);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = value is bool b && b;
            if (!isChecked || parameter == null)
            {
                return Binding.DoNothing;
            }

            string enumParam = parameter.ToString();
            return Enum.Parse(targetType, enumParam);
        }
    }
}
