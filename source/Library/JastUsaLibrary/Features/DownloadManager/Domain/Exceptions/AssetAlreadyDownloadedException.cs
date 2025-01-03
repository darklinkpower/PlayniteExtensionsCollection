using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Exceptions
{
    public class AssetAlreadyDownloadedException : Exception
    {
        public JastGameDownloadData DownloadData { get; }
        public string DownloadPath { get; }

        public AssetAlreadyDownloadedException(JastGameDownloadData downloadData, string downloadPath)
            : base($"Download {downloadData.Label} with Id \"{downloadData.GameId}-{downloadData.GameLinkId}\" file download already existed in {downloadPath}")
        {
            DownloadData = Guard.Against.Null(downloadData);
            DownloadPath = Guard.Against.NullOrEmpty(downloadPath);
        }
    }
}