using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate;
using VNDBMetadata.VndbDomain.Aggregates.VnAggregate;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Enums;

namespace VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate
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