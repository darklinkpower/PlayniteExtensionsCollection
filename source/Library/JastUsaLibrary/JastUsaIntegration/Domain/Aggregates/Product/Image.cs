using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
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
}