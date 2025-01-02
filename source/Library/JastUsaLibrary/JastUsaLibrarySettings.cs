using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.JastLibraryCacheService.Entities;
using JastUsaLibrary.JastUsaIntegration.Application.Services;
using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace JastUsaLibrary
{
    public class GameCache
    {
        public ObservableCollection<JastAssetWrapper> Assets;
        public JastProduct Product = null;
        public Program Program = null;
        public string GameId;
        public GameCache GetClone()
        {
            // Create a new instance of GameCache
            var clone = new GameCache();
            if (Assets != null)
            {
                clone.Assets = new ObservableCollection<JastAssetWrapper>(
                    Assets.Select(asset => Serialization.GetClone(asset)).ToList());
            }

            if (Product != null)
            {
                clone.Product = Serialization.GetClone(Product);
            }
            if (Program != null)
            {
                clone.Program = Serialization.GetClone(Program);
            }

            clone.GameId = GameId;
            return clone;
        }
    }

    public class DownloadSettings : ObservableObject
    {

        private bool _extractOnDownload = true;
        public bool ExtractOnDownload { get => _extractOnDownload; set => SetValue(ref _extractOnDownload, value); }

        private bool _deleteOnExtract = true;
        public bool DeleteOnExtract { get => _deleteOnExtract; set => SetValue(ref _deleteOnExtract, value); }

        private string _downloadDirectory;
        public string DownloadDirectory { get => _downloadDirectory; set => SetValue(ref _downloadDirectory, value); }

        private string _extractDirectory;
        public string ExtractDirectory { get => _extractDirectory; set => SetValue(ref _extractDirectory, value); }
    }

    public class JastUsaLibrarySettings : ObservableObject
    {
        private int _settingsVersion = 1;
        public int SettingsVersion { get => _settingsVersion; set => SetValue(ref _settingsVersion, value); }

        private uint _maximumConcurrentDownloads = 2;
        public uint MaximumConcurrentDownloads { get => _maximumConcurrentDownloads; set => SetValue(ref _maximumConcurrentDownloads, value); }

        private bool _startDownloadsOnStartup = true;
        public bool StartDownloadsOnStartup { get => _startDownloadsOnStartup; set => SetValue(ref _startDownloadsOnStartup, value); }

        private Dictionary<string, GameCache> _libraryCache = new Dictionary<string, GameCache>();
        public Dictionary<string, GameCache> LibraryCache { get => _libraryCache; set => SetValue(ref _libraryCache, value); }
        public List<DownloadData> DownloadsData { get; set; } = new List<DownloadData>();

        private DownloadSettings _gamesDownloadSettings;
        public DownloadSettings GamesDownloadSettings { get => _gamesDownloadSettings; set => SetValue(ref _gamesDownloadSettings, value); }

        private DownloadSettings _patchesDownloadSettings;
        public DownloadSettings PatchesDownloadSettings { get => _patchesDownloadSettings; set => SetValue(ref _patchesDownloadSettings, value); }

        private DownloadSettings _extrasDownloadSettings;
        public DownloadSettings ExtrasDownloadSettings { get => _extrasDownloadSettings; set => SetValue(ref _extrasDownloadSettings, value); }
    }

    public class JastUsaLibrarySettingsViewModel : ObservableObject, ISettings
    {
        private readonly JastUsaLibrary _plugin;
        private JastUsaLibrarySettings editingClone { get; set; }

        private JastUsaLibrarySettings settings;
        private readonly IPlayniteAPI _playniteApi;
        private readonly JastUsaAccountClient _accountClient;

        public JastUsaLibrarySettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private string isUserLoggedIn = null;
        public string IsUserLoggedIn
        {
            get
            {
                if (isUserLoggedIn is null)
                {
                    isUserLoggedIn = _accountClient.GetIsUserLoggedIn().ToString();
                }

                return isUserLoggedIn;
            }
            set
            {
                isUserLoggedIn = value;
                OnPropertyChanged();
            }
        }

        private string loginEmail = string.Empty;
        public string LoginEmail
        {
            get => loginEmail;
            set
            {
                loginEmail = value;
                OnPropertyChanged();
            }
        }

        public JastUsaLibrarySettingsViewModel(JastUsaLibrary plugin, IPlayniteAPI api, JastUsaAccountClient accountClient)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            _plugin = plugin;

            // Load saved settings.
            var savedSettings = _plugin.LoadPluginSettings<JastUsaLibrarySettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new JastUsaLibrarySettings();
            }

            _playniteApi = api;
            _accountClient = accountClient;
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            var defaultBaseDownloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "JAST Downloads");
            var downloadsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (FileSystem.DirectoryExists(downloadsFolderPath))
            {
                defaultBaseDownloadsPath = Path.Combine(downloadsFolderPath, "JAST Downloads");
            }

            if (settings.GamesDownloadSettings is null)
            {
                var downloadsPath = Path.Combine(defaultBaseDownloadsPath, "Games");
                settings.GamesDownloadSettings = new DownloadSettings
                {
                    DownloadDirectory = downloadsPath,
                    ExtractDirectory = downloadsPath
                };
            }

            if (settings.PatchesDownloadSettings is null)
            {
                var downloadsPath = Path.Combine(defaultBaseDownloadsPath, "Patches");
                settings.PatchesDownloadSettings = new DownloadSettings
                {
                    DownloadDirectory = downloadsPath,
                    ExtractDirectory = downloadsPath
                };
            }

            if (settings.ExtrasDownloadSettings is null)
            {
                var downloadsPath = Path.Combine(defaultBaseDownloadsPath, "Extras");
                settings.ExtrasDownloadSettings = new DownloadSettings
                {
                    DownloadDirectory = downloadsPath,
                    ExtractDirectory = downloadsPath
                };
            }
        }

        public void UpgradeSettings()
        {
            var settingsUpdated = false;
            if (settings.SettingsVersion < 2)
            {
                var libraryGames = _playniteApi.Database.Games.Where(g => g.PluginId == _plugin.Id);
                var gamesInstallCache = new List<GameInstallCache>();
                var gameInstallCachePath = Path.Combine(_plugin.GetPluginUserDataPath(), "gameInstallCache.json");
                if (FileSystem.FileExists(gameInstallCachePath))
                {
                    gamesInstallCache = Serialization.FromJsonFile<List<GameInstallCache>>(gameInstallCachePath);
                }

                var cache = new List<JastProduct>();
                if (FileSystem.FileExists(_plugin.UserGamesCachePath))
                {
                    cache = Serialization.FromJsonFile<List<JastProduct>>(_plugin.UserGamesCachePath);
                }

                foreach (var game in libraryGames)
                {
                    var gameCache = new GameCache
                    {
                        GameId = game.GameId
                    };

                    var gameVariant = cache.FirstOrDefault(x => x.ProductVariant.GameId.ToString() == game.GameId);
                    if (gameVariant != null)
                    {
                        gameCache.Product = gameVariant;
                    }

                    var gameInstallCache = gamesInstallCache.FirstOrDefault(x => x.GameId == game.GameId);
                    if (gameInstallCache != null)
                    {
                        gameCache.Program = gameInstallCache.Program;
                    }

                    settings.LibraryCache[game.GameId] = gameCache;
                }

                settings.SettingsVersion = 2;
                settingsUpdated = true;
            }

            if (settingsUpdated)
            {
                SaveSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            isUserLoggedIn = null;
            LoginEmail = string.Empty;
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
            _plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        public RelayCommand<PasswordBox> LoginCommand
        {
            get => new RelayCommand<PasswordBox>((a) =>
            {
                Login(a);
            });
        }

        private void Login(PasswordBox passwordBox)
        {
            if (!LoginEmail.IsNullOrEmpty() && !passwordBox.Password.IsNullOrEmpty())
            {
                isUserLoggedIn = null;
                IsUserLoggedIn = _accountClient.Login(LoginEmail, passwordBox.Password, true).ToString();
            }
        }

        internal void SaveSettings()
        {
            _plugin.SavePluginSettings(settings);
        }

        public RelayCommand<DownloadSettings> SelectDownloadDirectoryCommand
        {
            get => new RelayCommand<DownloadSettings>((DownloadSettings downloadSettings) =>
            {
                var selectedDir = _playniteApi.Dialogs.SelectFolder();
                if (!selectedDir.IsNullOrEmpty())
                {
                    downloadSettings.DownloadDirectory = selectedDir;
                }
            });
        }

        public RelayCommand<DownloadSettings> SelectExtractDirectoryCommand
        {
            get => new RelayCommand<DownloadSettings>((DownloadSettings downloadSettings) =>
            {
                var selectedDir = _playniteApi.Dialogs.SelectFolder();
                if (!selectedDir.IsNullOrEmpty())
                {
                    downloadSettings.ExtractDirectory = selectedDir;
                }
            });
        }
    }
}