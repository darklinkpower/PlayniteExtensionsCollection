using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.Interfaces;
using ExtraMetadataLoader.LogoProviders;
using ExtraMetadataLoader.Services;
using ExtraMetadataLoader.ViewModels;
using ExtraMetadataLoader.Views;
using ImageMagick;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WebCommon;
using YouTubeCommon;

namespace ExtraMetadataLoader
{
    public class ExtraMetadataLoader : GenericPlugin
    {
        private const string _logoMissingTag = "[EMT] Logo Missing";
        private const string _videoMissingTag = "[EMT] Video missing";
        private const string _videoMicroMissingTag = "[EMT] Video Micro missing";
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly VideosDownloader videosDownloader;
        private readonly ExtraMetadataHelper extraMetadataHelper;
        private readonly List<ILogoProvider> _logoProviders;
        private VideoPlayerControl detailsVideoControl;
        private VideoPlayerControl gridVideoControl;
        private VideoPlayerControl genericVideoControl;
        private List<VideoPlayerControl> configuredVideoControls = new List<VideoPlayerControl>();
        private List<VideoPlayerControl> fullscreenModeVideoControls = new List<VideoPlayerControl>();

        public ExtraMetadataLoaderSettingsViewModel settings { get; private set; }

        public override Guid Id { get; } = Guid.Parse("705fdbca-e1fc-4004-b839-1d040b8b4429");

        public ExtraMetadataLoader(IPlayniteAPI api) : base(api)
        {
            settings = new ExtraMetadataLoaderSettingsViewModel(this, PlayniteApi);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = "ExtraMetadataLoader",
                ElementList = new List<string>
                {
                    "VideoLoaderControl",
                    "VideoLoaderControlAlternative",
                    "VideoLoaderControl_Controls_Sound",
                    "VideoLoaderControl_Controls_NoSound",
                    "VideoLoaderControl_NoControls_NoSound",
                    "VideoLoaderControl_NoControls_Sound",
                    "LogoLoaderControl"
                }
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "ExtraMetadataLoader",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            extraMetadataHelper = new ExtraMetadataHelper(PlayniteApi);
            videosDownloader = new VideosDownloader(PlayniteApi, settings.Settings, extraMetadataHelper);
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

            var iconResourcesToAdd = new Dictionary<string, string>
            {
                { "emtDownloadIcon", "\xEF08" },
                { "emtYoutubeIcon", "\xE95F" },
                { "emtSteamIcon", "\xED71" },
                { "emtVideoMicroGenIcon", "\xECB4" },
                { "emtVideoFileIcon", "\xEB0A" },
                { "emtImageFileIcon", "\xEB1A" },
                { "emtFileDeleteIcon", "\xEC53" },
                { "emtFolderIcon", "\xEC5B" },
                { "emtGoogleIcon", "\xE8DF" }
            };

            foreach (var iconResource in iconResourcesToAdd)
            {
                PlayniteUtilities.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }

            _logoProviders = new List<ILogoProvider>
            {
                new SteamProvider(PlayniteApi, settings.Settings),
                new SteamGridDBProvider(PlayniteApi, settings.Settings)
            };
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            switch (args.Name)
            {
                case "LogoLoaderControl":
                    return new LogoLoaderControl(PlayniteApi, settings);
                case "VideoLoaderControl":
                    return GetVideoLoaderControl();
                case "VideoLoaderControlAlternative":
                    return GetVideoLoaderAlternativeControl();
                default:
                    if (args.Name.StartsWith("VideoLoaderControl_"))
                    {
                        return GetVideoLoaderControlConfigured(args.Name);
                    }

                    return null;
            }
        }

        private Control GetVideoLoaderControlConfigured(string controlName)
        {
            const char splitChar = '_';
            const string noControlsToken = "NoControls";
            const string noSoundToken = "NoSound";

            var controlNameParts = controlName.Split(splitChar);
            var displayControls = !controlNameParts.Contains(noControlsToken, StringComparer.OrdinalIgnoreCase);
            var useSound = !controlNameParts.Contains(noSoundToken, StringComparer.OrdinalIgnoreCase);

            var videoPlayerControl = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath(), true, displayControls, useSound);
            configuredVideoControls.Add(videoPlayerControl);

            return videoPlayerControl;
        }

        private Control GetVideoLoaderAlternativeControl()
        {
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                return null;
            }

            if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Details && settings.Settings.EnableAlternativeDetailsVideoPlayer)
            {
                return CreateVideoPlayerControlIfNeeded(ref detailsVideoControl);
            }

            if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Grid && settings.Settings.EnableAlternativeGridVideoPlayer)
            {
                return CreateVideoPlayerControlIfNeeded(ref gridVideoControl);
            }

            if (settings.Settings.EnableAlternativeGenericVideoPlayer)
            {
                return CreateVideoPlayerControlIfNeeded(ref genericVideoControl);
            }

            return null;
        }

        private Control CreateVideoPlayerControlIfNeeded(ref VideoPlayerControl control)
        {
            if (control == null)
            {
                control = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
            }

            return control;
        }

        private Control GetVideoLoaderControl()
        {
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Details && !settings.Settings.EnableAlternativeDetailsVideoPlayer)
                {
                    return CreateVideoPlayerControlIfNeeded(ref detailsVideoControl);
                }
                else if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Grid && !settings.Settings.EnableAlternativeGridVideoPlayer)
                {
                    return CreateVideoPlayerControlIfNeeded(ref gridVideoControl);
                }

                if (!settings.Settings.EnableAlternativeGenericVideoPlayer)
                {
                    return CreateVideoPlayerControlIfNeeded(ref genericVideoControl);
                }
            }
            else
            {
                var fullscreenModeVideoControl = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
                fullscreenModeVideoControls.Add(fullscreenModeVideoControl);
                return fullscreenModeVideoControl;
            }

            return null;
        }

        private void ClearVideoSources()
        {
            // This is done to free the video files and allow
            // deleting or overwriting it
            // Stops video from playing when game is starting
            detailsVideoControl?.ResetPlayerValues();
            gridVideoControl?.ResetPlayerValues();
            genericVideoControl?.ResetPlayerValues();

            foreach (var videoControl in configuredVideoControls)
            {
                videoControl.ResetPlayerValues();
            }

            foreach (var videoControl in fullscreenModeVideoControls)
            {
                videoControl.ResetPlayerValues();
            }
        }

        private void UpdatePlayersData()
        {
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Details)
                {
                    detailsVideoControl?.RefreshPlayer();
                    return;
                }
                else if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Grid)
                {
                    gridVideoControl?.RefreshPlayer();
                    return;
                }
            }

            genericVideoControl?.RefreshPlayer();
        }

        private void PauseAllVideoControls()
        {
            detailsVideoControl?.MediaPause();
            gridVideoControl?.MediaPause();
            genericVideoControl?.MediaPause();

            foreach (var videoControl in configuredVideoControls)
            {
                videoControl.MediaPause();
            }

            foreach (var videoControl in fullscreenModeVideoControls)
            {
                videoControl.MediaPause();
            }
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Stops video from playing when game is starting
            PauseAllVideoControls();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var logosSection = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemSectionLogos");
            var videosSection = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemSectionVideos");
            var videosMicroSection = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemSectionMicrovideos");
            
            //TODO Move each action to separate methods?
            var gameMenuItems = new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadSteamLogosSelectedGames"),
                    MenuSection = $"Extra Metadata|{logosSection}",
                    Icon = "emtSteamIcon",
                    Action = _ => {
                        var overwrite = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteLogosChoice"));
                        var isBackgroundDownload = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogAskSelectLogosAutomatically"));
                        var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingLogosSteam");

                        var progressOptions = new GlobalProgressOptions(progressTitle, true);
                        progressOptions.IsIndeterminate = false;
                        var logoProvider = _logoProviders.FirstOrDefault(x => x.Id == "steamProvider");
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count() + 1;
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                                GetGameLogo(logoProvider, game, isBackgroundDownload, overwrite, a.CancelToken);
                            };
                        }, progressOptions);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadSgdbLogosSelectedGames"),
                    MenuSection = $"Extra Metadata|{logosSection}",
                    Icon = "emtDownloadIcon",
                    Action = _ => {
                        var overwrite = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteLogosChoice"));
                        var isBackgroundDownload = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogAskSelectLogosAutomatically"));
                        var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingLogosSgdb");

                        var progressOptions = new GlobalProgressOptions(progressTitle, true);
                        progressOptions.IsIndeterminate = false;
                        var logoProvider = _logoProviders.FirstOrDefault(x => x.Id == "sgdbProvider");
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count() + 1;
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";

                                GetGameLogo(logoProvider, game, isBackgroundDownload, overwrite, a.CancelToken);
                            };
                        }, progressOptions);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadGoogleLogoSelectedGame"),
                    MenuSection = $"Extra Metadata|{logosSection}",
                    Icon = "emtGoogleIcon",
                    Action = _ =>
                    {
                        var game = args.Games.Last();
                        var googleProvider = new GoogleProvider(PlayniteApi, settings.Settings);
                        GetGameLogo(googleProvider, game, false, true);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionSetLogoFromFile"),
                    MenuSection = $"Extra Metadata|{logosSection}",
                    Icon = "emtImageFileIcon",
                    Action = _ => {
                        var game = args.Games.Last();
                        var filePath = PlayniteApi.Dialogs.SelectFile("Logo|*.png");
                        if (!filePath.IsNullOrEmpty())
                        {
                            var logoPath = extraMetadataHelper.GetGameLogoPath(game, true);
                            var fileCopied = FileSystem.CopyFile(filePath, logoPath, true);
                            if (settings.Settings.ProcessLogosOnDownload && fileCopied)
                            {
                                ProcessLogoImage(logoPath);
                            }
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                        }
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDeleteLogosSelectedGames"),
                    MenuSection = $"Extra Metadata|{logosSection}",
                    Icon = "emtFileDeleteIcon",
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
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadSteamVideosSelectedGames"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosSection}",
                    Icon = "emtSteamIcon",
                    Action = _ =>
                    {
                        if (!ValidateExecutablesSettings(true, false))
                        {
                            return;
                        }
                        var overwrite = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteVideosChoice"));
                        var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingVideosSteam");

                        var progressOptions = new GlobalProgressOptions(progressTitle, true);
                        progressOptions.IsIndeterminate = false;
                        ClearVideoSources();
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count() + 1;
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                                videosDownloader.DownloadSteamVideo(game, overwrite, false, a.CancelToken, true, false);
                            };
                        }, progressOptions);
                        UpdatePlayersData();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadVideoFromYoutube"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosSection}",
                    Icon = "emtYoutubeIcon",
                    Action = _ =>
                    {
                        ClearVideoSources();
                        if (!ValidateExecutablesSettings(true, true))
                        {
                            return;
                        }
                        CreateYoutubeWindow();
                        UpdatePlayersData();
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadVideoFromYoutubeBatch"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosSection}",
                    Icon = "emtYoutubeIcon",
                    Action = _ =>
                    {
                        ClearVideoSources();
                        if (!ValidateExecutablesSettings(true, true))
                        {
                            return;
                        }

                        DownloadYoutubeVideosBatch();
                        UpdatePlayersData();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadSteamVideosMicroSelectedGames"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosMicroSection}",
                    Icon = "emtSteamIcon",
                    Action = _ => 
                    {
                        if (!ValidateExecutablesSettings(true, false))
                        {
                            return;
                        }
                        ClearVideoSources();
                        var overwrite = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteVideosChoice"));
                        var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingVideosMicroSteam");

                        var progressOptions = new GlobalProgressOptions(progressTitle, true);
                        progressOptions.IsIndeterminate = false;
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count() + 1;
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                                videosDownloader.DownloadSteamVideo(game, overwrite, false, a.CancelToken, false, true);
                            };
                        }, progressOptions);
                        UpdatePlayersData();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionOpenExtraMetadataDirectory"),
                    MenuSection = $"Extra Metadata",
                    Icon = "emtFolderIcon",
                    Action = _ =>
                    {
                        foreach (var game in args.Games.Distinct())
                        {
                            Process.Start(extraMetadataHelper.GetExtraMetadataDirectory(game, true));
                        };
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionGenerateMicroFromVideo"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosMicroSection}",
                    Icon = "emtVideoMicroGenIcon",
                    Action = _ =>
                    {
                        if (!ValidateExecutablesSettings(true, false))
                        {
                            return;
                        }
                        ClearVideoSources();
                        var overwrite = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteVideosChoice"));
                        var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageGeneratingMicroVideosFromVideos");

                        var progressOptions = new GlobalProgressOptions(progressTitle, true);
                        progressOptions.IsIndeterminate = false;
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count() + 1;
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                                videosDownloader.ConvertVideoToMicro(game, overwrite);
                            };
                        }, progressOptions);
                        UpdatePlayersData();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionSetVideoFromSelectionToSelGame"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosSection}",
                    Icon = "emtVideoFileIcon",
                    Action = _ =>
                    {
                        if (!ValidateExecutablesSettings(true, false))
                        {
                            return;
                        }
                        ClearVideoSources();
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            videosDownloader.SelectedDialogFileToVideo(args.Games[0]);
                        }, new GlobalProgressOptions(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogProcessSettingVideoFromSelFile")));
                        UpdatePlayersData();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDeleteVideosSelectedGames"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosSection}",
                    Icon = "emtFileDeleteIcon",
                    Action = _ =>
                    {
                        ClearVideoSources();
                        foreach (var game in args.Games.Distinct())
                        {
                            extraMetadataHelper.DeleteGameVideo(game);
                        };
                        UpdatePlayersData();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDeleteVideosMicroSelectedGames"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosMicroSection}",
                    Icon = "emtFileDeleteIcon",
                    Action = _ =>
                    {
                        ClearVideoSources();
                        foreach (var game in args.Games.Distinct())
                        {
                            extraMetadataHelper.DeleteGameVideoMicro(game);
                        };
                        UpdatePlayersData();
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                }
            };

            if (settings.Settings.EnableYoutubeSearch)
            {
                gameMenuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemViewYoutubeReview"),
                    MenuSection = $"{videosSection}",
                    Icon = "emtYoutubeIcon",
                    Action = _ =>
                    {
                        ClearVideoSources();
                        var game = args.Games.Last();
                        var searchTerm = $"{game.Name} review";
                        var searchItems = YouTube.GetYoutubeSearchResults(searchTerm, false);
                        if (searchItems.Count > 0)
                        {
                            ViewYoutubeVideo(searchItems.First().VideoId);
                        }
                        else
                        {
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageVideoNotFound"));
                        }
                        UpdatePlayersData();
                    }
                });
                gameMenuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemViewYoutubeGameplay"),
                    MenuSection = $"{videosSection}",
                    Icon = "emtYoutubeIcon",
                    Action = _ =>
                    {
                        ClearVideoSources();
                        var game = args.Games.Last();
                        var searchTerm = $"{game.Name} gameplay";
                        var searchItems = YouTube.GetYoutubeSearchResults(searchTerm, false);
                        if (searchItems.Count > 0)
                        {
                            ViewYoutubeVideo(searchItems.First().VideoId);
                        }
                        else
                        {
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageVideoNotFound"));
                        }
                        UpdatePlayersData();
                    }
                });
                gameMenuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemViewYoutubeSearch"),
                    MenuSection = $"{videosSection}",
                    Icon = "emtYoutubeIcon",
                    Action = _ =>
                    {
                        ClearVideoSources();
                        var game = args.Games.Last();
                        var searchTerm = $"{game.Name}";
                        CreateYoutubeWindow(false, false, searchTerm);
                        UpdatePlayersData();
                    }
                });
            }

            return gameMenuItems;
        }

        private string GetPlatformName(Game game, bool appendSpace = false)
        {
            if (!game.Platforms.HasItems())
            {
                return string.Empty;
            }
            else if (appendSpace)
            {
                return game.Platforms[0].Name + " ";
            }
            else
            {
                return game.Platforms[0].Name;
            }
        }

        private bool GetGameLogo(ILogoProvider logoProvider, Game game, bool isBackgroundDownload, bool overwrite, CancellationToken cancelToken = default)
        {
            var logoPath = extraMetadataHelper.GetGameLogoPath(game, true);
            if (FileSystem.FileExists(logoPath) && !overwrite)
            {
                logger.Debug("Logo exists and overwrite is set to false, skipping");
                return true;
            }

            var logoUrl = logoProvider.GetLogoUrl(game, isBackgroundDownload, cancelToken);
            var downloadFileResult = HttpDownloader.DownloadFile(logoUrl, logoPath, cancelToken);
            if (downloadFileResult.Success && settings.Settings.ProcessLogosOnDownload)
            {
                ProcessLogoImage(logoPath);
            }

            return downloadFileResult.Success;
        }

        private bool ProcessLogoImage(string logoPath)
        {
            try
            {
                using (var image = new MagickImage(logoPath))
                {
                    var originalWitdh = image.Width;
                    var originalHeight = image.Height;
                    var imageChanged = false;
                    if (settings.Settings.LogoTrimOnDownload)
                    {
                        image.Trim();
                        if (originalWitdh != image.Width || originalHeight != image.Height)
                        {
                            imageChanged = true;
                            originalWitdh = image.Width;
                            originalHeight = image.Height;
                        }
                    }

                    if (settings.Settings.SetLogoMaxProcessDimensions)
                    {
                        if (settings.Settings.MaxLogoProcessWidth < image.Width || settings.Settings.MaxLogoProcessHeight < image.Height)
                        {
                            var targetWidth = settings.Settings.MaxLogoProcessWidth;
                            var targetHeight = settings.Settings.MaxLogoProcessHeight;
                            MagickGeometry size = new MagickGeometry(targetWidth, targetHeight)
                            {
                                IgnoreAspectRatio = false
                            };

                            image.Resize(size);
                            if (originalWitdh != image.Width || originalHeight != image.Height)
                            {
                                imageChanged = true;
                                originalWitdh = image.Width;
                                originalHeight = image.Height;
                            }
                        }
                    }

                    // Only save new image if dimensions changed
                    if (imageChanged)
                    {
                        image.Write(logoPath);
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while processing logo {logoPath}");
                return false;
            }
        }

        private void DownloadYoutubeVideosBatch()
        {
            var overwrite = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteVideosChoice"));
            var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogProgressMessageDownloadingYouTubeVideosAutomatic");
            var progressOptions = new GlobalProgressOptions(progressTitle, true);
            progressOptions.IsIndeterminate = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var games = PlayniteApi.MainView.SelectedGames.Distinct();
                a.ProgressMaxValue = games.Count() + 1;
                foreach (var game in games)
                {
                    if (a.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    a.CurrentProgressValue++;
                    a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                    if (!overwrite && FileSystem.FileExists(extraMetadataHelper.GetGameVideoPath(game)))
                    {
                        continue;
                    }

                    // Platform name is added to search for best results
                    var searchItems = YouTube.GetYoutubeSearchResults($"{game.Name} {GetPlatformName(game, true)}trailer", true);
                    if (searchItems.Count > 0)
                    {
                        videosDownloader.DownloadYoutubeVideoById(game, searchItems[0].VideoId, overwrite);
                    }
                };
            }, progressOptions);
        }

        private void ViewYoutubeVideo(string youtubeVideoId)
        {
            var youtubeLink = string.Format("https://www.youtube.com/embed/{0}", youtubeVideoId);
            var html = string.Format(@"
                <head>
                    <title>Extra Metadata</title>
                    <meta http-equiv='refresh' content='0; url={0}'>
                </head>
                <body style='margin:0'>
                </body>", youtubeLink);
            using (var webView = PlayniteApi.WebViews.CreateView(1280, 750))
            {
                // Age restricted videos can only be seen in the full version while logged in
                // so it's needed to redirect to the full YouTube site to view them
                var embedLoaded = false;
                webView.LoadingChanged += async (s, e) =>
                {
                    if (!embedLoaded && webView.GetCurrentAddress().StartsWith(@"https://www.youtube.com/embed/"))
                    {
                        var source = await webView.GetPageSourceAsync();
                        if (source.Contains("<div class=\"ytp-error-content-wrap\"><div class=\"ytp-error-content-wrap-reason\">"))
                        {
                            webView.Navigate($"https://www.youtube.com/watch?v={youtubeVideoId}");
                        }

                        embedLoaded = true;
                    }
                };

                webView.Navigate("data:text/html," + html);
                webView.OpenDialog();
            }
        }

        private void CreateYoutubeWindow(bool searchShortVideos = true, bool showDownloadButton = true, string defaultSearchTerm = "")
        {
            var selectedGames = PlayniteApi.MainView.SelectedGames;
            if (!selectedGames.HasItems())
            {
                return;
            }

            var game = PlayniteApi.MainView.SelectedGames.Last();
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Height = 600;
            window.Width = 840;
            window.Title = ResourceProvider.GetString("LOCExtra_Metadata_Loader_YoutubeWindowDownloadTitle");
            window.Content = new YoutubeSearchView();
            window.DataContext = new YoutubeSearchViewModel(PlayniteApi, game, videosDownloader, searchShortVideos, showDownloadButton, defaultSearchTerm);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.ShowDialog();
        }

        private bool GetBoolFromYesNoDialog(string caption)
        {
            var messageBoxTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectOption");
            var selection = PlayniteApi.Dialogs.ShowMessage(caption, messageBoxTitle, MessageBoxButton.YesNo);

            return selection == MessageBoxResult.Yes;
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // This needs to be done in this event because the ItemCollectionChanged raises the event
            // immediately when a game is added to the database, which means the games may not have
            // the necessary metadata added to download the assets automatically
            if (settings.Settings.DownloadLogosOnLibUpdate == true)
            {
                var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageLibUpdateAutomaticDownload");
                var progressOptions = new GlobalProgressOptions(progressTitle, true);
                progressOptions.IsIndeterminate = false;
                PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                {
                    var games = PlayniteApi.Database.Games.Where(x => x.Added != null && x.Added > settings.Settings.LastAutoLibUpdateAssetsDownload);
                    a.ProgressMaxValue = games.Count() + 1;
                    foreach (var game in games)
                    {
                        a.CurrentProgressValue++;
                        a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                        if (a.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        foreach (var logoProvider in _logoProviders)
                        {
                            var logoDownloaded = GetGameLogo(logoProvider, game, true, settings.Settings.LibUpdateSelectLogosAutomatically, a.CancelToken);
                            if (logoDownloaded)
                            {
                                break;
                            }
                        }
                    };
                }, progressOptions);
            }

            if ((settings.Settings.DownloadVideosOnLibUpdate || settings.Settings.DownloadVideosMicroOnLibUpdate) && ValidateExecutablesSettings(true, false))
            {
                var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageLibUpdateAutomaticDownloadVideos");
                var progressOptions = new GlobalProgressOptions(progressTitle, true);
                progressOptions.IsIndeterminate = false;
                PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                {
                    var games = PlayniteApi.Database.Games.Where(x => x.Added.HasValue && x.Added > settings.Settings.LastAutoLibUpdateAssetsDownload);
                    a.ProgressMaxValue = games.Count() + 1;
                    foreach (var game in games)
                    {
                        if (a.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        a.CurrentProgressValue++;
                        a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                        videosDownloader.DownloadSteamVideo(game, false, true, a.CancelToken, settings.Settings.DownloadVideosOnLibUpdate, settings.Settings.DownloadVideosMicroOnLibUpdate);
                    };
                }, progressOptions);
            }

            settings.Settings.LastAutoLibUpdateAssetsDownload = DateTime.Now;
            SavePluginSettings(settings.Settings);
            UpdateAssetsTagsStatus();
        }

        private void UpdateAssetsTagsStatus()
        {
            if (!ShouldUpdateAssetsTags())
            {
                return;
            }

            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                UpdateTagsForAllGames();
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCExtra_Metadata_Loader_ProgressMessageUpdatingAssetsTags")));
        }

        private bool ShouldUpdateAssetsTags()
        {
            return settings.Settings.UpdateMissingLogoTagOnLibUpdate ||
                   settings.Settings.UpdateMissingVideoTagOnLibUpdate ||
                   settings.Settings.UpdateMissingMicrovideoTagOnLibUpdate;
        }

        private void UpdateTagsForAllGames()
        {
            var tagSettings = new Dictionary<string, Tag>
            {
                { _logoMissingTag, settings.Settings.UpdateMissingLogoTagOnLibUpdate ? PlayniteApi.Database.Tags.Add(_logoMissingTag) : null },
                { _videoMissingTag, settings.Settings.UpdateMissingVideoTagOnLibUpdate ? PlayniteApi.Database.Tags.Add(_videoMissingTag) : null },
                { _videoMicroMissingTag, settings.Settings.UpdateMissingMicrovideoTagOnLibUpdate ? PlayniteApi.Database.Tags.Add(_videoMicroMissingTag) : null },
            };

            using (PlayniteApi.Database.BufferedUpdate())
            {
                foreach (var game in PlayniteApi.Database.Games)
                {
                    foreach (var tag in tagSettings)
                    {
                        if (tag.Value is null)
                        {
                            continue;
                        }
                        
                        if (HasFileForTag(tag.Value, game))
                        {
                            PlayniteUtilities.RemoveTagFromGame(PlayniteApi, game, tag.Value);
                        }
                        else
                        {
                            PlayniteUtilities.AddTagToGame(PlayniteApi, game, tag.Value);
                        }
                    }
                }
            }
        }

        private bool HasFileForTag(Tag tag, Game game)
        {
            return tag.Name == _logoMissingTag && FileSystem.FileExists(extraMetadataHelper.GetGameLogoPath(game)) ||
                   tag.Name == _videoMissingTag && FileSystem.FileExists(extraMetadataHelper.GetGameVideoPath(game)) ||
                   tag.Name == _videoMicroMissingTag && FileSystem.FileExists(extraMetadataHelper.GetGameVideoMicroPath(game));
        }

        private bool ValidateExecutablesSettings(bool validateFfmpeg, bool validateYtdl)
        {
            var success = true;
            if (validateFfmpeg)
            {
                success &= ValidateExecutable("ffmpeg", settings.Settings.FfmpegPath, "LOCExtra_Metadata_Loader_NotificationMessageFfmpegNotConfigured", "LOCExtra_Metadata_Loader_NotificationMessageFfmpegNotFound");
                success &= ValidateExecutable("ffprobe", settings.Settings.FfprobePath, "LOCExtra_Metadata_Loader_NotificationMessageFfprobeNotConfigured", "LOCExtra_Metadata_Loader_NotificationMessageFfprobeNotFound");
            }

            if (validateYtdl)
            {
                success &= ValidateExecutable("youtube-dl", settings.Settings.YoutubeDlPath, "LOCExtra_Metadata_Loader_NotificationMessageYoutubeDlNotConfigured", "LOCExtra_Metadata_Loader_NotificationMessageYoutubeDlNotFound");
            }

            return success;
        }

        private bool ValidateExecutable(string exeName, string exePath, string notConfiguredKey, string notFoundKey)
        {
            if (exePath.IsNullOrEmpty())
            {
                logger.Debug($"{exeName} has not been configured");
                ShowConfigurationError(notConfiguredKey);
                return false;
            }

            if (!FileSystem.FileExists(exePath))
            {
                logger.Debug($"{exeName} executable not found in {exePath}");
                ShowConfigurationError(notFoundKey);
                return false;
            }

            return true;
        }

        private void ShowConfigurationError(string messageKey)
        {
            PlayniteApi.Notifications.Add(new NotificationMessage(messageKey, ResourceProvider.GetString(messageKey), NotificationType.Error, () => OpenSettingsView()));
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