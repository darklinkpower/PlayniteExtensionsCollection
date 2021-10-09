using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SplashScreen.Views
{
    /// <summary>
    /// Interaction logic for SplashScreenImage.xaml
    /// </summary>
    public partial class SplashScreenImage : UserControl
    {
        public SplashScreenImage(SplashScreenSettings settings, string splashImagePath, string logoPath)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(splashImagePath) && File.Exists(splashImagePath))
            {
                BackgroundImage.Source = new BitmapImage(new Uri(splashImagePath));
            }
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                LogoImage.Source = new BitmapImage(new Uri(logoPath));
                LogoImage.VerticalAlignment = settings.LogoVerticalAlignment;
                switch (settings.LogoHorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        LogoImage.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case HorizontalAlignment.Center:
                        LogoImage.SetValue(Grid.ColumnProperty, 1);
                        break;
                    case HorizontalAlignment.Right:
                        LogoImage.SetValue(Grid.ColumnProperty, 2);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
