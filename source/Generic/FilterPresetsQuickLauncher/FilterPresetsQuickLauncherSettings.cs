using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using FilterPresetsQuickLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPresetsQuickLauncher
{
    public class FilterPresetsQuickLauncherSettings : ObservableObject
    {
        private ObservableCollection<FilterPresetDisplaySettings> filterPresetsDisplaySettings = new ObservableCollection<FilterPresetDisplaySettings>();
        public ObservableCollection<FilterPresetDisplaySettings> FilterPresetsDisplaySettings { get => filterPresetsDisplaySettings; set => SetValue(ref filterPresetsDisplaySettings, value); }
    }

    public class FilterPresetsQuickLauncherSettingsViewModel : ObservableObject, ISettings
    {
        private readonly FilterPresetsQuickLauncher plugin;
        private FilterPresetsQuickLauncherSettings editingClone { get; set; }

        private readonly IPlayniteAPI playniteApi;
        private readonly List<string> iconsToRemove = new List<string>();
        private readonly List<string> iconsToAdd = new List<string>();

        private FilterPresetsQuickLauncherSettings settings;
        public FilterPresetsQuickLauncherSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public string SelectedDisplaySettingIconPath
        {
            get
            {
                if (SelectedFilterPresetDisplaySetting != null && !selectedFilterPresetDisplaySetting.Image.IsNullOrEmpty())
                {
                    var iconFullPath = GetDisplaySettingIconFullPath(selectedFilterPresetDisplaySetting);
                    if (FileSystem.FileExists(iconFullPath))
                    {
                        return iconFullPath;
                    }
                }

                return null;
            }
        }

        private FilterPresetDisplaySettings selectedFilterPresetDisplaySetting;
        public FilterPresetDisplaySettings SelectedFilterPresetDisplaySetting
        {
            get => selectedFilterPresetDisplaySetting;
            set
            {
                selectedFilterPresetDisplaySetting = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedDisplaySettingIconPath));
            }
        }

        public FilterPresetsQuickLauncherSettingsViewModel(FilterPresetsQuickLauncher plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<FilterPresetsQuickLauncherSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new FilterPresetsQuickLauncherSettings();
            }

            playniteApi = plugin.PlayniteApi;
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            if (playniteApi.Database.FilterPresets.HasItems())
            {
                UpdateFilterPresetDisplaySettings();
            }
            else
            {
                settings.FilterPresetsDisplaySettings = new ObservableCollection<FilterPresetDisplaySettings>();
            }

            foreach (var displaySettings in settings.FilterPresetsDisplaySettings)
            {
                UpdateDisplaySettingFullImagePath(displaySettings);
            }
        }

        private void UpdateFilterPresetDisplaySettings()
        {
            if (!playniteApi.Database.FilterPresets.HasItems())
            {
                settings.FilterPresetsDisplaySettings.Clear();
                return;
            }

            foreach (var displaySetting in settings.FilterPresetsDisplaySettings.ToList())
            {
                if (!playniteApi.Database.FilterPresets.Any(x => x.Id == displaySetting.Id))
                {
                    settings.FilterPresetsDisplaySettings.Remove(displaySetting);
                }
            }

            foreach (var filterPreset in playniteApi.Database.FilterPresets.OrderBy(a => a.Name))
            {
                var displaySetting = settings.FilterPresetsDisplaySettings.FirstOrDefault(x => x.Id == filterPreset.Id);
                if (displaySetting is null)
                {
                    var filterDisplaySettings = new FilterPresetDisplaySettings
                    {
                        Name = filterPreset.Name,
                        Id = filterPreset.Id,
                    };

                    settings.FilterPresetsDisplaySettings.Add(filterDisplaySettings);
                }
                else if (displaySetting.Name != filterPreset.Name)
                {
                    displaySetting.Name = filterPreset.Name;
                }
            }
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
            DeleteIcons(iconsToAdd);
            iconsToRemove.Clear();
            iconsToAdd.Clear();
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);

            DeleteIcons(iconsToRemove);
            iconsToRemove.Clear();
            iconsToAdd.Clear();
        }

        private void DeleteIcons(List<string> iconsList)
        {
            foreach (var iconName in iconsList)
            {
                var iconPath = GetDisplaySettingIconFullPath(iconName);
                if (FileSystem.FileExists(iconPath))
                {
                    FileSystem.DeleteFileSafe(iconPath);
                }
            }
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        private void UpdateDisplaySettingFullImagePath(FilterPresetDisplaySettings displaySettings)
        {
            if (displaySettings is null)
            {
                return;
            }

            var fullImagePath = GetDisplaySettingIconFullPath(displaySettings);
            if (!fullImagePath.IsNullOrEmpty() && FileSystem.FileExists(fullImagePath))
            {
                displaySettings.ImageFullPath = fullImagePath;
            }
            else
            {
                displaySettings.ImageFullPath = null;
            }
        }

        private string GetDisplaySettingIconFullPath(FilterPresetDisplaySettings displaySetting)
        {
            return GetDisplaySettingIconFullPath(displaySetting.Image);
        }

        private string GetDisplaySettingIconFullPath(string iconName)
        {
            if (iconName.IsNullOrEmpty())
            {
                return null;
            }
            
            return Path.Combine(plugin.GetPluginUserDataPath(), iconName);
        }

        public RelayCommand SelectedFilterPresetSettingMoveUpCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedFilterPresetDisplaySetting is null)
                {
                    return;
                }

                var index = settings.FilterPresetsDisplaySettings.IndexOf(SelectedFilterPresetDisplaySetting);
                if (index != 0)
                {
                    settings.FilterPresetsDisplaySettings.Move(index, index - 1);
                }
            });
        }

        public RelayCommand SelectedFilterPresetSettingMoveDownCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedFilterPresetDisplaySetting is null)
                {
                    return;
                }

                var index = settings.FilterPresetsDisplaySettings.IndexOf(SelectedFilterPresetDisplaySetting);
                if (index != settings.FilterPresetsDisplaySettings.Count - 1)
                {
                    settings.FilterPresetsDisplaySettings.Move(index, index + 1);
                }
            });
        }

        public RelayCommand SelectedFilterPresetSettingRemoveIconCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedFilterPresetDisplaySetting is null || SelectedFilterPresetDisplaySetting.Image.IsNullOrEmpty())
                {
                    return;
                }

                iconsToRemove.Add(SelectedFilterPresetDisplaySetting.Image);
                SelectedFilterPresetDisplaySetting.Image = null;
                UpdateDisplaySettingFullImagePath(SelectedFilterPresetDisplaySetting);
                OnPropertyChanged(nameof(SelectedDisplaySettingIconPath));
            });
        }

        public RelayCommand SelectedFilterPresetSettingAddIconCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedFilterPresetDisplaySetting is null)
                {
                    return;
                }

                var selectedImage = playniteApi.Dialogs.SelectImagefile();
                if (selectedImage.IsNullOrEmpty())
                {
                    return;
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(selectedImage);
                var targetPath = GetDisplaySettingIconFullPath(fileName);
                FileSystem.CopyFile(selectedImage, targetPath);

                if (!SelectedFilterPresetDisplaySetting.Image.IsNullOrEmpty())
                {
                    iconsToRemove.Add(SelectedFilterPresetDisplaySetting.Image);
                }

                iconsToAdd.Add(fileName);
                SelectedFilterPresetDisplaySetting.Image = fileName;
                UpdateDisplaySettingFullImagePath(SelectedFilterPresetDisplaySetting);
                OnPropertyChanged(nameof(SelectedDisplaySettingIconPath));
            });
        }
    }
}