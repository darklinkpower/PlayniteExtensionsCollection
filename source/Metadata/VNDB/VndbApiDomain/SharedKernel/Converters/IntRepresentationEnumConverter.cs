using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiDomain.SharedKernel
{
    public class IntRepresentationEnumConverter<TEnum> : JsonConverter where TEnum : Enum
    {
        private static readonly ConcurrentDictionary<TEnum, int> _enumToIntMap = new ConcurrentDictionary<TEnum, int>();
        private static readonly ConcurrentDictionary<int, TEnum> _intToEnumMap = new ConcurrentDictionary<int, TEnum>();

        static IntRepresentationEnumConverter()
        {
            var enumType = typeof(TEnum);
            var enumFields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in enumFields)
            {
                var attribute = field.GetCustomAttribute<IntRepresentationAttribute>();
                var enumValue = (TEnum)field.GetValue(null);

                if (attribute != null)
                {
                    _enumToIntMap[enumValue] = attribute.Value;
                    _intToEnumMap[attribute.Value] = enumValue;
                }
                else
                {
                    var intValue = Convert.ToInt32(enumValue);
                    _enumToIntMap[enumValue] = intValue;
                    _intToEnumMap[intValue] = enumValue;
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
                if (_enumToIntMap.TryGetValue(enumValue, out var intValue))
                {
                    writer.WriteValue(intValue);
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
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            
            if (reader.TokenType != JsonToken.Integer)
            {
                throw new JsonSerializationException($"Unexpected token parsing {typeof(TEnum).Name}. Expected Integer, got {reader.TokenType}.");
            }

            var intValue = Convert.ToInt32(reader.Value);
            if (_intToEnumMap.TryGetValue(intValue, out var enumValue))
            {
                return enumValue;
            }

            throw new JsonSerializationException($"Unknown integer value for {typeof(TEnum).Name}: {intValue}");
        }
    }

}