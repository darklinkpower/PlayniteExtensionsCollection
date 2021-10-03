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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SplashScreen
{
    public class SplashScreen : GenericPlugin
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_CLOSE = 0x0010;

        private static readonly ILogger logger = LogManager.GetLogger();
        private string pluginInstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string splashImagePath = string.Empty;
        string logoPath = string.Empty;
        string videoPath = string.Empty;

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

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            var game = args.Game;
            splashImagePath = string.Empty;
            logoPath = string.Empty;
            videoPath = string.Empty;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && !settings.Settings.ExecuteInDesktopMode)
            {
                logger.Info("Execution disabled for Desktop mode in settings");
            }
            else if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen && !settings.Settings.ExecuteInFullscreenMode)
            {
                logger.Info("Execution disabled for Fullscreen mode in settings");
            }

            logger.Info($"Game: {game.Name}");

            var skipSplashImage = false;
            if (game.Features != null)
            {
                if (game.Features.Any(x => x.Name == "[Splash Screen] Skip splash image"))
                {
                    skipSplashImage = true;
                }
            }

            if (skipSplashImage == false)
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
                    CreateSplashVideoWindow(skipSplashImage);
                }
                else if (skipSplashImage == false)
                {
                    CreateSplashImageWindow();
                }
            }
            else if (skipSplashImage == false)
            {
                CreateSplashImageWindow();
            }
        }

        private void CreateSplashVideoWindow(bool skipSplashImage)
        {
            var content = new SplashScreenVideo(videoPath);
            var window = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                WindowState = WindowState.Maximized,
                Title = "PlayniteSplashScreenExtensionVideo",
                Content = content
            };

            content.VideoPlayer.MediaEnded += new RoutedEventHandler(delegate (object o, RoutedEventArgs a)
            {
                content.VideoPlayer.Source = null;
                if (!skipSplashImage)
                {
                    CreateSplashImageWindow();
                    System.Threading.Thread.Sleep(3000);
                }
                window.Close();
            });

            window.ShowDialog();
        }

        private void CreateSplashImageWindow()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    var window = new Window
                    {
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        WindowState = WindowState.Maximized,
                        Title = "PlayniteSplashScreenExtension",
                        Content = new SplashScreenImage(settings.Settings, splashImagePath, logoPath)
                    };

                    if ((PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && settings.Settings.CloseSplashScreenDesktopMode) ||
                        (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen && settings.Settings.CloseSplashScreenFullscreenMode))
                    {
                        var timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(30);
                        timer.Tick += new EventHandler(delegate (object o, EventArgs a)
                        {
                            window.Close();
                            timer.Stop();
                            return;
                        });
                        timer.Start();
                    }

                    window.Owner = null;
                    window.ShowDialog();
                });
            });

            Task.Delay(2000);
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
            var logoPath = string.Empty;
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
                logoPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString(), "Logo.png");
                if (File.Exists(logoPath))
                {
                    logger.Info(string.Format("Specific game logo found in {0}", logoPath));
                    return logoPath;
                }
            }

            logger.Info("logo not found");
            return logoPath;
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
            IntPtr windowPtr = FindWindowByCaption(IntPtr.Zero, "PlayniteSplashScreenExtension");
            if (windowPtr != IntPtr.Zero)
            {
                SendMessage(windowPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                logger.Info("Splash window was active after closing game and was closed");
            }
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
                    Description = ResourceProvider.GetString("LOCSplashScreen_MenuItemAdd-ImageSkipFeature"),
                    MenuSection = "@Splash Screen",
                    Action = a => {
                        AddImageSkipFeature();
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSplashScreen_MenuItemRemove-ImageSkipFeature"),
                    MenuSection = "@Splash Screen",
                    Action = a => {
                        RemoveImageSkipFeature();
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

        private void AddImageSkipFeature()
        {
            GameFeature feature = PlayniteApi.Database.Features.Add("[Splash Screen] Skip splash image");
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

        private void RemoveImageSkipFeature()
        {
            GameFeature feature = PlayniteApi.Database.Features.Add("[Splash Screen] Skip splash image");
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