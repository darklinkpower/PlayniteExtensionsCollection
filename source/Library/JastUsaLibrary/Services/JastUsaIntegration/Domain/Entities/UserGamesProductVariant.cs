using JastUsaLibrary.JastUsaIntegration.Domain.Enums;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
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
}