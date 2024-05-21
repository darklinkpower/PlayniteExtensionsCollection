using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.TagAggregate;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Enums;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    public class VnVndbTag : VndbTag
    {
        [JsonProperty("lie")]
        public bool Lie { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("spoiler")]
        [JsonConverter(typeof(IntRepresentationEnumConverter<SpoilerLevelEnum>))]
        public SpoilerLevelEnum Spoiler { get; set; }
    }
}