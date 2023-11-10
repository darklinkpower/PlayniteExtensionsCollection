using GameRelations.Interfaces;
using GameRelations.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TemporaryCache;

namespace GameRelations.PlayniteControls
{
    public partial class SimilarGamesControl : GameRelationsBase
    {
        public SimilarGamesControl(CacheManager<string, BitmapImage> _imagesCacheManager, IPlayniteAPI playniteApi, GameRelationsSettings settings, SimilarGamesControlSettings controlSettings)
            : base(_imagesCacheManager, playniteApi, settings, controlSettings)
        {
            SetSettings(controlSettings);
        }

        public override IEnumerable<Game> GetMatchingGames(Game game)
        {
            var tagsSet = game.TagIds?.ToHashSet() ?? new HashSet<Guid>();
            var genresSet = game.GenreIds?.ToHashSet() ?? new HashSet<Guid>();
            var categoriesSet = game.CategoryIds?.ToHashSet() ?? new HashSet<Guid>();
            var propertiesWeights = new Dictionary<string, double>
            {
                {"tags", 1 },
                {"genres", 1.2 },
                {"categories", 1.3 }
            };

            var minMatchPercent = 0.75;
            var minScoreThreshold = propertiesWeights.Count * minMatchPercent;
            var similarityScores = new Dictionary<Game, double>();
            foreach (var otherGame in PlayniteApi.Database.Games)
            {
                if (otherGame.Id == game.Id)
                {
                    continue;
                }

                if (!game.Hidden && otherGame.Hidden)
                {
                    continue;
                }
                
                var tagsScore = CalculateListHashSetMatchPercentage(otherGame.TagIds, tagsSet) * propertiesWeights["tags"];
                var genresScore = CalculateListHashSetMatchPercentage(otherGame.GenreIds, genresSet) * propertiesWeights["genres"];
                var categoriesScore = CalculateListHashSetMatchPercentage(otherGame.CategoryIds, categoriesSet) * propertiesWeights["categories"];

                var finalScore = tagsScore + genresScore + categoriesScore;
                if (finalScore >= minScoreThreshold)
                {
                    similarityScores.Add(otherGame, finalScore);
                }
            }

            var similarGames = similarityScores.OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key)
                .Take(8);

            return similarGames;
        }
    }
}