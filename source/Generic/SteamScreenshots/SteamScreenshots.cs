using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using PluginsCommon.Converters;
using SteamCommon.Models;
using SteamScreenshots.Application.Services;
using SteamScreenshots.Domain.Enums;
using SteamScreenshots.Domain.Interfaces;
using SteamScreenshots.Infrastructure.Providers;
using SteamScreenshots.Infrastructure.Repositories;
using SteamScreenshots.ScreenshotsControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamScreenshots
{
    public class SteamScreenshots : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly ScreenshotManagementService _screenshotManagementService;
        private readonly SteamAppDetailsRepository _steamRepository;

        public SteamScreenshotsSettingsViewModel Settings { get; private set; }

        public override Guid Id { get; } = Guid.Parse("8e77fe31-5e62-41e2-8fa2-64844cfd5b6b");
        private const string _pluginExtensionsSource = "SteamScreenshots";
        private const string _vndbVisualNovelViewControlName = "SteamScreenshotsViewControl";

        public SteamScreenshots(IPlayniteAPI api) : base(api)
        {
            Settings = new SteamScreenshotsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { _vndbVisualNovelViewControlName },
                SourceName = _pluginExtensionsSource,
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = _pluginExtensionsSource,
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });

            _steamRepository = new SteamAppDetailsRepository(_logger, Path.Combine(GetPluginUserDataPath(), "appdetails"), true);
            var steamAppDetailsService = new SteamAppDetailsService(_steamRepository);
            var steamScreenshotsProvider = new SteamScreenshotProvider(steamAppDetailsService);
            var screenshotProviders = new Dictionary<ScreenshotServiceType, IScreenshotProvider>
            {
                { ScreenshotServiceType.Steam, steamScreenshotsProvider }
            };

            var imageProvider = new UrlImageProvider(Path.Combine(GetPluginUserDataPath(), "ScreenshotsCache"), _logger);
            _screenshotManagementService = new ScreenshotManagementService(screenshotProviders, imageProvider, _logger);
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == _vndbVisualNovelViewControlName)
            {
                return new SteamScreenshotsControl(Settings, _screenshotManagementService);
            }

            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamScreenshotsSettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            MigrateSteamAppDetails();
        }

        private void MigrateSteamAppDetails()
        {
            if (Settings.Settings.SteamAppDetailsMigrationDone)
            {
                return;
            }

            MigrateAppDetails();
            CleanUpScreenshotsCache();

            Settings.Settings.SteamAppDetailsMigrationDone = true;
            SavePluginSettings(Settings.Settings);
        }

        private void MigrateAppDetails()
        {
            var appDetailsDir = Path.Combine(GetPluginUserDataPath(), "appdetails");
            if (!FileSystem.DirectoryExists(appDetailsDir))
            {
                return;
            }

            var jsonFiles = Directory.GetFiles(appDetailsDir, "*.json");
            foreach (var file in jsonFiles)
            {
                if (!Serialization.TryFromJsonFile<Dictionary<string, SteamAppDetails>>(file, out var parsedData))
                {
                    FileSystem.DeleteFileSafe(file);
                    continue;
                }

                if (parsedData.Keys?.Any() != true)
                {
                    FileSystem.DeleteFileSafe(file);
                    continue;
                }

                var steamId = parsedData.Keys.First();
                var appDetails = parsedData[steamId];

                try
                {
                    if (appDetails.success)
                    {
                        _steamRepository.SaveAppDetails(steamId, appDetails);
                    }
                    else
                    {
                        FileSystem.DeleteFileSafe(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to process Steam app details for {steamId}. Deleting file.");
                    FileSystem.DeleteFileSafe(file);
                }
            }
        }

        private void CleanUpScreenshotsCache()
        {
            var directoryPath = Path.Combine(GetPluginUserDataPath(), "ScreenshotsCache");
            if (!FileSystem.DirectoryExists(directoryPath))
            {
                return;
            }

            try
            {
                var files = Directory.GetFiles(directoryPath);
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Length == 0)
                    {
                        FileSystem.DeleteFileSafe(fileInfo.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while cleaning up screenshots cache.");
            }
        }


    }
}