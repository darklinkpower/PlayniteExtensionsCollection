using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.ProducerAggregate;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.VisualNovelAggregate
{
    public class VisualNovel
    {
        /// <summary>
        /// Array of strings, list of aliases.
        /// </summary>
        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        /// <summary>
        /// String, can be null. Alternative title, typically the same as title but in the original script.
        /// </summary>
        [JsonProperty("alttitle")]
        public string AlternativeTitle { get; set; }

        /// <summary>
        /// String, possibly null, may contain formatting codes.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// All producer fields can be used here.
        /// </summary>
        [JsonProperty("developers")]
        public List<Producer> Developers { get; set; }

        /// <summary>
        /// Integer, development status. 0 meaning ‘Finished’, 1 is ‘In development’ and 2 for ‘Cancelled’.
        /// </summary>
        [JsonProperty("devstatus")]
        [JsonConverter(typeof(IntRepresentationEnumConverter<VisualNovelDevelopmentStatusEnum>))]
        public VisualNovelDevelopmentStatusEnum DevelopmentStatus { get; set; }

        /// <summary>
        /// Array of objects, possibly empty.
        /// </summary>
        [JsonProperty("editions")]
        public List<VnEdition> Editions { get; set; }

        /// <summary>
        /// Array, links to external websites. This list is equivalent to the links displayed on the release pages on the site, so it may include redundant entries (e.g. if a Steam ID is known, links to both Steam and SteamDB are included) and links that are automatically fetched from external resources (e.g. PlayAsia, for which a GTIN lookup is performed).
        /// These extra sites are not listed in the extlinks list of the schema. 
        /// </summary>
        [JsonProperty("extlinks")]
        public List<ExternalLink<VisualNovel>> ExternalLinks { get; set; }

        /// <summary>
        /// Object, can be null. 
        /// </summary>
        [JsonProperty("image")]
        public VndbImage Image { get; set; }

        /// <summary>
        /// vndbid.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Array of strings, list of languages this VN is available in. Does not include machine translations.
        /// </summary>
        [JsonProperty("languages")]
        [JsonConverter(typeof(StringRepresentationEnumListConverter<LanguageEnum>))]
        public List<LanguageEnum> Languages { get; set; }

        /// <summary>
        /// Play time estimate, integer between 1 (Very short) and 5 (Very long). This filter uses the length votes average when available but falls back to the entries’ length field when there are no votes.
        /// </summary>
        [JsonConverter(typeof(IntRepresentationEnumConverter<VnLengthEnum>))]
        [JsonProperty("length")]
        public VnLengthEnum? Length { get; set; }

        /// <summary>
        /// Integer, possibly null, average of user-submitted play times in minutes.
        /// </summary>
        [JsonProperty("length_minutes")]
        public int? LengthMinutes { get; set; }

        /// <summary>
        /// Integer, number of submitted play times.
        /// </summary>
        [JsonProperty("length_votes")]
        public int LengthVotes { get; set; }

        /// <summary>
        ///      String, language the VN has originally been written in.
        /// </summary>
        [JsonProperty("olang")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<LanguageEnum>))]
        public LanguageEnum OriginalLanguage { get; set; }

        /// <summary>
        /// Match on available platforms.
        /// </summary>
        [JsonProperty("platforms")]
        [JsonConverter(typeof(StringRepresentationEnumListConverter<PlatformEnum>))]
        public List<PlatformEnum> Platforms { get; set; }

        /// <summary>
        /// Number between 10 and 100, null if nobody voted. 
        /// </summary>
        [JsonProperty("rating")]
        public double? Rating { get; set; }

        /// <summary>
        /// Release date.
        /// </summary>
        [JsonProperty("released")]
        [JsonConverter(typeof(VndbReleaseDateJsonConverter))]
        public VisualNovelReleaseDate ReleaseDate { get; set; }

        /// <summary>
        ///  Array of objects, list of VNs directly related to this entry.
        /// </summary>
        [JsonProperty("relations")]
        public List<VnRelation> Relations { get; set; }

        /// <summary>
        /// Array of objects, possibly empty.
        /// </summary>
        [JsonProperty("screenshots")]
        public List<VndbImage> Screenshots { get; set; }

        /// <summary>
        /// Array of objects, possibly empty.
        /// </summary>
        [JsonProperty("staff")]
        public List<VnStaff> Staff { get; set; }

        /// <summary>
        /// Tags applied to this VN, also matches parent tags.
        /// </summary>
        [JsonProperty("tags")]
        public List<VisualNovelTag> Tags { get; set; }

        /// <summary>
        /// String, main title as displayed on the site, typically romanized from the original script.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Array of objects, full list of titles associated with the VN, always contains at least one title.
        /// </summary>
        [JsonProperty("titles")]
        public List<VnTitle> Titles { get; set; }

        /// <summary>
        /// Integer, number of votes. 
        /// </summary>
        [JsonProperty("votecount")]
        public int VoteCount { get; set; }

        /// <summary>
        /// Array of objects, possibly empty. Each object represents a voice actor relation. The same voice actor may be listed multiple times for different characters and the same character may be listed multiple times if it has been voiced by several people.
        /// </summary>
        [JsonProperty("va")]
        public List<VisualNovelVoiceActor> VoiceActors { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }


}