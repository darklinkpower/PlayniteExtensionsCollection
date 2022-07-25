using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using ThemesDetailsViewToGridViewConverter.Models;

namespace ThemesDetailsViewToGridViewConverter
{
    public class ThemesDetailsViewToGridViewConverter : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string themeIdHelium = @"8b15c46a-90c2-4fe5-9ebb-1ab25ba7fcb1";
        private const string themeIdStardust = @"Stardust 2.0_1fb333b2-255b-43dd-aec1-8e2f2d5ea002";
        private const string themeIdMythic = @"Mythic_e231056c-4fa7-49d8-ad2b-0a6f1c589eb8";
        private const string themeIdHarmony = @"Harmony_d49ef7bc-49de-4fd0-9a67-bd1f26b56047";
        private const string themeIdDhDawn = @"felixkmh_DesktopTheme_DH_Dawn";
        private const string themeIdDhNight = @"felixkmh_DuplicateHider_Night_Theme";
        private const string messagesCaption = "darklinkpower's Grid View Converter";

        private readonly string baseThemesDirectory;

        private ThemesDetailsViewToGridViewConverterSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("ef8a7226-eedc-478d-a506-92ee6c088aa3");

        public ThemesDetailsViewToGridViewConverter(IPlayniteAPI api) : base(api)
        {
            settings = new ThemesDetailsViewToGridViewConverterSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            baseThemesDirectory = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "Themes", "Desktop");
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (PlayniteApi.ApplicationInfo.Mode != ApplicationMode.Desktop)
            {
                return;
            }

            ProcessThemeChanges(PlayniteApi.ApplicationSettings.DesktopTheme);

        }

        public void ProcessAllSupportedThemes()
        {
            var themesIds = new List<string>
            {
                themeIdHelium,
                themeIdStardust,
                themeIdMythic,
                themeIdHarmony,
                themeIdDhDawn,
                themeIdDhNight
            };

            foreach (var themeId in themesIds)
            {
                ProcessThemeChanges(themeId);
            }
        }

        private void ProcessThemeChanges(string themeId)
        {
            var activeThemeDirectoryName = GetThemeSubdirectory(themeId);
            if (activeThemeDirectoryName.IsNullOrEmpty())
            {
                return;
            }

            logger.Debug($"Active theme: {themeId}, DirName: {activeThemeDirectoryName}");
            var activeThemeDirectory = Path.Combine(baseThemesDirectory, activeThemeDirectoryName);
            if (!FileSystem.DirectoryExists(activeThemeDirectory))
            {
                logger.Warn($"Theme directory not found in {activeThemeDirectory}");
                return;
            }

            var manifestPath = Path.Combine(activeThemeDirectory, "theme.yaml");

            ExtensionManifest manifest;
            try
            {
                manifest = Serialization.FromYamlFile<ExtensionManifest>(manifestPath);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error deserializing theme manifest in {manifestPath}");
                return;
            }
            
            var minSupportedVersion = GetMinimumSupportedVersion(themeId);
            if (Version.Parse(manifest.Version) < minSupportedVersion)
            {
                logger.Debug($"Theme {manifest.Name} version not supported. Min version: {minSupportedVersion}, Theme version: {manifest.Version}");
                PlayniteApi.Notifications.Add(
                    new NotificationMessage(
                        Guid.NewGuid().ToString(),
                        messagesCaption + "\n\n" + string.Format(ResourceProvider.GetString("LOCThemeDetailsToGridConverter_ThemeVersionNotSupported"), manifest.Version, manifest.Name),
                        NotificationType.Error));
                
                return;
            }

            if (GetShouldConvertDetailsToGrid(themeId))
            {
                ConvertDetailsToGrid(manifest, activeThemeDirectory);
            }
            else
            {
                RestoreOriginalGridDetails(manifest, activeThemeDirectory);
            }
        }

        private void ConvertDetailsToGrid(ExtensionManifest manifest, string activeThemeDirectory)
        {
            var detailsViewPath = Path.Combine(activeThemeDirectory, "Views", "DetailsViewGameOverview.xaml");
            var gridViewPath = Path.Combine(activeThemeDirectory, "Views", "GridViewGameOverview.xaml");

            if (!FileSystem.FileExists(detailsViewPath))
            {
                logger.Warn($"Details view file not found in {detailsViewPath}");
                return;
            }
            else if (!FileSystem.FileExists(gridViewPath))
            {
                logger.Warn($"Grid view file not found in {gridViewPath}");
                return;
            }

            var detailsViewContent = FileSystem.ReadStringFromFile(detailsViewPath);
            var gridViewContent = FileSystem.ReadStringFromFile(gridViewPath);
            var gridViewNewContent = detailsViewContent.Replace(@"TargetType=""{x:Type DetailsViewGameOverview}""", @"TargetType=""{x:Type GridViewGameOverview}""");
            if (gridViewContent == gridViewNewContent)
            {
                logger.Debug("gridViewContent and gridViewNewContent were equal");
                return;
            }

            if (!IsNewContentValid(gridViewNewContent))
            {
                return;
            }

            var gridBackupPath = Path.Combine(GetPluginUserDataPath(), "Backup", manifest.Id, manifest.Version, "GridViewGameOverview.xaml");
            FileSystem.CopyFile(gridViewPath, gridBackupPath);

            try
            {
                FileSystem.WriteStringToFile(gridViewPath, gridViewNewContent);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to write gridViewNewContent to {gridViewPath}");
                return;
            }

            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCThemeDetailsToGridConverter_ConvertSuccessMessage"), manifest.Name),
                messagesCaption);
        }

        private bool IsNewContentValid(string gridViewNewContent)
        {
            // Check if there are multiple 1st level style definitions, to
            // prevent conflicts using the same key in both Details and Grid file
            var matches = Regex.Matches(gridViewNewContent, @"\n    <Style TargetType=""");
            if (matches.Count != 1)
            {
                logger.Warn($"Content matched more {matches.Count} Style definitions");
                return false;
            }

            // Check if content is XML valid
            try
            {
                XElement.Parse(gridViewNewContent);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void RestoreOriginalGridDetails(ExtensionManifest manifest, string activeThemeDirectory)
        {
            var gridViewPath = Path.Combine(activeThemeDirectory, "Views", "GridViewGameOverview.xaml");
            var gridBackupPath = Path.Combine(GetPluginUserDataPath(), "Backup", manifest.Id, manifest.Version, "GridViewGameOverview.xaml");
            if (!FileSystem.FileExists(gridBackupPath))
            {
                logger.Warn($"Grid view backup file not found in {gridBackupPath}");
                return;
            }

            var gridViewContent = FileSystem.ReadStringFromFile(gridViewPath);
            var gridViewBackupContent = FileSystem.ReadStringFromFile(gridBackupPath);
            if (gridViewContent == gridViewBackupContent)
            {
                logger.Debug("gridViewContent and gridViewBackupContent were equal");
                return;
            }

            FileSystem.CopyFile(gridBackupPath, gridViewPath);
            FileSystem.DeleteFileSafe(gridBackupPath);
            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCThemeDetailsToGridConverter_RestoreSuccessMessage"), manifest.Name),
                messagesCaption);
        }

        private string GetThemeSubdirectory(string themeId)
        {
            switch (themeId)
            {
                case themeIdHelium:
                    return "8b15c46a-90c2-4fe5-9ebb-1ab25ba7fcb1";
                case themeIdStardust:
                    return "Stardust 2.0_1fb333b2-255b-43dd-aec1-8e2f2d5ea002";
                case themeIdMythic:
                    return "Mythic_e231056c-4fa7-49d8-ad2b-0a6f1c589eb8";
                case themeIdHarmony:
                    return "Harmony_d49ef7bc-49de-4fd0-9a67-bd1f26b56047";
                case themeIdDhDawn:
                    return "felixkmh_DesktopTheme_DH_Dawn";
                case themeIdDhNight:
                    return "felixkmh_DuplicateHider_Night_Theme";
                default:
                    return null;
            }
        }

        private bool GetShouldConvertDetailsToGrid(string themeId)
        {
            switch (themeId)
            {
                case themeIdHelium:
                    return settings.Settings.ConvertHelium;
                case themeIdStardust:
                    return settings.Settings.ConvertStardust;
                case themeIdMythic:
                    return settings.Settings.ConvertMythic;
                case themeIdHarmony:
                    return settings.Settings.ConvertHarmony;
                case themeIdDhDawn:
                    return settings.Settings.ConvertDhDawn;
                case themeIdDhNight:
                    return settings.Settings.ConvertDhNight;
                default:
                    return false;
            }
        }

        private Version GetMinimumSupportedVersion(string themeId)
        {
            switch (themeId)
            {
                case themeIdHelium:
                    return Version.Parse("1.31");
                case themeIdStardust:
                    return Version.Parse("2.39");
                case themeIdMythic:
                    return Version.Parse("1.24");
                case themeIdHarmony:
                    return Version.Parse("2.33");
                case themeIdDhDawn:
                    return Version.Parse("1.0");
                case themeIdDhNight:
                    return Version.Parse("3.0");
                default:
                    return Version.Parse("9999");
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ThemesDetailsViewToGridViewConverterSettingsView();
        }
    }
}