using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using SpecialKHelper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper
{
    public class SpecialKHelperSettings : ObservableObject
    {

        private SpecialKExecutionMode specialKExecutionMode = SpecialKExecutionMode.Global;
        public SpecialKExecutionMode SpecialKExecutionMode { get => specialKExecutionMode; set => SetValue(ref specialKExecutionMode, value); }
        private bool enableStOverlayOnNewProfiles = true;
        public bool EnableStOverlayOnNewProfiles { get => enableStOverlayOnNewProfiles; set => SetValue(ref enableStOverlayOnNewProfiles, value); }
        private bool enableReshadeOnNewProfiles = true;
        public bool EnableReshadeOnNewProfiles { get => enableReshadeOnNewProfiles; set => SetValue(ref enableReshadeOnNewProfiles, value); }
        private bool onlyExecutePcGames = true;
        public bool OnlyExecutePcGames { get => onlyExecutePcGames; set => SetValue(ref onlyExecutePcGames, value); }
        private bool stopExecutionIfVac = true;
        public bool StopExecutionIfVac { get => stopExecutionIfVac; set => SetValue(ref stopExecutionIfVac, value); }
        private bool setDefaultFpsOnNewProfiles = false;
        public bool SetDefaultFpsOnNewProfiles { get => setDefaultFpsOnNewProfiles; set => SetValue(ref setDefaultFpsOnNewProfiles, value); }

        private bool disableNvidiaBlOnNewProfiles = false;
        public bool DisableNvidiaBlOnNewProfiles { get => disableNvidiaBlOnNewProfiles; set => SetValue(ref disableNvidiaBlOnNewProfiles, value); }

        private bool useFlipModelOnNewProfiles = true;
        public bool UseFlipModelOnNewProfiles { get => useFlipModelOnNewProfiles; set => SetValue(ref useFlipModelOnNewProfiles, value); }

        private double defaultFpsLimit = 0.0;
        public double DefaultFpsLimit { get => defaultFpsLimit; set => SetValue(ref defaultFpsLimit, value); }

        private SteamOverlay steamOverlayForBpm = SteamOverlay.Desktop;
        public SteamOverlay SteamOverlayForBpm { get => steamOverlayForBpm; set => SetValue(ref steamOverlayForBpm, value); }

        private bool showSidebarItem = true;
        public bool ShowSidebarItem { get => showSidebarItem; set => SetValue(ref showSidebarItem, value); }

        private bool stopIfEasyAntiCheat = true;
        public bool StopIfEasyAntiCheat { get => stopIfEasyAntiCheat; set => SetValue(ref stopIfEasyAntiCheat, value); }
        private string customSpecialKPath = string.Empty;
        public string CustomSpecialKPath { get => customSpecialKPath; set => SetValue(ref customSpecialKPath, value); }
    }

    public class SpecialKHelperSettingsViewModel : ObservableObject, ISettings
    {
        private readonly SpecialKHelper plugin;
        private SpecialKHelperSettings editingClone { get; set; }

        private SpecialKHelperSettings settings;
        public SpecialKHelperSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public SpecialKHelperSettingsViewModel(SpecialKHelper plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SpecialKHelperSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SpecialKHelperSettings();
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

        public RelayCommand OpenHelpCommand
        {
            get => new RelayCommand(() =>
            {
                ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper");
            });
        }

        public RelayCommand BrowseSelectSpecialKExecutableCommand
        {
            get => new RelayCommand(() =>
            {
                var filePath = plugin.PlayniteApi.Dialogs.SelectFile("SKIF|SKIF.exe");
                if (!filePath.IsNullOrEmpty())
                {
                    settings.CustomSpecialKPath = filePath;
                }
            });
        }

        public RelayCommand RemoveSpecialKExecutableCommand
        {
            get => new RelayCommand(() =>
            {
                settings.CustomSpecialKPath = string.Empty;
            });
        }
    }
}