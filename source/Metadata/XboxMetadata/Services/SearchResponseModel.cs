using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Playnite.SDK.Data;

namespace XboxMetadata.Services
{
    public class SearchRequestBody
    {
        [SerializationPropertyName("Query")]
        public string Query { get; set; }

        [SerializationPropertyName("Filters")]
        public string Filters { get; set; }

        [SerializationPropertyName("ReturnFilters")]
        public bool ReturnFilters { get; set; }

        [SerializationPropertyName("ChannelKeyToBeUsedInResponse")]
        public string ChannelKeyToBeUsedInResponse { get; set; }
    }

    public class SearchResponse
    {
        //[SerializationPropertyName("channels")]
        //public Channels Channels { get; set; }
        [SerializationPropertyName("channels")]
        public Dictionary<string, ChannelResult> Channels { get; set; }

        [SerializationPropertyName("supportItems")]
        public SupportItems SupportItems { get; set; }

        [SerializationPropertyName("productSummaries")]
        public ProductSummary[] ProductSummaries { get; set; }

        [SerializationPropertyName("skuSummaries")]
        public SkuSummary[] SkuSummaries { get; set; }

        [SerializationPropertyName("availabilitySummaries")]
        public AvailabilitySummary[] AvailabilitySummaries { get; set; }

        [SerializationPropertyName("searchBucketFilerIdMap")]
        public SearchBucketFilerIdMap SearchBucketFilerIdMap { get; set; }
    }

    public class AvailabilitySummary
    {
        [SerializationPropertyName("actions")]
        public Action[] Actions { get; set; }

        [SerializationPropertyName("availabilityId")]
        public string AvailabilityId { get; set; }

        //[SerializationPropertyName("endDateUtc")]
        //public EndDateUtc EndDateUtc { get; set; }

        [SerializationPropertyName("productId")]
        public string ProductId { get; set; }

        [SerializationPropertyName("price")]
        public Price Price { get; set; }

        [SerializationPropertyName("skuId")]
        public string SkuId { get; set; }
    }

    public class Price
    {
        [SerializationPropertyName("msrp")]
        public double Msrp { get; set; }

        [SerializationPropertyName("listPrice")]
        public double ListPrice { get; set; }

        [SerializationPropertyName("recurrencePrice")]
        public object RecurrencePrice { get; set; }

        [SerializationPropertyName("eligibilityInfo")]
        public EligibilityInfo EligibilityInfo { get; set; }

        [SerializationPropertyName("currency")]
        public string Currency { get; set; }

        [SerializationPropertyName("xPriceOfferInfo")]
        public object XPriceOfferInfo { get; set; }
    }

    public class EligibilityInfo
    {
        [SerializationPropertyName("eligibility")]
        public Eligibility Eligibility { get; set; }

        [SerializationPropertyName("type")]
        public Action Type { get; set; }
    }

    //public class Channels
    //{
    //    [SerializationPropertyName("SEARCH_GAMES_SEARCHQUERY=RESIDENT-EVIL_")]
    //    public SearchGamesSearchqueryResidentEvil SearchGamesSearchqueryResidentEvil { get; set; }
    //}

    public class ChannelResult
    {
        [SerializationPropertyName("channelKey")]
        public string ChannelKey { get; set; }

        [SerializationPropertyName("products")]
        public Product[] Products { get; set; }

        [SerializationPropertyName("totalItems")]
        public long TotalItems { get; set; }

        [SerializationPropertyName("encodedCT")]
        public string EncodedCt { get; set; }

        [SerializationPropertyName("isFilterable")]
        public bool IsFilterable { get; set; }
    }

    public class Product
    {
        [SerializationPropertyName("productId")]
        public string ProductId { get; set; }
    }

    public class ProductSummary
    {
        [SerializationPropertyName("availableOn")]
        public List<AvailableOn> AvailableOn { get; set; }

        [SerializationPropertyName("averageRating")]
        public double? AverageRating { get; set; }

        [SerializationPropertyName("contentRating")]
        public ContentRating ContentRating { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("developerName")]
        public string DeveloperName { get; set; }

        [SerializationPropertyName("hasAddOns")]
        public bool HasAddOns { get; set; }

        [SerializationPropertyName("images")]
        public Images Images { get; set; }

        [SerializationPropertyName("includedWithPassesProductIds")]
        public string[] IncludedWithPassesProductIds { get; set; }

        [SerializationPropertyName("maxInstallSize")]
        public long MaxInstallSize { get; set; }

        //[SerializationPropertyName("optimalSkuId", NullValueHandling = NullValueHandling.Ignore)]
        //public string OptimalSkuId { get; set; }

        [SerializationPropertyName("preferredSkuId")]
        public string PreferredSkuId { get; set; }

        [SerializationPropertyName("productFamily")]
        public ProductFamily ProductFamily { get; set; }

        [SerializationPropertyName("productId")]
        public string ProductId { get; set; }

        [SerializationPropertyName("productKind")]
        public ProductKind ProductKind { get; set; }

        [SerializationPropertyName("publisherName")]
        public string PublisherName { get; set; }

        [SerializationPropertyName("ratingCount")]
        public int RatingCount { get; set; }

        [SerializationPropertyName("releaseDate")]
        public DateTimeOffset? ReleaseDate { get; set; } = null;

        [SerializationPropertyName("shortDescription")]
        public string ShortDescription { get; set; }

        [SerializationPropertyName("showSupportedLanguageDisclaimer")]
        public bool ShowSupportedLanguageDisclaimer { get; set; }

        [SerializationPropertyName("specificPrices")]
        public SpecificPrices SpecificPrices { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("videos")]
        public Video[] Videos { get; set; }

        //[SerializationPropertyName("cmsVideos")]
        //public object[] CmsVideos { get; set; }

        //[SerializationPropertyName("xCloudProperties", NullValueHandling = NullValueHandling.Ignore)]
        //public XCloudProperties XCloudProperties { get; set; }

        //[SerializationPropertyName("optimalSatisfyingPassId", NullValueHandling = NullValueHandling.Ignore)]
        //public string OptimalSatisfyingPassId { get; set; }
    }

    public class ContentRating
    {
        [SerializationPropertyName("boardName")]
        public BoardName BoardName { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("disclaimers")]
        public object[] Disclaimers { get; set; }

        [SerializationPropertyName("descriptors")]
        public string[] Descriptors { get; set; }

        [SerializationPropertyName("imageUri")]
        public Uri ImageUri { get; set; }

        [SerializationPropertyName("imageLinkUri")]
        public Uri ImageLinkUri { get; set; }

        //[SerializationPropertyName("interactiveDescriptions")]
        //public object[] InteractiveDescriptions { get; set; }

        [SerializationPropertyName("rating")]
        public string Rating { get; set; }

        [SerializationPropertyName("ratingAge")]
        public int RatingAge { get; set; }

        [SerializationPropertyName("ratingDescription")]
        public string RatingDescription { get; set; }
    }

    public class Images
    {
        /// <summary>
        /// Square Cover Image
        /// </summary>
        [SerializationPropertyName("boxArt")]
        public BoxArt BoxArt { get; set; } = null;

        /// <summary>
        /// Vertical Cover Image
        /// </summary>
        [SerializationPropertyName("poster")]
        public BoxArt Poster { get; set; } = null;

        /// <summary>
        /// Background Image
        /// </summary>
        [SerializationPropertyName("superHeroArt")]
        public BoxArt SuperHeroArt { get; set; } = null;

        /// <summary>
        /// Icon
        /// </summary>
        [SerializationPropertyName("tile")]
        public BoxArt Tile { get; set; } = null;
    }

    public class BoxArt
    {
        [SerializationPropertyName("url")]
        public Uri Url { get; set; }

        [SerializationPropertyName("width")]
        public long Width { get; set; }

        [SerializationPropertyName("height")]
        public long Height { get; set; }

        //[SerializationPropertyName("backgroundColor", NullValueHandling = NullValueHandling.Ignore)]
        //public BackgroundColor? BackgroundColor { get; set; }

        [SerializationPropertyName("caption")]
        public string Caption { get; set; } = string.Empty;
    }

    public class SpecificPrices
    {
        [SerializationPropertyName("purchaseable")]
        public Able[] Purchaseable { get; set; }

        [SerializationPropertyName("giftable")]
        public Able[] Giftable { get; set; }

        [SerializationPropertyName("totalPurchaseablePricesCount")]
        public long TotalPurchaseablePricesCount { get; set; }
    }

    public class Able
    {
        [SerializationPropertyName("skuId")]
        public string SkuId { get; set; }

        [SerializationPropertyName("availabilityId")]
        public string AvailabilityId { get; set; }

        [SerializationPropertyName("listPrice")]
        public double ListPrice { get; set; }

        [SerializationPropertyName("msrp")]
        public double Msrp { get; set; }

        [SerializationPropertyName("recurrencePrice")]
        public object RecurrencePrice { get; set; }

        [SerializationPropertyName("discountPercentage")]
        public double DiscountPercentage { get; set; }

        [SerializationPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }

        [SerializationPropertyName("remediations")]
        public object[] Remediations { get; set; }

        [SerializationPropertyName("affirmationId")]
        public object AffirmationId { get; set; }

        [SerializationPropertyName("priceEligibilityInfo")]
        public PriceEligibilityInfo PriceEligibilityInfo { get; set; }

        [SerializationPropertyName("availabilityActions")]
        public Action[] AvailabilityActions { get; set; }

        [SerializationPropertyName("endDate")]
        public DateTimeOffset EndDate { get; set; }

        [SerializationPropertyName("hasXPriceOffer")]
        public bool HasXPriceOffer { get; set; }

        [SerializationPropertyName("xPriceOfferInfo")]
        public object XPriceOfferInfo { get; set; }
    }

    public class PriceEligibilityInfo
    {
        [SerializationPropertyName("key")]
        public long Key { get; set; }

        [SerializationPropertyName("bigId")]
        public string BigId { get; set; }
    }

    public class Video
    {
        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("url")]
        public Uri Url { get; set; }

        [SerializationPropertyName("width")]
        public long Width { get; set; }

        [SerializationPropertyName("height")]
        public long Height { get; set; }

        [SerializationPropertyName("previewImage")]
        public BoxArt PreviewImage { get; set; }

        [SerializationPropertyName("purpose")]
        public Purpose Purpose { get; set; }
    }

    public class XCloudProperties
    {
        [SerializationPropertyName("xCloudId")]
        public string XCloudId { get; set; }

        [SerializationPropertyName("xboxTitleId")]
        public long XboxTitleId { get; set; }

        [SerializationPropertyName("supportsTouch")]
        public bool SupportsTouch { get; set; }
    }

    public class SearchBucketFilerIdMap
    {
        [SerializationPropertyName("SearchGames")]
        public string[] SearchGames { get; set; }

        [SerializationPropertyName("SearchAddons")]
        public string[] SearchAddons { get; set; }
    }

    public class SkuSummary
    {
        [SerializationPropertyName("isGamesWithGoldSku")]
        public bool IsGamesWithGoldSku { get; set; }

        [SerializationPropertyName("isPreorder")]
        public bool IsPreorder { get; set; }

        //[SerializationPropertyName("optimalAvailabilityId", NullValueHandling = NullValueHandling.Ignore)]
        //public string OptimalAvailabilityId { get; set; }

        //[SerializationPropertyName("preferredAvailabilityId", NullValueHandling = NullValueHandling.Ignore)]
        //public string PreferredAvailabilityId { get; set; }

        [SerializationPropertyName("productId")]
        public string ProductId { get; set; }

        [SerializationPropertyName("skuDescription")]
        public string SkuDescription { get; set; }

        [SerializationPropertyName("skuId")]
        public string SkuId { get; set; }

        [SerializationPropertyName("skuImages")]
        public SupportItems SkuImages { get; set; }

        [SerializationPropertyName("skuTitle")]
        public string SkuTitle { get; set; }
    }

    public class SupportItems
    {
    }

    public enum Action { Browse, Curate, Details, Fulfill, Gift, Purchase, Redeem, Unknown };

    public enum Eligibility { None };

    public enum AvailableOn { Pc, XboxOne, XboxSeriesX, MobileDevice, HoloLens, Hub, XCloud, Handheld };

    [JsonConverter(typeof(BoardNameConverter))]
    public enum BoardName
    {
        Cero,       // Computer Entertainment Rating Organization (CERO) - Japan
        Esrb,       // Entertainment Software Rating Board (ESRB) - United States
        Iarc,       // International Age Rating Coalition (IARC) - Global
        Pegi,       // Pan European Game Information (PEGI) - Europe
        Microsoft,  // Microsoft Store Rating - Global
        Cob_Au,     // Australian Classification Board (ACB) - Australia
        Usk,        // Unterhaltungssoftware Selbstkontrolle (USK) - Germany
        Grb,        // Game Rating and Administration Committee (GRAC) - South Korea
        Djctq       // Department of Justice, Rating, Titles, and Qualification (DJCTQ) - Brazil
    };

    public enum ProductFamily { Games };

    public enum ProductKind { Game };

    public enum Purpose { Trailer, HeroTrailer };

    public class BoardNameConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string enumValue = (string)reader.Value;
            if (enumValue.Equals("COB-AU", StringComparison.OrdinalIgnoreCase))
            {
                return BoardName.Cob_Au;
            }
            else
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
        }
    }

    //internal static class Converter
    //{
    //    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    //    {
    //        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
    //        DateParseHandling = DateParseHandling.None,
    //        Converters =
    //        {
    //            ActionConverter.Singleton,
    //            EligibilityConverter.Singleton,
    //            AvailableOnConverter.Singleton,
    //            BoardNameConverter.Singleton,
    //            ProductFamilyConverter.Singleton,
    //            ProductKindConverter.Singleton,
    //            PurposeConverter.Singleton,
    //            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
    //        },
    //    };
    //}

    //internal class ActionConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(Action) || t == typeof(Action?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        switch (value)
    //        {
    //            case "Browse":
    //                return Action.Browse;
    //            case "Curate":
    //                return Action.Curate;
    //            case "Details":
    //                return Action.Details;
    //            case "Fulfill":
    //                return Action.Fulfill;
    //            case "Gift":
    //                return Action.Gift;
    //            case "Purchase":
    //                return Action.Purchase;
    //            case "Redeem":
    //                return Action.Redeem;
    //            case "Unknown":
    //                return Action.Unknown;
    //        }
    //        throw new Exception("Cannot unmarshal type Action");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (Action)untypedValue;
    //        switch (value)
    //        {
    //            case Action.Browse:
    //                serializer.Serialize(writer, "Browse");
    //                return;
    //            case Action.Curate:
    //                serializer.Serialize(writer, "Curate");
    //                return;
    //            case Action.Details:
    //                serializer.Serialize(writer, "Details");
    //                return;
    //            case Action.Fulfill:
    //                serializer.Serialize(writer, "Fulfill");
    //                return;
    //            case Action.Gift:
    //                serializer.Serialize(writer, "Gift");
    //                return;
    //            case Action.Purchase:
    //                serializer.Serialize(writer, "Purchase");
    //                return;
    //            case Action.Redeem:
    //                serializer.Serialize(writer, "Redeem");
    //                return;
    //            case Action.Unknown:
    //                serializer.Serialize(writer, "Unknown");
    //                return;
    //        }
    //        throw new Exception("Cannot marshal type Action");
    //    }

    //    public static readonly ActionConverter Singleton = new ActionConverter();
    //}

    //internal class EligibilityConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(Eligibility) || t == typeof(Eligibility?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "None")
    //        {
    //            return Eligibility.None;
    //        }
    //        throw new Exception("Cannot unmarshal type Eligibility");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (Eligibility)untypedValue;
    //        if (value == Eligibility.None)
    //        {
    //            serializer.Serialize(writer, "None");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type Eligibility");
    //    }

    //    public static readonly EligibilityConverter Singleton = new EligibilityConverter();
    //}

    //internal class AvailableOnConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(AvailableOn) || t == typeof(AvailableOn?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        switch (value)
    //        {
    //            case "PC":
    //                return AvailableOn.Pc;
    //            case "XboxOne":
    //                return AvailableOn.XboxOne;
    //            case "XboxSeriesX":
    //                return AvailableOn.XboxSeriesX;
    //        }
    //        throw new Exception("Cannot unmarshal type AvailableOn");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (AvailableOn)untypedValue;
    //        switch (value)
    //        {
    //            case AvailableOn.Pc:
    //                serializer.Serialize(writer, "PC");
    //                return;
    //            case AvailableOn.XboxOne:
    //                serializer.Serialize(writer, "XboxOne");
    //                return;
    //            case AvailableOn.XboxSeriesX:
    //                serializer.Serialize(writer, "XboxSeriesX");
    //                return;
    //        }
    //        throw new Exception("Cannot marshal type AvailableOn");
    //    }

    //    public static readonly AvailableOnConverter Singleton = new AvailableOnConverter();
    //}

    //internal class BoardNameConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(BoardName) || t == typeof(BoardName?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "ESRB")
    //        {
    //            return BoardName.Esrb;
    //        }
    //        throw new Exception("Cannot unmarshal type BoardName");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (BoardName)untypedValue;
    //        if (value == BoardName.Esrb)
    //        {
    //            serializer.Serialize(writer, "ESRB");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type BoardName");
    //    }

    //    public static readonly BoardNameConverter Singleton = new BoardNameConverter();
    //}

    //internal class ProductFamilyConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(ProductFamily) || t == typeof(ProductFamily?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "Games")
    //        {
    //            return ProductFamily.Games;
    //        }
    //        throw new Exception("Cannot unmarshal type ProductFamily");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (ProductFamily)untypedValue;
    //        if (value == ProductFamily.Games)
    //        {
    //            serializer.Serialize(writer, "Games");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type ProductFamily");
    //    }

    //    public static readonly ProductFamilyConverter Singleton = new ProductFamilyConverter();
    //}

    //internal class ProductKindConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(ProductKind) || t == typeof(ProductKind?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "Game")
    //        {
    //            return ProductKind.Game;
    //        }
    //        throw new Exception("Cannot unmarshal type ProductKind");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (ProductKind)untypedValue;
    //        if (value == ProductKind.Game)
    //        {
    //            serializer.Serialize(writer, "Game");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type ProductKind");
    //    }

    //    public static readonly ProductKindConverter Singleton = new ProductKindConverter();
    //}

    //internal class PurposeConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(Purpose) || t == typeof(Purpose?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "trailer")
    //        {
    //            return Purpose.Trailer;
    //        }
    //        throw new Exception("Cannot unmarshal type Purpose");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (Purpose)untypedValue;
    //        if (value == Purpose.Trailer)
    //        {
    //            serializer.Serialize(writer, "trailer");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type Purpose");
    //    }

    //    public static readonly PurposeConverter Singleton = new PurposeConverter();
    //}
}
