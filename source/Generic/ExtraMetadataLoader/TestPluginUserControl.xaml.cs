using Playnite.SDK.Controls;
using Playnite.SDK.Models;
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

namespace ExtraMetadataLoader
{
    /// <summary>
    /// Interaction logic for TestPluginUserControl.xaml
    /// </summary>
    public partial class TestPluginUserControl : PluginUserControl
    {
        public ExtraMetadataLoaderSettingsViewModel SettingsModel { get; set; }

        public TestPluginUserControl(ExtraMetadataLoaderSettingsViewModel settings)
        {
            InitializeComponent();
            DataContext = this;
            SettingsModel = settings;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            Console.WriteLine($"---- TestPluginUserControl ---- {newContext?.ToString()}");
        }
    }
}
