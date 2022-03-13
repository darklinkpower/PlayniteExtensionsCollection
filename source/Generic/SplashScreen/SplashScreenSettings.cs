using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SplashScreen
{
    public class SplashScreenSettings : ObservableObject
    {
        public bool ExecuteInDesktopMode { get; set; } = true;
        public bool ViewImageSplashscreenDesktopMode { get; set; } = true;
        public bool ViewVideoDesktopMode { get; set; } = true;
        public bool CloseSplashScreenDesktopMode { get; set; } = true;
        public bool ExecuteInFullscreenMode { get; set; } = true;
        public bool ViewImageSplashscreenFullscreenMode { get; set; } = true;
        public bool ViewVideoFullscreenMode { get; set; } = true;
        public bool CloseSplashScreenFullscreenMode { get; set; } = true;
        public bool ShowLogoInSplashscreen { get; set; } = true;
        public bool UseIconAsLogo { get; set; } = false;
        public HorizontalAlignment LogoHorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment LogoVerticalAlignment { get; set; } = VerticalAlignment.Bottom;
        public bool UseBlackSplashscreen { get; set; } = false;
        public bool UseGlobalSplashImage { get; set; } = false;
        public string GlobalSplashFile { get; set; } = string.Empty;
        public bool UseLogoInGlobalSplashImage { get; set; } = false;
        [DontSerialize]
        private string globalSplashImagePath { get; set; }
        [DontSerialize]
        public string GlobalSplashImagePath
        {
            get => globalSplashImagePath;
            set
            {
                globalSplashImagePath = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        public Dictionary<HorizontalAlignment, string> LogoHorizontalSource { get; set; } = new Dictionary<HorizontalAlignment, string>
        {
            { HorizontalAlignment.Left, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentLeftLabel") },
            { HorizontalAlignment.Center, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentCenterLabel") },
            { HorizontalAlignment.Right, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentRightLabel") },
        };

        [DontSerialize]
        public Dictionary<VerticalAlignment, string> LogoVerticalSource { get; set; } = new Dictionary<VerticalAlignment, string>
        {
            { VerticalAlignment.Top, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentTopLabel") },
            { VerticalAlignment.Center, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentCenterLabel") },
            { VerticalAlignment.Bottom, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentBottomLabel") },
        };

        public bool ControllerNoticeShowed { get; set; } = true;
    }

    public class SplashScreenSettingsViewModel : ObservableObject, ISettings
    {
        private readonly SplashScreen plugin;
        private readonly string pluginUserDataPath;
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();

        private SplashScreenSettings editingClone { get; set; }

        private SplashScreenSettings settings;
        public SplashScreenSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public SplashScreenSettingsViewModel(SplashScreen plugin, IPlayniteAPI playniteApi, string pluginUserDataPath)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SplashScreenSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SplashScreenSettings();
            }

            this.pluginUserDataPath = pluginUserDataPath;
            this.playniteApi = playniteApi;
            SetGlobalSplashImagePath();
        }

        private void SetGlobalSplashImagePath()
        {
            if (string.IsNullOrEmpty(Settings.GlobalSplashFile))
            {
                Settings.GlobalSplashImagePath = null;
                return;
            }

            var globalSplashImagePath = Path.Combine(pluginUserDataPath, Settings.GlobalSplashFile);
            if (FileSystem.FileExists(globalSplashImagePath))
            {
                Settings.GlobalSplashImagePath = globalSplashImagePath;
            }
            else
            {
                Settings.GlobalSplashImagePath = null;
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

        public RelayCommand BrowseSelectGlobalImageCommand
        {
            get => new RelayCommand(() =>
            {
                var filePath = playniteApi.Dialogs.SelectImagefile();
                if (!string.IsNullOrEmpty(filePath) && RemoveGlobalImage())
                {
                    var fileName = Path.GetFileName(filePath);
                    var globalSplashImagePath = Path.Combine(pluginUserDataPath, fileName);
                    try
                    {
                        FileSystem.CopyFile(filePath, globalSplashImagePath);
                        Settings.GlobalSplashFile = Path.GetFileName(filePath);
                        SetGlobalSplashImagePath();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, $"Error copying global splash image from {filePath} to {globalSplashImagePath}");
                    }
                }
            });
        }

        public RelayCommand RemoveGlobalImageCommand
        {
            get => new RelayCommand(() =>
            {
                RemoveGlobalImage();
            });
        }

        private bool RemoveGlobalImage()
        {
            if (Settings.GlobalSplashFile.IsNullOrEmpty())
            {
                return true;
            }

            var globalSplashImagePath = Path.Combine(pluginUserDataPath, Settings.GlobalSplashFile);
            FileSystem.DeleteFileSafe(globalSplashImagePath);

            Settings.GlobalSplashFile = null;
            SetGlobalSplashImagePath();
            return true;
        }
    }
}