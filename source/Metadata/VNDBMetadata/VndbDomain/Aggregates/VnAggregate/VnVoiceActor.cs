using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate;
using VNDBMetadata.VndbDomain.Aggregates.StaffAggregate;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    public class VnVoiceActor
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
