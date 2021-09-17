using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.IO;
using Playnite.SDK;

namespace SimplePlayer
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

        private SimplePlayerSettings settings;
        public SimplePlayerSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
            }
        }

        private string logoSource;
        public string LogoSource
        {
            get => logoSource;
            set
            {
                logoSource = value;
                OnPropertyChanged();
            }
        }

        public LogoLoaderControl(IPlayniteAPI PlayniteApi, SimplePlayerSettingsViewModel PluginSettings)
        {
            this.PlayniteApi = PlayniteApi;
            InitializeComponent();
            DataContext = this;
            Settings = PluginSettings.Settings;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            LogoSource = null;
            settings.IsLogoAvailable = false;
            if (settings.EnableLogos)
            {
                ControlGrid.Visibility = Visibility.Visible;
            }
            else
            {
                ControlGrid.Visibility = Visibility.Collapsed;
            }
            if (newContext != null)
            {
                var logoPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", newContext.Id.ToString(), "Logo.png");
                if (File.Exists(logoPath))
                {
                    LogoSource = logoPath;
                    settings.IsLogoAvailable = true;
                }
            }
        }
    }
}