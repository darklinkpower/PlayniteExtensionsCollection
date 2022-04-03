using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUtilitiesCommon
{
    public static class PlayniteUtilities
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        public static bool AddFeatureToGame(IPlayniteAPI PlayniteApi, Game game, string featureName)
        {
            var feature = PlayniteApi.Database.Features.Add(featureName);
            return AddFeatureToGame(PlayniteApi, game, feature);
        }

        public static bool GetGameHasFeature(Game game, string featureName, bool ignoreOrdinalCase = false)
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
            foreach (var game in games)
            {
                if (AddFeatureToGame(PlayniteApi, game, feature))
                {
                    addedCount++;
                }
            }

            return addedCount;
        }

        public static int RemoveFeatureFromGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, string featureName)
        {
            var removedCount = 0;
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

            if (game.FeatureIds.Any(x => x == feature.Id))
            {
                game.FeatureIds.Remove(feature.Id);
                PlayniteApi.Database.Games.Update(game);
                return true;
            }

            return false;
        }

        public static bool AddTagToGame(IPlayniteAPI PlayniteApi, Game game, string tagName)
        {
            var tag = PlayniteApi.Database.Tags.Add(tagName);
            return AddTagToGame(PlayniteApi, game, tag);
        }

        public static bool AddTagToGame(IPlayniteAPI PlayniteApi, Game game, Tag tag)
        {
            if (game.Tags == null)
            {
                game.TagIds = new List<Guid> { tag.Id };
                PlayniteApi.Database.Games.Update(game);
                return true;
            }
            else if (!game.TagIds.Contains(tag.Id))
            {
                game.TagIds.Add(tag.Id);
                PlayniteApi.Database.Games.Update(game);
                return true;
            }

            return false;
        }

        public static int AddTagToGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, string tagName)
        {
            var tag = PlayniteApi.Database.Tags.Add(tagName);
            return AddTagToGames(PlayniteApi, games, tag);
        }

        public static int AddTagToGames(IPlayniteAPI PlayniteApi, IEnumerable<Game> games, Tag tag)
        {
            var addedCount = 0;
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
            foreach (var game in games)
            {
                if (RemoveTagFromGame(PlayniteApi, game, tagName))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        public static bool RemoveTagFromGame(IPlayniteAPI PlayniteApi, Game game, string tagName)
        {
            if (game.Tags == null)
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

        public static bool IsGamePcGame(Game game)
        {
            if (!game.Platforms.HasItems())
            {
                return false;
            }

            if (game.Platforms.Any(x => x.Name == "PC (Windows)" ||
                      !string.IsNullOrEmpty(x.SpecificationId) && x.SpecificationId == "pc_windows"))
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
    }
}
