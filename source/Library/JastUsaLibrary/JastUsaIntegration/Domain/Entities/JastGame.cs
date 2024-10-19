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
    public class JastGame
    {
        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public GameType Type { get; set; }

        [SerializationPropertyName("translations")]
        public Dictionary<Locale, UserGamesResponseTranslation> Translations { get; set; }
    }
}