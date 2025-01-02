using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
    public class AttributeConfigurationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, Dictionary<Locale, string>>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<string, Dictionary<Locale, string>>();
            var jsonObject = JObject.Load(reader);
            foreach (var property in jsonObject.Properties())
            {
                var currentKey = property.Name;
                var innerToken = property.Value;
                if (innerToken.Type == JTokenType.Object)
                {
                    var innerDictionary = innerToken.ToObject<Dictionary<string, string>>();
                    var localeDictionary = new Dictionary<Locale, string>();
                    foreach (var innerProperty in innerDictionary)
                    {
                        if (Enum.TryParse<Locale>(innerProperty.Key, true, out var locale))
                        {
                            localeDictionary[locale] = innerProperty.Value;
                        }
                    }

                    result[currentKey] = localeDictionary;
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}