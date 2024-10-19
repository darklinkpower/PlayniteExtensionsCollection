using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.JastLibraryCacheService.Interfaces;
using JastUsaLibrary.ProgramsHelper.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastLibraryCacheService.Application
{
    public class LibraryCacheService : ILibraryCacheService
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly JastUsaLibrarySettingsViewModel _settingsViewModel;
        private readonly Guid _pluginId;
        public List<JastGameWrapper> LibraryGames => GetLibraryGames();

        public LibraryCacheService(IPlayniteAPI playniteApi, JastUsaLibrarySettingsViewModel settingsViewModel, Guid pluginId)
        {
            _playniteApi = playniteApi;
            _settingsViewModel = settingsViewModel;
            _pluginId = pluginId;
        }

        public bool ApplyProgramToGameCache(Game databaseGame, Program program)
        {
            if (_settingsViewModel.Settings.LibraryCache.TryGetValue(databaseGame.GameId, out var cache))
            {
                cache.Program = program;
                _settingsViewModel.SaveSettings();

                databaseGame.InstallDirectory = Path.GetDirectoryName(program.Path);
                databaseGame.IsInstalled = true;
                _playniteApi.Database.Games.Update(databaseGame);
                //OnGameInstallationApplied(databaseGame, cache);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ApplyAssetsToCache(string gameId, IEnumerable<JastAssetWrapper> assetWrappers)
        {
            if (_settingsViewModel.Settings.LibraryCache.TryGetValue(gameId, out var cache))
            {
                cache.Assets = assetWrappers.ToObservable();
                _settingsViewModel.SaveSettings();
                return true;
            }
            else
            {
                return false;
            }
        }

        public GameCache GetCacheById(string gameId)
        {
            if (_settingsViewModel.Settings.LibraryCache.TryGetValue(gameId, out var cache))
            {
                return cache;
            }

            return null;
        }

        private List<JastGameWrapper> GetLibraryGames()
        {
            return _playniteApi.Database.Games
                .Where(g => g.PluginId == _pluginId)
                .OrderBy(g => g.Name)
                .Select(game =>
                {
                    var gameAssets = _settingsViewModel.Settings.LibraryCache.TryGetValue(game.GameId, out var cache)
                        ? cache.Assets
                        : new ObservableCollection<JastAssetWrapper>();
                    return new JastGameWrapper(game, gameAssets);
                }).ToList();
        }


    }
}