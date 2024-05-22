using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Aggregates.VnAggregate;
using VNDBFuze.VndbDomain.Common.Converters;

namespace VNDBFuze.VndbDomain.Aggregates.ReleaseAggregate
{
    public class ReleaseVn : Vn
    {
        [JsonProperty("rtype")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<ReleaseTypeEnum>))]
        public ReleaseTypeEnum ReleaseType { get; set; }
    }
}