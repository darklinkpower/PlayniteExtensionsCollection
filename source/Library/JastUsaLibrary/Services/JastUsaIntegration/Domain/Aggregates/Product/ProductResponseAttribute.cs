using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
    public class ProductResponseAttribute
    {
        [SerializationPropertyName("@type")]
        public AttributeType Type { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("localeCode")]
        public Locale LocaleCode { get; set; }

        [SerializationPropertyName("type")]
        public TypeEnum AttributeType { get; set; }

        [SerializationPropertyName("code")]
        public string Code { get; set; }

        // This property is sometimes a string, array of strings or property so we have to fix it
        [SerializationPropertyName("value"), JsonConverter(typeof(SingleValueArrayConverter<string>))]
        public string[] Value { get; set; }

        [SerializationPropertyName("attribute_id")]
        public int AttributeId { get; set; }

        [JsonProperty("configuration", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(ConfigurationClassConverter))]
        public ProductResponseAttributeConfiguration Configuration { get; set; }
    }
}