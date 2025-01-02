using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
    internal class ConfigurationClassConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProductResponseAttributeConfiguration);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Skip();
                return new ProductResponseAttributeConfiguration();
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                return serializer.Deserialize<ProductResponseAttributeConfiguration>(reader);
            }

            throw new Exception("Cannot unmarshal type ConfigurationClass");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}