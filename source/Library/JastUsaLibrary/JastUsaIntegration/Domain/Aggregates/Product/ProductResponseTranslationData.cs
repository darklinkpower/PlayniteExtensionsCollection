using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
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
}