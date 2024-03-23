using GameRelations.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRelations.PlayniteControls
{
    public partial class SimilarGamesControl : GameRelationsBase
    {
        private class GameToMatchInfo
        {
            public Game Game { get; set; }
            public Dictionary<GameField, HashSet<Guid>> FilteredValues { get; set; }
            public HashSet<Guid> SeriesIds { get; set; }

            public GameToMatchInfo(Game game)
            {
                Game = game;
                SeriesIds = game.SeriesIds?.ToHashSet() ?? new HashSet<Guid>();
                FilteredValues = new Dictionary<GameField, HashSet<Guid>>();
            }
        }

        private readonly SimilarGamesControlSettings _controlSettings;

        public SimilarGamesControl(IPlayniteAPI playniteApi, GameRelationsSettings settings, SimilarGamesControlSettings controlSettings)
            : base(playniteApi, settings, controlSettings)
        {
            _controlSettings = controlSettings;
        }

        public override IEnumerable<Game> GetMatchingGames(Game game)
        {
            var gtm = GetGameToMatchInfo(game);

            var similarityScores = new Dictionary<Game, double>();
            foreach (var otherGame in PlayniteApi.Database.Games)
            {
                if (otherGame.Id == gtm.Game.Id)
                {
                    continue;
                }

                if (!gtm.Game.Hidden && otherGame.Hidden)
                {
                    continue;
                }

                if (_controlSettings.ExcludeGamesSameSeries && HashSetContainsAnyItem(otherGame.SeriesIds, gtm.SeriesIds))
                {
                    continue;
                }

                if (GamesAreSimilar(gtm, otherGame, out double similarity))
                {
                    similarityScores.Add(otherGame, similarity);
                }
            }

            var similarGames = similarityScores.OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key);

            return similarGames;
        }

        private GameToMatchInfo GetGameToMatchInfo(Game game)
        {
            var gtm = new GameToMatchInfo(game);
            foreach (var field in _controlSettings.FieldSettings)
            {
                if (field.Enabled)
                {
                    gtm.FilteredValues[field.Field] = GetFilteredValue(game, field.Field).ToHashSet();
                }
            }
            return gtm;
        }

        protected HashSet<Guid> GetItemsToIgnore(GameField field)
        {
            switch (field)
            {
                case GameField.TagIds: return _controlSettings.TagsToIgnore;
                case GameField.GenreIds: return _controlSettings.GenresToIgnore;
                case GameField.CategoryIds: return _controlSettings.CategoriesToIgnore;
                default: return new HashSet<Guid>();
            }
        }

        private static List<Guid> GetFieldValue(Game game, GameField field)
        {
            switch (field)
            {
                case GameField.TagIds: return game.TagIds;
                case GameField.GenreIds: return game.GenreIds;
                case GameField.CategoryIds: return game.CategoryIds;
                default: return new List<Guid>();
            }
        }

        private IEnumerable<Guid> GetFilteredValue(Game game, GameField field)
        {
            return GetItemsNotInHashSet(GetFieldValue(game, field), GetItemsToIgnore(field));
        }

        private bool GamesAreSimilar(GameToMatchInfo gameToMatch, Game otherGame, out double similarity)
        {
            double matchThreshold = 0;
            similarity = 0;

            foreach (var field in _controlSettings.FieldSettings)
            {
                if (!field.Enabled || !gameToMatch.FilteredValues.TryGetValue(field.Field, out var propertyValues))
                {
                    continue;
                }

                var otherValues = GetFilteredValue(otherGame, field.Field);
                if (!propertyValues.Any() && !otherValues.Any())
                {
                    continue;
                }

                matchThreshold += _controlSettings.JacardSimilarityPerField;
                similarity += CalculateJaccardSimilarity(otherValues, propertyValues) * field.Weight;
            }

            return matchThreshold > 0 && similarity > matchThreshold;
        }
    }
}