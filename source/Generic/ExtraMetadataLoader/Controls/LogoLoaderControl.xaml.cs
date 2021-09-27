using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
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
        IPlayniteAPI PlayniteApi; public ExtraMetadataLoaderSettingsViewModel SettingsModel { get; set; }

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

        public LogoLoaderControl(IPlayniteAPI PlayniteApi, ExtraMetadataLoaderSettingsViewModel settings)
        {
            InitializeComponent();
            this.PlayniteApi = PlayniteApi;
            SettingsModel = settings;
            DataContext = this;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            Task.Run(() =>
            {
                LogoSource = null;
                SettingsModel.Settings.IsLogoAvailable = false;
            
                if (!SettingsModel.Settings.EnableLogos)
                {
                    ControlVisibility = Visibility.Collapsed;
                    return;
                }

                if (newContext != null)
                {
                    var logoPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", newContext.Id.ToString(), "Logo.png");
                    if (File.Exists(logoPath))
                    {
                        LogoSource = logoPath;
                        SettingsModel.Settings.IsLogoAvailable = true;
                        ControlVisibility = Visibility.Visible;
                    }
                }
            });
        }
    }
}
