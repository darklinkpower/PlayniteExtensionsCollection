using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Exceptions
{
    public class DownloadAlreadyInQueueException : Exception
    {
        public JastGameDownloadData DownloadData { get; }
        public DownloadAlreadyInQueueException(JastGameDownloadData downloadData) :
            base($"Download {downloadData.Label} with Id \"{downloadData.GameId}-{downloadData.GameLinkId}\" was already in downloads queue")
        {
            DownloadData = Guard.Against.Null(downloadData);
        }
    }
}