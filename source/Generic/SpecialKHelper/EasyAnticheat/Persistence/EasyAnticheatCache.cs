using Playnite.SDK.Data;
using PluginsCommon;
using SpecialKHelper.EasyAnticheat.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.EasyAnticheat.Persistence
{
    internal class EasyAnticheatCache : IEasyAnticheatCache
    {
        private readonly string _configurationDirectory;

        public EasyAnticheatCache(string configurationDirectory)
        {
            _configurationDirectory = configurationDirectory;
        }

        public GameDataCache LoadCache(Guid gameId)
        {
            var cachePath = Path.Combine(_configurationDirectory, $"{gameId}_cache.json");
            if (!FileSystem.FileExists(cachePath))
            {
                return null;
            }

            try
            {
                return Serialization.FromJsonFile<GameDataCache>(cachePath);
            }
            catch (Exception)
            {
                FileSystem.DeleteFileSafe(cachePath);
                return null;
            }
        }

        public void SaveCache(GameDataCache cache)
        {
            var cachePath = Path.Combine(_configurationDirectory, $"{cache.Id}_cache.json");
            FileSystem.WriteStringToFile(cachePath, Serialization.ToJson(cache));
        }
    }
}