using FlowHttp;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using PluginsCommon.Converters;
using SteamCommon;
using SteamCommon.Models;
using SteamScreenshots.Application.Services;
using SteamScreenshots.Domain.Enums;
using SteamScreenshots.Domain.ValueObjects;
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

namespace SteamScreenshots.ScreenshotsControl
{
    /// <summary>
    /// Interaction logic for SteamScreenshotsControl.xaml
    /// </summary>
    public partial class SteamScreenshotsControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly ScreenshotManagementService _screenshotManagementService;
        private readonly IPlayniteAPI _playniteApi;
        private readonly SteamScreenshotsSettingsViewModel _settingsViewModel;
        private readonly DesktopView _activeViewAtCreation;
        private readonly DispatcherTimer _updateControlDataDelayTimer;

        private bool _screenshotsFullBitmapImagesAccessed = false;
        private bool _isValuesDefaultState = true;
        private Game _currentGame;
        private Guid _activeContext = default;
        private DoubleAnimation _fadeOutAnimation;
        private DoubleAnimation _fadeInAnimation;

        private ObservableCollection<Screenshot> _screenshots = new ObservableCollection<Screenshot>();
        private BitmapImage _oldImageBitmap;
        private BitmapImage _currentImageBitmap;
        private Screenshot _selectedScreenshot;


        public ObservableCollection<Screenshot> Screenshots
        {
            get => _screenshots;
            set
            {
                _screenshots = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage OldImageBitmap
        {
            get => _oldImageBitmap;
            set
            {
                _oldImageBitmap = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage CurrentImageBitmap
        {
            get => _currentImageBitmap;
            set
            {
                _currentImageBitmap = value;
                OnPropertyChanged();
            }
        }

        public Screenshot SelectedScreenshot
        {
            get => _selectedScreenshot;
            set
            {
                if (_selectedScreenshot != value)
                {
                    _selectedScreenshot = value;
                    OldImageBitmap = CurrentImageBitmap;
                    CurrentImageBitmap = _selectedScreenshot?.FullImage;
                    FadeImages();
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<BitmapImage> ScreenshotsBitmapImages => _screenshots.Select(x => x.ThumbnailImage);

        public IEnumerable<BitmapImage> ScreenshotsFullBitmapImages
        {
            get
            {
                _screenshotsFullBitmapImagesAccessed = true;
                return _screenshots.Select(x => x.FullImage);
            }
        }

        public List<Screenshot> ScreenshotsForExport
        {
            get
            {
                _screenshotsFullBitmapImagesAccessed = true;
                return _screenshots.ToList();
            }
        }

        public RelayCommand OpenScreenshotsViewCommand { get; }
        public RelayCommand SelectPreviousScreenshotCommand { get; }
        public RelayCommand SelectNextScreenshotCommand { get; }


        public SteamScreenshotsControl(SteamScreenshotsSettingsViewModel settingsViewModel, ScreenshotManagementService screenshotManagementService)
        {
            _screenshotManagementService = screenshotManagementService;
            _settingsViewModel = settingsViewModel;
            _playniteApi = API.Instance;


            SetControlTextBlockStyle();

            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = _playniteApi.MainView.ActiveDesktopView;
            }

            _updateControlDataDelayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1100)
            };
            _updateControlDataDelayTimer.Tick += new EventHandler(UpdateControlData);

            OpenScreenshotsViewCommand = new RelayCommand(() => OpenScreenshotsView(_selectedScreenshot));
            SelectPreviousScreenshotCommand = new RelayCommand(() => SelectPreviousImageScreenshot());
            SelectNextScreenshotCommand = new RelayCommand(() => SelectNextScreenshot());

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
            OnPropertyChanged(nameof(ScreenshotsForExport));
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
            OldImageBitmap = null;
            CurrentImageBitmap = null;
            OnPropertyChanged(nameof(ScreenshotsBitmapImages));
            OnPropertyChanged(nameof(ScreenshotsFullBitmapImages));
            OnPropertyChanged(nameof(ScreenshotsForExport));
            _isValuesDefaultState = true;
        }

        private async Task UpdateControlAsync()
        {
            if (GameContext is null)
            {
                return;
            }

            await LoadControlData(GameContext).ConfigureAwait(false);
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

            var shouldLazyLoadFullImages = !_screenshotsFullBitmapImagesAccessed;
            var screenshots = await _screenshotManagementService
                .GetScreenshots(ScreenshotServiceType.Steam, steamId, new ScreenshotInitializationOptions(false, shouldLazyLoadFullImages));
            if (screenshots.HasItems() && GameContext?.Id == game.Id)
            {
                try
                {
                    _playniteApi.MainView.UIDispatcher.Invoke(() =>
                    {  
                        Screenshots.Clear();
                        foreach (var screenshot in screenshots)
                        {
                            Screenshots.Add(screenshot);
                        }

                        SelectedScreenshot = Screenshots.FirstOrDefault();
                        SetVisibleVisibility();
                    });
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error during LoadControlData");
                }
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

            var screenshotsViewModel = new ScreenshotsViewModel(window, _screenshots.ToList());
            if (selectedImage != null)
            {
                screenshotsViewModel.SelectScreenshot(selectedImage);
            }

            window.DataContext = screenshotsViewModel;
            window.ShowDialog();
            var index = _screenshots.IndexOf(screenshotsViewModel.LastDisplayedScreenshot);
            if (index != -1)
            {
                SelectedScreenshot = _screenshots[index];
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}