using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.SharedKernel.Entities;
using VndbApiDomain.TagAggregate;

namespace VndbApiInfrastructure.DatabaseDumpTagAggregate
{
    public class DatabaseDumpTag
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("applicable")]
        public bool Applicable { get; set; }

        [JsonProperty("category")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<TagCategoryEnum>))]
        public TagCategoryEnum Category { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("meta")]
        public bool Meta { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parents")]
        public List<int> Parents { get; set; }

        [JsonProperty("searchable")]
        public bool Searchable { get; set; }

        [JsonProperty("vns")]
        public int Vns { get; set; }
        public override string ToString()
        {
            return Name;
        }

    }
}