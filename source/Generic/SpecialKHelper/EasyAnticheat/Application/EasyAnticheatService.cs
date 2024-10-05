using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SpecialKHelper.EasyAnticheat.Application;
using SpecialKHelper.EasyAnticheat.Domain;
using SpecialKHelper.EasyAnticheat.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.EasyAnticheat.Application
{
    internal class EasyAnticheatService : IEasyAnticheatService
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly IEasyAnticheatCache _easyAnticheatCache;
        public EasyAnticheatService(IEasyAnticheatCache easyAnticheatCache)
        {
            _easyAnticheatCache = easyAnticheatCache;
        }

        public bool IsGameEacEnabled(Game game)
        {
            var cache = _easyAnticheatCache.LoadCache(game.Id);
            if (cache != null && cache.EasyAnticheatStatus == EasyAnticheatStatus.Detected)
            {
                return true;
            }

            var status = GetGameEasyAnticheatStatus(game);
            _easyAnticheatCache.SaveCache(new GameDataCache
            {
                Id = game.Id,
                EasyAnticheatStatus = status
            });

            return status == EasyAnticheatStatus.Detected;
        }

        private EasyAnticheatStatus GetGameEasyAnticheatStatus(Game game)
        {
            if (!PlayniteUtilities.IsGamePcGame(game))
            {
                return EasyAnticheatStatus.NotDetected;
            }

            if (!PlayniteUtilities.IsGameInstallDirectoryValid(game))
            {
                return EasyAnticheatStatus.Unknown;
            }

            try
            {
                var eacFile = Directory
                     .EnumerateFiles(game.InstallDirectory, "EasyAntiCheat*", SearchOption.AllDirectories)
                     .FirstOrDefault();
                if (eacFile != null)
                {
                    _logger.Info($"EasyAntiCheat file {eacFile} detected for {game.Name}");
                    return EasyAnticheatStatus.Detected;
                }
                else
                {
                    _logger.Info($"EasyAntiCheat not detected for {game.Name}");
                    return EasyAnticheatStatus.NotDetected;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error during EAC enumeration for {game.Name} with dir {game.InstallDirectory}");
                return EasyAnticheatStatus.ErrorOnDetection;
            }
        }
    }
}