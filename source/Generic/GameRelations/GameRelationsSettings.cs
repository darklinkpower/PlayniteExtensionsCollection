using GameRelations.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
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
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}