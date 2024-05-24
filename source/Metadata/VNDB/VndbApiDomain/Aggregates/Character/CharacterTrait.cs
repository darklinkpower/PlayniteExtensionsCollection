using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.TraitAggregate;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.CharacterAggregate
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
