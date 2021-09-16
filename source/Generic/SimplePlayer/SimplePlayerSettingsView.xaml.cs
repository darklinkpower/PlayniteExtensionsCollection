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

namespace SimplePlayer
{
    public partial class SimplePlayerSettingsView : UserControl
    {
        public SimplePlayerSettingsView()
        {
            InitializeComponent();

            cmbHorizontalLogoAlignment.ItemsSource = new Dictionary<HorizontalAlignment, string>
            {
                { HorizontalAlignment.Left, "Left" },
                { HorizontalAlignment.Center, "Center" },
                { HorizontalAlignment.Right, "Right" },
            };

            cmbVerticalLogoAlignment.ItemsSource = new Dictionary<VerticalAlignment, string>
            {
                { VerticalAlignment.Top, "Top" },
                { VerticalAlignment.Center, "Center" },
                { VerticalAlignment.Bottom, "Bottom" },
            };
        }
    }
}