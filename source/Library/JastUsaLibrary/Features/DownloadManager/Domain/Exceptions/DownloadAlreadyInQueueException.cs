using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Exceptions
{
    public class DownloadAlreadyInQueueException : Exception
    {
        public GameLink GameLink { get; }
        public DownloadAlreadyInQueueException(GameLink gameLink) :
            base($"Download {gameLink.Label} with Id \"{gameLink.GameId}-{gameLink.GameLinkId}\" was already in downloads queue")
        {
            GameLink = gameLink;
        }
    }
}