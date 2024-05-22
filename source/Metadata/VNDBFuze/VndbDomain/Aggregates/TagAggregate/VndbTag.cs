using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Converters;

namespace VNDBFuze.VndbDomain.Aggregates.TagAggregate
{
    public class VndbTag
    {
        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vn_count")]
        public uint VnCount { get; set; }

        [JsonProperty("category")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<TagCategoryEnum>))]
        public TagCategoryEnum Category { get; set; }

        [JsonProperty("searchable")]
        public bool Searchable { get; set; }

        [JsonProperty("applicable")]
        public bool Applicable { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
