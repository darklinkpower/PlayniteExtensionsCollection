using JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product;
using JastUsaLibrary.JastUsaIntegration.Domain.Enums;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
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
}