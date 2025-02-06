using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Infrastructure.DTOs
{
    public class GetGamesResponse
    {
        [JsonProperty("@context")]
        public string Context { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("products")]
        public Product[] Products { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }
    }

    public class Product
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public ProductType Type { get; set; }

        [JsonProperty("variants")]
        public Variant[] Variants { get; set; }

        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }

    public class Variant
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public HydraMemberType Type { get; set; }

        [JsonProperty("game")]
        public Game Game { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("productVariantName")]
        public string ProductVariantName { get; set; }

        [JsonProperty("productImage")]
        public string ProductImage { get; set; }

        [JsonProperty("productImageBackground")]
        public string ProductImageBackground { get; set; }

        [JsonProperty("platforms")]
        public Platform[] Platforms { get; set; }

        [JsonProperty("productCode")]
        public string ProductCode { get; set; }

        [JsonProperty("gameId")]
        public int GameId { get; set; }

        [JsonProperty("userGameTags")]
        public List<UserGameTag> UserGameTags { get; set; }

        [JsonProperty("hasPatches_en_US")]
        public bool HasPatchesEnUs { get; set; }

        [JsonProperty("hasPatches_zh_Hans")]
        public bool HasPatchesZhHans { get; set; }

        [JsonProperty("hasPatches_zh_Hant")]
        public bool HasPatchesZhHant { get; set; }
    }

    public class Game
    {
        [JsonProperty("@id")]
        public string ApiRoute { get; set; } // /api/v2/shop/games/79

        [JsonProperty("@type")]
        public GameType Type { get; set; }

        [JsonProperty("translations")]
        public Translations Translations { get; set; }
    }

    public class Translations
    {
        [JsonProperty("en_US", NullValueHandling = NullValueHandling.Ignore)]
        public Translation EnUs { get; set; }

        [JsonProperty("ja", NullValueHandling = NullValueHandling.Ignore)]
        public Translation Ja { get; set; }

        [JsonProperty("zh_Hans", NullValueHandling = NullValueHandling.Ignore)]
        public Translation ZhHans { get; set; }

        [JsonProperty("zh_Hant", NullValueHandling = NullValueHandling.Ignore)]
        public Translation ZhHant { get; set; }
    }

    public class Translation
    {
        [JsonProperty("@id")]
        public string ApiRoute { get; set; } ///api/v2/shop/account/game-translations/348

        [JsonProperty("@type")]
        public TranslationType Type { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("locale")]
        public Locale Locale { get; set; }
    }

    public class Platform
    {
        [JsonProperty("en_US", NullValueHandling = NullValueHandling.Ignore)]
        public JastPlatforms EnUs { get; set; }

        [JsonProperty("zh_Hans", NullValueHandling = NullValueHandling.Ignore)]
        public JastPlatforms ZhHans { get; set; }

        [JsonProperty("zh_Hant", NullValueHandling = NullValueHandling.Ignore)]
        public JastPlatforms ZhHant { get; set; }
    }

    public class UserGameTag
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public UserGameTagType Type { get; set; }

        [JsonProperty("id")]
        public long UserGameTagId { get; set; }

        [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
        public Locale? Locale { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string UserGameTagType { get; set; }
    }

    public enum Locale
    {
        En_Us,
        ja,
        Zh_Hans,
        Zh_Hant
    };

    public enum TranslationType { GameLinkTranslation };

    public enum GameType { Game };

    public enum JastPlatforms { Linux, Mac, Windows };

    public enum HydraMemberType { ProductVariant };

    public enum ProductType { Product };

    public enum UserGameTagType { UserGameTag };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                HydraMemberTypeConverter.Singleton,
                GameTypeConverter.Singleton,
                EnUsTypeConverter.Singleton,
                LocaleConverter.Singleton,
                EnUsEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class HydraMemberTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(HydraMemberType) || t == typeof(HydraMemberType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "ProductVariant")
            {
                return HydraMemberType.ProductVariant;
            }
            throw new Exception("Cannot unmarshal type HydraMemberType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (HydraMemberType)untypedValue;
            if (value == HydraMemberType.ProductVariant)
            {
                serializer.Serialize(writer, "ProductVariant");
                return;
            }
            throw new Exception("Cannot marshal type HydraMemberType");
        }

        public static readonly HydraMemberTypeConverter Singleton = new HydraMemberTypeConverter();
    }

    internal class GameTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(GameType) || t == typeof(GameType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "Game")
            {
                return GameType.Game;
            }
            throw new Exception("Cannot unmarshal type GameType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (GameType)untypedValue;
            if (value == GameType.Game)
            {
                serializer.Serialize(writer, "Game");
                return;
            }
            throw new Exception("Cannot marshal type GameType");
        }

        public static readonly GameTypeConverter Singleton = new GameTypeConverter();
    }

    internal class EnUsTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TranslationType) || t == typeof(TranslationType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "GameLinkTranslation")
            {
                return TranslationType.GameLinkTranslation;
            }
            throw new Exception("Cannot unmarshal type EnUsType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TranslationType)untypedValue;
            if (value == TranslationType.GameLinkTranslation)
            {
                serializer.Serialize(writer, "GameLinkTranslation");
                return;
            }
            throw new Exception("Cannot marshal type EnUsType");
        }

        public static readonly EnUsTypeConverter Singleton = new EnUsTypeConverter();
    }

    internal class LocaleConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Locale) || t == typeof(Locale?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "en_US":
                    return Locale.En_Us;
                case "ja":
                    return Locale.ja;
                case "zh_Hans":
                    return Locale.Zh_Hans;
                case "zh_Hant":
                    return Locale.Zh_Hant;
            }
            throw new Exception("Cannot unmarshal type Locale");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Locale)untypedValue;
            switch (value)
            {
                case Locale.En_Us:
                    serializer.Serialize(writer, "en_US");
                    return;
                case Locale.ja:
                    serializer.Serialize(writer, "ja");
                    return;
                case Locale.Zh_Hans:
                    serializer.Serialize(writer, "zh_Hans");
                    return;
                case Locale.Zh_Hant:
                    serializer.Serialize(writer, "zh_Hant");
                    return;
            }
            throw new Exception("Cannot marshal type Locale");
        }

        public static readonly LocaleConverter Singleton = new LocaleConverter();
    }

    internal class EnUsEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(JastPlatforms) || t == typeof(JastPlatforms?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Linux":
                    return JastPlatforms.Linux;
                case "Mac":
                    return JastPlatforms.Mac;
                case "Windows":
                    return JastPlatforms.Windows;
            }
            throw new Exception("Cannot unmarshal type EnUsEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (JastPlatforms)untypedValue;
            switch (value)
            {
                case JastPlatforms.Linux:
                    serializer.Serialize(writer, "Linux");
                    return;
                case JastPlatforms.Mac:
                    serializer.Serialize(writer, "Mac");
                    return;
                case JastPlatforms.Windows:
                    serializer.Serialize(writer, "Windows");
                    return;
            }
            throw new Exception("Cannot marshal type EnUsEnum");
        }

        public static readonly EnUsEnumConverter Singleton = new EnUsEnumConverter();
    }
}