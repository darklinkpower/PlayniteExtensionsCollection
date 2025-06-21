using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PluginsCommon.Converters
{
    public class IntSubstitutionConverter : IValueConverter
    {
        private static readonly Dictionary<string, Dictionary<int, string>> _parsedCache = new Dictionary<string, Dictionary<int, string>>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is int intValue))
            {
                return value;
            }

            if (!(parameter is string paramStr) || string.IsNullOrWhiteSpace(paramStr))
            {
                return intValue.ToString();
            }

            if (!_parsedCache.TryGetValue(paramStr, out var map))
            {
                map = new Dictionary<int, string>();

                var pairs = paramStr.Split(';');
                foreach (string pair in pairs)
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2)
                    {
                        var keyPart = parts[0].Trim();
                        var valuePart = parts[1].Trim();

                        if (int.TryParse(keyPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var key))
                        {
                            map[key] = valuePart;
                        }
                    }
                }

                _parsedCache[paramStr] = map;
            }

            if (map.TryGetValue(intValue, out var replacement))
            {
                return replacement;
            }

            return intValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
