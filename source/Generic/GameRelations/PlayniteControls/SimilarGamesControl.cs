using GameRelations.Interfaces;
using GameRelations.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRelations.PlayniteControls
{
    public partial class SimilarGamesControl : GameRelationsBase
    {
        private readonly Dictionary<string, double> _propertiesWeights;
        private const double _minMatchValueFactor = 0.73;
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

        internal override object CreateMatchingSettingsSnapshot()
        {
            return new SimilarGamesMatchingSettings
            {
                TagsToIgnore = _controlSettings.TagsToIgnore.ToHashSet(),
                CategoriesToIgnore = _controlSettings.CategoriesToIgnore.ToHashSet(),
                GenresToIgnore = _controlSettings.GenresToIgnore.ToHashSet(),
                ExcludeGamesSameSeries = _controlSettings.ExcludeGamesSameSeries
            };
        }

        internal override IEnumerable<GameRelationSnapshot> GetMatchingGames(GameRelationSnapshot game, List<GameRelationSnapshot> libraryGames, object matchingSettings)
        {
            var controlSettings = (SimilarGamesMatchingSettings)matchingSettings;
            var tagsSet = GetItemsNotInHashSet(game.TagIds, controlSettings.TagsToIgnore).ToHashSet();
            var genresSet = GetItemsNotInHashSet(game.GenreIds, controlSettings.GenresToIgnore).ToHashSet();
            var categoriesSet = GetItemsNotInHashSet(game.CategoryIds, controlSettings.CategoriesToIgnore).ToHashSet();
            var seriesHashSet = game.SeriesIds.ToHashSet();

            var minScoreThreshold = _propertiesWeights.Count * _minMatchValueFactor;
            var similarityScores = new Dictionary<GameRelationSnapshot, double>();
            foreach (var otherGame in libraryGames)
            {
                if (otherGame.Id == game.Id)
                {
                    continue;
                }

                if (!game.Hidden && otherGame.Hidden)
                {
                    continue;
                }

                if (controlSettings.ExcludeGamesSameSeries && HashSetContainsAnyItem(otherGame.SeriesIds, seriesHashSet))
                {
                    continue;
                }

                var tagsScore = CalculateJaccardSimilarity(GetItemsNotInHashSet(otherGame.TagIds, controlSettings.TagsToIgnore), tagsSet) * _propertiesWeights["tags"];
                var genresScore = CalculateJaccardSimilarity(GetItemsNotInHashSet(otherGame.GenreIds, controlSettings.GenresToIgnore), genresSet) * _propertiesWeights["genres"];
                var categoriesScore = CalculateJaccardSimilarity(GetItemsNotInHashSet(otherGame.CategoryIds, controlSettings.CategoriesToIgnore), categoriesSet) * _propertiesWeights["categories"];

                var finalScore = tagsScore + genresScore + categoriesScore;
                if (finalScore >= minScoreThreshold)
                {
                    similarityScores.Add(otherGame, finalScore);
                }
            }

            return similarityScores.OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key);
        }
    }
}
