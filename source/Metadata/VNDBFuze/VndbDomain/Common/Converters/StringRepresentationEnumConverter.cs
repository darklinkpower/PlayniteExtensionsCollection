using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Common.Converters
{
    public class StringRepresentationEnumConverter<TEnum> : JsonConverter where TEnum : Enum
    {
        private static readonly ConcurrentDictionary<TEnum, string> _enumToStringMap = new ConcurrentDictionary<TEnum, string>();
        private static readonly ConcurrentDictionary<string, TEnum> _stringToEnumMap = new ConcurrentDictionary<string, TEnum>();
        private const string _stringKeyForNull = "StringValueUsedForNullKeys";

        static StringRepresentationEnumConverter()
        {
            var enumType = typeof(TEnum);
            var enumFields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in enumFields)
            {
                var attribute = field.GetCustomAttribute<StringRepresentationAttribute>();
                var enumValue = (TEnum)field.GetValue(null);

                if (attribute != null)
                {
                    _enumToStringMap[enumValue] = attribute.Value;
                    if (string.IsNullOrEmpty(attribute.Value))
                    {
                        _stringToEnumMap[_stringKeyForNull] = enumValue;
                    }
                    else
                    {
                        _stringToEnumMap[attribute.Value] = enumValue;
                    }
                }
                else
                {
                    _enumToStringMap[enumValue] = enumValue.ToString();
                    _stringToEnumMap[enumValue.ToString()] = enumValue;
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TEnum);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is TEnum enumValue)
            {
                if (_enumToStringMap.TryGetValue(enumValue, out var stringValue))
                {
                    writer.WriteValue(stringValue);
                }
                else
                {
                    throw new JsonSerializationException($"Unknown value for {typeof(TEnum).Name}: {enumValue}");
                }
            }
            else
            {
                throw new JsonSerializationException($"Expected {typeof(TEnum).Name} object value.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var stringValue = reader.Value?.ToString() ?? _stringKeyForNull;
            if (_stringToEnumMap.TryGetValue(stringValue, out var enumValue))
            {
                return enumValue;
            }

            return null;
        }
    }

}