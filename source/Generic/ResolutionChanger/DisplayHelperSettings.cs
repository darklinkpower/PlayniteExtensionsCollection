using Playnite.SDK;
using Playnite.SDK.Data;
using DisplayHelper.Models;
using DisplayHelper.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper
{
    public class DisplayHelperSettings : ObservableObject
    {
        [DontSerialize]
        private bool changeResOnlyGamesNotRunning = true;
        public bool ChangeResOnlyGamesNotRunning { get => changeResOnlyGamesNotRunning; set => SetValue(ref changeResOnlyGamesNotRunning, value); }

        [DontSerialize]
        private bool moveWindowToTargetDisplay = true;
        public bool MoveWindowToTargetDisplay { get => moveWindowToTargetDisplay; set => SetValue(ref moveWindowToTargetDisplay, value); }

        [DontSerialize]
        private ModeDisplaySettings fullscreenModeDisplayInfo = new ModeDisplaySettings();
        public ModeDisplaySettings FullscreenModeDisplayInfo { get => fullscreenModeDisplayInfo; set => SetValue(ref fullscreenModeDisplayInfo, value); }

        [DontSerialize]
        private ModeDisplaySettings desktopModeDisplayInfo = new ModeDisplaySettings();
        public ModeDisplaySettings DesktopModeDisplayInfo { get => desktopModeDisplayInfo; set => SetValue(ref desktopModeDisplayInfo, value); }

        // Monitor device path -> custom name
        [DontSerialize]
        private Dictionary<string, string> customMonitorNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> CustomMonitorNames { get => customMonitorNames; set => SetValue(ref customMonitorNames, value); }
    }

    public class MonitorNameEntry : ObservableObject
    {
        public string DevicePath { get; set; }
        public string DisplayNumber { get; set; }
        public int WindowsNumber { get; set; }
        public string WindowsNumberText => WindowsNumber > 0 ? WindowsNumber.ToString() : "?";
        public string RealName { get; set; }

        private string customName = string.Empty;
        public string CustomName { get => customName; set => SetValue(ref customName, value); }
    }

    public class DisplayHelperSettingsViewModel : ObservableObject, ISettings
    {
        private readonly DisplayHelper plugin;
        private DisplayHelperSettings editingClone { get; set; }

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

        private ObservableCollection<MonitorNameEntry> monitorNameEntries = new ObservableCollection<MonitorNameEntry>();
        public ObservableCollection<MonitorNameEntry> MonitorNameEntries { get => monitorNameEntries; set => SetValue(ref monitorNameEntries, value); }

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

        private DisplayHelperSettings settings;
        public DisplayHelperSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public DisplayHelperSettingsViewModel(DisplayHelper plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<DisplayHelperSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new DisplayHelperSettings();
            }

            ApplyCustomMonitorNames();
        }

        private void ApplyCustomMonitorNames()
        {
            MonitorNames.CustomNames = new Dictionary<string, string>(
                Settings.CustomMonitorNames ?? new Dictionary<string, string>(),
                StringComparer.OrdinalIgnoreCase);
        }

        private void LoadMonitorNameEntries()
        {
            var entries = new ObservableCollection<MonitorNameEntry>();
            var saved = Settings.CustomMonitorNames ?? new Dictionary<string, string>();
            var monitors = MonitorNames.GetMonitors();
            var numbers = MonitorNames.GetWindowsNumbers(monitors);
            var list = new List<MonitorNameEntry>();
            foreach (var monitor in monitors)
            {
                if (list.Any(x => string.Equals(x.DevicePath, monitor.DevicePath, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                saved.TryGetValue(monitor.DevicePath, out var custom);
                numbers.TryGetValue(monitor.GdiName, out var windowsNumber);
                list.Add(new MonitorNameEntry
                {
                    DevicePath = monitor.DevicePath,
                    WindowsNumber = windowsNumber,
                    DisplayNumber = monitor.GdiName.Replace(@"\\.\", string.Empty),
                    RealName = string.IsNullOrEmpty(monitor.FriendlyName) ? "(no name reported)" : monitor.FriendlyName,
                    CustomName = custom ?? string.Empty
                });
            }

            foreach (var entry in list.OrderBy(x => x.WindowsNumber == 0 ? int.MaxValue : x.WindowsNumber))
            {
                entries.Add(entry);
            }

            MonitorNameEntries = entries;
        }

        private void SaveMonitorNameEntries()
        {
            // Keep names of monitors that are not connected right now.
            var names = new Dictionary<string, string>(
                Settings.CustomMonitorNames ?? new Dictionary<string, string>(),
                StringComparer.OrdinalIgnoreCase);
            foreach (var entry in MonitorNameEntries)
            {
                var value = entry.CustomName?.Trim();
                if (string.IsNullOrEmpty(value))
                {
                    names.Remove(entry.DevicePath);
                }
                else
                {
                    names[entry.DevicePath] = value;
                }
            }

            Settings.CustomMonitorNames = names;
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            LoadMonitorNameEntries();
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
            SaveMonitorNameEntries();
            plugin.SavePluginSettings(Settings);
            ApplyCustomMonitorNames();
            plugin.RefreshGameMenuItems();
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        public RelayCommand IdentifyDisplaysCommand
        {
            get => new RelayCommand(() => MonitorIdentifier.Show());
        }

        public RelayCommand SetDisplayCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedDisplay is null)
                {
                    return;
                }

                SelectedGlobalSettings.TargetDisplayName = SelectedDisplay.DeviceName;
            });
        }

        public RelayCommand ClearDisplayCommand
        {
            get => new RelayCommand(() =>
            {
                if (!SelectedGlobalSettings.TargetDisplayName.IsNullOrEmpty())
                {
                    SelectedGlobalSettings.TargetDisplayName = string.Empty;
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