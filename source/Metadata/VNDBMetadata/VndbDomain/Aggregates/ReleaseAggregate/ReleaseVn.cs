using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.VnAggregate;
using VNDBMetadata.VndbDomain.Common.Converters;

namespace VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate
{
    public class ReleaseVn : Vn
    {
        [JsonProperty("rtype")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<ReleaseTypeEnum>))]
        public ReleaseTypeEnum Rtype { get; set; }
    }
}