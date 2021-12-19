using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
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


        [DontSerialize]
        public string AltTabPsScriptDescription { get; set; } = @"Currently there is an issue in Fullscreen mode where Playnite doesn't get controller input after returning from the game when the extension is installed.
As a temporary workaround to fix this, you can add this script as a global exit script in Settings -> Script.
It will alt tab twice and will make Playnite properly get controller input again.";

        [DontSerialize]
        public string AltTabPsScript { get; set; } = @"if ($PlayniteApi.ApplicationInfo.Mode -eq ""Fullscreen"")
{
    $wshell = New-Object -ComObject wscript.shell
    $wshell.SendKeys('%{TAB}')

    # To not send keys too quickly
    Start-Sleep -Milliseconds 1000

    $wshell.SendKeys('%{TAB}')
}";

        public bool ControllerNoticeShowed { get; set; } = false;
    }

    public class SplashScreenSettingsViewModel : ObservableObject, ISettings
    {
        private readonly SplashScreen plugin;
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

        public SplashScreenSettingsViewModel(SplashScreen plugin)
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