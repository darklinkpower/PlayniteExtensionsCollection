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
                foreach (var removedItem in ItemCollectionChangedArgs.RemovedItems)
                {
                    // Removed Game items have their ExtraMetadataDirectory deleted for cleanup
                    extraMetadataHelper.DeleteExtraMetadataDir(removedItem);
                }
            };

            PlayniteApi.Database.Platforms.ItemCollectionChanged += (sender, ItemCollectionChangedArgs) =>
            {
                foreach (var removedItem in ItemCollectionChangedArgs.RemovedItems)
                {
                    // Removed Platform items have their ExtraMetadataDirectory deleted for cleanup
                    extraMetadataHelper.DeleteExtraMetadataDir(removedItem);
                }
            };

            PlayniteApi.Database.Sources.ItemCollectionChanged += (sender, ItemCollectionChangedArgs) =>
            {
                foreach (var removedItem in ItemCollectionChangedArgs.RemovedItems)
                {
                    // Removed Source items have their ExtraMetadataDirectory deleted for cleanup
                    extraMetadataHelper.DeleteExtraMetadataDir(removedItem);
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
                    MenuSection = $"Extra Metadata|{logosSection}",
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
                                logosDownloader.DownloadSteamLogo(game, overwrite, false);
                                a.CurrentProgressValue++;
                            };
                        }, progressOptions);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadSgdbLogosSelectedGames"),
                    MenuSection = $"Extra Metadata|{logosSection}",
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
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDeleteLogosSelectedGames"),
                    MenuSection = $"Extra Metadata|{logosSection}",
                    Action = _ => {
                        foreach (var game in args.Games.Distinct())
                        {
                            extraMetadataHelper.DeleteGameLogo(game);
                        };
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionOpenExtraMetadataDirectory"),
                    MenuSection = $"Extra Metadata",
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
                        if (!logosDownloader.DownloadSteamLogo(game, false, settings.Settings.LibUpdateSelectLogosAutomatically))
                        {
                            logosDownloader.DownloadSgdbLogo(game, false, settings.Settings.LibUpdateSelectLogosAutomatically);
                        }
                        a.CurrentProgressValue++;
                    };
                }, progressOptions);
            }

            settings.Settings.LastAutoLibUpdateAssetsDownload = DateTime.Now;
            SavePluginSettings(settings.Settings);
            UpdateAssetsTagsStatus();
        }

        private void UpdateAssetsTagsStatus()
        {
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                if (settings.Settings.UpdateMissingLogoTagOnLibUpdate ||
    settings.Settings.UpdateMissingVideoTagOnLibUpdate ||
    settings.Settings.UpdateMissingMicrovideoTagOnLibUpdate)
                {
                    Tag logoMissingTag = null;
                    Tag videoMissingTag = null;
                    Tag microvideoMissingTag = null;
                    if (settings.Settings.UpdateMissingLogoTagOnLibUpdate)
                    {
                        logoMissingTag = PlayniteApi.Database.Tags.Add("[EMT] Logo Missing");
                    }
                    if (settings.Settings.UpdateMissingVideoTagOnLibUpdate)
                    {
                        videoMissingTag = PlayniteApi.Database.Tags.Add("[EMT] Video missing");
                    }
                    if (settings.Settings.UpdateMissingMicrovideoTagOnLibUpdate)
                    {
                        microvideoMissingTag = PlayniteApi.Database.Tags.Add("[EMT] Video Micro missing");
                    }

                    foreach (var game in PlayniteApi.Database.Games)
                    {
                        var gameUpdated = false;
                        if (logoMissingTag != null)
                        {
                            if (File.Exists(extraMetadataHelper.GetGameLogoPath(game)))
                            {
                                var tagRemoved = RemoveTag(game, logoMissingTag, false);
                                if (tagRemoved)
                                {
                                    gameUpdated = true;
                                }
                            }
                            else
                            {
                                var tagAdded = AddTag(game, logoMissingTag, false);
                                if (tagAdded)
                                {
                                    gameUpdated = true;
                                }
                            }
                        }
                        if (videoMissingTag != null)
                        {
                            if (File.Exists(extraMetadataHelper.GetGameVideoPath(game)))
                            {
                                var tagRemoved = RemoveTag(game, videoMissingTag, false);
                                if (tagRemoved)
                                {
                                    gameUpdated = true;
                                }
                            }
                            else
                            {
                                var tagAdded = AddTag(game, videoMissingTag, false);
                                if (tagAdded)
                                {
                                    gameUpdated = true;
                                }
                            }
                        }
                        if (microvideoMissingTag != null)
                        {
                            if (File.Exists(extraMetadataHelper.GetGameVideoPath(game)))
                            {
                                var tagRemoved = RemoveTag(game, microvideoMissingTag, false);
                                if (tagRemoved)
                                {
                                    gameUpdated = true;
                                }
                            }
                            else
                            {
                                var tagAdded = AddTag(game, microvideoMissingTag, false);
                                if (tagAdded)
                                {
                                    gameUpdated = true;
                                }
                            }
                        }
                        if (gameUpdated)
                        {
                            PlayniteApi.Database.Games.Update(game);
                        }
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCExtra_Metadata_Loader_ProgressMessageUpdatingAssetsTags")));
        }

        private bool AddTag(Game game, Tag tag, bool updateGame = true)
        {
            var tagAdded = false;
            if (game.TagIds == null)
            {
                game.TagIds = new List<Guid> { tag.Id };
                tagAdded = true;
            }
            else if (!game.TagIds.Contains(tag.Id))
            {
                game.TagIds.Add(tag.Id);
                tagAdded = true;
            }
            if (tagAdded && updateGame)
            {
                PlayniteApi.Database.Games.Update(game);
            }
            return tagAdded;
        }

        private bool RemoveTag(Game game, Tag tag, bool updateGame = true)
        {
            var tagRemoved = false;
            if (game.TagIds == null)
            {
                return false;
            }
            else if (game.TagIds.Contains(tag.Id))
            {
                game.TagIds.Remove(tag.Id);
                tagRemoved = true;
            }
            if (tagRemoved && updateGame)
            {
                PlayniteApi.Database.Games.Update(game);
            }
            return tagRemoved;
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