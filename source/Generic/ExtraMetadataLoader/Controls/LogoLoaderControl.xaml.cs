using ExtraMetadataLoader.Models;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ExtraMetadataLoader
{
    /// <summary>
    /// Interaction logic for LogoLoaderControl.xaml
    /// </summary>
    public partial class LogoLoaderControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static readonly ILogger logger = LogManager.GetLogger();
        IPlayniteAPI PlayniteApi;

        private ExtraMetadataLoaderSettings _settings;
        private readonly LogoControlThemeSettings _controlSettings;

        public ExtraMetadataLoaderSettings _Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        private Visibility controlVisibility = Visibility.Collapsed;
        private Game gameContext;
        private readonly Dispatcher _dispatcher;
        private readonly DoubleAnimation _opacityAnimation;

        public Visibility ControlVisibility
        {
            get => controlVisibility;
            set
            {
                controlVisibility = value;
                OnPropertyChanged();
            }
        }

        public string logoSource { get; set; }
        public string LogoSource
        {
            get => logoSource;
            set
            {
                logoSource = value;
                OnPropertyChanged();
            }
        }

        public DesktopView ActiveViewAtCreation { get; }

        public LogoLoaderControl(IPlayniteAPI PlayniteApi, ExtraMetadataLoaderSettings settings, ExtraMetadataLoader extraMetadataLoader, LogoControlThemeSettings controlThemeSettings)
        {
            InitializeComponent();
            this.PlayniteApi = PlayniteApi;
            _settings = settings;
            _controlSettings = controlThemeSettings;
            DataContext = this;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }

            _dispatcher = Application.Current.Dispatcher;
            _opacityAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(950)
            };

            extraMetadataLoader.LogoUpdatedEvent += ExtraMetadataLoader_LogoUpdatedEvent;
        }

        private void ExtraMetadataLoader_LogoUpdatedEvent(object sender, LogoUpdatedEventArgs e)
        {
            if (e.GameId == gameContext?.Id)
            {
                _dispatcher.Invoke(() =>
                {
                    UpdateLogo();
                });
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            gameContext = newContext;
            UpdateLogo();
        }

        private void UpdateLogo()
        {
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                ActiveViewAtCreation != PlayniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (!(LogoImage.Source is null))
            {
                LogoImage.Source = null;
            }

            _settings.IsLogoAvailable = false;
            if (!_settings.EnableLogos)
            {
                ControlVisibility = Visibility.Collapsed;
                return;
            }

            if (gameContext is null)
            {
                return;
            }

            var logoPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", gameContext.Id.ToString(), "Logo.png");
            if (!FileSystem.FileExists(logoPath))
            {
                return;
            }

            try
            {
                var adjustedBitmap = CreateResizedBitmapImageFromPath(logoPath, Convert.ToInt32(_settings.LogoMaxWidth), Convert.ToInt32(_settings.LogoMaxHeight));
                LogoImage.Source = adjustedBitmap;
                _settings.IsLogoAvailable = true;
                ControlVisibility = Visibility.Visible;
                StartLogoAnimation();
            }
            catch (FileFormatException)
            {
                FileSystem.DeleteFileSafe(logoPath);
            }
            catch (NotSupportedException)
            {
                FileSystem.DeleteFileSafe(logoPath);
            }
            catch (Exception)
            {

            }
        }

        private void StartLogoAnimation()
        {
            if (_controlSettings.EnableOpacityAnimation && _settings.LogoEnableOpacityAnimation)
            {
                LogoImage.BeginAnimation(OpacityProperty, _opacityAnimation);
            }
        }

        private BitmapImage CreateResizedBitmapImageFromPath(string filePath, int maxWidth, int maxHeight)
        {
            using (var fileStream = FileSystem.OpenReadFileStreamSafe(filePath))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);

                    // Using a buffered Steam apparently prevents memory not being released https://stackoverflow.com/a/76687288
                    using (var bufferedStream = new BufferedStream(memoryStream))
                    {
                        BitmapImage originalBitmap = GetBitmapImageFromBufferedStream(bufferedStream);
                        if (maxWidth <= 0 && maxHeight <= 0)
                        {
                            return originalBitmap;
                        }

                        int decodeWidth = 0;
                        int decodeHeight = 0;
                        double aspectRatio = originalBitmap.PixelWidth / (double)originalBitmap.PixelHeight;
                        if (aspectRatio > 1)
                        {
                            // Landscape image
                            if (originalBitmap.PixelWidth > maxWidth)
                            {
                                decodeWidth = maxWidth;
                            }
                            else
                            {
                                return originalBitmap;
                            }
                        }
                        else
                        {
                            // Portrait image or square image
                            if (originalBitmap.PixelHeight > maxHeight)
                            {
                                decodeHeight = maxHeight;
                            }
                            else
                            {
                                return originalBitmap;
                            }
                        }

                        bufferedStream.Seek(0, SeekOrigin.Begin);
                        return GetBitmapImageFromBufferedStream(bufferedStream, decodeWidth, decodeHeight);
                    }
                }
            }
        }

        private static BitmapImage GetBitmapImageFromBufferedStream(BufferedStream bufferedStream, int decodeWidth = 0, int decodeHeight = 0)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.DecodePixelWidth = decodeWidth;
            bitmapImage.DecodePixelHeight = decodeHeight;
            bitmapImage.StreamSource = bufferedStream;
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}