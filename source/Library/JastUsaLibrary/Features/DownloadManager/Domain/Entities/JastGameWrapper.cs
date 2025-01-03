using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Entities
{
    public class JastGameWrapper : ObservableObject
    {
        public Game Game { get; }
        public ObservableCollection<JastGameDownloadData> Assets { get; }
        public GameCache GameCache { get; }
        public JastGameWrapper(Game game, GameCache gameCache)
        {
            Game = Guard.Against.Null(game);
            GameCache = gameCache;

            Assets = new ObservableCollection<JastGameDownloadData>();
            UpdateDownloads();
        }

        public void UpdateDownloads()
        {
            Assets.Clear();
            var downloadsData = new List<JastGameDownloadData>();
            if (GameCache != null)
            {
                downloadsData.AddRange(GameCache.Downloads?.GameDownloads ?? Enumerable.Empty<JastGameDownloadData>());
                downloadsData.AddRange(GameCache.Downloads?.ExtraDownloads ?? Enumerable.Empty<JastGameDownloadData>());
                downloadsData.AddRange(GameCache.Downloads?.PatchDownloads ?? Enumerable.Empty<JastGameDownloadData>());
            }

            foreach (var downloadData in downloadsData)
            {
                Assets.Add(downloadData);
            }
        }
    }
}