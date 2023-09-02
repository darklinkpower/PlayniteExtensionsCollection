using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Playnite.SDK.Data;
using Newtonsoft.Json.Linq;

namespace JastUsaLibrary.Models
{
    public class ProductResponse
    {
        [SerializationPropertyName("@context")]
        public string Context { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("productESRB")]
        public ProductEsrb ProductEsrb { get; set; }

        [SerializationPropertyName("sku")]
        public string Sku { get; set; }

        [SerializationPropertyName("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        [SerializationPropertyName("originalReleaseDate")]
        public DateTime OriginalReleaseDate { get; set; }

        [SerializationPropertyName("productTaxons")]
        public string[] ProductTaxons { get; set; }

        [SerializationPropertyName("mainTaxon")]
        public string MainTaxon { get; set; }

        [SerializationPropertyName("reviews")]
        public object[] Reviews { get; set; }

        [SerializationPropertyName("averageRating")]
        public int AverageRating { get; set; }

        [SerializationPropertyName("images")]
        public Image[] Images { get; set; }

        [SerializationPropertyName("id")]
        public int ProductResponseId { get; set; }

        [SerializationPropertyName("code")]
        public string Code { get; set; }

        [SerializationPropertyName("attributes")]
        public ProductResponseAttribute[] Attributes { get; set; }

        [SerializationPropertyName("variants")]
        public ProductResponseVariant[] Variants { get; set; }

        [SerializationPropertyName("options")]
        public object[] Options { get; set; }

        [SerializationPropertyName("associations")]
        public object[] Aassociations { get; set; }

        [SerializationPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [SerializationPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [SerializationPropertyName("translations")]
        public Dictionary<string, ProductResponseTranslationData> Translations { get; set; }

        [SerializationPropertyName("shortDescription")]
        public string ShortDescription { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }
        [SerializationPropertyName("slug")]
        public string Slug { get; set; }

        [SerializationPropertyName("defaultVariant")]
        public string DefaultVariant { get; set; }

        [SerializationPropertyName("bonusPoints")]
        public BonusPoints BonusPoints { get; set; }
    }

    public class ProductResponseAttribute
    {
        [SerializationPropertyName("@type")]
        public AttributeType Type { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("localeCode")]
        public Locale LocaleCode { get; set; }

        [SerializationPropertyName("type")]
        public TypeEnum AttributeType { get; set; }

        [SerializationPropertyName("code")]
        public string Code { get; set; }

        // This property is sometimes a string, array of strings or property so we have to fix it
        [SerializationPropertyName("value"), JsonConverter(typeof(SingleValueArrayConverter<string>))]
        public string[] Value { get; set; }

        [SerializationPropertyName("attribute_id")]
        public int AttributeId { get; set; }

        [JsonProperty("configuration", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(ConfigurationClassConverter))]
        public ProductResponseAttributeConfiguration Configuration { get; set; }
    }

    public class ProductResponseVariant
    {
        [SerializationPropertyName("@id")]
        public string EndpointId { get; set; }

        [SerializationPropertyName("@type")]
        public ProductVariantType Type { get; set; }

        [SerializationPropertyName("game")]
        public string Game { get; set; }

        [SerializationPropertyName("@channelPricings")]
        public Dictionary<string, ChannelPricing> ChannelPricings { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("@translations")]
        public Dictionary<Locale, ProductResponseVariantTranslation> Translations { get; set; }

        [SerializationPropertyName("inStock")]
        public bool InStock { get; set; }

        [SerializationPropertyName("price")]
        public int Price { get; set; }

        [SerializationPropertyName("originalPrice")]
        public int OriginalPrice { get; set; }

        [SerializationPropertyName("discount")]
        public int Discount { get; set; }

        [SerializationPropertyName("isFree")]
        public bool IsFree { get; set; }
    }

    public class ProductResponseVariantTranslation
    {
        [SerializationPropertyName("@id")]
        public string ApiEndpoint { get; set; }

        [SerializationPropertyName("@type")]
        public TranslationType Type { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("@name")]
        public string Name { get; set; }

        [SerializationPropertyName("locale")]
        public Locale Locale { get; set; }
    }

    public class ProductResponseAttributeConfiguration
    {
        [JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(AttributeConfigurationConverter))]
        public Dictionary<string, Dictionary<Locale, string>> Choices { get; set; }

        [SerializationPropertyName("multiple")]
        public bool Multiple { get; set; }

        [SerializationPropertyName("min")]
        public int? Min { get; set; }

        [SerializationPropertyName("max")]
        public int? Max { get; set; }
    }

    public class BonusPoints
    {
        [SerializationPropertyName("value")]
        public long Value { get; set; }

        [SerializationPropertyName("amount")]
        public long Amount { get; set; }

        [SerializationPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }
    }

    public class Image
    {
        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public ImageType Type { get; set; }

        [SerializationPropertyName("priority")]
        public int? Priority { get; set; }

        [SerializationPropertyName("id")]
        public int ImageId { get; set; }

        [SerializationPropertyName("type")]
        public string ImageType { get; set; }

        [SerializationPropertyName("path")]
        public string Path { get; set; }
    }

    public class ProductEsrb
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("ESRBRating")]
        public string EsrbRating { get; set; }

        [SerializationPropertyName("ESRBContent")]
        public string EsrbContent { get; set; }

        [SerializationPropertyName("matureContent")]
        public bool MatureContent { get; set; }
    }

    public class ProductResponseTranslationData
    {
        [SerializationPropertyName("@id")]
        public string EndpointId { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("shortDescription")]
        public string ShortDescription { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("slug")]
        public string Slug { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }
    }

    public class ChannelPricing
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("channelCode")]
        public string ChannelCode { get; set; }

        [SerializationPropertyName("price")]
        public int Price { get; set; }

        [SerializationPropertyName("originalPrice")]
        public int? OriginalPrice { get; set; }
    }

    public enum TypeEnum { Select, Text, Textarea };

    public enum Locale { En_Us, ja, Zh_Hans, Zh_Hant };

    public enum AttributeType { ProductAttributeValue };

    public enum ImageType { ProductImage };

    internal class ConfigurationClassConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ProductResponseAttributeConfiguration);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Skip();
                return new ProductResponseAttributeConfiguration();
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                return serializer.Deserialize<ProductResponseAttributeConfiguration>(reader);
            }

            throw new Exception("Cannot unmarshal type ConfigurationClass");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class AttributeConfigurationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, Dictionary<Locale, string>>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<string, Dictionary<Locale, string>>();
            var jsonObject = JObject.Load(reader);
            foreach (var property in jsonObject.Properties())
            {
                var currentKey = property.Name;
                var innerToken = property.Value;
                if (innerToken.Type == JTokenType.Object)
                {
                    var innerDictionary = innerToken.ToObject<Dictionary<string, string>>();
                    var localeDictionary = new Dictionary<Locale, string>();
                    foreach (var innerProperty in innerDictionary)
                    {
                        if (Enum.TryParse<Locale>(innerProperty.Key, true, out var locale))
                        {
                            localeDictionary[locale] = innerProperty.Value;
                        }
                    }

                    result[currentKey] = localeDictionary;
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
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
            if (reader.TokenType == JsonToken.StartObject)
            {
                // Skip the entire object
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndObject)
                    {
                        break;
                    }
                }

                return new T[] { };
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<T[]>(reader);
            }

            return new T[] { serializer.Deserialize<T>(reader) };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<T>);
        }
    }
}