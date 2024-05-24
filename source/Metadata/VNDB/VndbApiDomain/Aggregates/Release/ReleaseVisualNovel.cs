using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.VisualNovelAggregate;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.ReleaseAggregate
{
    public class ReleaseVisualNovel : VisualNovel
    {
        [JsonProperty("rtype")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<ReleaseTypeEnum>))]
        public ReleaseTypeEnum ReleaseType { get; set; }
    }
}