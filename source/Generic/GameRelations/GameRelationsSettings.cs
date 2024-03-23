using GameRelations.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRelations
{
    public class GameRelationsSettings : ObservableObject
    {
        private int coversHeight = 150;
        public int CoversHeight { get => coversHeight; set => SetValue(ref coversHeight, value); }

        private GameRelationsControlSettings sameDeveloperControlSettings = new GameRelationsControlSettings();
        public GameRelationsControlSettings SameDeveloperControlSettings { get => sameDeveloperControlSettings; set => SetValue(ref sameDeveloperControlSettings, value); }

        private GameRelationsControlSettings samePublisherControlSettings = new GameRelationsControlSettings();
        public GameRelationsControlSettings SamePublisherControlSettings { get => samePublisherControlSettings; set => SetValue(ref samePublisherControlSettings, value); }

        private GameRelationsControlSettings sameSeriesControlSettings = new GameRelationsControlSettings();
        public GameRelationsControlSettings SameSeriesControlSettings { get => sameSeriesControlSettings; set => SetValue(ref sameSeriesControlSettings, value); }

        private SimilarGamesControlSettings similarGamesControlSettings = new SimilarGamesControlSettings();
        public SimilarGamesControlSettings SimilarGamesControlSettings { get => similarGamesControlSettings; set => SetValue(ref similarGamesControlSettings, value); }

        public int SettingsVersion = 1;
    }

    public class GameRelationsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly GameRelations plugin;
        private GameRelationsSettings editingClone { get; set; }
        public ObservableCollection<DatabaseObject> SimilarGamesExcludeTags { get; private set; } = new ObservableCollection<DatabaseObject>();
        public ObservableCollection<DatabaseObject> SimilarGamesNotExcludeTags { get; private set; } = new ObservableCollection<DatabaseObject>();
        public ObservableCollection<DatabaseObject> SimilarGamesExcludeGenres { get; private set; } = new ObservableCollection<DatabaseObject>();
        public ObservableCollection<DatabaseObject> SimilarGamesNotExcludeGenres { get; private set; } = new ObservableCollection<DatabaseObject>();
        public ObservableCollection<DatabaseObject> SimilarGamesExcludeCategories { get; private set; } = new ObservableCollection<DatabaseObject>();
        public ObservableCollection<DatabaseObject> SimilarGamesNotExcludeCategories { get; private set; } = new ObservableCollection<DatabaseObject>();

        public ObservableCollection<DatabaseObject> SgNotExcludeTagsSelectedItems = new ObservableCollection<DatabaseObject>();
        public ObservableCollection<DatabaseObject> SgExcludeTagsSelectedItems = new ObservableCollection<DatabaseObject>();

        public ObservableCollection<DatabaseObject> SgNotExcludeGenresSelectedItems = new ObservableCollection<DatabaseObject>();
        public ObservableCollection<DatabaseObject> SgExcludeGenresSelectedItems = new ObservableCollection<DatabaseObject>();

        public ObservableCollection<DatabaseObject> SgNotExcludeCategoriesSelectedItems = new ObservableCollection<DatabaseObject>();
        public ObservableCollection<DatabaseObject> SgExcludeCategoriesSelectedItems = new ObservableCollection<DatabaseObject>();

        private GameRelationsSettings settings;
        public GameRelationsSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public GameRelationsSettingsViewModel(GameRelations plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<GameRelationsSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
                UpgradeSettings();
            }
            else
            {
                Settings = new GameRelationsSettings();
                SetAdvancedSectionDefaults();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);

            AddItemsToExclusionCollections(SimilarGamesExcludeTags, SimilarGamesNotExcludeTags, plugin.PlayniteApi.Database.Tags, Settings.SimilarGamesControlSettings.TagsToIgnore);
            AddItemsToExclusionCollections(SimilarGamesExcludeGenres, SimilarGamesNotExcludeGenres, plugin.PlayniteApi.Database.Genres, Settings.SimilarGamesControlSettings.GenresToIgnore);
            AddItemsToExclusionCollections(SimilarGamesExcludeCategories, SimilarGamesNotExcludeCategories, plugin.PlayniteApi.Database.Categories, Settings.SimilarGamesControlSettings.CategoriesToIgnore);
        }

        private void AddItemsToExclusionCollections<T>(ObservableCollection<DatabaseObject> excludeCollection, ObservableCollection<DatabaseObject> notExcludeCollection, IItemCollection<T> itemCollection, HashSet<Guid> idsToExclude) where T : DatabaseObject
        {
            excludeCollection.Clear();
            notExcludeCollection.Clear();
            var excludeList = new List<DatabaseObject>();
            var notExcludeList = new List<DatabaseObject>();
            foreach (var item in itemCollection)
            {
                if (idsToExclude.Contains(item.Id))
                {
                    excludeList.Add(item);
                }
                else
                {
                    notExcludeList.Add(item);
                }
            }

            excludeList.OrderBy(x => x.Name).ForEach(x => excludeCollection.Add(x));
            notExcludeList.OrderBy(x => x.Name).ForEach(x => notExcludeCollection.Add(x));
        }

        public void CancelEdit()
        {
            Settings = editingClone;
            ClearEditingTags();
        }

        public void EndEdit()
        {
            Settings.SimilarGamesControlSettings.TagsToIgnore = SimilarGamesExcludeTags.Select(x => x.Id).ToHashSet();
            Settings.SimilarGamesControlSettings.GenresToIgnore = SimilarGamesExcludeGenres.Select(x => x.Id).ToHashSet();
            Settings.SimilarGamesControlSettings.CategoriesToIgnore = SimilarGamesExcludeCategories.Select(x => x.Id).ToHashSet();

            plugin.SavePluginSettings(Settings);
            ClearEditingTags();
        }

        private void ClearEditingTags()
        {
            SimilarGamesExcludeTags.Clear();
            SimilarGamesNotExcludeTags.Clear();
            SimilarGamesExcludeGenres.Clear();
            SimilarGamesNotExcludeGenres.Clear();
            SimilarGamesExcludeCategories.Clear();
            SimilarGamesNotExcludeCategories.Clear();

            SgExcludeTagsSelectedItems.Clear();
            SgNotExcludeTagsSelectedItems.Clear();
            SgExcludeGenresSelectedItems.Clear();
            SgNotExcludeGenresSelectedItems.Clear();
            SgExcludeCategoriesSelectedItems.Clear();
            SgNotExcludeCategoriesSelectedItems.Clear();
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        public void NotifyCommandsPropertyChanged()
        {
            OnPropertyChanged(nameof(AddSelectedTagsToExcludeCommand));
            OnPropertyChanged(nameof(RemoveSelectedTagsFromExcludeCommand));

            OnPropertyChanged(nameof(AddSelectedGenresToExcludeCommand));
            OnPropertyChanged(nameof(RemoveSelectedGenresFromExcludeCommand));

            OnPropertyChanged(nameof(AddSelectedCategoriesToExcludeCommand));
            OnPropertyChanged(nameof(RemoveSelectedCategoriesFromExcludeCommand));
        }

        private void AddSelectedItemsToExclude(ObservableCollection<DatabaseObject> selectedItemsCollection, ObservableCollection<DatabaseObject> excludeCollection, ObservableCollection<DatabaseObject> notExcludeCollection)
        {
            foreach (var item in selectedItemsCollection.ToList())
            {
                selectedItemsCollection.Remove(item);
                notExcludeCollection.Remove(item);
                excludeCollection.Add(item);
            }
        }

        private void RemoveSelectedItemsFromExclude(ObservableCollection<DatabaseObject> selectedItemsCollection, ObservableCollection<DatabaseObject> excludeCollection, ObservableCollection<DatabaseObject> notExcludeCollection)
        {
            foreach (var item in selectedItemsCollection.ToList())
            {
                selectedItemsCollection.Remove(item);
                excludeCollection.Remove(item);
                notExcludeCollection.Add(item);
            }
        }

        private string GetResourceString(string key) => plugin.PlayniteApi.Resources.GetString(key);

        public void SetAdvancedSectionDefaults()
        {
            Settings.SimilarGamesControlSettings.JacardSimilarityPerField = 0.73D;
            Settings.SimilarGamesControlSettings.FieldSettings.Clear();
            settings.SimilarGamesControlSettings.FieldSettings.Add(new SimilarGamesFieldSettings(GameField.TagIds, GetResourceString("LOCTagsLabel"), true, 1));
            settings.SimilarGamesControlSettings.FieldSettings.Add(new SimilarGamesFieldSettings(GameField.GenreIds, GetResourceString("LOCGenresLabel"), true, 1.2));
            settings.SimilarGamesControlSettings.FieldSettings.Add(new SimilarGamesFieldSettings(GameField.CategoryIds, GetResourceString("LOCCategoriesLabel"), true, 1.3));
        }

        public void UpgradeSettings()
        {
            int currentVersion = 2;

            if (settings.SettingsVersion < 2)
                SetAdvancedSectionDefaults();

            Settings.SettingsVersion = currentVersion;
        }

        public RelayCommand AddSelectedTagsToExcludeCommand
        {
            get => new RelayCommand(() =>
            {
                AddSelectedItemsToExclude(SgNotExcludeTagsSelectedItems, SimilarGamesExcludeTags, SimilarGamesNotExcludeTags);
            }, () => SgNotExcludeTagsSelectedItems.Count > 0);
        }

        public RelayCommand RemoveSelectedTagsFromExcludeCommand
        {
            get => new RelayCommand(() =>
            {
                RemoveSelectedItemsFromExclude(SgExcludeTagsSelectedItems, SimilarGamesExcludeTags, SimilarGamesNotExcludeTags);
            }, () => SgExcludeTagsSelectedItems.Count > 0);
        }

        public RelayCommand AddSelectedGenresToExcludeCommand
        {
            get => new RelayCommand(() =>
            {
                AddSelectedItemsToExclude(SgNotExcludeGenresSelectedItems, SimilarGamesExcludeGenres, SimilarGamesNotExcludeGenres);
            }, () => SgNotExcludeGenresSelectedItems.Count > 0);
        }

        public RelayCommand RemoveSelectedGenresFromExcludeCommand
        {
            get => new RelayCommand(() =>
            {
                RemoveSelectedItemsFromExclude(SgExcludeGenresSelectedItems, SimilarGamesExcludeGenres, SimilarGamesNotExcludeGenres);
            }, () => SgExcludeGenresSelectedItems.Count > 0);
        }

        public RelayCommand AddSelectedCategoriesToExcludeCommand
        {
            get => new RelayCommand(() =>
            {
                AddSelectedItemsToExclude(SgNotExcludeCategoriesSelectedItems, SimilarGamesExcludeCategories, SimilarGamesNotExcludeCategories);
            }, () => SgNotExcludeCategoriesSelectedItems.Count > 0);
        }

        public RelayCommand RemoveSelectedCategoriesFromExcludeCommand
        {
            get => new RelayCommand(() =>
            {
                RemoveSelectedItemsFromExclude(SgExcludeCategoriesSelectedItems, SimilarGamesExcludeCategories, SimilarGamesNotExcludeCategories);
            }, () => SgExcludeCategoriesSelectedItems.Count > 0);
        }

        public RelayCommand SetAdvancedSectionDefaultsCommand
        {
            get => new RelayCommand(SetAdvancedSectionDefaults);
        }
    }
}