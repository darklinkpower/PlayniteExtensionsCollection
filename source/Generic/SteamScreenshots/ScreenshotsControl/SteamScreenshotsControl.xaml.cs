using FlowHttp;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using PluginsCommon.Converters;
using SteamCommon;
using SteamCommon.Models;
using SteamScreenshots.Screenshots;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using static SteamCommon.Models.SteamAppDetails.AppDetails;

namespace SteamScreenshots.ScreenshotsControl
{
    /// <summary>
    /// Interaction logic for SteamScreenshotsControl.xaml
    /// </summary>
    public partial class SteamScreenshotsControl : PluginUserControl, INotifyPropertyChanged
    {
        private class UriIndexPair
        {
            public Uri Uri { get; }
            public int Index { get; }

            internal UriIndexPair(Uri uri, int index)
            {
                Uri = uri;
                Index = index;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public IEnumerable<BitmapImage> ScreenshotsBitmapImages => GetScreenshotsBitmapImages(_screenshots?.Select(x => x.PathThumbnail));
        public IEnumerable<BitmapImage> ScreenshotsFullBitmapImages => GetScreenshotsBitmapImages(_screenshots?.Select(x => x.PathFull));

        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _playniteApi;
        private readonly string _pluginStoragePath;
        private readonly SteamScreenshotsSettingsViewModel _settingsViewModel;
        private readonly DesktopView _activeViewAtCreation;
        private readonly DispatcherTimer _updateControlDataDelayTimer;
        private readonly ImageUriToBitmapImageConverter _imageUriToBitmapImageConverter;
        private bool _isValuesDefaultState = true;
        private Game _currentGame;
        private Guid _activeContext = default;
        private DoubleAnimation _fadeOutAnimation;
        private DoubleAnimation _fadeInAnimation;

        private ObservableCollection<Screenshot> _screenshots = new ObservableCollection<SteamAppDetails.AppDetails.Screenshot>();
        public ObservableCollection<Screenshot> Screenshots
        {
            get => _screenshots;
            set
            {
                _screenshots = value;
                OnPropertyChanged();
            }
        }

        private Uri _oldImageUri;
        public Uri OldImageUri
        {
            get => _oldImageUri;
            set
            {
                _oldImageUri = value;
                OnPropertyChanged();
            }
        }

        private Uri _currentImageUri;
        public Uri CurrentImageUri
        {
            get => _currentImageUri;
            set
            {
                _currentImageUri = value;
                OnPropertyChanged();
            }
        }

        private Screenshot _selectedScreenshot;
        public Screenshot SelectedScreenshot
        {
            get => _selectedScreenshot;
            set
            {
                if (_selectedScreenshot != value)
                {
                    _selectedScreenshot = value;                    
                    OldImageUri = CurrentImageUri;
                    CurrentImageUri = _selectedScreenshot?.PathThumbnail;
                    FadeImages();
                    OnPropertyChanged();
                }
            }
        }

        public SteamScreenshotsControl(SteamScreenshots plugin, SteamScreenshotsSettingsViewModel settingsViewModel, ImageUriToBitmapImageConverter imageUriToBitmapImageConverter)
        {
            _imageUriToBitmapImageConverter = imageUriToBitmapImageConverter;
            Resources.Add("ImageUriToBitmapImageConverter", imageUriToBitmapImageConverter);
            _playniteApi = API.Instance;
            SetControlTextBlockStyle();
            _pluginStoragePath = plugin.GetPluginUserDataPath();
            _settingsViewModel = settingsViewModel;
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = _playniteApi.MainView.ActiveDesktopView;
            }

            _updateControlDataDelayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1100)
            };

            _updateControlDataDelayTimer.Tick += new EventHandler(UpdateControlData);
            InitializeComponent();
            InitializeAnimations();
            DataContext = this;
        }

        private void InitializeAnimations()
        {
            _fadeOutAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.4),
                FillBehavior = FillBehavior.HoldEnd
            };

            _fadeInAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.4),
                FillBehavior = FillBehavior.HoldEnd
            };
        }

        private void FadeImages()
        {
            Storyboard.SetTarget(_fadeOutAnimation, OldImage);
            Storyboard.SetTargetProperty(_fadeOutAnimation, new PropertyPath("Opacity"));
            Storyboard.SetTarget(_fadeInAnimation, NewImage);
            Storyboard.SetTargetProperty(_fadeInAnimation, new PropertyPath("Opacity"));

            var storyboard = new Storyboard();
            storyboard.Children.Add(_fadeOutAnimation);
            storyboard.Children.Add(_fadeInAnimation);
            storyboard.Begin();
        }

        private IEnumerable<BitmapImage> GetScreenshotsBitmapImages(IEnumerable<Uri> uris)
        {
            if (uris is null || !uris.HasItems())
            {
                return Enumerable.Empty<BitmapImage>();
            }

            var bitmapImages = new BitmapImage[uris.Count()];
            var lockObject = new object();
            var uriIndexPairs = uris.Select((uri, index) => new UriIndexPair(uri, index)).ToList();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            Parallel.ForEach(uriIndexPairs, options, (pair) =>
            {
                var bitmapImage = UriToBitmapImage(pair.Uri);
                if (bitmapImage != null)
                {
                    lock (lockObject)
                    {
                        bitmapImages[pair.Index] = bitmapImage;
                    }
                }
            });

            return bitmapImages.Where(bmp => bmp != null);
        }

        private void SetControlTextBlockStyle()
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = _playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle && baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                Resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        private async void UpdateControlData(object sender, EventArgs e)
        {
            _updateControlDataDelayTimer.Stop();
            await UpdateControlAsync();
        }

        private void SetCollapsedVisibility()
        {
            Visibility = Visibility.Collapsed;
            _settingsViewModel.Settings.IsControlVisible = false;
        }

        private void SetVisibleVisibility()
        {
            OnPropertyChanged(nameof(ScreenshotsBitmapImages));
            OnPropertyChanged(nameof(ScreenshotsFullBitmapImages));
            Visibility = Visibility.Visible;
            _settingsViewModel.Settings.IsControlVisible = true;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            _updateControlDataDelayTimer.Stop();
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && _activeViewAtCreation != _playniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (!_isValuesDefaultState)
            {
                ResetToDefaultValues();
            }

            if (newContext is null)
            {
                return;
            }

            _currentGame = newContext;
            _updateControlDataDelayTimer.Start();
        }

        private void ResetToDefaultValues()
        {
            SetCollapsedVisibility();
            _activeContext = default;
            Screenshots.Clear();
            OldImageUri = null;
            CurrentImageUri = null;
            OnPropertyChanged(nameof(ScreenshotsBitmapImages));
            OnPropertyChanged(nameof(ScreenshotsFullBitmapImages));
            _isValuesDefaultState = true;
        }

        private async Task UpdateControlAsync()
        {
            if (_currentGame is null)
            {
                return;
            }

            await LoadControlData(_currentGame).ConfigureAwait(false);
        }

        private async Task LoadControlData(Game game, CancellationToken cancellationToken = default)
        {
            var scopeContext = Guid.NewGuid();
            _activeContext = scopeContext;
            _isValuesDefaultState = false;
            var steamId = Steam.GetGameSteamId(game, true);
            if (steamId.IsNullOrEmpty())
            {
                return;
            }

            var gameDataPath = Path.Combine(_pluginStoragePath, "appdetails", $"{steamId}_appdetails.json");
            if (!FileSystem.FileExists(gameDataPath) || ShouldRefreshAppDetails(gameDataPath))
            {
                var url = string.Format(@"https://store.steampowered.com/api/appdetails?appids={0}", steamId);
                var result = await HttpRequestFactory.GetHttpFileRequest()
                    .WithUrl(url)
                    .WithDownloadTo(gameDataPath)
                    .DownloadFileAsync(cancellationToken);
                if (!result.IsSuccess || _activeContext != scopeContext)
                {
                    return;
                }

                await SetScreenshots(gameDataPath, true, scopeContext);
            }
            else
            {
                await SetScreenshots(gameDataPath, false, scopeContext);
            }
        }

        private bool ShouldRefreshAppDetails(string gameDataPath)
        {
            var fi = new FileInfo(FileSystem.FixPathLength(gameDataPath));
            var shouldRefresh = fi.LastWriteTime < DateTime.Now.AddDays(-12);
            return shouldRefresh;
        }

        private async Task SetScreenshots(string gameDataPath, bool downloadScreenshots, Guid scopeContext)
        {
            try
            {
                var parsedData = Serialization.FromJsonFile<Dictionary<string, SteamAppDetails>>(gameDataPath);
                if (parsedData.Keys?.Any() != true)
                {
                    return;
                }

                var response = parsedData[parsedData.Keys.First()];
                if (!response.success)
                {
                    // #550 Due to unknown circumstances, the response can return success:false
                    // despite data being available but a redownload fixes it
                    _logger.Warn($"Data in {gameDataPath} is not successful");
                    FileSystem.DeleteFile(gameDataPath);
                    return;
                }

                if (response.data is null)
                {
                    _logger.Warn($"Data in {gameDataPath} is null");
                    return;
                }

                if (!response.data.screenshots.HasItems())
                {
                    _logger.Warn($"Data in {gameDataPath} does not contain screenshots");
                    return;
                }

                if (downloadScreenshots)
                {
                    await DownloadScreenshotsThumbnails(response.data.screenshots);
                    if (_activeContext != scopeContext)
                    {
                        return;
                    }
                }

                // To prevent loading all the images when first displayed in the UI.
                // we load some of them in the background to prevent major stutters
                // Helps in cases where the theme doesn't initially display them, e.g. in a tab control
                var numberToPreload = Math.Max((int)Math.Ceiling(response.data.screenshots.Count / 2.0), 5);
                var screenshotsToPreload = response.data.screenshots.Take(numberToPreload);
                var preloadTasks = screenshotsToPreload
                    .Select(screenshot => Task.Run(() => UriToBitmapImage(screenshot.PathThumbnail))
                );
                await Task.WhenAll(preloadTasks);

                foreach (var screenshot in response.data.screenshots)
                {
                    Screenshots.Add(screenshot);
                }

                SelectedScreenshot = Screenshots.FirstOrDefault();
                _playniteApi.MainView.UIDispatcher.Invoke(() => SetVisibleVisibility());
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error during DownloadAppScreenshotsThumbnails");
            }
        }

        private BitmapImage UriToBitmapImage(Uri uri)
        {
            var bitmapImage = _imageUriToBitmapImageConverter.Convert(uri, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
            if (bitmapImage != null)
            {
                return bitmapImage as BitmapImage;
            }

            return null;
        }

        private async Task DownloadScreenshotsThumbnails(List<SteamAppDetails.AppDetails.Screenshot> screenshots)
        {
            var tasks = new List<Func<Task>>();
            foreach (var screenshot in screenshots)
            {
                if (screenshot.PathThumbnail is null)
                {
                    continue;
                }

                tasks.Add(async () =>
                {
                    await _imageUriToBitmapImageConverter
                    .DownloadUriToStorageAsync(screenshot.PathThumbnail);
                });
            }

            using (var taskExecutor = new TaskExecutor(4))
            {
                await taskExecutor.ExecuteAsync(tasks);
            }
        }

        private void SelectPreviousImageScreenshot()
        {
            if (Screenshots.Count <= 1 || _selectedScreenshot is null)
            {
                return;
            }

            var currentSelectIndex = Screenshots.IndexOf(_selectedScreenshot);
            if (currentSelectIndex == -1)
            {
                return;
            }

            if (currentSelectIndex == 0)
            {
                SelectedScreenshot = Screenshots[Screenshots.Count - 1];
            }
            else
            {
                SelectedScreenshot = Screenshots[currentSelectIndex - 1];
            }
        }

        private void SelectNextScreenshot()
        {
            if (Screenshots.Count <= 1 || _selectedScreenshot is null)
            {
                return;
            }

            var currentSelectIndex = Screenshots.IndexOf(_selectedScreenshot);
            if (currentSelectIndex == -1)
            {
                return;
            }

            if (currentSelectIndex == Screenshots.Count - 1)
            {
                SelectedScreenshot = Screenshots[0];
            }
            else
            {
                SelectedScreenshot = Screenshots[currentSelectIndex + 1];
            }
        }

        private void OpenScreenshotsView(Screenshot selectedImage = null)
        {
            if (_currentGame is null || !_screenshots.HasItems())
            {
                return;
            }

            var window = new Window
            {
                Width = 1330,
                Height = 845,
                WindowStyle = WindowStyle.None,
                WindowState = WindowState.Maximized,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Colors.Black),
                Title = _currentGame.Name,
                Owner = API.Instance.Dialogs.GetCurrentAppWindow(),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new ScreenshotsView()
            };

            var screenshotsViewModel = new ScreenshotsViewModel(window, _imageUriToBitmapImageConverter);
            var urisToLoad = _screenshots.Select(x => x.PathFull);
            screenshotsViewModel.LoadUris(urisToLoad);
            if (selectedImage != null)
            {
                screenshotsViewModel.SelectImage(selectedImage.PathFull);
            }

            window.DataContext = screenshotsViewModel;
            window.ShowDialog();
            var windowLastDisplayedScreenshotUri = screenshotsViewModel.LastDisplayedUri;
            var matchingScreenshot = _screenshots.FirstOrDefault(x => x.PathFull == windowLastDisplayedScreenshotUri);
            if (matchingScreenshot != null)
            {
                SelectedScreenshot = matchingScreenshot;
            }
        }

        public RelayCommand OpenScreenshotsViewCommand
        {
            get => new RelayCommand(() =>
            {
                OpenScreenshotsView(_selectedScreenshot);
            });
        }

        public RelayCommand SelectPreviousScreenshotCommand
        {
            get => new RelayCommand(() =>
            {
                SelectPreviousImageScreenshot();
            });
        }

        public RelayCommand SelectNextScreenshotCommand
        {
            get => new RelayCommand(() =>
            {
                SelectNextScreenshot();
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}