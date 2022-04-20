using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Models
{
    public class GameTranslationsResponse
    {
        [SerializationPropertyName("@context")]
        public TranslationContext Context { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("gamePathLinks")]
        public GameLinks GamePathLinks { get; set; }

        [SerializationPropertyName("gameExtraLinks")]
        public GameLinks GameExtraLinks { get; set; }

        [SerializationPropertyName("gamePatchLinks")]
        public GameLinks GamePatchLinks { get; set; }
    }

    public class TranslationContext
    {
        [SerializationPropertyName("@vocab")]
        public Uri Vocab { get; set; }

        [SerializationPropertyName("hydra")]
        public Uri Hydra { get; set; }

        [SerializationPropertyName("gamePathLinks")]
        public string GamePathLinks { get; set; }

        [SerializationPropertyName("gameExtraLinks")]
        public string GameExtraLinks { get; set; }

        [SerializationPropertyName("gamePatchLinks")]
        public string GamePatchLinks { get; set; }

        [SerializationPropertyName("gamePathLink")]
        public string GamePathLink { get; set; }

        [SerializationPropertyName("gameExtraLink")]
        public string GameExtraLink { get; set; }

        [SerializationPropertyName("gamePatchLink")]
        public string GamePatchLink { get; set; }
    }

    public class GameLinks
    {
        [SerializationPropertyName("@context")]
        public string Context { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("hydra:member")]
        public List<HydraMember> HydraMember { get; set; }

        [SerializationPropertyName("hydra:totalItems")]
        public int HydraTotalItems { get; set; }
    }

    public class HydraMember
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("gameId")]
        public int GameId { get; set; }

        [SerializationPropertyName("gameLinkId")]
        public int GameLinkId { get; set; }

        [SerializationPropertyName("label")]
        public string Label { get; set; }

        [SerializationPropertyName("platforms")]
        public string[] Platforms { get; set; }
    }
}