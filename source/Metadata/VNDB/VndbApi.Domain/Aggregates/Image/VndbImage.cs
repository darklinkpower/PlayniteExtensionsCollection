using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.ReleaseAggregate;

namespace VndbApi.Domain.ImageAggregate
{
    public class VndbImage
    {
        [JsonIgnore]
        public ImageViolenceLevelEnum ViolenceLevel => GetViolenceLevel();
        [JsonIgnore]
        public ImageSexualityLevelEnum SexualityLevel => GetSexualityLevel();

        [JsonProperty("votecount")]
        public int Votecount { get; set; }

        [JsonProperty("sexual")]
        public double Sexual { get; set; }

        [JsonProperty("violence")]
        public double Violence { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("dims")]
        [JsonConverter(typeof(ImageResolutionConverter))]
        public ImageDimensions Dimensions { get; set; }

        [JsonProperty("thumbnail_dims")]
        [JsonConverter(typeof(ImageResolutionConverter))]
        public ImageDimensions ThumbnailDimensions { get; set; }

        [JsonProperty("thumbnail")]
        public Uri ThumbnailUrl { get; set; }

        [JsonProperty("release", NullValueHandling = NullValueHandling.Ignore)]
        public Release Release { get; set; }


        private ImageViolenceLevelEnum GetViolenceLevel()
        {
            if (Votecount == 0)
            {
                return ImageViolenceLevelEnum.Brutal;
            }

            if (Violence > 1.3)
            {
                return ImageViolenceLevelEnum.Brutal;
            }
            else if (Violence > 0.4)
            {
                return ImageViolenceLevelEnum.Violent;
            }

            return ImageViolenceLevelEnum.Tame;
        }

        private ImageSexualityLevelEnum GetSexualityLevel()
        {
            if (Votecount == 0)
            {
                return ImageSexualityLevelEnum.Explicit;
            }

            if (Sexual > 1.3)
            {
                return ImageSexualityLevelEnum.Explicit;
            }
            else if (Sexual > 0.4)
            {
                return ImageSexualityLevelEnum.Suggestive;
            }

            return ImageSexualityLevelEnum.Safe;
        }

        public override string ToString()
        {
            return Url.ToString();
        }
    }

    public class ImageResolutionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                ImageDimensions resolution = (ImageDimensions)value;
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
                    return new ImageDimensions { Width = width, Height = height };
                }
            }

            throw new JsonSerializationException("Unexpected token or value when parsing release resolution.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ImageDimensions);
        }
    }

}