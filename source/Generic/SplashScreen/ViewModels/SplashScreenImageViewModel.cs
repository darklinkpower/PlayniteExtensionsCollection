using PluginsCommon;
using SplashScreen.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SplashScreen.ViewModels
{
    public class SplashScreenImageViewModel : ObservableObject
    {
        private string splashImagePath = null;
        public string SplashImagePath
        {
            get => splashImagePath;
            set
            {
                splashImagePath = value;
                OnPropertyChanged();
            }
        }

        private string logoPath = null;
        public string LogoPath
        {
            get => logoPath;
            set
            {
                logoPath = value;
                OnPropertyChanged();
            }
        }

        private GeneralSplashSettings settings;
        public GeneralSplashSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public SplashScreenImageViewModel(GeneralSplashSettings settings, string splashImagePath, string logoPath)
        {
            Settings = settings;
            if (!splashImagePath.IsNullOrEmpty() && FileSystem.FileExists(splashImagePath))
            {
                SplashImagePath = splashImagePath;
            }

            if (!logoPath.IsNullOrEmpty() && FileSystem.FileExists(logoPath))
            {
                LogoPath = logoPath;
            }
        }

    }
}