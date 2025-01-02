using JastUsaLibrary.JastLibraryCacheService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastLibraryCacheService.Persistence
{
    public class LibraryCachePersistencePluginSettings : ILibraryCachePersistence
    {
        private readonly JastUsaLibrarySettingsViewModel _jastUsaLibrarySettingsViewModel;

        public LibraryCachePersistencePluginSettings(JastUsaLibrarySettingsViewModel jastUsaLibrarySettingsViewModel)
        {
            _jastUsaLibrarySettingsViewModel = jastUsaLibrarySettingsViewModel;
        }

        public List<GameCache> GetAllCache()
        {
            return _jastUsaLibrarySettingsViewModel.Settings.LibraryCache.Values.ToList();
        }

        public GameCache GetCacheById(string gameId)
        {
            if (_jastUsaLibrarySettingsViewModel.Settings.LibraryCache.TryGetValue(gameId, out var cache))
            {
                return cache.GetClone();
            }

            return null;
        }

        public void RemoveCacheById(string id)
        {
            var removed = _jastUsaLibrarySettingsViewModel.Settings.LibraryCache.Remove(id);
            if (removed)
            {
                _jastUsaLibrarySettingsViewModel.SaveSettings();
            }
        }

        public bool SaveCache(GameCache cache)
        {
            _jastUsaLibrarySettingsViewModel.Settings.LibraryCache[cache.GameId] = cache;
            _jastUsaLibrarySettingsViewModel.SaveSettings();
            return true;
        }
    }
}
