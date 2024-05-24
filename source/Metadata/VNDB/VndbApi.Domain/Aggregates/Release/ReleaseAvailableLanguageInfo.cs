using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.ReleaseAggregate
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