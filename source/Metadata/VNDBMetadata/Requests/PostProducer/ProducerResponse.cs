using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.DeserializationCommonConverters;
using VNDBMetadata.VNDB.Enums;

namespace VNDBMetadata.Requests.PostProducer
{
    public class ProducerResponse
    {
        [JsonProperty("more")]
        public bool More { get; set; }

        [JsonProperty("results")]
        public Producer[] Results { get; set; }
    }

    public class Producer
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lang")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnums>))]
        public LanguageEnums Lang { get; set; }

        [JsonProperty("aliases")]
        public string[] Aliases { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<ProducerTypeEnum>))]
        public ProducerTypeEnum Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}