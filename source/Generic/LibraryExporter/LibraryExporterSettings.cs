using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExporter
{
    public class ExportSettings : ObservableObject
    {
        // Added
        private bool added = false;
        public bool Added { get => added; set => SetValue(ref added, value); }
        // AgeRatings
        private bool ageRatings = false;
        public bool AgeRatings { get => ageRatings; set => SetValue(ref ageRatings, value); }
        // Categories
        private bool categories = false;
        public bool Categories { get => categories; set => SetValue(ref categories, value); }
        // CommunityScore
        private bool communityScore = false;
        public bool CommunityScore { get => communityScore; set => SetValue(ref communityScore, value); }
        // CriticScore
        private bool criticScore = false;
        public bool CriticScore { get => criticScore; set => SetValue(ref criticScore, value); }
        // Description
        private bool description = false;
        public bool Description { get => description; set => SetValue(ref description, value); }
        // Developers
        private bool developers = true;
        public bool Developers { get => developers; set => SetValue(ref developers, value); }
        // Favorite
        private bool favorite = false;
        public bool Favorite { get => favorite; set => SetValue(ref favorite, value); }
        // Features
        private bool features = false;
        public bool Features { get => features; set => SetValue(ref features, value); }
        // GameId
        private bool gameId = false;
        public bool GameId { get => gameId; set => SetValue(ref gameId, value); }
        // Id
        private bool id = false;
        public bool Id { get => id; set => SetValue(ref id, value); }
        // Genres
        private bool genres = false;
        public bool Genres { get => genres; set => SetValue(ref genres, value); }
        // Hidden
        private bool hidden = false;
        public bool Hidden { get => hidden; set => SetValue(ref hidden, value); }
        // InstallDirectory
        private bool installDirectory = false;
        public bool InstallDirectory { get => installDirectory; set => SetValue(ref installDirectory, value); }
        // InstallSize
        private bool installSize = false;
        public bool InstallSize { get => installSize; set => SetValue(ref installSize, value); }
        // IsInstalled
        private bool isInstalled = true;
        public bool IsInstalled { get => isInstalled; set => SetValue(ref isInstalled, value); }
        // LastActivity
        private bool lastActivity = false;
        public bool LastActivity { get => lastActivity; set => SetValue(ref lastActivity, value); }
        // Links
        private bool links = false;
        public bool Links { get => links; set => SetValue(ref links, value); }
        // Manual
        private bool manual = false;
        public bool Manual { get => manual; set => SetValue(ref manual, value); }
        // Modified
        private bool modified = false;
        public bool Modified { get => modified; set => SetValue(ref modified, value); }
        // Notes
        private bool notes = false;
        public bool Notes { get => notes; set => SetValue(ref notes, value); }
        // Platforms
        private bool platforms = true;
        public bool Platforms { get => platforms; set => SetValue(ref platforms, value); }
        // PlayCount
        private bool playCount = false;
        public bool PlayCount { get => playCount; set => SetValue(ref playCount, value); }
        // Playtime
        private bool playtime = false;
        public bool Playtime { get => playtime; set => SetValue(ref playtime, value); }
        // PluginId
        private bool pluginId = false;
        public bool PluginId { get => pluginId; set => SetValue(ref pluginId, value); }
        // Publishers
        private bool publishers = true;
        public bool Publishers { get => publishers; set => SetValue(ref publishers, value); }
        // RecentActivity
        private bool recentActivity = false;
        public bool RecentActivity { get => recentActivity; set => SetValue(ref recentActivity, value); }
        // Roms
        private bool roms = false;
        public bool Roms { get => roms; set => SetValue(ref roms, value); }
        // Regions
        private bool regions = false;
        public bool Regions { get => regions; set => SetValue(ref regions, value); }
        // ReleaseDate
        private bool releaseDate = false;
        public bool ReleaseDate { get => releaseDate; set => SetValue(ref releaseDate, value); }
        // Series
        private bool series = false;
        public bool Series { get => series; set => SetValue(ref series, value); }
        // Source
        private bool source = true;
        public bool Source { get => source; set => SetValue(ref source, value); }
        // Tags
        private bool tags = false;
        public bool Tags { get => tags; set => SetValue(ref tags, value); }
        // UserScore
        private bool userScore = false;
        public bool UserScore { get => userScore; set => SetValue(ref userScore, value); }
        // Version
        private bool version = false;
        public bool Version { get => version; set => SetValue(ref version, value); }
    }

    public class LibraryExporterSettings : ObservableObject
    {
        private ExportSettings exportSettings = new ExportSettings();
        public ExportSettings ExportSettings { get => exportSettings; set => SetValue(ref exportSettings, value); }
        private string listsSeparator = ", ";
        public string ListsSeparator { get => listsSeparator; set => SetValue(ref listsSeparator, value); }
    }

    public class LibraryExporterSettingsViewModel : ObservableObject, ISettings
    {
        private readonly LibraryExporter plugin;
        private LibraryExporterSettings editingClone { get; set; }

        private LibraryExporterSettings settings;
        public LibraryExporterSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public LibraryExporterSettingsViewModel(LibraryExporter plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<LibraryExporterSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new LibraryExporterSettings();
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