using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiDomain.SharedKernel
{
    public class StringRepresentationEnumListConverter<TEnum> : JsonConverter where TEnum : Enum
    {
        private static readonly StringRepresentationEnumConverter<TEnum> _enumConverter = new StringRepresentationEnumConverter<TEnum>();
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<TEnum>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (List<TEnum>)value;
            writer.WriteStartArray();
            foreach (var item in list)
            {
                _enumConverter.WriteJson(writer, item, serializer);
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var list = new List<TEnum>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }

                var enumValue = (TEnum)_enumConverter.ReadJson(reader, typeof(TEnum), null, serializer);
                list.Add(enumValue);
            }

            return list;
        }
    }


}