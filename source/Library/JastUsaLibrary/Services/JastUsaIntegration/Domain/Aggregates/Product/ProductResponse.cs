using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
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
        public DateTime? OriginalReleaseDate { get; set; }

        [SerializationPropertyName("productTaxons")]
        public string[] ProductTaxons { get; set; }

        [SerializationPropertyName("mainTaxon")]
        public string MainTaxon { get; set; }

        [SerializationPropertyName("reviews")]
        public object[] Reviews { get; set; }

        [SerializationPropertyName("averageRating")]
        public double AverageRating { get; set; }

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
}