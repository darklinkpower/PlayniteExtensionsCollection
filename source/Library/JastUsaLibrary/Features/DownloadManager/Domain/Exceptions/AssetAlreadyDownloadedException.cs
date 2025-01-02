using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Exceptions
{
    public class AssetAlreadyDownloadedException : Exception
    {
        public GameLink GameLink { get; }
        public string DownloadPath { get; }

        public AssetAlreadyDownloadedException(GameLink gameLink, string downloadPath)
            : base($"Download {gameLink.Label} with Id \"{gameLink.GameId}-{gameLink.GameLinkId}\" file download already existed in {downloadPath}")
        {
            GameLink = gameLink;
            DownloadPath = downloadPath;
        }
    }
}