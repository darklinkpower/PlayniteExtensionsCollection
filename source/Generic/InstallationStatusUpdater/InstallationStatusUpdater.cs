using InstallationStatusUpdater.Application;
using InstallationStatusUpdater.Presentation;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace InstallationStatusUpdater
{
    public class InstallationStatusUpdater : GenericPlugin
    {
        private readonly DispatcherTimer _initiateStatusDetectionTimer;
        private readonly DirectoryWatcherService _dirWatcher;
        private readonly DeviceEventWatcherService _deviceWatcher;
        private readonly GamesInstallationStatusScanner _statusScanner;
        private readonly TagsUpdater _tagUpdater;
        private readonly PathsResolver _pathResolver;
        private readonly ILogger _logger;
        private readonly InstallationStatusUpdaterSettingsViewModel _settings;

        public override Guid Id { get; } = Guid.Parse("ed9c467f-5ab5-478f-a09f-936146188ad0");

        public InstallationStatusUpdater(IPlayniteAPI _playniteApi) : base(_playniteApi)
        {
            _logger = LogManager.GetLogger();
            _settings = new InstallationStatusUpdaterSettingsViewModel(this, _playniteApi);
            Properties = new GenericPluginProperties { HasSettings = true };

            _initiateStatusDetectionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _initiateStatusDetectionTimer.Tick += (s, e) =>
            {
                _initiateStatusDetectionTimer.Stop();
                _statusScanner.DetectInstallationStatus(false);
            };

            _dirWatcher = new DirectoryWatcherService(_logger, _settings);
            _deviceWatcher = new DeviceEventWatcherService(_logger, _settings);
            _pathResolver = new PathsResolver(_playniteApi, _settings);
            _statusScanner = new GamesInstallationStatusScanner(_playniteApi, _logger, _settings, _pathResolver);
            _tagUpdater = new TagsUpdater(_playniteApi, _logger, _settings);

            _dirWatcher.StartWatching();
            _dirWatcher.OnTrigger = () => RestartTimer();
            _deviceWatcher.OnTrigger = () => RestartTimer();
        }

        private void RestartTimer()
        {
            _initiateStatusDetectionTimer.Stop();
            _initiateStatusDetectionTimer.Start();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (System.Windows.Application.Current.MainWindow is Window mainWindow)
            {
                var interop = new WindowInteropHelper(mainWindow);
                _deviceWatcher.HookDeviceEvents(interop);
            }
            else
            {
                _logger.Debug("[InstallationStatusUpdater] Main window not found.");
            }

            if (_settings.Settings.UpdateOnStartup)
            {
                _statusScanner.DetectInstallationStatus(false);
            }
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (_settings.Settings.UpdateOnLibraryUpdate)
            {
                _statusScanner.DetectInstallationStatus(false);
            }

            if (_settings.Settings.UpdateLocTagsOnLibUpdate)
            {
                _tagUpdater.UpdateTagsWithInstallationRoot(PlayniteApi.Database.Games);
            }

            _dirWatcher.StartWatching();
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new InstallationStatusUpdaterSettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {

            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuItemStatusUpdaterDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = _ => _statusScanner.DetectInstallationStatus(true)
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuUpdateDriveInstallTagDescription"),
                    MenuSection = "@Installation Status Updater",
                    Action = _ =>
                    {
                        _tagUpdater.UpdateTagsWithInstallationRoot(PlayniteApi.Database.Games);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterUpdatingTagsFinishMessage"), "Installation Status Updater");
                    }
                }
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuAddIgnoreFeatureDescription"),
                    MenuSection = "Installation Status Updater",
                    Action = a => AddIgnoreFeature(a.Games.Distinct())
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCInstallation_Status_Updater_MenuRemoveIgnoreFeatureDescription"),
                    MenuSection = "Installation Status Updater",
                    Action = a => RemoveIgnoreFeature(a.Games.Distinct())
                }
            };
        }

        private void AddIgnoreFeature(IEnumerable<Game> games)
        {
            var count = PlayniteUtilities.AddFeatureToGames(PlayniteApi, games, "[Status Updater] Ignore");
            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterAddIgnoreFeatureMessage"), count),
                "Installation Status Updater");
        }

        private void RemoveIgnoreFeature(IEnumerable<Game> games)
        {
            var count = PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, games, "[Status Updater] Ignore");
            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterRemoveIgnoreFeatureMessage"), count),
                "Installation Status Updater");
        }
    }
}
