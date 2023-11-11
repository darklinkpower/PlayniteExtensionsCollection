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
    }

    public class GameRelationsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly GameRelations plugin;
        private GameRelationsSettings editingClone { get; set; }
        public ObservableCollection<Tag> SimilarGamesExcludeTags { get; private set; } = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> SimilarGamesNotExcludeTags { get; private set; } = new ObservableCollection<Tag>();

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

        private ObservableCollection<Tag> _sgNotExcludeTagsSelectedItems = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> SgNotExcludeTagsSelectedItems
        {
            get { return _sgNotExcludeTagsSelectedItems; }
            set
            {
                _sgNotExcludeTagsSelectedItems = value;
                OnPropertyChanged(nameof(SgNotExcludeTagsSelectedItems));
            }
        }

        private ObservableCollection<Tag> _sgExcludeTagsSelectedItems = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> SgExcludeTagsSelectedItems
        {
            get { return _sgExcludeTagsSelectedItems; }
            set
            {
                _sgExcludeTagsSelectedItems = value;
                OnPropertyChanged(nameof(SgExcludeTagsSelectedItems));
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
            }
            else
            {
                Settings = new GameRelationsSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            var excludeTags = new List<Tag>();
            var notExcludeTags = new List<Tag>();
            foreach (var tag in plugin.PlayniteApi.Database.Tags)
            {
                if (Settings.SimilarGamesControlSettings.TagsToIgnore.Contains(tag.Id))
                {
                    excludeTags.Add(tag);
                }
                else
                {
                    notExcludeTags.Add(tag);
                }
            }

            SimilarGamesNotExcludeTags = notExcludeTags.OrderBy(x => x.Name).ToObservable();
            SimilarGamesExcludeTags = excludeTags.OrderBy(x => x.Name).ToObservable();
        }

        public void CancelEdit()
        {
            Settings = editingClone;
            ClearEditingTags();
        }

        public void EndEdit()
        {
            Settings.SimilarGamesControlSettings.TagsToIgnore = SimilarGamesExcludeTags.Select(x => x.Id).ToHashSet();
            plugin.SavePluginSettings(Settings);
            ClearEditingTags();
        }

        private void ClearEditingTags()
        {
            SimilarGamesExcludeTags.Clear();
            SimilarGamesNotExcludeTags.Clear();
            SgExcludeTagsSelectedItems.Clear();
            SgNotExcludeTagsSelectedItems.Clear();
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
        }

        public RelayCommand AddSelectedTagsToExcludeCommand
        {
            get => new RelayCommand(() =>
            {
                foreach (var tag in SgNotExcludeTagsSelectedItems.ToList())
                {
                    SgNotExcludeTagsSelectedItems.Remove(tag);
                    SimilarGamesNotExcludeTags.Remove(tag);
                    SimilarGamesExcludeTags.Add(tag);
                }
            }, () => SgNotExcludeTagsSelectedItems.Count > 0);
        }

        public RelayCommand RemoveSelectedTagsFromExcludeCommand
        {
            get => new RelayCommand(() =>
            {
                foreach (var tag in SgExcludeTagsSelectedItems.ToList())
                {
                    SgExcludeTagsSelectedItems.Remove(tag);
                    SimilarGamesExcludeTags.Remove(tag);
                    SimilarGamesNotExcludeTags.Add(tag);
                }
            }, () => SgExcludeTagsSelectedItems.Count > 0);
        }


    }
}