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
        public Context Context { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("products")]
        public Products Products { get; set; }

        [SerializationPropertyName("attributes")]
        public Attributes Attributes { get; set; }

        [SerializationPropertyName("total")]
        public int Total { get; set; }

        [SerializationPropertyName("pages")]
        public int Pages { get; set; }
    }

    public class Attributes
    {
        [SerializationPropertyName("@context")]
        public string Context { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("hydra:member")]
        public AttributesHydraMember[][] HydraMember { get; set; }

        [SerializationPropertyName("hydra:totalItems")]
        public long HydraTotalItems { get; set; }

        [SerializationPropertyName("hydra:view")]
        public HydraView HydraView { get; set; }
    }

    public class AttributesHydraMember
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("values")]
        public Value[] Values { get; set; }

        [SerializationPropertyName("code")]
        public long Code { get; set; }
    }

    public class Value
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("label")]
        public string Label { get; set; }

        [SerializationPropertyName("code")]
        public string Code { get; set; }

        [SerializationPropertyName("counter")]
        public long Counter { get; set; }
    }

    public class HydraView
    {
        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }
    }

    public class Context
    {
        [SerializationPropertyName("@vocab")]
        public Uri Vocab { get; set; }

        [SerializationPropertyName("hydra")]
        public Uri Hydra { get; set; }

        [SerializationPropertyName("products")]
        public string Products { get; set; }

        [SerializationPropertyName("attributes")]
        public string Attributes { get; set; }

        [SerializationPropertyName("total")]
        public string Total { get; set; }

        [SerializationPropertyName("pages")]
        public string Pages { get; set; }
    }

    public class Products
    {
        [SerializationPropertyName("@context")]
        public string Context { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("hydra:member")]
        public JastProduct[] JastProducts { get; set; }

        [SerializationPropertyName("hydra:totalItems")]
        public long HydraTotalItems { get; set; }

        [SerializationPropertyName("hydra:view")]
        public HydraView HydraView { get; set; }
    }

    public class JastProduct
    {
        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("variant")]
        public ProductVariant ProductVariant { get; set; }
    }

    public class ProductVariant
    {
        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("game")]
        public JastGame Game { get; set; }

        [SerializationPropertyName("price")]
        public long Price { get; set; }

        [SerializationPropertyName("inStock")]
        public bool InStock { get; set; }

        [SerializationPropertyName("productName")]
        public string ProductName { get; set; }

        [SerializationPropertyName("productVariantName")]
        public string ProductVariantName { get; set; }

        [SerializationPropertyName("productImage")]
        public string ProductImage { get; set; }

        [SerializationPropertyName("productImageBackground")]
        public string ProductImageBackground { get; set; }

        [SerializationPropertyName("platforms")]
        public Dictionary<string, string>[] Platforms { get; set; }

        [SerializationPropertyName("productCode")]
        public string ProductCode { get; set; }

        [SerializationPropertyName("gameId")]
        public int GameId { get; set; }
    }

    public class JastGame
    {
        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("translations")]
        public Dictionary<string, Translation> Translations { get; set; }
    }

    public class Translations
    {
        [SerializationPropertyName("en_US")]
        public Translation EnUs { get; set; }
    }

    public class Translation
    {
        [SerializationPropertyName("@id")]
        public string ApiIdUri { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("locale")]
        public string Locale { get; set; }
    }

    public class Platform
    {
        [SerializationPropertyName("en_US")]
        public string EnUs { get; set; }
    }


}