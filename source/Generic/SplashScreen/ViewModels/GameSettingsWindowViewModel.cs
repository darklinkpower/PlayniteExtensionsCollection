using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using SplashScreen.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SplashScreen.ViewModels
{
    public class GameSettingsWindowViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private readonly string pluginUserDataPath;
        private readonly string customBackgroundsDirectory;
        private readonly string gameSettingsPath;
        private bool saveCustomBackground = false;
        private bool removeCustomBackground = false;

        private Game game;
        public Game Game { get => game; set => SetValue(ref game, value); }

        private string customBackgroundPath = null;
        public string CustomBackgroundPath { get => customBackgroundPath; set => SetValue(ref customBackgroundPath, value); }

        private GameSplashSettings settings = new GameSplashSettings();
        public GameSplashSettings Settings { get => settings; set => SetValue(ref settings, value); }

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

        public GameSettingsWindowViewModel(IPlayniteAPI playniteApi, string pluginUserDataPath, Game game)
        {
            this.playniteApi = playniteApi;
            this.pluginUserDataPath = pluginUserDataPath;
            customBackgroundsDirectory = Path.Combine(pluginUserDataPath, "CustomBackgrounds");
            Game = game;

            gameSettingsPath = Path.Combine(pluginUserDataPath, $"{game.Id}.json");
            if (FileSystem.FileExists(gameSettingsPath))
            {
                Settings = Serialization.FromJsonFile<GameSplashSettings>(gameSettingsPath);
                if (!Settings.GeneralSplashSettings.CustomBackgroundImage.IsNullOrEmpty())
                {
                    CustomBackgroundPath = Path.Combine(customBackgroundsDirectory, Settings.GeneralSplashSettings.CustomBackgroundImage);
                }
            }
        }

        private void DeleteCurrentGameBackground()
        {
            if (Settings.GeneralSplashSettings.CustomBackgroundImage.IsNullOrEmpty())
            {
                return;
            }

            var backgroundPath = Path.Combine(customBackgroundsDirectory, Settings.GeneralSplashSettings.CustomBackgroundImage);
            if (FileSystem.FileExists(backgroundPath))
            {
                FileSystem.DeleteFileSafe(backgroundPath);
            }

            Settings.GeneralSplashSettings.CustomBackgroundImage = null;
        }

        private void SaveGameSettings()
        {
            if (removeCustomBackground)
            {
                DeleteCurrentGameBackground();
            }
            else if (saveCustomBackground && !CustomBackgroundPath.IsNullOrEmpty() && FileSystem.FileExists(CustomBackgroundPath))
            {
                DeleteCurrentGameBackground();
                var fileName = Guid.NewGuid() + Path.GetExtension(CustomBackgroundPath);
                var customBackgroundImagePathTarget = Path.Combine(customBackgroundsDirectory, fileName);
                try
                {
                    FileSystem.CopyFile(CustomBackgroundPath, customBackgroundImagePathTarget);
                    Settings.GeneralSplashSettings.CustomBackgroundImage = fileName;
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error copying game custom background image from {CustomBackgroundPath} to {customBackgroundImagePathTarget}");
                }
            }

            FileSystem.WriteStringToFile(gameSettingsPath, Serialization.ToJson(Settings));
            playniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSplashScreen_GameSettingsWindowSettingsSavedLabel"), "Splash Screen");
        }

        public RelayCommand RemoveCustomBackgroundCommand
        {
            get => new RelayCommand(() =>
            {
                removeCustomBackground = true;
                saveCustomBackground = false;
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
                var filePath = playniteApi.Dialogs.SelectImagefile();
                if (filePath.IsNullOrEmpty() || !FileSystem.FileExists(filePath))
                {
                    return;
                }

                removeCustomBackground = false;
                saveCustomBackground = true;
                CustomBackgroundPath = filePath;
            });
        }
    }
}