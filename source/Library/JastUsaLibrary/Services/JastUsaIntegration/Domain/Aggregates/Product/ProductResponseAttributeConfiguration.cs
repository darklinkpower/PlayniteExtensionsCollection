using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
    public class ProductResponseAttributeConfiguration
    {
        [JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(AttributeConfigurationConverter))]
        public Dictionary<string, Dictionary<Locale, string>> Choices { get; set; }

        [SerializationPropertyName("multiple")]
        public bool Multiple { get; set; }

        [SerializationPropertyName("min")]
        public int? Min { get; set; }

        [SerializationPropertyName("max")]
        public int? Max { get; set; }
    }
}