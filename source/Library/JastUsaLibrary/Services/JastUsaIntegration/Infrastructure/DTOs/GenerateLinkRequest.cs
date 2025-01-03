using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Infrastructure.DTOs
{
    public class GenerateLinkRequest
    {
        [SerializationPropertyName("downloaded")]
        public bool Downloaded;
        [SerializationPropertyName("gameLinkId")]
        public int GameLinkId;
        [SerializationPropertyName("gameId")]
        public int GameId;
        [SerializationPropertyName("type")]
        public string Type;

        public GenerateLinkRequest(int gameId, int gameLinkId)
        {
            Downloaded = true;
            GameId = gameId;
            GameLinkId = gameLinkId;
            Type = "default";
        }
    }
}