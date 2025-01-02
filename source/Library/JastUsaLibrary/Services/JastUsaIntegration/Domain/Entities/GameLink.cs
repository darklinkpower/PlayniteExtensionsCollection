using JastUsaLibrary.JastUsaIntegration.Domain.Enums;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
    public class GameLink
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("gameId")]
        public int GameId { get; set; }

        [SerializationPropertyName("gameLinkId")]
        public int GameLinkId { get; set; }

        [SerializationPropertyName("label")]
        public string Label { get; set; }

        [SerializationPropertyName("platforms")]
        public JastPlatform[] Platforms { get; set; }

        [SerializationPropertyName("version")]
        public string Version { get; set; }
    }
}