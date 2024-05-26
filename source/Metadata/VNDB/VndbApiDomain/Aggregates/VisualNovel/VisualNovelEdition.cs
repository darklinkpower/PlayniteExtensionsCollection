using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.VisualNovelAggregate
{
    public class VisualNovelEdition
    {
        [JsonProperty("lang", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnum>))]
        public LanguageEnum Lang { get; set; }

        [JsonProperty("eid")]
        public int Eid { get; set; }

        [JsonProperty("official")]
        public bool Official { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
