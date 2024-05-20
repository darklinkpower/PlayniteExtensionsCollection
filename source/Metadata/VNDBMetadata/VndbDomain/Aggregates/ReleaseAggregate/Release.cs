using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Entities;
using VNDBMetadata.VndbDomain.Common.Enums;

namespace VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate
{
    public enum Platformw { Mac, Win };

    public class Release
    {
        [JsonProperty("alttitle")]
        public string AlternativeTitle { get; set; }

        [JsonProperty("extlinks")]
        public List<Extlink<Release>> Extlinks { get; set; }

        [JsonProperty("languages")]
        public List<ReleaseAvailableLanguageInfo> LanguagesAvailability { get; set; }

        [JsonProperty("vns")]
        public List<ReleaseVn> Vns { get; set; }

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
        [JsonConverter(typeof(ReleaseResolutionConverter))]
        public ReleaseResolution Resolution { get; set; }

        [JsonProperty("freeware")]
        public bool Freeware { get; set; }

        [JsonProperty("engine")]
        public string Engine { get; set; }

        [JsonProperty("minage")]
        public long MinimumAge { get; set; }

        [JsonProperty("uncensored")]
        public bool? Uncensored { get; set; }

        [JsonProperty("gtin")]
        public string Gtin { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("official")]
        public bool Official { get; set; }

        [JsonProperty("producers")]
        public List<Producer> Producers { get; set; }

        [JsonProperty("released")]
        public DateTimeOffset Released { get; set; }
    }

    public class ReleaseResolutionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                ReleaseResolution resolution = (ReleaseResolution)value;
                writer.WriteStartArray();
                writer.WriteValue(resolution.Width);
                writer.WriteValue(resolution.Height);
                writer.WriteEndArray();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray array = JArray.Load(reader);
                if (array.Count == 2 && array[0].Type == JTokenType.Integer && array[1].Type == JTokenType.Integer)
                {
                    var width = array[0].ToObject<int>();
                    var height = array[1].ToObject<int>();
                    return new ReleaseResolution { Width = width, Height = height };
                }
            }

            throw new JsonSerializationException("Unexpected token or value when parsing release resolution.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ReleaseResolution);
        }
    }


}
