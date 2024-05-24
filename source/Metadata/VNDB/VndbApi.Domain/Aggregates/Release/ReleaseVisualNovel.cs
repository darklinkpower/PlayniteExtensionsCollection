using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.VisualNovelAggregate;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.ReleaseAggregate
{
    public class ReleaseVisualNovel : VisualNovel
    {
        [JsonProperty("rtype")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<ReleaseTypeEnum>))]
        public ReleaseTypeEnum ReleaseType { get; set; }
    }
}