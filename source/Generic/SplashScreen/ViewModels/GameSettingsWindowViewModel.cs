using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using SplashScreen.Helpers;
using SplashScreen.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace SplashScreen.ViewModels
{
    public class GameSettingsWindowViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _playniteApi;
        private readonly string _pluginUserDataPath;
        private readonly string _customBackgroundsDirectory;
        private readonly string _gameSettingsPath;
        private readonly GeneralSplashSettings _globalSettings;
        private bool _saveCustomBackground = false;
        private bool _removeCustomBackground = false;

        private Game _game;
        public Game Game { get => _game; set => SetValue(ref _game, value); }

        private string _customBackgroundPath = null;
        public string CustomBackgroundPath { get => _customBackgroundPath; set => SetValue(ref _customBackgroundPath, value); }

        private GameSplashSettings _settings = new GameSplashSettings();
        public GameSplashSettings Settings { get => _settings; set => SetValue(ref _settings, value); }

        public Dictionary<HorizontalAlignment, string> LogoHorizontalSource { get; set; } = new Dictionary<HorizontalAlignment, string>
        {
            { HorizontalAlignment.Left, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentLeftLabel") },
            { HorizontalAlignment.Center, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentCenterLabel") },
            { HorizontalAlignment.Right, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentRightLabel") },
        };

        public Dictionary<VerticalAlignment, string> LogoVerticalSource { get; set; } = new Dictionary<VerticalAlignment, string>
        {
            { VerticalAlignment.Top, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentTopLabel") },
            { VerticalAlignment.Center, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentCenterLabel") },
            { VerticalAlignment.Bottom, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentBottomLabel") },
        };

        public GameSettingsWindowViewModel(IPlayniteAPI playniteApi, string pluginUserDataPath, Game game, GeneralSplashSettings globalSettings)
        {
            this._playniteApi = playniteApi;
            this._pluginUserDataPath = pluginUserDataPath;
            this._globalSettings = Serialization.GetClone(globalSettings);
            _customBackgroundsDirectory = Path.Combine(pluginUserDataPath, "CustomBackgrounds");
            Game = game;

            _gameSettingsPath = Path.Combine(pluginUserDataPath, $"{game.Id}.json");
            if (FileSystem.FileExists(_gameSettingsPath))
            {
                Settings = Serialization.FromJsonFile<GameSplashSettings>(_gameSettingsPath);
                if (!Settings.GeneralSplashSettings.CustomBackgroundImage.IsNullOrEmpty())
                {
                    CustomBackgroundPath = Path.Combine(_customBackgroundsDirectory, Settings.GeneralSplashSettings.CustomBackgroundImage);
                }
            }

            SplashSettingsSyncHelper.ApplyGlobalIndicatorSettings(Settings.GeneralSplashSettings, globalSettings);
        }

        private void DeleteCurrentGameBackground()
        {
            if (Settings.GeneralSplashSettings.CustomBackgroundImage.IsNullOrEmpty())
            {
                return;
            }

            var backgroundPath = Path.Combine(_customBackgroundsDirectory, Settings.GeneralSplashSettings.CustomBackgroundImage);
            if (FileSystem.FileExists(backgroundPath))
            {
                FileSystem.DeleteFileSafe(backgroundPath);
            }

            Settings.GeneralSplashSettings.CustomBackgroundImage = null;
        }

        private void SaveGameSettings()
        {
            SplashSettingsSyncHelper.ApplyGlobalIndicatorSettings(Settings.GeneralSplashSettings, _globalSettings);

            if (_removeCustomBackground)
            {
                DeleteCurrentGameBackground();
            }
            else if (_saveCustomBackground && !CustomBackgroundPath.IsNullOrEmpty() && FileSystem.FileExists(CustomBackgroundPath))
            {
                DeleteCurrentGameBackground();
                var fileName = Guid.NewGuid() + Path.GetExtension(CustomBackgroundPath);
                var customBackgroundImagePathTarget = Path.Combine(_customBackgroundsDirectory, fileName);
                try
                {
                    FileSystem.CopyFile(CustomBackgroundPath, customBackgroundImagePathTarget);
                    Settings.GeneralSplashSettings.CustomBackgroundImage = fileName;
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error copying game custom background image from {CustomBackgroundPath} to {customBackgroundImagePathTarget}");
                }
            }

            FileSystem.WriteStringToFile(_gameSettingsPath, Serialization.ToJson(Settings));
            _playniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSplashScreen_GameSettingsWindowSettingsSavedLabel"), "Splash Screen");
        }

        public RelayCommand RemoveCustomBackgroundCommand
        {
            get => new RelayCommand(() =>
            {
                _removeCustomBackground = true;
                _saveCustomBackground = false;
                CustomBackgroundPath = null;
            }, () => CustomBackgroundPath != null);
        }

        public RelayCommand SaveGameSettingsCommand
        {
            get => new RelayCommand(() =>
            {
                SaveGameSettings();
            });
        }

        public RelayCommand BrowseSelectCustomBackgroundCommand
        {
            get => new RelayCommand(() =>
            {
                var filePath = _playniteApi.Dialogs.SelectImagefile();
                if (filePath.IsNullOrEmpty() || !FileSystem.FileExists(filePath))
                {
                    return;
                }

                _removeCustomBackground = false;
                _saveCustomBackground = true;
                CustomBackgroundPath = filePath;
            });
        }
    }
}