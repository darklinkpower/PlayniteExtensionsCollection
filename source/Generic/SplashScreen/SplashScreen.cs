using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using SplashScreen.ViewModels;
using SplashScreen.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SplashScreen
{
    public class SplashScreen : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private string pluginInstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private Window currentSplashWindow;
        private bool? isMusicMutedBackup;

        private SplashScreenSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d8c4f435-2bd2-49d8-98f6-87b1d415934a");

        public SplashScreen(IPlayniteAPI api) : base(api)
        {
            settings = new SplashScreenSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        private void MuteBackgroundMusic()
        {
            if (PlayniteApi.ApplicationInfo.Mode != ApplicationMode.Fullscreen)
            {
                return;
            }

            if (PlayniteApi.ApplicationSettings.Fullscreen.IsMusicMuted == false)
            {
                PlayniteApi.ApplicationSettings.Fullscreen.IsMusicMuted = true;
                isMusicMutedBackup = false;
            }
        }

        private void RestoreBackgroundMusic()
        {
            if (PlayniteApi.ApplicationInfo.Mode != ApplicationMode.Fullscreen)
            {
                return;
            }

            if (isMusicMutedBackup != null && isMusicMutedBackup == false)
            {
                PlayniteApi.ApplicationSettings.Fullscreen.IsMusicMuted = false;
                isMusicMutedBackup = null;
            }
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && !settings.Settings.ExecuteInDesktopMode)
            {
                logger.Info("Execution disabled for Desktop mode in settings");
                return;
            }
            else if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen && !settings.Settings.ExecuteInFullscreenMode)
            {
                logger.Info("Execution disabled for Fullscreen mode in settings");
                return;
            }

            // In case somebody starts another game or if splash screen was not closed before for some reason
            if (currentSplashWindow != null)
            {
                // Dispatcher is needed since the window is created on a different thread
                currentSplashWindow.Dispatcher.Invoke(() => currentSplashWindow.Close());
                currentSplashWindow = null;
                RestoreBackgroundMusic();
            }

            var game = args.Game;
            if (game.Features != null &&
                game.Features.Any(x => x.Name.Equals("[Splash Screen] Disable", StringComparison.OrdinalIgnoreCase)))
            {
                logger.Info($"{game.Name} has splashscreen disable feature");
                return;
            }

            currentSplashWindow = null;
            
            var splashImagePath = string.Empty;
            var logoPath = string.Empty;
            var videoPath = string.Empty;

            var showSplashImage = true;
            if (game.Features != null)
            {
                if (game.Features.Any(x => x.Name == "[Splash Screen] Skip splash image"))
                {
                    logger.Info($"Game has splash image skip feature");
                    showSplashImage = false;
                }
            }
            if (showSplashImage == true)
            {
                if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
                {
                    showSplashImage = settings.Settings.ViewImageSplashscreenDesktopMode;
                }
                else
                {
                    showSplashImage = settings.Settings.ViewImageSplashscreenFullscreenMode;
                }
            }

            if (showSplashImage == true)
            {
                if (settings.Settings.UseBlackSplashscreen == true)
                {
                    splashImagePath = Path.Combine(pluginInstallPath, "Images", "SplashScreenBlack.png");
                }
                else
                {
                    splashImagePath = GetSplashImagePath(game);
                }

                bool closeSplashScreenAutomatic = false;
                if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
                {
                    closeSplashScreenAutomatic = settings.Settings.CloseSplashScreenDesktopMode;
                }
                else
                {
                    closeSplashScreenAutomatic = settings.Settings.CloseSplashScreenFullscreenMode;
                }

                if (settings.Settings.ShowLogoInSplashscreen)
                {
                    logoPath = GetSplashLogoPath(game);
                }
            }

            if ((PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && settings.Settings.ViewVideoDesktopMode) ||
                (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen && settings.Settings.ViewVideoFullscreenMode))
            {
                videoPath = GetSplashVideoPath(game);
                if (videoPath != string.Empty)
                {
                    CreateSplashVideoWindow(showSplashImage, videoPath, splashImagePath, logoPath);
                }
                else if (showSplashImage == true)
                {
                    CreateSplashImageWindow(splashImagePath, logoPath);
                }
            }
            else if (showSplashImage == true)
            {
                CreateSplashImageWindow(splashImagePath, logoPath);
            }
        }

        private void CreateSplashVideoWindow(bool showSplashImage, string videoPath, string splashImagePath, string logoPath)
        {
            // Mutes Playnite Background music to make sure its not playing when video or splash screen image
            // is active and prevents music not stopping when game is already running
            MuteBackgroundMusic();

            // This will tell them main (Playnite) thread when to continue later
            var stopBlockingEvent = new AutoResetEvent(false);

            // This creates new UI thread for splash window.
            // This way we can do any UI things we want with our window without affecting Playnite's UI.
            var splashWindowThread = new Thread(new ThreadStart(() =>
            {
                var content = new SplashScreenVideo(videoPath);
                currentSplashWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    WindowState = WindowState.Maximized,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Focusable = false,
                    Content = content
                };

                currentSplashWindow.Closed += (_, __) =>
                {
                    // To prevent video sound from playing in case window was closed manually
                    content.VideoPlayer.Source = null;

                    // Apparently necessary to properly cleanup UI thread we created for this window.
                    currentSplashWindow.Dispatcher.InvokeShutdown();

                    // Unblock main thread and let Playnite start a game. Works if window is closed manually or by event
                    stopBlockingEvent?.Set();
                    currentSplashWindow = null;
                };

                content.VideoPlayer.MediaEnded += async (_, __) =>
                {
                    content.VideoPlayer.Source = null;
                    if (showSplashImage == true)
                    {
                        currentSplashWindow.Content = new SplashScreenImage(settings.Settings, splashImagePath, logoPath);
                        // This needs to run async otherwise we would block our UI thread and prevent splash image from showing
                        await Task.Delay(3000);

                        // Unblock main thread and let Playnite start a game.
                        stopBlockingEvent?.Set();

                        if ((PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && settings.Settings.CloseSplashScreenDesktopMode) ||
                            (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen && settings.Settings.CloseSplashScreenFullscreenMode))
                        {
                            // Closes splash screen after another 60 seconds, but the game is already starting.
                            await Task.Delay(60000);
                            currentSplashWindow.Close();
                            currentSplashWindow = null;
                        }
                    }
                    else
                    {
                        // Unblock main thread and let Playnite start a game.
                        stopBlockingEvent?.Set();
                        currentSplashWindow.Close();
                    }
                };

                currentSplashWindow.Show();
                System.Windows.Threading.Dispatcher.Run();
            }));

            splashWindowThread.SetApartmentState(ApartmentState.STA);
            splashWindowThread.IsBackground = true;
            splashWindowThread.Start();

            // Blocks execution until the event occurs from splash window
            stopBlockingEvent.WaitOne();
            stopBlockingEvent.Dispose();
            stopBlockingEvent = null;
        }

        private void CreateSplashImageWindow(string splashImagePath, string logoPath)
        {
            // Mutes Playnite Background music to make sure its not playing when video or splash screen image
            // is active and prevents music not stopping when game is already running
            MuteBackgroundMusic();

            var stopBlockingEvent = new AutoResetEvent(false);
            var splashWindowThread = new Thread(new ThreadStart(() =>
            {
                currentSplashWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    WindowState = WindowState.Maximized,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Focusable = false,
                    Content = new SplashScreenImage(settings.Settings, splashImagePath, logoPath)
                };

                currentSplashWindow.Closed += (_, __) =>
                {
                    // Apparently necessary to properly cleanup UI thread we created for this window.
                    currentSplashWindow.Dispatcher.InvokeShutdown();

                    // Unblock main thread and let Playnite start a game. Works if window is closed manually or by event
                    stopBlockingEvent?.Set();
                    currentSplashWindow = null;
                };

                Task.Delay(3000).ContinueWith(_ =>
                {
                    stopBlockingEvent?.Set();
                });

                if ((PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && settings.Settings.CloseSplashScreenDesktopMode) ||
                    (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen && settings.Settings.CloseSplashScreenFullscreenMode))
                {
                    Task.Delay(60000).ContinueWith(_ =>
                    {
                        // It needs to be checked if window still exists, in case it was closed manually or
                        // closed by the extension with game close
                        if (currentSplashWindow != null)
                        {
                            currentSplashWindow.Dispatcher.Invoke(() => currentSplashWindow.Close());
                            currentSplashWindow = null;
                        }
                    });
                }

                currentSplashWindow.Show();
                System.Windows.Threading.Dispatcher.Run();
            }));

            splashWindowThread.SetApartmentState(ApartmentState.STA);
            splashWindowThread.IsBackground = true;
            splashWindowThread.Start();
            stopBlockingEvent.WaitOne();
            stopBlockingEvent.Dispose();
            stopBlockingEvent = null;
        }

        private string GetSplashVideoPath(Game game)
        {
            string videoPathTemplate = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "{0}", "{1}", "VideoIntro.mp4");

            string splashVideo = string.Format(videoPathTemplate, "games", game.Id.ToString());
            if (File.Exists(splashVideo))
            {
                logger.Info(string.Format("Specific game video found in {0}", splashVideo));
                return splashVideo;
            }

            if (game.Source != null)
            {
                splashVideo = string.Format(videoPathTemplate, "sources", game.Source.Id.ToString());
                if (File.Exists(splashVideo))
                {
                    logger.Info(string.Format("Source video found in {0}", splashVideo));
                    return splashVideo;
                }
            }

            if (game.Platforms != null)
            {
                splashVideo = string.Format(videoPathTemplate, "platforms", game.Platforms[0].Id.ToString());
                if (File.Exists(splashVideo))
                {
                    logger.Info(string.Format("Platform video found in {0}", splashVideo));
                    return splashVideo;
                }
            }

            logger.Info("Video not found");
            return string.Empty;
        }

        private string GetSplashLogoPath(Game game)
        {
            if (settings.Settings.UseIconAsLogo)
            {
                if (game.Icon != null)
                {
                    if (!game.Icon.StartsWith("http"))
                    {
                        logger.Info("Found game icon");
                        return PlayniteApi.Database.GetFullFilePath(game.Icon);
                    }
                }
            }
            else
            {
                var logoPathSearch = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString(), "Logo.png");
                if (File.Exists(logoPathSearch))
                {
                    logger.Info(string.Format("Specific game logo found in {0}", logoPathSearch));
                    return logoPathSearch;
                }
            }

            logger.Info("Logo not found");
            return string.Empty;
        }

        private string GetSplashImagePath(Game game)
        {
            if (game.BackgroundImage != null)
            {
                if (!game.BackgroundImage.StartsWith("http"))
                {
                    logger.Info("Found game background image");
                    return PlayniteApi.Database.GetFullFilePath(game.BackgroundImage);
                }
            }

            if (game.Platforms != null)
            {
                if (game.Platforms[0].Background != null)
                {
                    logger.Info("Found platform background image");
                    return PlayniteApi.Database.GetFullFilePath(game.Platforms[0].Background);
                }
            }

            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                logger.Info("Using generic Desktop mode background image");
                return Path.Combine(pluginInstallPath, "Images", "SplashScreenDesktopMode.png");
            }

            logger.Info("Using generic Fullscreen mode background image");
            return Path.Combine(pluginInstallPath, "Images", "SplashScreenFullscreenMode.png");
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Close splash screen manually it was not closed automatically
            if (currentSplashWindow != null)
            {
                currentSplashWindow.Dispatcher.Invoke(() => currentSplashWindow.Close());
                currentSplashWindow = null;
            }
            RestoreBackgroundMusic();
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SplashScreenSettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSplashScreen_MenuItemInvoke-OpenVideoManagerWindowDescription"),
                    MenuSection = "@Splash Screen",
                    Action = a => {
                        OpenVideoManager();
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSplashScreen_MenuItemAdd-DisableFeature"),
                    MenuSection = "@Splash Screen",
                    Action = a => {
                        AddImageSkipFeature("[Splash Screen] Disable");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSplashScreen_MenuItemRemove-DisableFeature"),
                    MenuSection = "@Splash Screen",
                    Action = a => {
                        RemoveImageSkipFeature("[Splash Screen] Disable");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSplashScreen_MenuItemAdd-ImageSkipFeature"),
                    MenuSection = "@Splash Screen",
                    Action = a => {
                        AddImageSkipFeature("[Splash Screen] Skip splash image");
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSplashScreen_MenuItemRemove-ImageSkipFeature"),
                    MenuSection = "@Splash Screen",
                    Action = a => {
                        RemoveImageSkipFeature("[Splash Screen] Skip splash image");
                    }
                }
            };
        }

        private void OpenVideoManager()
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Height = 600;
            window.Width = 800;
            window.Title = $"Splash Screen - {ResourceProvider.GetString("LOCSplashScreen_VideoManagerTitle")}";
            window.Content = new VideoManager();
            window.DataContext = new VideoManagerViewModel(PlayniteApi);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.ShowDialog();
        }

        private void AddImageSkipFeature(string featureName)
        {
            GameFeature feature = PlayniteApi.Database.Features.Add(featureName);
            int featureAddedCount = 0;
            foreach (var game in PlayniteApi.MainView.SelectedGames)
            {
                if (game.FeatureIds == null)
                {
                    game.FeatureIds = new List<Guid> { feature.Id };
                    PlayniteApi.Database.Games.Update(game);
                    featureAddedCount++;
                }
                else if (!game.FeatureIds.Contains(feature.Id))
                {
                    game.FeatureIds.AddMissing(feature.Id);
                    PlayniteApi.Database.Games.Update(game);
                    featureAddedCount++;
                }
            }

            PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCSplashScreen_ExcludeFeatureAddResultsMessage"), feature.Name, featureAddedCount), "Splash Screen");
        }

        private void RemoveImageSkipFeature(string featureName)
        {
            GameFeature feature = PlayniteApi.Database.Features.Add(featureName);
            int featureRemovedCount = 0;
            foreach (var game in PlayniteApi.MainView.SelectedGames)
            {
                if (game.FeatureIds != null)
                {
                    if (game.FeatureIds.Contains(feature.Id))
                    {
                        game.FeatureIds.Remove(feature.Id);
                        PlayniteApi.Database.Games.Update(game);
                        featureRemovedCount++;
                        logger.Info(string.Format("Removed \"{0}\" feature from \"{1}\"", feature.Name, game.Name));
                    }
                }
            }
            PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCSplashScreen_ExcludeFeatureRemoveResultsMessage"), feature.Name, featureRemovedCount), "Splash Screen");
        }
    }
}