using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
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
}