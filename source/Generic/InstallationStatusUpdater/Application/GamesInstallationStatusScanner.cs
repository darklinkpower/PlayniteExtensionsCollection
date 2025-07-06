using InstallationStatusUpdater.Domain.Results;
using InstallationStatusUpdater.Presentation;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace InstallationStatusUpdater.Application
{
    public class GamesInstallationStatusScanner
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;
        private readonly InstallationStatusUpdaterSettingsViewModel _settings;
        private readonly PathsResolver _pathsResolver;
        private const string ScanSkipFeatureName = "[Status Updater] Ignore";

        public GamesInstallationStatusScanner(
            IPlayniteAPI playniteApi,
            ILogger logger,
            InstallationStatusUpdaterSettingsViewModel settings,
            PathsResolver pathResolver)
        {
            _playniteApi = playniteApi ?? throw new ArgumentNullException(nameof(playniteApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _pathsResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
        }

        public StatusUpdateResults DetectInstallationStatusWithProgressDialog(bool showResultsDialog)
        {
            var progressOptions = new GlobalProgressOptions(
                ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterUpdatingInstallStatusProgressMessage"))
            {
                Cancelable = true,
                IsIndeterminate = true
            };

            StatusUpdateResults results = null;

            _playniteApi.Dialogs.ActivateGlobalProgress(
                args =>
                {
                    results = DetectInstallationStatusInternal(args.CancelToken);
                },
                progressOptions);

            OpenWindowOrAddResultsNotification(showResultsDialog, results);
            return results;
        }

        public StatusUpdateResults DetectInstallationStatus(bool showResultsDialog, CancellationToken cancelToken = default)
        {
            var results = DetectInstallationStatusInternal(cancelToken);
            OpenWindowOrAddResultsNotification(showResultsDialog, results);
            return results;
        }

        public StatusUpdateResults DetectInstallationStatusInternal(CancellationToken cancelToken)
        {
            int markedInstalled = 0;
            int markedUninstalled = 0;
            var updateResults = new StatusUpdateResults();

            using (_playniteApi.Database.BufferedUpdate())
            {
                foreach (var game in _playniteApi.Database.Games)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        var detectedStatus = DetectGameInstallationStatus(game);
                        if (game.IsInstalled && detectedStatus == InstallationDetectionResult.Uninstalled)
                        {
                            game.IsInstalled = false;
                            _playniteApi.Database.Games.Update(game);
                            markedUninstalled++;
                            updateResults.AddUninstalled(game);
                        }
                        else if (!game.IsInstalled && detectedStatus == InstallationDetectionResult.Installed)
                        {
                            game.IsInstalled = true;
                            _playniteApi.Database.Games.Update(game);
                            markedInstalled++;
                            updateResults.AddInstalled(game);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"[GameStatusScanner] Failed to process game '{game.Name}' (ID: {game.Id})");
                    }
                }
            }

            return updateResults;
        }

        private void OpenWindowOrAddResultsNotification(bool showResultsDialog, StatusUpdateResults statusUpdateResults)
        {
            if (showResultsDialog)
            {
                OpenResultsWindow(statusUpdateResults);
            }
            else if (statusUpdateResults.Installed.Count > 0 || statusUpdateResults.Uninstalled.Count > 0)
            {
                var notificationMessage = new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    string.Format(
                        ResourceProvider.GetString("LOCInstallation_Status_Updater_NotificationMessageMarkedInstalledResults"),
                        statusUpdateResults.Installed.Count,
                        statusUpdateResults.Uninstalled.Count),
                    NotificationType.Info,
                    () => OpenResultsWindow(statusUpdateResults));
                _playniteApi.Notifications.Add(notificationMessage);
            }
        }

        private void OpenResultsWindow(StatusUpdateResults updateResults)
        {
            var window = _playniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 500;
            window.Width = 650;
            window.Title = ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdateResultsWindowTitle");

            window.Content = new StatusUpdateResultsWindow();
            window.DataContext = new StatusUpdateResultsWindowViewModel(updateResults);
            window.Owner = _playniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }

        private InstallationDetectionResult DetectGameInstallationStatus(Game game)
        {
            if (PlayniteUtilities.GetGameHasFeature(game, ScanSkipFeatureName))
            {
                return InstallationDetectionResult.Skipped;
            }

            var resolvedInstallationDirectory = _pathsResolver.GetInstallDirForDetection(game);
            if (game.GameActions?.Any() == true && _pathsResolver.IsAnyActionInstalled(game, resolvedInstallationDirectory))
            {
                return InstallationDetectionResult.Installed;
            }

            if (_pathsResolver.IsAnyRomInstalled(game, resolvedInstallationDirectory))
            {
                return InstallationDetectionResult.Installed;
            }

            var isLibraryPluginGame = game.PluginId != Guid.Empty;
            if (isLibraryPluginGame)
            {
                if (_settings.Settings.ScanGamesHandledByLibPlugins && !game.InstallDirectory.IsNullOrEmpty())
                {
                    return Directory.Exists(game.InstallDirectory)
                        ? InstallationDetectionResult.Installed
                        : InstallationDetectionResult.Uninstalled;
                }

                return InstallationDetectionResult.Skipped;
            }

            return InstallationDetectionResult.Uninstalled;
        }
    }
}
