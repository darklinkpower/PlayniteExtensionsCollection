using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Aggregates.ReleaseAggregate;
using VNDBFuze.VndbDomain.Aggregates.VnAggregate;
using VNDBFuze.VndbDomain.Common.Converters;
using VNDBFuze.VndbDomain.Common.Enums;

namespace VNDBFuze.VndbDomain.Aggregates.CharacterAggregate
{
    public class CharacterVn : Vn
    {
        [JsonProperty("role")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<CharacterRoleEnum>))]
        public CharacterRoleEnum Role { get; set; }

        [JsonProperty("spoiler")]
        public SpoilerLevelEnum Spoiler { get; set; }

        /// <summary>
        /// Object, usually null, specific release that this character appears in. 
        /// </summary>
        [JsonProperty("release")]
        public Release Release { get; set; }
    }
}