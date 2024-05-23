using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Converters;
using VNDBFuze.VndbDomain.Common.Entities;

namespace VNDBFuze.VndbDomain.Aggregates.TraitAggregate
{
    public class Trait
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("char_count")]
        public long CharCount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("searchable")]
        public bool Searchable { get; set; }

        [JsonProperty("applicable")]
        public bool Applicable { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("group_name")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<TraitGroupEnum>))]
        public TraitGroupEnum Group { get; set; }

        [JsonProperty("group_id")]
        public string GroupId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}