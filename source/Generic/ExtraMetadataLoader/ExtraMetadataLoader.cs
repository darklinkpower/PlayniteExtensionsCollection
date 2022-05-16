using ExtraMetadataLoader.Controls;
using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.Services;
using ExtraMetadataLoader.ViewModels;
using ExtraMetadataLoader.Views;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YouTubeCommon;

namespace ExtraMetadataLoader
{
    public class ExtraMetadataLoader : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly LogosDownloader logosDownloader;
        private readonly VideosDownloader videosDownloader;
        private readonly ExtraMetadataHelper extraMetadataHelper;
        private VideoPlayerControl detailsVideoControl;
        private VideoPlayerControl gridVideoControl;
        private VideoPlayerControl genericVideoControl;

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
                ElementList = new List<string> { "VideoLoaderControl", "VideoLoaderControlAlternative", "LogoLoaderControl", "YouTubeEmbeddedVideoControl" },
                SourceName = "ExtraMetadataLoader",
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "ExtraMetadataLoader",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            extraMetadataHelper = new ExtraMetadataHelper(PlayniteApi);
            logosDownloader = new LogosDownloader(PlayniteApi, settings.Settings, extraMetadataHelper);
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
        }
        
        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "LogoLoaderControl")
            {
                return new LogoLoaderControl(PlayniteApi, settings);
            }

            if (args.Name == "VideoLoaderControl")
            {
                if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
                {
                    if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Details && detailsVideoControl == null && !settings.Settings.EnableAlternativeDetailsVideoPlayer)
                    {
                        detailsVideoControl = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
                        return detailsVideoControl;
                    }
                    else if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Grid && gridVideoControl == null && !settings.Settings.EnableAlternativeGridVideoPlayer)
                    {
                        gridVideoControl = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
                        return gridVideoControl;
                    }
                }

                if (genericVideoControl == null && !settings.Settings.EnableAlternativeGenericVideoPlayer)
                {
                    genericVideoControl = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
                    return gridVideoControl;
                }
            }

            if (args.Name == "VideoLoaderControlAlternative")
            {
                if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
                {
                    if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Details && detailsVideoControl == null && settings.Settings.EnableAlternativeDetailsVideoPlayer)
                    {
                        detailsVideoControl = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
                        return detailsVideoControl;
                    }
                    else if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Grid && gridVideoControl == null && settings.Settings.EnableAlternativeGridVideoPlayer)
                    {
                        gridVideoControl = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
                        return gridVideoControl;
                    }

                    if (genericVideoControl == null && settings.Settings.EnableAlternativeGenericVideoPlayer)
                    {
                        genericVideoControl = new VideoPlayerControl(PlayniteApi, settings, GetPluginUserDataPath());
                        return gridVideoControl;
                    }
                }
            }
            
            if (args.Name == "YouTubeEmbeddedVideoControl")
            {
                return new YouTubeEmbeddedVideoControl(GetPluginUserDataPath(), PlayniteApi, settings);
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
                    Action = _ => {
                        var overwrite = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteLogosChoice"));
                        var isBackgroundDownload = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogAskSelectLogosAutomatically"));
                        var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingLogosSteam");

                        var progressOptions = new GlobalProgressOptions(progressTitle, true);
                        progressOptions.IsIndeterminate = false;
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count();
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                                logosDownloader.DownloadSteamLogo(game, overwrite, isBackgroundDownload);
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
                        var overwrite = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageOverwriteLogosChoice"));
                        var isBackgroundDownload = GetBoolFromYesNoDialog(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogAskSelectLogosAutomatically"));
                        var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingLogosSgdb");

                        var progressOptions = new GlobalProgressOptions(progressTitle, true);
                        progressOptions.IsIndeterminate = false;
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            var games = args.Games.Distinct();
                            a.ProgressMaxValue = games.Count();
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }
                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                                logosDownloader.DownloadSgdbLogo(game, overwrite, isBackgroundDownload);
                            };
                        }, progressOptions);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadGoogleLogoSelectedGame"),
                    MenuSection = $"Extra Metadata|{logosSection}",
                    Action = _ =>
                    {
                        CreateGoogleWindow();
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionSetLogoFromFile"),
                    MenuSection = $"Extra Metadata|{logosSection}",
                    Action = _ => {
                        var game = args.Games.Last();
                        var filePath = PlayniteApi.Dialogs.SelectFile("Logo|*.png");
                        if (!filePath.IsNullOrEmpty())
                        {
                            var logoPath = extraMetadataHelper.GetGameLogoPath(game, true);
                            var fileCopied = FileSystem.CopyFile(filePath, logoPath, true);
                            if (settings.Settings.ProcessLogosOnDownload && fileCopied)
                            {
                                logosDownloader.ProcessLogoImage(logoPath);
                            }
                            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
                        }
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
                    Description = ResourceProvider.GetString("LOCExtra_Metadata_Loader_MenuItemDescriptionDownloadSteamVideosSelectedGames"),
                    MenuSection = $"Extra Metadata|{videosSection}|{videosSection}",
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
                            a.ProgressMaxValue = games.Count();
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                                videosDownloader.DownloadSteamVideo(game, overwrite, false, true, false);
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
                            a.ProgressMaxValue = games.Count();
                            foreach (var game in games)
                            {
                                if (a.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                a.CurrentProgressValue++;
                                a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                                videosDownloader.DownloadSteamVideo(game, overwrite, false, false, true);
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
                            a.ProgressMaxValue = games.Count();
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

        private string GetPlatformName(Game game, bool addSpaceEnd = false)
        {
            if (game.Platforms == null)
            {
                return string.Empty;
            }
            else if (addSpaceEnd)
            {
                return game.Platforms[0].Name + " ";
            }
            else
            {
                return game.Platforms[0].Name;
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
                a.ProgressMaxValue = games.Count();
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
            var webView = PlayniteApi.WebViews.CreateView(1280, 750);

            // Age restricted videos can only be seen in the full version while logged in
            // so it's needed to redirect to the full YouTube site to view them
            var embedLoaded = false;
            webView.LoadingChanged += async (s, e) =>
            {
                if (!embedLoaded)
                {
                    if (webView.GetCurrentAddress().StartsWith(@"https://www.youtube.com/embed/"))
                    {
                        var source = await webView.GetPageSourceAsync();
                        if (source.Contains("<div class=\"ytp-error-content-wrap\"><div class=\"ytp-error-content-wrap-reason\">"))
                        {
                            webView.Navigate($"https://www.youtube.com/watch?v={youtubeVideoId}");
                        }
                        embedLoaded = true;
                    }
                }
            };

            webView.Navigate("data:text/html," + html);
            webView.OpenDialog();
            webView.Dispose();
        }

        private void CreateGoogleWindow()
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Height = 600;
            window.Width = 840;
            var game = PlayniteApi.MainView.SelectedGames.Last();
            window.Title = string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectLogo"), game.Name);

            window.Content = new GoogleImageDownloaderView();
            window.DataContext = new GoogleImageDownloaderViewModel(PlayniteApi, game, logosDownloader);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.ShowDialog();
        }

        private void CreateYoutubeWindow(bool searchShortVideos = true, bool showDownloadButton = true, string defaultSearchTerm = null)
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Height = 600;
            window.Width = 840;
            window.Title = ResourceProvider.GetString("LOCExtra_Metadata_Loader_YoutubeWindowDownloadTitle");

            window.Content = new YoutubeSearchView();
            window.DataContext = new YoutubeSearchViewModel(PlayniteApi, PlayniteApi.MainView.SelectedGames.Last(), videosDownloader, searchShortVideos, showDownloadButton, defaultSearchTerm);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.ShowDialog();
        }

        private bool GetBoolFromYesNoDialog(string caption)
        {
            var selection = PlayniteApi.Dialogs.ShowMessage(caption,
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
                var progressTitle = ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageLibUpdateAutomaticDownload");
                var progressOptions = new GlobalProgressOptions(progressTitle, true);
                progressOptions.IsIndeterminate = false;
                PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                {
                    var games = PlayniteApi.Database.Games.Where(x => x.Added != null && x.Added > settings.Settings.LastAutoLibUpdateAssetsDownload);
                    a.ProgressMaxValue = games.Count();
                    foreach (var game in games)
                    {
                        a.CurrentProgressValue++;
                        a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                        if (a.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }
                        if (!logosDownloader.DownloadSteamLogo(game, false, settings.Settings.LibUpdateSelectLogosAutomatically))
                        {
                            logosDownloader.DownloadSgdbLogo(game, false, settings.Settings.LibUpdateSelectLogosAutomatically);
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
                    var games = PlayniteApi.Database.Games.Where(x => x.Added != null && x.Added > settings.Settings.LastAutoLibUpdateAssetsDownload);
                    a.ProgressMaxValue = games.Count();
                    foreach (var game in games)
                    {
                        if (a.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        a.CurrentProgressValue++;
                        a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                        videosDownloader.DownloadSteamVideo(game, false, true, settings.Settings.DownloadVideosOnLibUpdate, settings.Settings.DownloadVideosMicroOnLibUpdate);
                    };
                }, progressOptions);
            }

            settings.Settings.LastAutoLibUpdateAssetsDownload = DateTime.Now;
            SavePluginSettings(settings.Settings);
            UpdateAssetsTagsStatus();
        }

        private void UpdateAssetsTagsStatus()
        {
            if (settings.Settings.UpdateMissingLogoTagOnLibUpdate ||
                settings.Settings.UpdateMissingVideoTagOnLibUpdate ||
                settings.Settings.UpdateMissingMicrovideoTagOnLibUpdate)
            {
                PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
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
                        if (logoMissingTag != null)
                        {
                            if (FileSystem.FileExists(extraMetadataHelper.GetGameLogoPath(game)))
                            {
                                PlayniteUtilities.RemoveTagFromGame(PlayniteApi, game, logoMissingTag);
                            }
                            else
                            {
                                PlayniteUtilities.AddTagToGame(PlayniteApi, game, logoMissingTag);
                            }
                        }
                        if (videoMissingTag != null)
                        {
                            if (FileSystem.FileExists(extraMetadataHelper.GetGameVideoPath(game)))
                            {
                                PlayniteUtilities.RemoveTagFromGame(PlayniteApi, game, videoMissingTag);
                            }
                            else
                            {
                                PlayniteUtilities.AddTagToGame(PlayniteApi, game, videoMissingTag);
                            }
                        }
                        if (microvideoMissingTag != null)
                        {
                            if (FileSystem.FileExists(extraMetadataHelper.GetGameVideoMicroPath(game)))
                            {
                                PlayniteUtilities.RemoveTagFromGame(PlayniteApi, game, microvideoMissingTag);
                            }
                            else
                            {
                                PlayniteUtilities.AddTagToGame(PlayniteApi, game, microvideoMissingTag);
                            }
                        }
                    }
                }, new GlobalProgressOptions(ResourceProvider.GetString("LOCExtra_Metadata_Loader_ProgressMessageUpdatingAssetsTags")));
            }
        }

        private bool ValidateExecutablesSettings(bool validateFfmpeg, bool validateYtdl)
        {
            var success = true;
            if (validateFfmpeg)
            {
                // ffmpeg
                if (settings.Settings.FfmpegPath.IsNullOrEmpty())
                {
                    logger.Debug($"ffmpeg has not been configured");
                    PlayniteApi.Notifications.Add(new NotificationMessage("ffmpegExeNotConfigured", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageFfmpegNotConfigured"), NotificationType.Error, () => OpenSettingsView()));
                    success = false;
                }
                else if (!FileSystem.FileExists(settings.Settings.FfmpegPath))
                {
                    logger.Debug($"ffmpeg executable not found in {settings.Settings.FfmpegPath}");
                    PlayniteApi.Notifications.Add(new NotificationMessage("ffmpegExeNotFound", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageFfmpegNotFound"), NotificationType.Error, () => OpenSettingsView()));
                    success = false;
                }

                // ffprobe
                if (settings.Settings.FfprobePath.IsNullOrEmpty())
                {
                    logger.Debug($"ffprobe has not been configured");
                    PlayniteApi.Notifications.Add(new NotificationMessage("ffprobeExeNotConfigured", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageFfprobeNotConfigured"), NotificationType.Error, () => OpenSettingsView()));
                    success = false;
                }
                else if (!FileSystem.FileExists(settings.Settings.FfprobePath))
                {
                    logger.Debug($"ffprobe executable not found in {settings.Settings.FfprobePath}");
                    PlayniteApi.Notifications.Add(new NotificationMessage("ffprobeExeNotFound", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageFfprobeNotFound"), NotificationType.Error, () => OpenSettingsView()));
                    success = false;
                }
            }
            
            if (validateYtdl)
            {
                // youtube-dl
                if (settings.Settings.YoutubeDlPath.IsNullOrEmpty())
                {
                    logger.Debug($"youtube-dl has not been configured");
                    PlayniteApi.Notifications.Add(new NotificationMessage("ytdlExeNotConfigured", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageYoutubeDlNotConfigured"), NotificationType.Error, () => OpenSettingsView()));
                    success = false;
                }
                else if (!FileSystem.FileExists(settings.Settings.YoutubeDlPath))
                {
                    logger.Debug($"youtube-dl executable not found in {settings.Settings.YoutubeDlPath}");
                    PlayniteApi.Notifications.Add(new NotificationMessage("ytdlExeNotFound", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageYoutubeDlNotFound"), NotificationType.Error, () => OpenSettingsView()));
                    success = false;
                }
            }

            return success;
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