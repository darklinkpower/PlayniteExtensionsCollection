using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Application.DTOs
{
    public class GenerateLinkRequest
    {
        public bool downloaded;
        public int gameId;
        public int gameLinkId;

        public GenerateLinkRequest(GameLink gameLink)
        {
            downloaded = true;
            gameId = gameLink.GameId;
            gameLinkId = gameLink.GameLinkId;
        }
    }
}