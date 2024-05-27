using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PluginsCommon.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        protected static ConcurrentDictionary<Type, Dictionary<Enum, string>> _enumStringMappers = new ConcurrentDictionary<Type, Dictionary<Enum, string>>();
        public EnumToStringConverter()
        {

        }

        public static void AddStringMapDictionary<TEnum>(Dictionary<Enum, string> stringMap) where TEnum : Enum
        {
            _enumStringMappers[typeof(TEnum)] = stringMap;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }

            var enumType = value.GetType();
            if (!enumType.IsEnum)
            {
                return null;
            }

            if (_enumStringMappers.TryGetValue(enumType, out var enumStringMapper))
            {
                var enumValue = (Enum)value;
                if (enumStringMapper.TryGetValue(enumValue, out var str))
                {
                    return str;
                }
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}