using Newtonsoft.Json;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Aggregates.ImageAggregate;
using VNDBFuze.VndbDomain.Aggregates.ProducerAggregate;
using VNDBFuze.VndbDomain.Common.Converters;
using VNDBFuze.VndbDomain.Common.Entities;
using VNDBFuze.VndbDomain.Common.Enums;
using VNDBFuze.VndbDomain.Common.Models;

namespace VNDBFuze.VndbDomain.Aggregates.ReleaseAggregate
{
    public class Release
    {
        [JsonProperty("alttitle")]
        public string AlternativeTitle { get; set; }

        [JsonProperty("extlinks")]
        public List<Extlink<Release>> Extlinks { get; set; }

        [JsonProperty("languages")]
        public List<ReleaseAvailableLanguageInfo> LanguagesAvailability { get; set; }

        [JsonProperty("vns")]
        public List<ReleaseVn> RelatedVisualNovels { get; set; }

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
        public long? Voiced { get; set; }

        [JsonProperty("catalog")]
        public string CatalogCode { get; set; }

        [JsonProperty("patch")]
        public bool Patch { get; set; }

        [JsonProperty("has_ero")]
        public bool HasEro { get; set; }

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
        public VndbReleaseDate ReleaseDate { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }


}