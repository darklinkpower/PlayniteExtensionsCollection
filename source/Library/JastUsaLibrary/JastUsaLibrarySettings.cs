using JastUsaLibrary.DownloadManager.Models;
using JastUsaLibrary.Models;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services;
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
    }

    public class JastUsaLibrarySettings : ObservableObject
    {
        private int _settingsVersion = 1;
        public int SettingsVersion { get => _settingsVersion; set => SetValue(ref _settingsVersion, value); }
        private string _gameDownloadsPath;
        public string GameDownloadsPath { get => _gameDownloadsPath; set => SetValue(ref _gameDownloadsPath, value); }

        private string _extrasDownloadsPath;
        public string ExtrasDownloadsPath { get => _extrasDownloadsPath; set => SetValue(ref _extrasDownloadsPath, value); }

        private string _patchDownloadsPath;
        public string PatchDownloadsPath { get => _patchDownloadsPath; set => SetValue(ref _patchDownloadsPath, value); }

        private bool _extractFilesOnDownload = true;
        public bool ExtractFilesOnDownload { get => _extractFilesOnDownload; set => SetValue(ref _extractFilesOnDownload, value); }
        private bool _deleteFilesOnExtract = false;
        public bool DeleteFilesOnExtract { get => _deleteFilesOnExtract; set => SetValue(ref _deleteFilesOnExtract, value); }

        private uint _maximumConcurrentDownloads = 2;
        public uint MaximumConcurrentDownloads { get => _maximumConcurrentDownloads; set => SetValue(ref _maximumConcurrentDownloads, value); }

        private bool _startDownloadsOnStartup = true;
        public bool StartDownloadsOnStartup { get => _startDownloadsOnStartup; set => SetValue(ref _startDownloadsOnStartup, value); }

        private Dictionary<string, GameCache> _libraryCache = new Dictionary<string, GameCache>();
        public Dictionary<string, GameCache> LibraryCache { get => _libraryCache; set => SetValue(ref _libraryCache, value); }
        public List<DownloadData> DownloadsData { get; set; } = new List<DownloadData>();
    }

    public class JastUsaLibrarySettingsViewModel : ObservableObject, ISettings
    {
        private readonly JastUsaLibrary plugin;
        private JastUsaLibrarySettings editingClone { get; set; }

        private JastUsaLibrarySettings settings;
        private readonly IPlayniteAPI playniteApi;
        private readonly JastUsaAccountClient accountClient;

        public JastUsaLibrarySettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private bool? isUserLoggedIn = null;
        public bool? IsUserLoggedIn
        {
            get
            {
                if (isUserLoggedIn == null)
                {
                    isUserLoggedIn = accountClient.GetIsUserLoggedIn();
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
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<JastUsaLibrarySettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new JastUsaLibrarySettings();
            }

            playniteApi = api;
            this.accountClient = accountClient;

            var defaultBaseDownloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "JAST Downloads");
            var downloadsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (FileSystem.DirectoryExists(downloadsFolderPath))
            {
                defaultBaseDownloadsPath = Path.Combine(downloadsFolderPath, "JAST Downloads");
            }

            if (settings.GameDownloadsPath.IsNullOrEmpty())
            {
                settings.GameDownloadsPath = Path.Combine(defaultBaseDownloadsPath, "Games");
            }

            if (settings.ExtrasDownloadsPath.IsNullOrEmpty())
            {
                settings.ExtrasDownloadsPath = Path.Combine(defaultBaseDownloadsPath, "Extras");
            }

            if (settings.PatchDownloadsPath.IsNullOrEmpty())
            {
                settings.PatchDownloadsPath = Path.Combine(defaultBaseDownloadsPath, "Patches");
            }
        }

        public void UpgradeSettings()
        {
            var settingsUpdated = false;
            if (settings.SettingsVersion < 2)
            {
                var libraryGames = playniteApi.Database.Games.Where(g => g.PluginId == plugin.Id);
                var gamesInstallCache = new List<GameInstallCache>();
                var gameInstallCachePath = Path.Combine(plugin.GetPluginUserDataPath(), "gameInstallCache.json");
                if (FileSystem.FileExists(gameInstallCachePath))
                {
                    gamesInstallCache = Serialization.FromJsonFile<List<GameInstallCache>>(gameInstallCachePath);
                }

                var cache = new List<JastProduct>();
                if (FileSystem.FileExists(plugin.UserGamesCachePath))
                {
                    cache = Serialization.FromJsonFile<List<JastProduct>>(plugin.UserGamesCachePath);
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
                plugin.SavePluginSettings();
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
                IsUserLoggedIn = accountClient.Login(LoginEmail, passwordBox.Password);
            }
        }

        public RelayCommand SelectGamesDownloadDirectoryCommand
        {
            get => new RelayCommand(() =>
            {
                var selectedDir = playniteApi.Dialogs.SelectFolder();
                if (!selectedDir.IsNullOrEmpty())
                {
                    settings.GameDownloadsPath = selectedDir;
                }
            });
        }

        public RelayCommand SelectPatchesDownloadDirectoryCommand
        {
            get => new RelayCommand(() =>
            {
                var selectedDir = playniteApi.Dialogs.SelectFolder();
                if (!selectedDir.IsNullOrEmpty())
                {
                    settings.PatchDownloadsPath = selectedDir;
                }
            });
        }

        public RelayCommand SelectExtrasDownloadDirectoryCommand
        {
            get => new RelayCommand(() =>
            {
                var selectedDir = playniteApi.Dialogs.SelectFolder();
                if (!selectedDir.IsNullOrEmpty())
                {
                    settings.ExtrasDownloadsPath = selectedDir;
                }
            });
        }
    }
}