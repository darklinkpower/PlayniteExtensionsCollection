using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JastUsaLibrary.Models
{
    public class ProductResponse
    {
        [JsonProperty("@context")]
        public string Context { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("productESRB")]
        public ProductEsrb ProductEsrb { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("productTaxons")]
        public string[] ProductTaxons { get; set; }

        [JsonProperty("mainTaxon")]
        public string MainTaxon { get; set; }

        [JsonProperty("reviews")]
        public object[] Reviews { get; set; }

        [JsonProperty("averageRating")]
        public long AverageRating { get; set; }

        [JsonProperty("images")]
        public Image[] Images { get; set; }

        [JsonProperty("id")]
        public long ProductResponseId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("attributes")]
        public Attribute[] Attributes { get; set; }

        [JsonProperty("variants")]
        public Variant[] Variants { get; set; }

        [JsonProperty("options")]
        public object[] Options { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("translations")]
        public ProductResponseTranslations Translations { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("defaultVariant")]
        public string DefaultVariant { get; set; }

        [JsonProperty("bonusPoints")]
        public BonusPoints BonusPoints { get; set; }
    }

    public class Attribute
    {
        [JsonProperty("@type")]
        public AttributeType Type { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("id")]
        public long AttributeId { get; set; }

        [JsonProperty("localeCode")]
        public string LocaleCode { get; set; }

        // This property is sometimes a string and sometimes an array of strings so we have to fix it
        [JsonProperty("value"), JsonConverter(typeof(SingleValueArrayConverter<string>))]
        public string[] Value { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("type")]
        public TypeEnum AttributeType { get; set; }

        [JsonProperty("attribute_id")]
        public long AttributeAttributeId { get; set; }



        [JsonProperty("configuration", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(ConfigurationClassConverter))]
        public ConfigurationClass Configuration { get; set; }
    }

    public class ConfigurationClass
    {
        [JsonProperty("choices")]
        public Dictionary<string, Dictionary<string, string>> Choices { get; set; }

        [JsonProperty("multiple")]
        public bool Multiple { get; set; }

        [JsonProperty("min")]
        public long? Min { get; set; }

        [JsonProperty("max")]
        public long? Max { get; set; }
    }

    public class BonusPoints
    {
        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
    }

    public class Image
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public ImageType Type { get; set; }

        [JsonProperty("priority")]
        public long Priority { get; set; }

        [JsonProperty("id")]
        public long ImageId { get; set; }

        [JsonProperty("type")]
        public string ImageType { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public class ProductEsrb
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("ESRBRating")]
        public object EsrbRating { get; set; }

        [JsonProperty("ESRBContent")]
        public object EsrbContent { get; set; }

        [JsonProperty("matureContent")]
        public bool MatureContent { get; set; }
    }

    public class ProductResponseTranslations
    {
        [JsonProperty("en_US")]
        public PurpleEnUs EnUs { get; set; }
    }

    public class PurpleEnUs
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("id")]
        public long EnUsId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class Variant
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("channelPricings")]
        public ChannelPricings ChannelPricings { get; set; }

        [JsonProperty("id")]
        public long VariantId { get; set; }

        [JsonProperty("translations")]
        public VariantTranslations Translations { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("originalPrice")]
        public long OriginalPrice { get; set; }

        [JsonProperty("discount")]
        public long Discount { get; set; }

        [JsonProperty("isFree")]
        public bool IsFree { get; set; }

        [JsonProperty("inStock")]
        public bool InStock { get; set; }
    }

    public class ChannelPricings
    {
        [JsonProperty("JASTUSA")]
        public Jastusa Jastusa { get; set; }
    }

    public class Jastusa
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("channelCode")]
        public string ChannelCode { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("originalPrice")]
        public object OriginalPrice { get; set; }
    }

    public class VariantTranslations
    {
        [JsonProperty("en_US")]
        public FluffyEnUs EnUs { get; set; }
    }

    public class FluffyEnUs
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public long EnUsId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }
    }

    public enum TypeEnum { Select, Text, Textarea };

    public enum Locale { EnUs, ZhHans, ZhHant };

    public enum AttributeType { ProductAttributeValue };

    public enum ImageType { ProductImage };

    internal class ConfigurationClassConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ConfigurationClass);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                //serializer.Deserialize<string[]>(reader);
                reader.Skip();
                return new ConfigurationClass();
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                return serializer.Deserialize<ConfigurationClass>(reader);
            }

            throw new Exception("Cannot unmarshal type ConfigurationClass");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    internal class SingleValueArrayConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject
                || reader.TokenType == JsonToken.String
                || reader.TokenType == JsonToken.Integer)
            {
                return new T[] { serializer.Deserialize<T>(reader) };
            }
            return serializer.Deserialize<T[]>(reader);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
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
                    return Locale.EnUs;
                case "zh_Hans":
                    return Locale.ZhHans;
                case "zh_Hant":
                    return Locale.ZhHant;
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
                case Locale.EnUs:
                    serializer.Serialize(writer, "en_US");
                    return;
                case Locale.ZhHans:
                    serializer.Serialize(writer, "zh_Hans");
                    return;
                case Locale.ZhHant:
                    serializer.Serialize(writer, "zh_Hant");
                    return;
            }
            throw new Exception("Cannot marshal type Locale");
        }

        public static readonly LocaleConverter Singleton = new LocaleConverter();
    }

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "select":
                    return TypeEnum.Select;
                case "text":
                    return TypeEnum.Text;
                case "textarea":
                    return TypeEnum.Textarea;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            switch (value)
            {
                case TypeEnum.Select:
                    serializer.Serialize(writer, "select");
                    return;
                case TypeEnum.Text:
                    serializer.Serialize(writer, "text");
                    return;
                case TypeEnum.Textarea:
                    serializer.Serialize(writer, "textarea");
                    return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }


    internal class ImageTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ImageType) || t == typeof(ImageType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "ProductImage")
            {
                return ImageType.ProductImage;
            }
            throw new Exception("Cannot unmarshal type ImageType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (ImageType)untypedValue;
            if (value == ImageType.ProductImage)
            {
                serializer.Serialize(writer, "ProductImage");
                return;
            }
            throw new Exception("Cannot marshal type ImageType");
        }

        public static readonly ImageTypeConverter Singleton = new ImageTypeConverter();
    }

}