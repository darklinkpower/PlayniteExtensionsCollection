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

namespace SplashScreen.Views
{
    /// <summary>
    /// Interaction logic for VideoManager.xaml
    /// </summary>
    public partial class VideoManager : UserControl
    {
        public VideoManager()
        {
            InitializeComponent();
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
        }
    }
}
