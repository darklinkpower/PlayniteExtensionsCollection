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
        public event PropertyChangedEventHandler PropertyChanged;
        public IEnumerable<BitmapImage> ScreenshotsBitmapImages => GetScreenshotsBitmapImages();
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

        private ObservableCollection<SteamAppDetails.AppDetails.Screenshot> _screenshots = new ObservableCollection<SteamAppDetails.AppDetails.Screenshot>();
        public ObservableCollection<SteamAppDetails.AppDetails.Screenshot> Screenshots
        {
            get => _screenshots;
            set
            {
                _screenshots = value;
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

        private SteamAppDetails.AppDetails.Screenshot _selectedScreenshot;
        public SteamAppDetails.AppDetails.Screenshot SelectedScreenshot
        {
            get => _selectedScreenshot;
            set
            {
                _selectedScreenshot = value;
                OnPropertyChanged();
                CurrentImageUri = _selectedScreenshot != null
                    ? _selectedScreenshot.PathThumbnail
                    : null;
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
            DataContext = this;
        }

        private IEnumerable<BitmapImage> GetScreenshotsBitmapImages()
        {
            if (_screenshots.Count == 0)
            {
                return Enumerable.Empty<BitmapImage>();
            }

            var bitmapImages = new List<BitmapImage>();
            foreach (var screenshot in _screenshots)
            {
                var bitmapImage = UriToBitmapImage(screenshot.PathThumbnail);
                if (bitmapImage != null)
                {
                    bitmapImages.Add(bitmapImage);
                }
            }

            return bitmapImages;
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
            OnPropertyChanged(nameof(ScreenshotsBitmapImages));
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
            if (FileSystem.FileExists(gameDataPath))
            {
                await SetScreenshots(gameDataPath, false, scopeContext);
                return;
            }
            
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
                if (!response.success || response.data is null)
                {
                    return;
                }

                if (!response.data.screenshots.HasItems())
                {
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

            var window = new Window();
            window.Width = 1330;
            window.Height = 845;
            window.WindowStyle = WindowStyle.None;
            window.WindowState = WindowState.Maximized;
            window.ResizeMode = ResizeMode.NoResize;
            window.Background = new SolidColorBrush(Colors.Black);
            window.Title = _currentGame.Name;

            var screenshotsViewModel = new ScreenshotsViewModel(window);
            var urisToLoad = _screenshots.Select(x => x.PathFull);
            screenshotsViewModel.LoadUris(urisToLoad);
            if (selectedImage != null)
            {
                screenshotsViewModel.SelectImage(selectedImage.PathFull);
            }

            window.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.DataContext = screenshotsViewModel;
            window.Content = new ScreenshotsView(_imageUriToBitmapImageConverter);
            window.ShowDialog();
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