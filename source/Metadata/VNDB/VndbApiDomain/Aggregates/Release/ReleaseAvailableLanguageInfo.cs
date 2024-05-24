using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.ReleaseAggregate
{
    public partial class ReleaseAvailableLanguageInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("main")]
        public bool Main { get; set; }

        [JsonProperty("mtl")]
        public bool MachineTranslated { get; set; }

        [JsonProperty("lang")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnum>))]
        public LanguageEnum Language { get; set; }

        [JsonProperty("latin")]
        public string Latin { get; set; }
    }
}