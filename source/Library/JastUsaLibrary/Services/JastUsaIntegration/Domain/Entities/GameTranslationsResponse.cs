using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
    public class GameTranslationsResponse
    {
        [SerializationPropertyName("@context")]
        public string ContextApiEndpoint { get; set; }

        [SerializationPropertyName("@type")]
        public string TypeApiEndpoint { get; set; }

        [SerializationPropertyName("@id")]
        public string IdApiEndpoint { get; set; }

        [SerializationPropertyName("gamePathLinks")]
        public List<GameLink> GamePathLinks { get; set; }

        [SerializationPropertyName("gameExtraLinks")]
        public List<GameLink> GameExtraLinks { get; set; }

        [SerializationPropertyName("gamePatchLinks")]
        public List<GameLink> GamePatchLinks { get; set; }
    }
}