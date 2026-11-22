using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace GameRelations.Models
{
    internal sealed class GameRelationSnapshot
    {
        public Game Game { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SortingName { get; set; }
        public bool Hidden { get; set; }
        public bool IsInstalled { get; set; }
        public string CoverImagePath { get; set; }
        public List<Guid> TagIds { get; set; }
        public List<Guid> GenreIds { get; set; }
        public List<Guid> CategoryIds { get; set; }
        public List<Guid> SeriesIds { get; set; }
        public List<GameRelationValueSnapshot> Series { get; set; }
        public List<GameRelationValueSnapshot> Developers { get; set; }
        public List<GameRelationValueSnapshot> Publishers { get; set; }

        public string SortName
        {
            get
            {
                return string.IsNullOrEmpty(SortingName) ? Name : SortingName;
            }
        }
    }

    internal sealed class GameRelationValueSnapshot
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    internal sealed class GameRelationsUpdateResult
    {
        public List<MatchedGameWrapper> GameWrappers { get; set; }
    }

    internal sealed class GameRelationsUpdateSettings
    {
        public int MaxItems { get; set; }
        public bool DisplayOnlyInstalled { get; set; }
        public int CoversHeight { get; set; }
        public object MatchingSettings { get; set; }
    }

    internal sealed class SimilarGamesMatchingSettings
    {
        public HashSet<Guid> TagsToIgnore { get; set; }
        public HashSet<Guid> CategoriesToIgnore { get; set; }
        public HashSet<Guid> GenresToIgnore { get; set; }
        public bool ExcludeGamesSameSeries { get; set; }
    }
}
