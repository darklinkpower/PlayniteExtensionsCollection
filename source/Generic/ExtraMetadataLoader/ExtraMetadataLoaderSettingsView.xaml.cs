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

namespace ExtraMetadataLoader
{
    public partial class ExtraMetadataLoaderSettingsView : UserControl
    {
        public ExtraMetadataLoaderSettingsView()
        {
            InitializeComponent();
            cmbLogoHorizontalAlignment.ItemsSource = new Dictionary<HorizontalAlignment, string>
            {
                { HorizontalAlignment.Left, ResourceProvider.GetString("LOCExtra_Metadata_Loader_Browser_OptionHorizontalAlignmentLeft") },
                { HorizontalAlignment.Center, ResourceProvider.GetString("LOCExtra_Metadata_Loader_Browser_OptionHorizontalAlignmentCenter") },
                { HorizontalAlignment.Right, ResourceProvider.GetString("LOCExtra_Metadata_Loader_Browser_OptionHorizontalAlignmentRight") },
            };

            cmbLogoVerticalAlignment.ItemsSource = new Dictionary<VerticalAlignment, string>
            {
                { VerticalAlignment.Top, ResourceProvider.GetString("LOCExtra_Metadata_Loader_Browser_OptionVerticalAlignmentTop") },
                { VerticalAlignment.Center, ResourceProvider.GetString("LOCExtra_Metadata_Loader_Browser_OptionVerticalAlignmentCenter") },
                { VerticalAlignment.Bottom, ResourceProvider.GetString("LOCExtra_Metadata_Loader_Browser_OptionVerticalAlignmentBottom") },
            };

            cmbVideoVerticalAlignment.ItemsSource = new Dictionary<VerticalAlignment, string>
            {
                { VerticalAlignment.Top, ResourceProvider.GetString("LOCExtra_Metadata_Loader_Browser_OptionVerticalAlignmentTop") },
                { VerticalAlignment.Bottom, ResourceProvider.GetString("LOCExtra_Metadata_Loader_Browser_OptionVerticalAlignmentBottom") },
            };
        }
    }
}