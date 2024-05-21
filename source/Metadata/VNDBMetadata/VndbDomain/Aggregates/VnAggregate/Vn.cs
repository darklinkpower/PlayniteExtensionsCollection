using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ImageAggregate;
using VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Enums;
using VNDBMetadata.VndbDomain.Common.Models;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    public class Vn
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("developers")]
        public List<Producer> Developers { get; set; }

        [JsonProperty("screenshots")]
        public List<VndbImage> Screenshots { get; set; }

        [JsonProperty("length_minutes")]
        public int? LengthMinutes { get; set; }

        [JsonProperty("olang")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnum>))]
        public LanguageEnum OriginalLanguage { get; set; }

        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("devstatus")]
        [JsonConverter(typeof(IntRepresentationEnumConverter<VnDevelopmentStatusEnum>))]
        public VnDevelopmentStatusEnum DevelopmentStatus { get; set; }

        [JsonProperty("relations")]
        public List<VnRelation> Relations { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("length")]
        public int? Length { get; set; }

        [JsonProperty("image")]
        public VndbImage Image { get; set; }

        [JsonProperty("alttitle")]
        public string AlternativeTitle { get; set; }

        [JsonProperty("editions")]
        public List<VnEdition> Editions { get; set; }

        [JsonProperty("tags")]
        public List<VnVndbTag> Tags { get; set; }

        [JsonProperty("released")]
        [JsonConverter(typeof(VndbReleaseDateJsonConverter))]
        public VndbReleaseDate ReleaseDate { get; set; }

        [JsonProperty("platforms")]
        [JsonConverter(typeof(StringRepresentationEnumListConverter<PlatformEnum>))]
        public List<PlatformEnum> Platforms { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("staff")]
        public List<VnStaff> Staff { get; set; }

        [JsonProperty("votecount")]
        public int VoteCount { get; set; }

        [JsonProperty("languages")]
        [JsonConverter(typeof(StringRepresentationEnumListConverter<LanguageEnum>))]
        public List<LanguageEnum> Languages { get; set; }

        [JsonProperty("length_votes")]
        public int LengthVotes { get; set; }

        [JsonProperty("va")]
        public List<VnVoiceActor> VoiceActors { get; set; }

        [JsonProperty("titles")]
        public List<VnTitle> Titles { get; set; }

        [JsonProperty("rating")]
        public double? Rating { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}