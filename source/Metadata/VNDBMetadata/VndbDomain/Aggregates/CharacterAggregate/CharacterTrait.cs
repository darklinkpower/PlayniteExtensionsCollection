using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.TraitAggregate;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Enums;

namespace VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate
{
    public class CharacterTrait : Trait
    {
        [JsonProperty("spoiler")]
        [JsonConverter(typeof(IntRepresentationEnumConverter<SpoilerLevelEnum>))]
        public SpoilerLevelEnum SpoilerLevel { get; set; }

        [JsonProperty("lie")]
        public bool Lie { get; set; }
    }
}
