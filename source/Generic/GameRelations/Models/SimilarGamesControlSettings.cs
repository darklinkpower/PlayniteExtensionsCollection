using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace GameRelations.Models
{
    public class SimilarGamesControlSettings : GameRelationsControlSettings
    {
        public HashSet<Guid> TagsToIgnore = new HashSet<Guid>();
        public HashSet<Guid> CategoriesToIgnore = new HashSet<Guid>();
        public HashSet<Guid> GenresToIgnore = new HashSet<Guid>();

        public ObservableCollection<SimilarGamesFieldSettings> FieldSettings { get; set; } = new ObservableCollection<SimilarGamesFieldSettings>();

        public SimilarGamesFieldSettings Tags = new SimilarGamesFieldSettings();
        public SimilarGamesFieldSettings Genres = new SimilarGamesFieldSettings();
        public SimilarGamesFieldSettings Categories = new SimilarGamesFieldSettings();

        private bool excludeGamesSameSeries = true;
        public bool ExcludeGamesSameSeries { get => excludeGamesSameSeries; set => SetValue(ref excludeGamesSameSeries, value); }

        private double jacardSimilarityPerField = 0.73D;
        public double JacardSimilarityPerField { get => jacardSimilarityPerField; set => SetValue(ref jacardSimilarityPerField, value); }

        public SimilarGamesControlSettings()
        {
        }

        public SimilarGamesControlSettings(bool displayGameNames, int maxItems, bool isEnabled) : base(displayGameNames, maxItems, isEnabled)
        {
        }
    }

    public class SimilarGamesFieldSettings : ObservableObject
    {
        public GameField Field { get; set; }
        public string Name { get; set; }

        private bool enabled = true;
        public bool Enabled { get => enabled; set=> SetValue(ref enabled, value); }

        private double weight = 1.0;
        public double Weight { get => weight; set => SetValue(ref weight, value); }

        public SimilarGamesFieldSettings() { }

        public SimilarGamesFieldSettings(GameField field, string name, bool enabled, double weight)
        {
            Field = field;
            Name = name;
            Enabled = enabled;
            Weight = weight;
        }
    }
}