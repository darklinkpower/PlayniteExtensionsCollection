using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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

            BitmapImage originalBitmap = new BitmapImage(new Uri(logoPath));

            int newWidth = 0;
            int newHeight = 0;
            double aspectRatio = originalBitmap.PixelWidth / originalBitmap.PixelHeight;
            if (aspectRatio > 1)
            {
                // Landscape image
                newWidth = (int)_settings.LogoMaxWidth;
            }
            else
            {
                // Portrait image or square image
                newHeight = (int)_settings.LogoMaxHeight;
            }

            BitmapImage adjustedBitmap = new BitmapImage();
            adjustedBitmap.BeginInit();
            adjustedBitmap.UriSource = new Uri(logoPath, UriKind.RelativeOrAbsolute);
            adjustedBitmap.DecodePixelWidth = newWidth;
            adjustedBitmap.DecodePixelHeight = newHeight;
            adjustedBitmap.EndInit();
            adjustedBitmap.Freeze();

            LogoImage.Source = adjustedBitmap;
            _settings.IsLogoAvailable = true;
            ControlVisibility = Visibility.Visible;
        }
    }
}
