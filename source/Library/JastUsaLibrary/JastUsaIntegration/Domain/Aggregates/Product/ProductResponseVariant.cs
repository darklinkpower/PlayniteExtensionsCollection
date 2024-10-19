using JastUsaLibrary.JastUsaIntegration.Domain.Enums;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
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
        public double Discount { get; set; }

        [SerializationPropertyName("isFree")]
        public bool IsFree { get; set; }
    }
}