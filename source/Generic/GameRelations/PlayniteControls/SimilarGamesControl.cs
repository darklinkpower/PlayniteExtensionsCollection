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
        private readonly Dictionary<string, double> _propertiesWeights;
        private const double _minMatchValueFactor = 0.75;
        private readonly SimilarGamesControlSettings _controlSettings;

        public SimilarGamesControl(IPlayniteAPI playniteApi, GameRelationsSettings settings, SimilarGamesControlSettings controlSettings)
            : base(playniteApi, settings, controlSettings)
        {
            _controlSettings = controlSettings;
            _propertiesWeights = new Dictionary<string, double>
            {
                {"tags", 1 },
                {"genres", 1.2 },
                {"categories", 1.3 }
            };
        }

        public override IEnumerable<Game> GetMatchingGames(Game game)
        {
            var tagsSet = GetItemsNotInHashSet(game.TagIds, _controlSettings.TagsToIgnore).ToHashSet();
            var genresSet = GetItemsNotInHashSet(game.GenreIds, _controlSettings.GenresToIgnore).ToHashSet();
            var categoriesSet = GetItemsNotInHashSet(game.CategoryIds, _controlSettings.CategoriesToIgnore).ToHashSet();

            var seriesHashSet = game.SeriesIds?.ToHashSet() ?? new HashSet<Guid>();

            var minScoreThreshold = _propertiesWeights.Count * _minMatchValueFactor;
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
                
                if (_controlSettings.ExcludeGamesSameSeries && HashSetContainsAnyItem(otherGame.SeriesIds, seriesHashSet))
                {
                    continue;
                }

                var tagsScore = CalculateListHashSetMatchPercentage(GetItemsNotInHashSet(otherGame.TagIds, _controlSettings.TagsToIgnore), tagsSet) * _propertiesWeights["tags"];
                var genresScore = CalculateListHashSetMatchPercentage(GetItemsNotInHashSet(otherGame.GenreIds, _controlSettings.GenresToIgnore), genresSet) * _propertiesWeights["genres"];
                var categoriesScore = CalculateListHashSetMatchPercentage(GetItemsNotInHashSet(otherGame.CategoryIds, _controlSettings.CategoriesToIgnore), categoriesSet) * _propertiesWeights["categories"];

                var finalScore = tagsScore + genresScore + categoriesScore;
                if (finalScore >= minScoreThreshold)
                {
                    similarityScores.Add(otherGame, finalScore);
                }
            }

            var similarGames = similarityScores.OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key);

            return similarGames;
        }
    }
}