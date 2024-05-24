using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.ProducerAggregate
{
    public class Producer
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lang")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnum>))]
        public LanguageEnum Lang { get; set; }

        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<ProducerTypeEnum>))]
        public ProducerTypeEnum Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
