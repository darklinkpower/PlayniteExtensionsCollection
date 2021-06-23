using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace GamePassCatalogBrowser
{
    public partial class GamePassCatalogBrowserSettingsView : UserControl
    {
        public GamePassCatalogBrowserSettingsView()
        {
            InitializeComponent();
            string[] regionCodes = { "AR", "AU", "AT", "BE", "BR", "CA", "CL", "CN", "CO", "CZ", "DK", "FI", "FR", "GE", "DE", "GR", "HK", "HU", "IN", "IL", "IT", "JP", "KR", "MX", "NL", "NZ", "NO", "PL", "RU", "SA", "ES", "SE", "CH", "TW", "TR", "AE", "GB", "US" };
            var regionsDictionary = new Dictionary<string, string> { };
            foreach (string regionCode in regionCodes)
            {
                var regionInfo = new RegionInfo(regionCode);
                regionsDictionary.Add(regionCode, regionInfo.NativeName);
            }

            regionsComboBox.ItemsSource = regionsDictionary;
        }
    }
}