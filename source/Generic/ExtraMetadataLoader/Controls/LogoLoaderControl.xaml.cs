using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using PluginsCommon;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

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

        IPlayniteAPI PlayniteApi;

        private ExtraMetadataLoaderSettings _settings;
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

        public LogoLoaderControl(IPlayniteAPI PlayniteApi, ExtraMetadataLoaderSettings settings)
        {
            InitializeComponent();
            this.PlayniteApi = PlayniteApi;
            _settings = settings;
            DataContext = this;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
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

            if (newContext is null)
            {
                return;
            }
                
            var logoPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", newContext.Id.ToString(), "Logo.png");
            if (!FileSystem.FileExists(logoPath))
            {
                return;
            }

            var adjustedBitmap = CreateResizedBitmapImageFromPath(logoPath, (int)_settings.LogoMaxWidth, (int)_settings.LogoMaxHeight);
            LogoImage.Source = adjustedBitmap;
            _settings.IsLogoAvailable = true;
            ControlVisibility = Visibility.Visible;
        }

        private static BitmapImage CreateResizedBitmapImageFromPath(string filePath, int maxWidth, int maxHeight)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    BitmapImage originalBitmap = GetBitmapImageFromMemoryStream(memoryStream);
                    if (maxWidth <= 0 && maxHeight <= 0)
                    {
                        return originalBitmap;
                    }

                    int newWidth = 0;
                    int newHeight = 0;
                    double aspectRatio = originalBitmap.PixelWidth / (double)originalBitmap.PixelHeight;
                    if (aspectRatio > 1)
                    {
                        // Landscape image
                        newWidth = maxWidth;
                    }
                    else
                    {
                        // Portrait image or square image
                        newHeight = maxHeight;
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage resizedBitmap = GetBitmapImageFromMemoryStream(memoryStream, newWidth, newHeight);
                    return resizedBitmap;
                }
            }
        }

        private static BitmapImage GetBitmapImageFromMemoryStream(MemoryStream memoryStream, int newWidth = 0, int newHeight = 0)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.DecodePixelWidth = newWidth;
            bitmapImage.DecodePixelHeight = newHeight;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}