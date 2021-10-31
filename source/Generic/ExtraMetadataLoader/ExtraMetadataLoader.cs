using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.Services;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ExtraMetadataLoader
{
    public class ExtraMetadataLoader : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly Guid steamPluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
        private readonly LogosDownloader logosDownloader;
        private readonly ExtraMetadataHelper extraMetadataHelper;

        public ExtraMetadataLoaderSettingsViewModel settings { get; private set; }

        public override Guid Id { get; } = Guid.Parse("705fdbca-e1fc-4004-b839-1d040b8b4429");

        public ExtraMetadataLoader(IPlayniteAPI api) : base(api)
        {
            settings = new ExtraMetadataLoaderSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "VideoLoaderControl", "LogoLoaderControl" },
                SourceName = "ExtraMetadataLoader",
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "ExtraMetadataLoader",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            extraMetadataHelper = new ExtraMetadataHelper(PlayniteApi);
            logosDownloader = new LogosDownloader(PlayniteApi, settings.Settings, extraMetadataHelper);
            PlayniteApi.Database.Games.ItemCollectionChanged += (sender, ItemCollectionChangedArgs) =>
            {
                foreach (var removedGame in ItemCollectionChangedArgs.RemovedItems)
                {
                    // Removed games have their ExtraMetadataDirectory deleted for cleanup
                    extraMetadataHelper.DeleteGameExtraMetadataDir(removedGame);
                }
            };
        }
        
        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "LogoLoaderControl")
            {
                return new LogoLoaderControl(PlayniteApi, settings);
            }
            if (args.Name == "VideoLoaderControl")
            {
                return new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
            }

            return null;
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var logosSection = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemSectionLogos");
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadSteamLogosSelectedGames"),
                    MenuSection = $"ExtraMetadataLoader|{logosSection}",
                    Action = _ => {
                        var overwrite = GetBoolFromYesNoDialog();
                        var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingLogosSteam"));
                        progressOptions.IsIndeterminate = false;
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count();
                            foreach (var game in games)
                            {
                                logosDownloader.DownloadSteamLogo(game, overwrite, false, GetSteamId(game));
                                a.CurrentProgressValue++;
                            };
                        }, progressOptions);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadSgdbLogosSelectedGames"),
                    MenuSection = $"ExtraMetadataLoader|{logosSection}",
                    Action = _ => {
                        var overwrite = GetBoolFromYesNoDialog();
                        var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingLogosSgdb"));
                        progressOptions.IsIndeterminate = false;
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count();
                            foreach (var game in games)
                            {
                                logosDownloader.DownloadSgdbLogo(game, overwrite, false);
                                a.CurrentProgressValue++;
                            };
                        }, progressOptions);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDeleteLogosSelectedGames"),
                    MenuSection = $"ExtraMetadataLoader",
                    Action = _ => {
                        foreach (var game in args.Games.Distinct())
                        {
                            extraMetadataHelper.DeleteGameLogo(game);
                        };
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionOpenExtraMetadataDirectory"),
                    MenuSection = $"ExtraMetadataLoader",
                    Action = _ => {
                        foreach (var game in args.Games.Distinct())
                        {
                            Process.Start(extraMetadataHelper.GetExtraMetadataDirectory(game, true));
                        };
                    }
                },
            };
        }

        private bool GetBoolFromYesNoDialog()
        {
            var selection = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteLogosChoice"),
                ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectOption"),
                MessageBoxButton.YesNo);
            if (selection == MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetSteamId(Game game)
        {
            if (game.PluginId == steamPluginId)
            {
                return game.GameId;
            }
            else
            {
                if (game.Links == null)
                {
                    return null;
                }

                foreach (Link gameLink in game.Links)
                {
                    var linkMatch = Regex.Match(gameLink.Url, @"^https?:\/\/store\.steampowered\.com\/app\/(\d+)");
                    if (linkMatch.Success)
                    {
                        return linkMatch.Groups[1].Value;
                    }
                }
                return null;
            }
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // This needs to be done in this event because the ItemCollectionChanged raises the event
            // immediately when a game is added to the database, which means the games may not have
            // the necessary metadata added to download the assets automatically
            if (settings.Settings.DownloadLogosOnLibUpdate == true)
            {
                var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageLibUpdateAutomaticDownload"));
                progressOptions.IsIndeterminate = false;
                PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                {
                    var games = PlayniteApi.Database.Games.Where(x => x.Added > settings.Settings.LastAutoLibUpdateAssetsDownload);
                    a.ProgressMaxValue = games.Count();
                    foreach (var game in games)
                    {
                        if (!logosDownloader.DownloadSteamLogo(game, false, settings.Settings.LibUpdateSelectLogosAutomatically, GetSteamId(game)))
                        {
                            logosDownloader.DownloadSgdbLogo(game, false, settings.Settings.LibUpdateSelectLogosAutomatically);
                        }
                        a.CurrentProgressValue++;
                    };
                }, progressOptions);
            }

            settings.Settings.LastAutoLibUpdateAssetsDownload = DateTime.Now;
            SavePluginSettings(settings.Settings);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ExtraMetadataLoaderSettingsView();
        }
    }
}