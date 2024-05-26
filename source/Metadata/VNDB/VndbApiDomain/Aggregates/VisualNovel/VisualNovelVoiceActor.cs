using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.StaffAggregate;

namespace VndbApiDomain.VisualNovelAggregate
{
    public class VisualNovelVoiceActor
    {
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("character")]
        public Character Character { get; set; }

        [JsonProperty("staff")]
        public Staff Staff { get; set; }

        public override string ToString()
        {
            return $"Voice actor: {Staff}, Character: {Character}";
        }
    }
}