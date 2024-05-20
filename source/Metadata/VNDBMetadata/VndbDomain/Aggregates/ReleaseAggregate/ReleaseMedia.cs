using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Enums;

namespace VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate
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
