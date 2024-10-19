using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
    internal class SingleValueArrayConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                // Skip the entire object
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndObject)
                    {
                        break;
                    }
                }

                return new T[] { };
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<T[]>(reader);
            }

            return new T[] { serializer.Deserialize<T>(reader) };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<T>);
        }
    }
}