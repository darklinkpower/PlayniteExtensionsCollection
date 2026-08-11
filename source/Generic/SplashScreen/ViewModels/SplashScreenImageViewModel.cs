using PluginsCommon;
using SplashScreen.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SplashScreen.ViewModels
{
    public class SplashScreenImageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _splashImagePath = null;
        public string SplashImagePath
        {
            get => _splashImagePath;
            set
            {
                _splashImagePath = value;
                OnPropertyChanged();
            }
        }

        private string _logoPath = null;
        public string LogoPath
        {
            get => _logoPath;
            set
            {
                _logoPath = value;
                OnPropertyChanged();
            }
        }

        private GeneralSplashSettings _settings;
        public GeneralSplashSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public SplashScreenImageViewModel(GeneralSplashSettings settings, string splashImagePath, string logoPath)
        {
            Settings = settings;
            if (!string.IsNullOrEmpty(splashImagePath) && FileSystem.FileExists(splashImagePath))
            {
                SplashImagePath = splashImagePath;
            }

            if (!string.IsNullOrEmpty(logoPath) && FileSystem.FileExists(logoPath))
            {
                LogoPath = logoPath;
            }
        }

    }
}