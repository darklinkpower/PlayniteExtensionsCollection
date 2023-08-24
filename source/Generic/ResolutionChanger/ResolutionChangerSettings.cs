using Playnite.SDK;
using Playnite.SDK.Data;
using ResolutionChanger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolutionChanger
{
    public class ResolutionChangerSettings : ObservableObject
    {
        [DontSerialize]
        private bool changeResOnlyGamesNotRunning = true;
        public bool ChangeResOnlyGamesNotRunning { get => changeResOnlyGamesNotRunning; set => SetValue(ref changeResOnlyGamesNotRunning, value); }

        [DontSerialize]
        private ModeDisplaySettings fullscreenModeDisplayInfo = new ModeDisplaySettings();
        public ModeDisplaySettings FullscreenModeDisplayInfo { get => fullscreenModeDisplayInfo; set => SetValue(ref fullscreenModeDisplayInfo, value); }

        [DontSerialize]
        private ModeDisplaySettings desktopModeDisplayInfo = new ModeDisplaySettings();
        public ModeDisplaySettings DesktopModeDisplayInfo { get => desktopModeDisplayInfo; set => SetValue(ref desktopModeDisplayInfo, value); }
    }

    public class ResolutionChangerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ResolutionChanger plugin;
        private ResolutionChangerSettings editingClone { get; set; }

        private Dictionary<string, ApplicationMode> playniteModes = new Dictionary<string, ApplicationMode>
        {
            { "Desktop Mode",  ApplicationMode.Desktop},
            { "Fullscreen Mode",  ApplicationMode.Fullscreen}
        };

        public Dictionary<string, ApplicationMode> PlayniteModes { get => playniteModes; set => SetValue(ref playniteModes, value); }

        private KeyValuePair<string, ApplicationMode> selectedPlayniteMode;
        public KeyValuePair<string, ApplicationMode> SelectedPlayniteMode
        {
            get => selectedPlayniteMode;
            set
            {
                selectedPlayniteMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedGlobalSettings));
            }
        }

        private List<DisplayInfo> availableDisplays = new List<DisplayInfo>();
        public List<DisplayInfo> AvailableDisplays { get => availableDisplays; set => SetValue(ref availableDisplays, value); }

        private DisplayInfo selectedDisplay = null;
        public DisplayInfo SelectedDisplay
        {
            get => selectedDisplay;
            set
            {
                selectedDisplay = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedDisplayModes));
            }
        }

        public List<DisplayMode> SelectedDisplayModes
        {
            get
            {
                var list = selectedDisplay is null ? new List<DisplayMode>() : SelectedDisplay.DisplayModes;
                SelectedDisplayMode = list?.FirstOrDefault();
                return list;
            }
        }

        private DisplayMode selectedDisplayMode = null;
        public DisplayMode SelectedDisplayMode
        {
            get => selectedDisplayMode;
            set
            {
                selectedDisplayMode = value;
                OnPropertyChanged();
            }
        }

        public ModeDisplaySettings SelectedGlobalSettings
        {
            get
            {
                if (selectedPlayniteMode.Value == ApplicationMode.Desktop)
                {
                    return settings.DesktopModeDisplayInfo;
                }
                else
                {
                    return settings.FullscreenModeDisplayInfo;
                }
            }
        }

        private ResolutionChangerSettings settings;
        public ResolutionChangerSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ResolutionChangerSettingsViewModel(ResolutionChanger plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ResolutionChangerSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ResolutionChangerSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            AvailableDisplays = DisplayUtilities.GetAvailableDisplaysInfo();
            SelectedDisplay = AvailableDisplays.FirstOrDefault();
            SelectedPlayniteMode = PlayniteModes.FirstOrDefault();
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

        public RelayCommand SetDisplayCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedDisplay is null)
                {
                    return;
                }

                SelectedGlobalSettings.TargetDisplay = SelectedDisplay.DeviceName;
            });
        }

        public RelayCommand ClearDisplayCommand
        {
            get => new RelayCommand(() =>
            {
                if (!SelectedGlobalSettings.TargetDisplay.IsNullOrEmpty())
                {
                    SelectedGlobalSettings.TargetDisplay = string.Empty;
                }
            });
        }

        public RelayCommand SetResolutionCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedGlobalSettings is null || SelectedDisplayMode is null)
                {
                    return;
                }

                SelectedGlobalSettings.Width = SelectedDisplayMode.Width;
                SelectedGlobalSettings.Height = SelectedDisplayMode.Height;
            });
        }

        public RelayCommand ClearResolutionCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedGlobalSettings is null)
                {
                    return;
                }

                SelectedGlobalSettings.Width = null;
                SelectedGlobalSettings.Height = null;
            });
        }

        public RelayCommand SetRefreshRateCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedGlobalSettings is null || SelectedDisplayMode is null)
                {
                    return;
                }

                SelectedGlobalSettings.RefreshRate = SelectedDisplayMode.DisplayFrenquency;
            });
        }

        public RelayCommand ClearRefreshRateCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedGlobalSettings is null)
                {
                    return;
                }

                SelectedGlobalSettings.RefreshRate = null;
            });
        }
    }
}