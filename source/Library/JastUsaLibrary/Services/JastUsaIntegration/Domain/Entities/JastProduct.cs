using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
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

}
