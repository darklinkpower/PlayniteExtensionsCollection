using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Models
{
    public class UserGamesResponse
    {
        [SerializationPropertyName("@context")]
        public string Context { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("products")]
        public JastProduct[] Products { get; set; }

        [SerializationPropertyName("attributes")]
        public UserGamesResponseAtribute[][] Attributes { get; set; }

        [SerializationPropertyName("total")]
        public int Total { get; set; }

        [SerializationPropertyName("pages")]
        public int Pages { get; set; }
    }

    public class UserGamesResponseAtribute
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("values")]
        public Value[] Values { get; set; }

        [SerializationPropertyName("code")]
        public int Code { get; set; }

        [SerializationPropertyName("position")]
        public int Position { get; set; }
    }

    public class Value
    {
        [SerializationPropertyName("@type")]
        public ValueType Type { get; set; }

        [SerializationPropertyName("label")]
        public string Label { get; set; }

        [SerializationPropertyName("code")]
        public string Code { get; set; }

        [SerializationPropertyName("counter")]
        public int Counter { get; set; }
    }

    public class JastProduct
    {
        [SerializationPropertyName("@id")]
        public string IdApiEndpoint { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("variants")]
        public UserGamesProductVariant[] ProductVariants { get; set; }

        [SerializationPropertyName("variant")]
        public UserGamesProductVariant ProductVariant { get; set; }
    }

    public class UserGamesProductVariant
    {
        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public ProductVariantType Type { get; set; }

        [SerializationPropertyName("game")]
        public JastGame Game { get; set; }

        [SerializationPropertyName("inStock")]
        public bool InStock { get; set; }

        [SerializationPropertyName("price")]
        public int Price { get; set; }

        [SerializationPropertyName("originalPrice")]
        public int OriginalPrice { get; set; }

        [SerializationPropertyName("productName")]
        public string ProductName { get; set; }

        [SerializationPropertyName("productVariantName")]
        public string ProductVariantName { get; set; }

        [SerializationPropertyName("productImage")]
        public string ProductImage { get; set; }

        [SerializationPropertyName("productImageBackground")]
        public string ProductImageBackground { get; set; }

        [SerializationPropertyName("platforms")]
        public Dictionary<string, JastPlatform>[] Platforms { get; set; }

        [SerializationPropertyName("productCode")]
        public string ProductCode { get; set; }

        [SerializationPropertyName("gameId")]
        public int GameId { get; set; }

        [SerializationPropertyName("userGameTags")]
        public UserGameTag[] UserGameTags { get; set; }

        [SerializationPropertyName("hasPatches_en_US")]
        public bool HasPatchesEnUs { get; set; }

        [SerializationPropertyName("hasPatches_zh_Hans")]
        public bool HasPatchesZhHans { get; set; }

        [SerializationPropertyName("hasPatches_zh_Hant")]
        public bool HasPatchesZhHant { get; set; }
    }

    public class JastGame
    {
        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public GameType Type { get; set; }

        [SerializationPropertyName("translations")]
        public Dictionary<Locale, UserGamesResponseTranslation> Translations { get; set; }
    }

    public class UserGamesResponseTranslation
    {
        [SerializationPropertyName("@id")]
        public string ApiEndpoint { get; set; }

        [SerializationPropertyName("@type")]
        public TranslationType Type { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("locale")]
        public Locale Locale { get; set; }
    }

    public class UserGameTag
    {
        [SerializationPropertyName("@id")]
        public string ApiEndpoint { get; set; }

        [SerializationPropertyName("@type")]
        public UserGameTagType Type { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("type")]
        public string Name { get; set; }
    }

    public enum ValueType { AttributeValue };

    public enum UserGameTagType { UserGameTag };

    public enum TranslationType { GameLinkTranslation };

    public enum GameType { Game };

    public enum JastPlatform { Linux, Mac, Windows };

    public enum ProductVariantType { ProductVariant };

}