using GamesSizeCalculator.SteamSizeCalculation;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GamesSizeCalculator
{
    public class GamesSizeCalculator : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private GamesSizeCalculatorSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("97cc59db-3f80-4852-8bfc-a80304f9efe9");

        public GamesSizeCalculator(IPlayniteAPI api) : base(api)
        {
            settings = new GamesSizeCalculatorSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GamesSizeCalculatorSettingsView();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCGame_Sizes_Calculator_MenuItemDescriptionCalculateSizesSelGames"),
                    MenuSection = "Games Size Calculator",
                    Action = a =>
                    {
                        UpdateGamesListSizes(args.Games.Distinct().ToList(), false);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCGame_Sizes_Calculator_MenuItemDescriptinoCalculateSizesSelGamesForce"),
                    MenuSection = "Games Size Calculator",
                    Action = a =>
                    {
                        UpdateGamesListSizes(args.Games.Distinct().ToList(), true);
                    }
                }
            };
        }

        private void UpdateGamesListSizes(List<Game> games, bool forceNonEmpty)
        {
            var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("LOCGame_Sizes_Calculator_DialogMessageCalculatingSizes"), true);
            progressOptions.IsIndeterminate = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                a.ProgressMaxValue = games.Count();

                using (PlayniteApi.Database.BufferedUpdate())
                using (var steamClient = new SteamApiClient())
                {
                    var steamSizeCalculator = new SteamSizeCalculator(steamClient);
                    var steamAppIdUtility = GetDefaultSteamAppUtility();
                    foreach (var game in games)
                    {
                        if (a.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        a.CurrentProgressValue++;
                        a.Text = $"{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                        CalculateGameSize(game, steamSizeCalculator, steamAppIdUtility, forceNonEmpty);
                    }
                }
            }, progressOptions);

            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCGame_Sizes_Calculator_DialogMessageDone"));
        }

        private ISteamAppIdUtility GetDefaultSteamAppUtility()
        {
            var appListCache = new CachedFileDownloader("https://api.steampowered.com/ISteamApps/GetAppList/v2/",
                    Path.Combine(GetPluginUserDataPath(), "SteamAppList.json"),
                    TimeSpan.FromDays(3),
                    Encoding.UTF8);

            return new SteamAppIdUtility(appListCache);
        }

        private long GetGameDirectorySize(Game game, DateTime? onlyIfNewerThan = null)
        {
            if (game.InstallDirectory.IsNullOrEmpty() || !Directory.Exists(game.InstallDirectory))
            {
                return 0;
            }

            if (onlyIfNewerThan.HasValue &&
                (Directory.GetLastWriteTime(game.InstallDirectory) < onlyIfNewerThan.Value))
            {
                return 0;
            }

            try
            {
                return FileSystem.GetDirectorySizeOnDisk(game.InstallDirectory);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while getting directory size in {game.InstallDirectory} of game {game.Name}");
                PlayniteApi.Notifications.Messages.Add(
                    new NotificationMessage("GetInstalledGameSize" + game.Id.ToString(),
                        string.Format(ResourceProvider.GetString("LOCGame_Sizes_Calculator_NotificationMessageErrorGetDirSize"), game.InstallDirectory, game.Name, e.Message),
                        NotificationType.Error)
                    );

                return 0;
            }
        }

        private long GetGameRomSize(Game game, DateTime? onlyIfNewerThan = null)
        {
            var romPath = FileSystem.FixPathLength(game.Roms.First().Path);
            if (romPath.IsNullOrEmpty())
            {
                return 0;
            }

            if (!game.InstallDirectory.IsNullOrEmpty())
            {
                romPath = romPath.Replace("{InstallDir}", game.InstallDirectory).Replace("\\\\", "\\");
            }

            if (!FileSystem.FileExists(romPath))
            {
                return 0;
            }

            if (onlyIfNewerThan.HasValue &&
                (File.GetLastWriteTime(romPath) < onlyIfNewerThan))
            {
                return 0;
            }

            try
            {
                return FileSystem.GetFileSizeOnDisk(romPath);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while getting rom file size in {romPath}");
                PlayniteApi.Notifications.Messages.Add(
                    new NotificationMessage("GetRomSizeError" + game.Id.ToString(),
                        string.Format(ResourceProvider.GetString("LOCGame_Sizes_Calculator_NotificationMessageErrorGetRomFileSize"), game.InstallDirectory, game.Name, e.Message),
                        NotificationType.Error)
                );

                return 0;
            }
        }

        private long GetSteamInstallSizeOnline(Game game, SteamSizeCalculator sizeCalculator, ISteamAppIdUtility appIdUtility)
        {
            var appIdStr = appIdUtility.GetSteamGameId(game);
            if (!uint.TryParse(appIdStr, out uint appId))
            {
                return 0L;
            }

            var sizeTask = sizeCalculator.GetInstallSizeAsync(appId, includeDLC: settings.Settings.IncludeDlcInSteamCalculation, includeOptional: settings.Settings.IncludeOptionalInSteamCalculation);
            if (sizeTask.Wait(7000))
            {
                return sizeTask.Result ?? 0L;
            }
            else
            {
                logger.Warn($"Timed out while getting online Steam install size for {game.Name}");
                return 0L;
            }
        }

        private void CalculateGameSize(Game game, SteamSizeCalculator steamSizeCalculator, ISteamAppIdUtility steamAppIdUtility, bool forceNonEmpty, bool onlyIfNewerThanSetting = false)
        {
            if (!forceNonEmpty && !game.Version.IsNullOrEmpty())
            {
                return;
            }

            var onlyIfNewerThan = onlyIfNewerThanSetting ? settings.Settings.LastRefreshOnLibUpdate : (DateTime?)null;

            long size = 0;
            if (PlayniteUtilities.IsGamePcGame(game))
            {
                if (game.IsInstalled)
                {
                    size = GetGameDirectorySize(game, onlyIfNewerThan);
                }
                else if (settings.Settings.GetUninstalledGameSizeFromSteam && steamSizeCalculator != null && steamAppIdUtility != null)
                {
                    if (Steam.IsGameSteamGame(game) || settings.Settings.GetSizeFromSteamNonSteamGames)
                    {
                        size = GetSteamInstallSizeOnline(game, steamSizeCalculator, steamAppIdUtility);
                    }
                }
            }
            else if (game.IsInstalled && game.Roms.HasItems())
            {
                size = GetGameRomSize(game, onlyIfNewerThan);
            }

            if (size == 0)
            {
                return;
            }

            var fSize = GetBytesReadable(size);
            if (game.Version.IsNullOrEmpty() || (!game.Version.IsNullOrEmpty() && game.Version != fSize))
            {
                logger.Info($"Updated {game.Name} version field from {game.Version} to {fSize}");
                game.Version = fSize;
                PlayniteApi.Database.Games.Update(game);
            }
        }

        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // Returns in format "111.111 GB"
        // From https://stackoverflow.com/a/11124118
        private static string GetBytesReadable(long i)
        {
            // Only use GB so values can be sorted on Playnite
            double readable = i >> 20;

            // Divide by 1024 to get fractional value
            readable /= 1024;

            // Return formatted number with suffix
            return readable.ToString("000.000 GB");
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (!settings.Settings.CalculateNewGamesOnLibraryUpdate && !settings.Settings.CalculateModifiedGamesOnLibraryUpdate)
            {
                settings.Settings.LastRefreshOnLibUpdate = DateTime.Now;
                SavePluginSettings(settings.Settings);
                return;
            }

            var progressTitle = ResourceProvider.GetString("LOCGame_Sizes_Calculator_DialogMessageCalculatingSizes");
            var progressOptions = new GlobalProgressOptions(progressTitle, true);
            progressOptions.IsIndeterminate = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                ProcessGamesOnLibUpdate(a);
            }, progressOptions);

            settings.Settings.LastRefreshOnLibUpdate = DateTime.Now;
            SavePluginSettings(settings.Settings);
        }

        private void ProcessGamesOnLibUpdate(GlobalProgressActionArgs a)
        {
            var games = PlayniteApi.Database.Games.Where(x => x.IsInstalled);
            a.ProgressMaxValue = games.Count();

            using (PlayniteApi.Database.BufferedUpdate())
            using (var steamClient = new SteamApiClient())
            {
                var steamSizeCalculator = new SteamSizeCalculator(steamClient);
                var steamAppIdUtility = GetDefaultSteamAppUtility();
                foreach (var game in games)
                {
                    a.CurrentProgressValue++;
                    if (a.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (game.Added != null && game.Added > settings.Settings.LastRefreshOnLibUpdate)
                    {
                        if (!settings.Settings.CalculateNewGamesOnLibraryUpdate)
                        {
                            continue;
                        }

                        CalculateGameSize(game, steamSizeCalculator, steamAppIdUtility, false);
                    }
                    else if (settings.Settings.CalculateModifiedGamesOnLibraryUpdate)
                    {
                        // To make sure only Version fields filled by the extension are
                        // replaced
                        if (!game.Version.IsNullOrEmpty() && !game.Version.EndsWith(" GB"))
                        {
                            continue;
                        }

                        CalculateGameSize(game, steamSizeCalculator, steamAppIdUtility, true, true);
                    }
                };
            }
        }
    }
}