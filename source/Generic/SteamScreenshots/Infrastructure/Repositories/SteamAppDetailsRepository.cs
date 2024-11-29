using FlowHttp;
using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using SteamCommon.Models;
using SteamScreenshots.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TemporaryCache;

namespace SteamScreenshots.Infrastructure.Repositories
{
    public class SteamAppDetailsRepository : ISteamAppDetailsRepository
    {
        private readonly ILogger _logger;
        private readonly string _appDetailsDownloadDir;
        private readonly bool _useMemoryCache;
        private readonly CacheManager<string, SteamAppDetails> _cacheManager;

        public SteamAppDetailsRepository(ILogger logger, string appDetailsDownloadDir, bool useMemoryCache, TimeSpan? cacheExpirationTime = null)
        {
            _logger = logger;
            _appDetailsDownloadDir = appDetailsDownloadDir;
            _useMemoryCache = useMemoryCache;
            if (_useMemoryCache)
            {
                var memoryExpiryTime = cacheExpirationTime ?? TimeSpan.FromSeconds(60);
                _cacheManager = new CacheManager<string, SteamAppDetails>(memoryExpiryTime);
            }
        }

        public SteamAppDetails GetAppDetails(string steamId)
        {
            if (_useMemoryCache && _cacheManager.TryGetValue(steamId, out var cachedAppDetails))
            {
                return cachedAppDetails;
            }

            var appDetailsFromFile = LoadAppDetailsFromFile(steamId);
            if (appDetailsFromFile is null)
            {
                return null;
            }

            if (_useMemoryCache)
            {
                _cacheManager.Add(steamId, appDetailsFromFile);
            }
            
            return appDetailsFromFile;
        }

        public void SaveAppDetails(string id, SteamAppDetails details)
        {
            var gameDataPath = GetDataPathFromId(id);
            FileSystem.WriteStringToFile(gameDataPath, Serialization.ToJson(details));
        }

        public DateTime? GetAppDetailsCreationDate(string id)
        {
            var gameDataPath = GetDataPathFromId(id);
            if (FileSystem.FileExists(gameDataPath))
            {
                var fi = new FileInfo(FileSystem.FixPathLength(gameDataPath));
                return fi.LastWriteTime;
            }

            return null;
        }

        public void DeleteAppDetails(string steamId)
        {
            var gameDataPath = GetDataPathFromId(steamId);
            FileSystem.DeleteFile(gameDataPath);
            if (_useMemoryCache)
            {
                _cacheManager.Remove(steamId);
            }
        }

        private SteamAppDetails LoadAppDetailsFromFile(string steamId)
        {
            var gameDataPath = GetDataPathFromId(steamId);
            if (!FileSystem.FileExists(gameDataPath))
            {
                return null;
            }

            try
            {
                return Serialization.FromJsonFile<SteamAppDetails>(gameDataPath);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error during {gameDataPath} steam app details deserialize");
                FileSystem.DeleteFileSafe(gameDataPath);
                return null;
            }
        }

        private string GetDataPathFromId(string steamId)
        {
            return Path.Combine(_appDetailsDownloadDir, $"{steamId}_appdetails.json");
        }


    }

}