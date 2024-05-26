using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.SharedKernel.Entities;

namespace VndbApiDomain.ProducerAggregate
{
    public class Producer : IAggregateRoot
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lang")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnum>))]
        public LanguageEnum Language { get; set; }

        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<ProducerTypeEnum>))]
        public ProducerTypeEnum Type { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
