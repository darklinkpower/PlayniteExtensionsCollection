using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.VisualNovelAggregate;

namespace VndbApiDomain.ReleaseAggregate
{
    public class Release
    {
        [JsonProperty("alttitle")]
        public string AlternativeTitle { get; set; }

        [JsonProperty("extlinks")]
        public List<ExternalLink<Release>> ExternalLinks { get; set; }

        [JsonProperty("languages")]
        public List<ReleaseAvailableLanguageInfo> LanguagesAvailability { get; set; }

        [JsonProperty("vns")]
        public List<ReleaseVisualNovel> RelatedVisualNovels { get; set; }

        [JsonProperty("media")]
        public List<ReleaseMedia> Media { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("platforms")]
        [JsonConverter(typeof(StringRepresentationEnumListConverter<PlatformEnum>))]
        public List<PlatformEnum> Platforms { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("voiced")]
        [JsonConverter(typeof(IntRepresentationEnumConverter<ReleaseVoicedEnum>))]
        public ReleaseVoicedEnum? Voiced { get; set; }

        [JsonProperty("catalog")]
        public string CatalogCode { get; set; }

        [JsonProperty("patch")]
        public bool Patch { get; set; }

        [JsonProperty("has_ero")]
        public bool HasErotic { get; set; }

        [JsonProperty("resolution")]
        [JsonConverter(typeof(ImageResolutionConverter))]
        public ImageDimensions Resolution { get; set; }

        [JsonProperty("freeware")]
        public bool Freeware { get; set; }

        [JsonProperty("engine")]
        public string Engine { get; set; }

        [JsonProperty("minage")]
        public int? MinimumAge { get; set; }

        [JsonProperty("uncensored")]
        public bool? Uncensored { get; set; }

        [JsonProperty("gtin")]
        public string Gtin { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("official")]
        public bool Official { get; set; }

        [JsonProperty("producers")]
        public List<ReleaseProducer> Producers { get; set; }

        [JsonProperty("released")]
        [JsonConverter(typeof(VndbReleaseDateJsonConverter))]
        public VisualNovelReleaseDate ReleaseDate { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }


}