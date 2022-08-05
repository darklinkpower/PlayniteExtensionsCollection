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
using System.Threading.Tasks;
using System.Windows.Controls;

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

        public override Guid Id { get; } = Guid.Parse("c010f3aa-481f-490a-9448-52b8fd333a9a");

        public SteamGameStatusDetector(IPlayniteAPI api) : base(api)
        {
            settings = new SteamGameStatusDetectorSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "SteamGameStatusDetector",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });
        }

        private void ResetValues()
        {
            if (valuesAreDefault)
            {
                return;
            }

            manifestPath = null;
            lastManifestCheck = null;
            manifestLastUpdatedValue = null;

            settings.Settings.AppState = string.Empty;
            settings.Settings.BytesDownloaded = 0;
            settings.Settings.BytesToDownload = 0;
            settings.Settings.DownloadProgress = string.Empty;
            settings.Settings.HasData = false;
            currentGame = null;
        }

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            ResetValues();
            if (!args.NewValue.HasItems())
            {
                return;
            }

            currentGame = args.NewValue.Last();
            settings.Settings.DownloadProgress = "TEST";
            SetCurrentGameManifest();
        }

        private void SetCurrentGameManifest()
        {
            if (!Steam.IsGameSteamGame(currentGame) || currentGame.InstallDirectory.IsNullOrEmpty())
            {
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

            var acfReader = new AcfReader(manifestPath);
            var acfStruct = acfReader.ACFFileToStruct();
            lastManifestCheck = DateTime.Now;

            var manifestStateFlags = int.Parse(acfStruct.SubACF["AppState"].SubItems["StateFlags"]);
            var appState = (AppState)manifestStateFlags;
            if (appState == AppState.FullyInstalled)
            {
                settings.Settings.HasData = false;
                return;
            }

            var lastUpdated = acfStruct.SubACF["AppState"].SubItems["LastUpdated"];
            if (lastUpdated == manifestLastUpdatedValue)
            {
                return;
            }

            manifestLastUpdatedValue = lastUpdated;


            settings.Settings.AppState = appState.ToString();
            settings.Settings.BytesDownloaded = long.Parse(acfStruct.SubACF["AppState"].SubItems["BytesDownloaded"]);
            settings.Settings.BytesToDownload = long.Parse(acfStruct.SubACF["AppState"].SubItems["BytesToDownload"]);
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
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
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