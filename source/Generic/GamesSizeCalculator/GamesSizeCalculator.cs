using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
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
            var menuSection = ResourceProvider.GetString("LOCGame_Sizes_Calculator_MenuSectionDescriptionGameSizesCalculator");

            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCGame_Sizes_Calculator_MenuItemDescriptionCalculateSizesSelGames"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        UpdateGamesListSizes(args.Games.Distinct().ToList(), false);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCGame_Sizes_Calculator_MenuItemDescriptinoCalculateSizesSelGamesForce"),
                    MenuSection = menuSection,
                    Action = a =>
                    {
                        UpdateGamesListSizes(args.Games.Distinct().ToList(), true);
                    }
                }
            };
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (settings.Settings.CalculateOnGameClose)
            {
                CalculateGameSize(args.Game, false);
            }
        }

        private void UpdateGamesListSizes(List<Game> games, bool forceNonEmpty)
        {
            var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("LOCGame_Sizes_Calculator_DialogMessageCalculatingSizes"), true);
            progressOptions.IsIndeterminate = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                a.ProgressMaxValue = games.Count();
                foreach (var game in games)
                {
                    if (a.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    a.CurrentProgressValue++;
                    a.Text = $"{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                    CalculateGameSize(game, forceNonEmpty);
                }
            }, progressOptions);

            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCGame_Sizes_Calculator_DialogMessageDone"));
        }

        private void CalculateGameSize(Game game, bool forceNonEmpty)
        {
            if (!game.IsInstalled || (!forceNonEmpty && !string.IsNullOrEmpty(game.Version)))
            {
                return;
            }

            long size = 0;
            if (game.Platforms != null && game.Platforms.Any(p => p.Name == "PC (Windows)" || p.Name == "PC"))
            {
                if (string.IsNullOrEmpty(game.InstallDirectory) || !Directory.Exists(game.InstallDirectory))
                {
                    return;
                }

                try
                {
                    size = FileSystem.GetDirectorySizeOnDisk(game.InstallDirectory);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error while getting directory size in {game.InstallDirectory} of game {game.Name}");
                    PlayniteApi.Notifications.Messages.Add(
                        new NotificationMessage(game.Id.ToString(),
                            string.Format(ResourceProvider.GetString("LOCGame_Sizes_Calculator_NotificationMessageErrorGetDirSize"), game.InstallDirectory, game.Name, e.Message),
                            NotificationType.Error)
                        );

                    return;
                }
            }
            else
            {
                var romPath = game.Roms?.FirstOrDefault()?.Path;
                if (romPath == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(game.InstallDirectory))
                {
                    romPath = romPath.Replace("{InstallDir}", game.InstallDirectory).Replace("\\\\", "\\");
                }

                if (string.IsNullOrEmpty(romPath) || !FileSystem.FileExists(romPath))
                {
                    return;
                }

                try
                {
                    size = FileSystem.GetFileSizeOnDisk(romPath);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error while getting rom file size in {romPath}");
                    PlayniteApi.Notifications.Messages.Add(
                        new NotificationMessage(game.Id.ToString(),
                            string.Format(ResourceProvider.GetString("LOCGame_Sizes_Calculator_NotificationMessageErrorGetRomFileSize"), game.InstallDirectory, game.Name, e.Message),
                            NotificationType.Error)
                    );

                    return;
                }
            }

            if (size == 0)
            {
                return;
            }

            var fSize = GetBytesReadable(size);
            if (game.Version.IsNullOrEmpty() || (!game.Version.IsNullOrEmpty() && game.Version != fSize))
            {
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

    }
}