using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using SteamCommon;
using SteamGameStatusDetector.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SteamGameStatusDetector
{
    public class SteamGameStatusDetector : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private string manifestPath = string.Empty;
        private string manifestLastUpdatedValue = string.Empty;
        private DateTime? lastManifestCheck = null;
        private bool valuesAreDefault = true;
        private Game currentGame = null;

        public SteamGameStatusDetectorSettingsViewModel settings { get; set; }

        private readonly AcfReader acfReader;
        private readonly DispatcherTimer watcherManifestUpdateTimer;
        private readonly FileSystemWatcher watcher;

        public override Guid Id { get; } = Guid.Parse("c010f3aa-481f-490a-9448-52b8fd333a9a");

        public SteamGameStatusDetector(IPlayniteAPI api) : base(api)
        {
            settings = new SteamGameStatusDetectorSettingsViewModel(this);
            acfReader = new AcfReader();
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "SteamGameStatusDetector",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            watcher = new FileSystemWatcher(@"G:\Games\PC\Steam\steamapps")
            {
                NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size,
                Filter = "*.acf"
            };

            watcher.EnableRaisingEvents = true;

            watcher.Changed += Watcher_Changed;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;
            watcher.Error += Watcher_Error;


            watcherManifestUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800),
            };
            watcherManifestUpdateTimer.Tick += WatcherManifestUpdateTimer_Tick;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            RestartTimer();
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            RestartTimer();
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            RestartTimer();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            RestartTimer();
        }

        private void WatcherManifestUpdateTimer_Tick(object sender, EventArgs e)
        {
            watcherManifestUpdateTimer.Stop();
            SetCurrentGameManifest();
        }

        private void ResetValues()
        {
            if (valuesAreDefault)
            {
                return;
            }

            //watcher.EnableRaisingEvents = false;
            //watcher.Path = string.Empty;
            watcherManifestUpdateTimer.Stop();

            manifestPath = null;
            lastManifestCheck = null;
            manifestLastUpdatedValue = null;

            settings.Settings.AppState = string.Empty;
            settings.Settings.BytesDownloaded = 0;
            settings.Settings.BytesToDownload = 1;
            settings.Settings.DownloadProgress = string.Empty;
            settings.Settings.HasData = false;
        }

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            ResetValues();
            if (!args.NewValue.HasItems())
            {
                if (currentGame != null)
                {
                    currentGame = null;
                }

                return;
            }

            currentGame = args.NewValue.Last();
            RestartTimer();
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            
            if (currentGame == null || args.Game.Id != currentGame.Id)
            {
                return;
            }

            RestartTimer();
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            if (currentGame == null || args.Game.Id != currentGame.Id)
            {
                return;
            }

            RestartTimer();
        }

        private void RestartTimer()
        {
            watcherManifestUpdateTimer.Stop();
            watcherManifestUpdateTimer.Start();
        }

        private void SetCurrentGameManifest()
        {
            watcherManifestUpdateTimer.Stop();
            if (currentGame == null || !Steam.IsGameSteamGame(currentGame) || currentGame.InstallDirectory.IsNullOrEmpty())
            {
                ResetValues();
                return;
            }

            var manifestDir = Path.GetFullPath(Path.Combine(currentGame.InstallDirectory, @"..\..\"));
            manifestPath = Path.Combine(manifestDir, $"appmanifest_{currentGame.GameId}.acf");
            valuesAreDefault = false;
            DetectCurrentGameStatus();
        }

        private void DetectCurrentGameStatus()
        {
            if (manifestPath.IsNullOrEmpty() || !FileSystem.FileExists(manifestPath))
            {
                ResetValues();
                return;
            }

            if (lastManifestCheck != null)
            {
                var fi = new FileInfo(manifestPath);
                if (fi.LastWriteTime <= lastManifestCheck)
                {
                    return;
                }
            }

            ACF_Struct acfStruct;
            using (FileStream stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 4096))
                {
                    var text = reader.ReadToEnd();
                    acfStruct = acfReader.ACFStringToStruct(text);
                }
            }

            // Fully Downloaded games or starting have same values
            settings.Settings.BytesDownloaded = long.Parse(acfStruct.SubACF["AppState"].SubItems["BytesDownloaded"]);
            settings.Settings.BytesToDownload = long.Parse(acfStruct.SubACF["AppState"].SubItems["BytesToDownload"]);
            if (settings.Settings.BytesDownloaded == settings.Settings.BytesToDownload)
            {
                settings.Settings.HasData = false;
                return;
            }

            var manifestStateFlags = int.Parse(acfStruct.SubACF["AppState"].SubItems["StateFlags"]);
            var appState = (AppState)manifestStateFlags;

            foreach (AppState appStateFlag in Enum.GetValues(typeof(AppState)))
            {
                if (appStateFlag != 0 && appState.HasFlag(appStateFlag))
                {
                    settings.Settings.AppState = appStateFlag.ToString();
                }
            }

            var bytesDownloadedReadable = GetBytesReadable(settings.Settings.BytesDownloaded);
            var bytesToDownloadReadable = GetBytesReadable(settings.Settings.BytesToDownload);
            settings.Settings.DownloadProgress = $"{bytesDownloadedReadable} of {bytesToDownloadReadable}";
            settings.Settings.HasData = true;
        }

        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        // From https://stackoverflow.com/a/11124118
        public string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable /= 1024;
            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamGameStatusDetectorSettingsView();
        }
    }
}