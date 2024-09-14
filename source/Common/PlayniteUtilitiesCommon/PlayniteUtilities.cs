using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PlayniteUtilitiesCommon
{
    public static class PlayniteUtilities
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly IPlayniteAPI _playniteApi = API.Instance;
        private const string pcWinPlatformName = "PC (Windows)";
        private const string pcPlatformName = "PC";
        private const string pcSpecId = "pc_windows";
        private static HashSet<string> _addedIcoFontResources = new HashSet<string>();

        public static bool AddFeatureToGame(IPlayniteAPI PlayniteApi, Game game, string featureName)
        {
            var feature = PlayniteApi.Database.Features.Add(featureName);
            return AddFeatureToGame(PlayniteApi, game, feature);
        }

        public static bool GetGameHasFeature(Game game, string featureName, bool ignoreOrdinalCase = true)
        {
            if (!game.Features.HasItems())
            {
                return false;
            }
            
            if (ignoreOrdinalCase)
            {
                return game.Features.Any(x => x.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return game.Features.Any(x => x.Name == featureName);
            }
        }

        public static bool AddFeatureToGame(IPlayniteAPI PlayniteApi, Game game, GameFeature feature)
        {
            if (game.Features == null)
            {
                game.FeatureIds = new List<Guid> { feature.Id };
                PlayniteApi.Database.Games.Update(game);
                return true;
            }
            else if (!game.FeatureIds.Contains(feature.Id))
            {
                game.FeatureIds.Add(feature.Id);
                PlayniteApi.Database.Games.Update(game);
                return true;
            }

            return false;
        }

        public static int AddFeatureToGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, string featureName)
        {
            var feature = PlayniteApi.Database.Features.Add(featureName);
            return AddFeatureToGames(PlayniteApi, games, feature);
        }

        public static int AddFeatureToGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, GameFeature feature)
        {
            var addedCount = 0;
            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    if (AddFeatureToGame(PlayniteApi, game, feature))
                    {
                        addedCount++;
                    }
                }
            }

            return addedCount;
        }

        public static int RemoveFeatureFromGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, string featureName)
        {
            var removedCount = 0;

            using (PlayniteApi.Database.BufferedUpdate())
            foreach (var game in games)
            {
                if (RemoveFeatureFromGame(PlayniteApi, game, featureName))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        public static bool RemoveFeatureFromGame(IPlayniteAPI PlayniteApi, Game game, string featureName)
        {
            if (game.Features == null)
            {
                return false;
            }

            var feature = game.Features.FirstOrDefault(x => x.Name == featureName);
            if (feature != null)
            {
                game.FeatureIds.Remove(feature.Id);
                PlayniteApi.Database.Games.Update(game);
                return true;
            }

            return false;
        }

        public static bool RemoveFeatureFromGame(IPlayniteAPI PlayniteApi, Game game, GameFeature feature)
        {
            if (game.Features == null)
            {
                return false;
            }

            if (game.FeatureIds.Remove(feature.Id))
            {
                PlayniteApi.Database.Games.Update(game);
                return true;
            }

            return false;
        }

        public static bool AddTagToGame(IPlayniteAPI PlayniteApi, Game game, string tagName, bool updateInDatabase = true)
        {
            var tag = PlayniteApi.Database.Tags.Add(tagName);
            return AddTagToGame(PlayniteApi, game, tag, updateInDatabase);
        }

        public static bool AddTagToGame(IPlayniteAPI PlayniteApi, Game game, Tag tag, bool updateInDatabase = true)
        {
            var itemAdded = false;
            if (game.Tags is null)
            {
                game.TagIds = new List<Guid> { tag.Id };
                itemAdded = true;
            }
            else if (!game.TagIds.Contains(tag.Id))
            {
                game.TagIds.Add(tag.Id);
                itemAdded = true;
            }

            if (itemAdded && updateInDatabase)
            {
                PlayniteApi.Database.Games.Update(game);
            }

            return itemAdded;
        }

        public static int AddTagToGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, string tagName)
        {
            var tag = PlayniteApi.Database.Tags.Add(tagName);
            return AddTagToGames(PlayniteApi, games, tag);
        }

        public static int AddTagToGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, Tag tag)
        {
            var addedCount = 0;
            using (PlayniteApi.Database.BufferedUpdate())
            foreach (var game in games)
            {
                if (AddTagToGame(PlayniteApi, game, tag))
                {
                    addedCount++;
                }
            }

            return addedCount;
        }

        public static int RemoveTagFromGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, string tagName)
        {
            var removedCount = 0;
            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    if (RemoveTagFromGame(PlayniteApi, game, tagName))
                    {
                        removedCount++;
                    }
                }

                return removedCount;
            }
        }

        public static bool RemoveTagFromGame(IPlayniteAPI PlayniteApi, Game game, string tagName)
        {
            if (!game.Tags.HasItems())
            {
                return false;
            }

            var tag = game.Tags.FirstOrDefault(x => x.Name == tagName);
            if (tag != null)
            {
                game.TagIds.Remove(tag.Id);
                PlayniteApi.Database.Games.Update(game);
                return true;
            }

            return false;
        }

        public static bool RemoveTagFromGame(IPlayniteAPI PlayniteApi, Game game, Tag tag)
        {
            if (game.Tags == null)
            {
                return false;
            }

            if (game.TagIds.Any(x => x == tag.Id))
            {
                game.TagIds.Remove(tag.Id);
                PlayniteApi.Database.Games.Update(game);
                return true;
            }

            return false;
        }

        public static bool AddGenreToGame(IPlayniteAPI PlayniteApi, Game game, string genreName, bool updateInDatabase = true)
        {
            var genre = PlayniteApi.Database.Genres.Add(genreName);
            return AddGenreToGame(PlayniteApi, game, genre, updateInDatabase);
        }

        public static bool AddGenreToGame(IPlayniteAPI PlayniteApi, Game game, Genre genre, bool updateInDatabase = true)
        {
            var itemAdded = false;
            if (game.Genres == null)
            {
                game.GenreIds = new List<Guid> { genre.Id };
                itemAdded = true;
            }
            else if (!game.GenreIds.Contains(genre.Id))
            {
                game.GenreIds.Add(genre.Id);
                itemAdded = true;
            }

            if (itemAdded && updateInDatabase)
            {
                PlayniteApi.Database.Games.Update(game);
            }

            return itemAdded;
        }

        public static bool IsGamePcGame(Game game)
        {
            if (!game.Platforms.HasItems())
            {
                return false;
            }

            if (game.Platforms.Any(x => x.Name == pcWinPlatformName || x.Name == pcPlatformName ||
                      !string.IsNullOrEmpty(x.SpecificationId) && x.SpecificationId == pcSpecId))
            {
                return true;
            }

            return false;
        }

        public static bool GetIsInstallDirectoryValid(Game game)
        {
            if (string.IsNullOrEmpty(game.InstallDirectory) || !Directory.Exists(game.InstallDirectory))
            {
                logger.Warn($"Installation directory of {game.Name} in {game.InstallDirectory ?? string.Empty} is not valid");
                return false;
            }

            return true;
        }

        public static string GetEmbeddedJsonFromWebViewSource(string pageSource)
        {
            var prefixEnd = @"pre-wrap;"">";
            var suffixStart = "</pre></body></html>";
            var prefixRemoveEndIndex = pageSource.IndexOf(prefixEnd);
            var suffixRemoveStartIndex = pageSource.LastIndexOf(suffixStart);
            if (prefixRemoveEndIndex == -1 || suffixRemoveStartIndex == -1)
            {
                return null;
            }

            var startIndex = prefixRemoveEndIndex + prefixEnd.Length;
            var lenght = suffixRemoveStartIndex - startIndex;
            if (pageSource[startIndex] != '{' || pageSource[startIndex + lenght -1] != '}')
            {
                return null;
            }

            // We validate if the json is valid by checking if they have equal
            // number open and close characters
            var startBracketCount = 0;
            var endBracketCount = 0;
            var startObjCount = 0;
            var endObjCount = 0;

            var lastCharIndex = startIndex + lenght;
            for (var i = startIndex; i < lastCharIndex; i++)
            {
                switch (pageSource[i])
                {
                    case '{':
                        startBracketCount++;
                        continue;
                    case '}':
                        endBracketCount++;
                        continue;
                    case '[':
                        startObjCount++;
                        continue;
                    case ']':
                        endObjCount++;
                        continue;
                    default:
                        continue;
                }
            }

            if (startBracketCount != endBracketCount)
            {
                return null;
            }
            else if (startObjCount != endObjCount)
            {
                return null;
            }

            return pageSource.Substring(startIndex, lenght);
        }

        public static bool ApplyFilterPreset(IPlayniteAPI PlayniteApi, FilterPreset filterPreset)
        {
            var filterApplied = false;
            var currentFilterPresetSettings = PlayniteApi.MainView.GetCurrentFilterSettings();
            if (currentFilterPresetSettings != filterPreset.Settings)
            {
                PlayniteApi.MainView.ApplyFilterPreset(filterPreset);
                filterApplied = true;
            }

            if (filterApplied && PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                PlayniteApi.MainView.SwitchToLibraryView();
            }

            return filterApplied;
        }

        public static void AddTextIcoFontResource(string key, char character)
        {
            AddTextIcoFontResource(key, character.ToString());
        }

        public static void AddTextIcoFontResource(string key, string text)
        {
            if (Application.Current.Resources.Contains(key))
            {
                return;
            }

            Application.Current.Resources.Add(key, new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
            });
        }

        public static string GetIcoFontGlyphResource(char character)
        {
            var key = $"IcoFontResource - {character}";
            if (!_addedIcoFontResources.Contains(key))
            {
                AddTextIcoFontResource(key, character);
                _addedIcoFontResources.Add(key);
            }

            return key;
        }

        public static void OpenUrlOnWebView(string url, int width = 1900, int height = 1000, double dpiScalingFactor = 1.25)
        {
            using (var webView = _playniteApi.WebViews.CreateView(Convert.ToInt32(Math.Floor(width / dpiScalingFactor)), Convert.ToInt32(Math.Floor(height / dpiScalingFactor))))
            {
                webView.Navigate(url);
                webView.OpenDialog();
            }
        }

        public static CultureInfo GetPlayniteMatchingLanguageCulture(CultureInfo defaultCulture = null)
        {
            if (defaultCulture is null)
            {
                defaultCulture = CultureInfo.InvariantCulture;
            }

            var settingsLanguage = _playniteApi.ApplicationSettings.Language;
            try
            {
                var cultureParts = settingsLanguage.Split('_');
                if (cultureParts.Length == 2)
                {
                    var languageCode = cultureParts[0];
                    var regionCode = cultureParts[1];

                    return new CultureInfo($"{languageCode}-{regionCode}");
                }
            }
            catch (CultureNotFoundException e)
            {
                logger.Error(e, $"Culture not found for language: {settingsLanguage}");
            }

            return defaultCulture;
        }
    }
}