using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Converters;
using VNDBFuze.VndbDomain.Common.Enums;

namespace VNDBFuze.VndbDomain.Aggregates.ReleaseAggregate
{
    public class ReleaseMedia
    {
        [JsonProperty("medium")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<MediumEnum>))]
        public MediumEnum Medium { get; set; }

        [JsonProperty("qty")]
        public long Qty { get; set; }
    }
}
