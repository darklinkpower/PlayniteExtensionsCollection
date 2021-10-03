using Playnite.SDK;
using System;
using System.Collections.Generic;
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

namespace SplashScreen
{
    public partial class SplashScreenSettingsView : UserControl
    {
        public SplashScreenSettingsView()
        {
            InitializeComponent();

            cbLogoHorizontalAlignment.ItemsSource = new Dictionary<HorizontalAlignment, string>
            {
                { HorizontalAlignment.Left, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentLeftLabel") },
                { HorizontalAlignment.Center, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentCenterLabel") },
                { HorizontalAlignment.Right, ResourceProvider.GetString("LOCSplashScreen_SettingHorizontalAlignmentRightLabel") },
            };

            cbLogoVerticalAlignment.ItemsSource = new Dictionary<VerticalAlignment, string>
            {
                { VerticalAlignment.Top, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentTopLabel") },
                { VerticalAlignment.Center, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentCenterLabel") },
                { VerticalAlignment.Bottom, ResourceProvider.GetString("LOCSplashScreen_SettingVerticalAlignmentBottomLabel") },
            };
        }
    }
}