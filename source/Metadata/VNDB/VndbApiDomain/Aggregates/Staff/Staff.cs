using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.SharedKernel.Entities;

namespace VndbApiDomain.StaffAggregate
{
    public class Staff : IAggregateRoot
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lang")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnum>))]
        public LanguageEnum Language { get; set; }

        [JsonProperty("ismain")]
        public bool IsMain { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("gender")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<StaffGenderEnum>))]
        public StaffGenderEnum? Gender { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("extlinks")]
        public List<ExternalLink<Staff>> ExternalLinks { get; set; }

        [JsonProperty("aliases")]
        public List<Alias<Staff>> Aliases { get; set; }

        [JsonProperty("aid")]
        public long Aid { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}