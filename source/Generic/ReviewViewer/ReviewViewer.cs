using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using PluginsCommon;
using FlowHttp;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DatabaseCommon;
using ReviewViewer.Application;
using ReviewViewer.Domain;
using ReviewViewer.Infrastructure;
using ReviewViewer.Presentation;

namespace ReviewViewer
{
    public class ReviewViewer : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly LiteDbRepository<ReviewsResponseRecord> _reviewsRecordsDatabase;
        private readonly SteamReviewsCoordinator _steamReviewsCoordinator;

        public ReviewViewerSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("ca24e37a-76d9-49bf-89ab-d3cba4a54bd1");

        public ReviewViewer(IPlayniteAPI api) : base(api)
        {
            Settings = new ReviewViewerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = "ReviewViewer",
                ElementList = new List<string> { "ReviewsControl" }
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "ReviewViewer",
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });

            var reviewsDatabasePath = Path.Combine(GetPluginUserDataPath(), "reviewsDatabase.db");
            _reviewsRecordsDatabase = new LiteDbRepository<ReviewsResponseRecord>(reviewsDatabasePath, _logger);
            var rawCollection = _reviewsRecordsDatabase.GetRawCollection();
            rawCollection.EnsureIndex(nameof(ReviewsResponseRecord.CacheKey), true);

            _steamReviewsCoordinator = new SteamReviewsCoordinator(
                new SteamReviewsService(TimeSpan.FromMilliseconds(1100)), _reviewsRecordsDatabase, TimeSpan.FromDays(Settings.Settings.DownloadIfOlderThanValue));
            Settings.OnSettingsChanged += Settings_OnSettingsChanged;
        }

        private void Settings_OnSettingsChanged(SettingsChangedInfo<ReviewViewerSettings> settingsChangedInfo)
        {
            _steamReviewsCoordinator.ChangeCacheDuration(TimeSpan.FromDays(Settings.Settings.DownloadIfOlderThanValue));
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "ReviewsControl")
            {
                return new ReviewsControl(Settings, PlayniteApi, _logger, _steamReviewsCoordinator);
            }

            return null;
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (Settings.Settings.DatabaseVersion == 1)
            {
                _logger.Info("Upgrading plugin data to database version 2. Deleting legacy JSON files.");
                var folderPath = GetPluginUserDataPath();
                if (Directory.Exists(folderPath))
                {
                    var jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
                    foreach (var file in jsonFiles)
                    {
                        FileSystem.DeleteFileSafe(file);
                    }
                }

                Settings.Settings.DatabaseVersion = 2;
                base.SavePluginSettings(Settings.Settings);
                _logger.Info("Plugin data upgraded to version 2.");
            }

            if (Settings.Settings.DatabaseVersion == 2)
            {
                _logger.Info("Upgrading plugin data to database version 3.");
                try
                {
                    _reviewsRecordsDatabase.DeleteAll();
                    Settings.Settings.DatabaseVersion = 3;
                    base.SavePluginSettings(Settings.Settings);
                    _logger.Info("Plugin data upgraded to version 3.");
                }
                catch (Exception e)
                {
                    _logger.Info(e, "Error upgrading Plugin data to version 3.");
                }
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            
            var lastCompactedTime = DateTime.UtcNow - Settings.Settings.LastReviewsDbCompactTime;
            if (lastCompactedTime.TotalDays >= 7)
            {
                // Use double the threshold as a safety margin in case newer data fails to download at runtime.
                // This ensures we keep slightly older entries as a fallback.
                var maxAge = TimeSpan.FromDays(Settings.Settings.DownloadIfOlderThanValue * 2);
                var earliestAllowedDate = DateTime.UtcNow - maxAge;
                var deletedCount = _reviewsRecordsDatabase.Delete(x => x.CreatedAt <= earliestAllowedDate);
                _logger.Info($"Deleted {deletedCount} outdated reviews.");

                _reviewsRecordsDatabase.ShrinkDatabase();
                Settings.Settings.LastReviewsDbCompactTime = DateTime.UtcNow;
                _logger.Info($"Shrinked database.");
            }

            base.SavePluginSettings(Settings.Settings);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ReviewViewerSettingsView();
        }

    }
}