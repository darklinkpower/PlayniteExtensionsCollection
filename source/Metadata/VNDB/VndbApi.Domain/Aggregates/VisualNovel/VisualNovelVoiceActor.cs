using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.CharacterAggregate;
using VndbApi.Domain.StaffAggregate;

namespace VndbApi.Domain.VisualNovelAggregate
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
