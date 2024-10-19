using JastUsaLibrary.DownloadManager.Domain.Enums;
using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using JastUsaLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Entities
{
    public class JastAssetWrapper : ObservableObject
    {
        public GameLink Asset { get; set; }
        public JastAssetType Type { get; set; }

        public JastAssetWrapper(GameLink asset, JastAssetType assetType)
        {
            Asset = asset;
            Type = assetType;
        }
    }
}