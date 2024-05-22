using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Aggregates.TraitAggregate;
using VNDBFuze.VndbDomain.Common.Converters;
using VNDBFuze.VndbDomain.Common.Enums;

namespace VNDBFuze.VndbDomain.Aggregates.CharacterAggregate
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
