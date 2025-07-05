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
            // From https://www.xbox.com/en-US/regions
            var regionCodes = new List<string>
            {
                "AL", // Albania
                "DZ", // Algeria
                "AR", // Argentina
                "AU", // Australia
                "AT", // Austria
                "BH", // Bahrain
                "BE", // Belgium
                "BO", // Bolivia
                "BA", // Bosnia & Herzegovina
                "BR", // Brazil
                "BG", // Bulgaria
                "CA", // Canada
                "CL", // Chile
                //"CN", // China
                "CO", // Colombia
                "CR", // Costa Rica
                "HR", // Croatia
                "CY", // Cyprus
                "CZ", // Czechia
                "DK", // Denmark
                "EC", // Ecuador
                "EG", // Egypt
                "SV", // El Salvador
                "EE", // Estonia
                "FI", // Finland
                "FR", // France
                "GE", // Georgia
                "DE", // Germany
                "GR", // Greece
                "GT", // Guatemala
                "HN", // Honduras
                "HK", // Hong Kong SAR
                "HU", // Hungary
                "IS", // Iceland
                "IN", // India
                "ID", // Indonesia
                "IE", // Ireland
                "IL", // Israel
                "IT", // Italy
                "JP", // Japan
                "KR", // Korea
                "KW", // Kuwait
                "LV", // Latvia
                "LY", // Libya
                "LI", // Liechtenstein
                "LT", // Lithuania
                "LU", // Luxembourg
                "MY", // Malaysia
                "MT", // Malta
                "MX", // Mexico
                "MD", // Moldova
                "ME", // Montenegro
                "MA", // Morocco
                "NL", // Netherlands
                "NZ", // New Zealand
                "NI", // Nicaragua
                "MK", // North Macedonia
                "NO", // Norway
                "OM", // Oman
                "PA", // Panama
                "PY", // Paraguay
                "PE", // Peru
                "PH", // Philippines
                "PL", // Poland
                "PT", // Portugal
                "QA", // Qatar
                "RO", // Romania
                "RU", // Russia
                "SA", // Saudi Arabia
                "RS", // Serbia
                "SG", // Singapore
                "SK", // Slovakia
                "SI", // Slovenia
                "ZA", // South Africa
                "ES", // Spain
                "SE", // Sweden
                "CH", // Switzerland
                "TW", // Taiwan
                "TH", // Thailand
                "TN", // Tunisia
                "TR", // Turkey
                "UA", // Ukraine
                "AE", // United Arab Emirates
                "GB", // United Kingdom
                "US", // United States
                "UY", // Uruguay
                "VN"  // Vietnam
            };

            var regionsDictionary = new Dictionary<string, string> { };
            foreach (var regionCode in regionCodes)
            {
                try
                {   
                    if (!regionsDictionary.ContainsKey(regionCode))
                    {
                        var regionInfo = new RegionInfo(regionCode);
                        regionsDictionary.Add(regionCode, regionInfo.NativeName);
                    }
                }
                catch (Exception)
                {

                }
            }

            regionsComboBox.ItemsSource = regionsDictionary;
        }
    }
}