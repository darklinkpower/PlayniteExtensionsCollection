using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Entities;
using VNDBMetadata.VndbDomain.Common.Enums;

namespace VNDBMetadata.VndbDomain.Aggregates.StaffAggregate
{
    public class Staff
    {
        [JsonProperty("lang")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnum>))]
        public LanguageEnum Lang { get; set; }

        [JsonProperty("ismain")]
        public bool IsMain { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("gender")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<StaffGenderEnum>))]
        public StaffGenderEnum Gender { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("extlinks")]
        public List<Extlink<Staff>> Extlinks { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("aliases")]
        public List<Alias<Staff>> Aliases { get; set; }

        [JsonProperty("aid")]
        public long Aid { get; set; }
    }
}