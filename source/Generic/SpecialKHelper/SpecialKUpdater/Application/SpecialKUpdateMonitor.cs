using FlowHttp;
using FlowHttp.Results;
using Playnite.SDK;
using PluginsCommon;
using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKUpdater.Domain;
using SpecialKHelper.SpecialKUpdater.Infrastructure;
using SpecialKHelper.SpecialKUpdater.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpecialKHelper.SpecialKUpdater.Application
{
    public sealed class SpecialKUpdateMonitor : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IPlayniteAPI _playniteApi;
        private readonly SpecialKUpdateService _specialKUpdateService;
        private readonly SpecialKServiceManager _specialKServiceManager;
        private readonly Sha256Validator _sha256Validator;
        private readonly SpecialKHelperSettingsViewModel _settings;
        private readonly Timer _timer;
        private int _checking;
        private const string UpdateNotificationId = "Sk_update_available";
        private SpecialKUpdateChannel _lastUpdateCheckChannel = SpecialKUpdateChannel.Website;
        private DateTime? _lastCheckTime;

        public SpecialKUpdateMonitor(
            ILogger logger,
            IPlayniteAPI playniteApi,
            SpecialKUpdateService specialKUpdateService,
            SpecialKServiceManager specialKServiceManager,
            Sha256Validator sha256Validator,
            SpecialKHelperSettingsViewModel settings)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _playniteApi = playniteApi
                ?? throw new ArgumentNullException(nameof(playniteApi));
            _specialKUpdateService = specialKUpdateService
                ?? throw new ArgumentNullException(nameof(specialKUpdateService));
            _specialKServiceManager = specialKServiceManager
                ?? throw new ArgumentNullException(nameof(specialKServiceManager));
            _sha256Validator = sha256Validator
                ?? throw new ArgumentNullException(nameof(sha256Validator));
            _settings = settings
                ?? throw new ArgumentNullException(nameof(settings));
            _timer = new Timer(
                OnTimerElapsed,
                null,
                Timeout.Infinite,
                Timeout.Infinite);
        }

        public void Start()
        {
            _timer.Change(
                TimeSpan.Zero,
                TimeSpan.FromMinutes(5));
        }

        private async void OnTimerElapsed(object state)
        {
            if (Interlocked.Exchange(
                ref _checking,
                1) == 1)
            {
                return;
            }

            if (!_settings.Settings.CheckForSpecialKUpdates)
            {
                return;
            }

            try
            {
                // Don't check for updates if the channel hasn't changed and the last check was less than 6 hours ago
                if (_lastUpdateCheckChannel == _settings.Settings.SpecialKUpdateChannel &&
                    !IsOlderThan(TimeSpan.FromHours(6)))
                {
                    return;
                }

                _lastUpdateCheckChannel = _settings.Settings.SpecialKUpdateChannel;
                _lastCheckTime = DateTime.UtcNow;
                await CheckForUpdatesInternalAsync();
            }
            catch (Exception e)
            {
                _logger.Error(
                    e,
                    "Failed update check.");
            }
            finally
            {
                Interlocked.Exchange(
                    ref _checking,
                    0);
            }
        }

        private bool IsOlderThan(
            TimeSpan interval)
        {
            if (_lastCheckTime is null)
            {
                return true;
            }

            return DateTime.UtcNow - _lastCheckTime.Value
                >= interval;
        }

        private async Task CheckForUpdatesInternalAsync()
        {
            try
            {
                var currentVersionIfo = _specialKServiceManager.GetCurrentVersionInformation();
                var updateChannel = _settings.Settings.SpecialKUpdateChannel;
                var result = await _specialKUpdateService.CheckForUpdatesAsync(
                    currentVersionIfo,
                    updateChannel);

                if (!result.IsUpdateAvailable)
                {
                    return;
                }

                var channelName = string.Empty;
                switch (updateChannel)
                {
                    case SpecialKUpdateChannel.Discord:
                        channelName = "Discord";
                        break;
                    case SpecialKUpdateChannel.Website:
                        channelName = "Website";
                        break;
                    case SpecialKUpdateChannel.Ancient:
                        channelName = "Ancient";
                        break;
                }

                _playniteApi.Notifications.Add(
                    new NotificationMessage(
                        UpdateNotificationId,
                        $"Special K update available on {channelName} channel: {result.CurrentVersion} → {result.LatestVersion}",
                        NotificationType.Info,
                        () =>
                        {
                            _ = OnUpdateAccepted(result);
                        }));
            }
            catch (Exception e)
            {
                _logger.Error(
                    e,
                    "Update check failed.");
            }
        }

        private async Task OnUpdateAccepted(UpdateCheckResult update)
        {
            var dialogResult = _playniteApi.Dialogs.ShowMessage(
                $"A new Special K version is available.\n\n" +

                $"Current Version: {update.CurrentVersion}\n" +
                $"New Version: {update.LatestVersion}\n\n" +

                $"Release Notes:\n" +
                $"{update.ReleaseNotes}\n\n" +

                $"Do you want to download and install the update now?",

                "Special K Update Available",
                MessageBoxButton.YesNo);

            if (dialogResult != MessageBoxResult.Yes)
            {
                return;
            }

            var downloadPath = Path.Combine(
                Path.GetTempPath(),
                $"SpecialK_{update.LatestVersion}.exe");
            HttpFileDownloadResult downloadResult = null;
            _playniteApi.Dialogs.ActivateGlobalProgress(async progArgs =>
            {
                progArgs.CurrentProgressValue = 0;
                progArgs.ProgressMaxValue = 100;
                var downloadRequest =
                    HttpRequestFactory.GetHttpFileRequest(update.InstallerUrl, downloadPath);

                downloadResult = await downloadRequest.DownloadFileAsync(
                    cancellationToken: progArgs.CancelToken,
                    progressChangedCallback:  (progressArgs) =>
                    {
                        progArgs.CurrentProgressValue = progressArgs.ProgressPercentage;
                    });
            }, new GlobalProgressOptions($"Downloading Special K update {update.LatestVersion}...", true)
            {
                IsIndeterminate = false
            });

            if (downloadResult.IsCancelled)
            {
                return;
            }

            if (!downloadResult.IsSuccess)
            {
                if (downloadResult.IsCancelled)
                {
                    return;
                }
                
                if (downloadResult.Error != null)
                {
                    _logger.Error(
                        downloadResult.Error,
                        "Special K update download failed.");
                }

                _playniteApi.Dialogs.ShowMessage(
                    "Failed to download the update. Please try again later." +
                     $"{(downloadResult.Error != null ? $"\n\nError: {downloadResult.Error}" : string.Empty)}",
                    "Download Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            try
            {
                var isValid = _sha256Validator.Validate(downloadResult.DownloadPath, update.Sha256);
                if (!isValid)
                {
                    _logger.Error(
                        $"SHA256 hash mismatch for downloaded file {downloadResult.Url}. Expected: {update.Sha256}");
                    _playniteApi.Dialogs.ShowMessage(
                        "The downloaded file is corrupted (SHA256 hash mismatch). Please try downloading the update again.",
                        "Download Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var anyServiceRunning = 
                    _specialKServiceManager.Is32BitsServiceRunning() ||
                    _specialKServiceManager.Is64BitsServiceRunning();
                if (anyServiceRunning)
                {
                    _logger.Info("Stopping Special K services before starting the installer.");
                    _specialKServiceManager.StopAllServices();
                }

                var process = ProcessStarter.StartProcess(downloadResult.DownloadPath);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                }
            }
            catch (Exception e)
            {
                _logger.Error(
                    e,
                    "Failed downloading installer");
            }
            finally
            {
                FileSystem.DeleteFileSafe(downloadResult.DownloadPath);
                _playniteApi.Notifications.Remove(UpdateNotificationId);
            }

            return;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
