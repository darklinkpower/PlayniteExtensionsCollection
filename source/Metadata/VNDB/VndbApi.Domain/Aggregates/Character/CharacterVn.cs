using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.ReleaseAggregate;
using VndbApi.Domain.VisualNovelAggregate;
using VndbApi.Domain.SharedKernel;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.CharacterAggregate
{
    public class CharacterVn : VisualNovel
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