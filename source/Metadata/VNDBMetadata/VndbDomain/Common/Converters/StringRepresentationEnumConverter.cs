using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;

namespace VNDBMetadata.VndbDomain.Common.Converters
{
    public class StringRepresentationEnumConverter<TEnum> : JsonConverter where TEnum : Enum
    {
        private static readonly ConcurrentDictionary<TEnum, string> EnumToStringMap = new ConcurrentDictionary<TEnum, string>();
        private static readonly ConcurrentDictionary<string, TEnum> StringToEnumMap = new ConcurrentDictionary<string, TEnum>();

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
                    EnumToStringMap[enumValue] = attribute.Value;
                    StringToEnumMap[attribute.Value] = enumValue;
                }
                else
                {
                    EnumToStringMap[enumValue] = enumValue.ToString();
                    StringToEnumMap[enumValue.ToString()] = enumValue;
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
                if (EnumToStringMap.TryGetValue(enumValue, out var stringValue))
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
            var stringValue = reader.Value?.ToString();
            if (string.IsNullOrEmpty(stringValue))
            {
                throw new JsonSerializationException($"Invalid value for {typeof(TEnum).Name}.");
            }

            if (StringToEnumMap.TryGetValue(stringValue, out var enumValue))
            {
                return enumValue;
            }

            throw new JsonSerializationException($"Unknown value: {stringValue}");
        }
    }

}