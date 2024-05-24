using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.TraitAggregate;
using VndbApi.Domain.SharedKernel;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.CharacterAggregate
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
