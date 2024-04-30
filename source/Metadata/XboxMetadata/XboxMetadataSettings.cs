using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XboxMetadata.Services;

namespace XboxMetadata
{
    public enum CoverFormat
    {
        Vertical,
        Square
    };

    public enum VerticalCoverResolution
    {
        Original,
        Resolution720x1080,
        Resolution600x900
    };

    public enum SquareCoverResolution
    {
        Original,
        Resolution2160x2160,
        Resolution1080x1080,
        Resolution600x600,
        Resolution300x300
    };

    public enum BackgroundImageResolution
    {
        Original,
        Resolution1920x1080
    };

    public class MetadataFieldsConfiguration : ObservableObject
    {
        private bool _enableName = false;
        public bool EnableName { get => _enableName; set => SetValue(ref _enableName, value); }

        private bool _enableDevelopers = true;
        public bool EnableDevelopers { get => _enableDevelopers; set => SetValue(ref _enableDevelopers, value); }

        private bool _enablePublishers = true;
        public bool EnablePublishers { get => _enablePublishers; set => SetValue(ref _enablePublishers, value); }

        private bool _enableAgeRating = true;
        public bool EnableAgeRating { get => _enableAgeRating; set => SetValue(ref _enableAgeRating, value); }

        private bool _enablePlatform = false;
        public bool EnablePlatform { get => _enablePlatform; set => SetValue(ref _enablePlatform, value); }

        private bool _enableDescription = true;
        public bool EnableDescription { get => _enableDescription; set => SetValue(ref _enableDescription, value); }

        private bool _enableIcon = true;
        public bool EnableIcon { get => _enableIcon; set => SetValue(ref _enableIcon, value); }

        private bool _enableBackgroundImage = true;
        public bool EnableBackgroundImage { get => _enableBackgroundImage; set => SetValue(ref _enableBackgroundImage, value); }

        private bool _enableCoverImage = true;
        public bool EnableCoverImage { get => _enableCoverImage; set => SetValue(ref _enableCoverImage, value); }

        private bool _enableCommunityScore = true;
        public bool EnableCommunityScore { get => _enableCommunityScore; set => SetValue(ref _enableCommunityScore, value); }

        private bool _enableReleaseDate = true;
        public bool EnableReleaseDate { get => _enableReleaseDate; set => SetValue(ref _enableReleaseDate, value); }
    }

    public class XboxMetadataSettings : ObservableObject
    {
        private XboxLocale _marketLanguagePreference = XboxLocale.UnitedStates_English;
        public XboxLocale MarketLanguagePreference { get => _marketLanguagePreference; set => SetValue(ref _marketLanguagePreference, value); }

        private MetadataFieldsConfiguration _metadataFieldsConfiguration = new MetadataFieldsConfiguration();
        public MetadataFieldsConfiguration MetadataFieldsConfiguration { get => _metadataFieldsConfiguration; set => SetValue(ref _metadataFieldsConfiguration, value); }

        private CoverFormat _coverFormat = CoverFormat.Vertical;
        public CoverFormat CoverFormat { get => _coverFormat; set => SetValue(ref _coverFormat, value); }

        private VerticalCoverResolution _verticalCoverResolution = VerticalCoverResolution.Resolution600x900;
        public VerticalCoverResolution VerticalCoverResolution { get => _verticalCoverResolution; set => SetValue(ref _verticalCoverResolution, value); }

        private SquareCoverResolution _squareCoverResolution = SquareCoverResolution.Resolution1080x1080;
        public SquareCoverResolution SquareCoverResolution { get => _squareCoverResolution; set => SetValue(ref _squareCoverResolution, value); }

        private int _coverImageJpegQuality = 95;
        public int CoverImageJpegQuality { get => _coverImageJpegQuality; set => SetValue(ref _coverImageJpegQuality, value); }

        private BackgroundImageResolution _backgroundImageResolution = BackgroundImageResolution.Resolution1920x1080;
        public BackgroundImageResolution BackgroundImageResolution { get => _backgroundImageResolution; set => SetValue(ref _backgroundImageResolution, value); }
        private int _backgroundImageJpegQuality = 95;
        public int BackgroundImageJpegQuality { get => _backgroundImageJpegQuality; set => SetValue(ref _backgroundImageJpegQuality, value); }
    }

    public class XboxMetadataSettingsViewModel : ObservableObject, ISettings
    {
        private readonly XboxMetadata plugin;
        private XboxMetadataSettings editingClone { get; set; }

        private XboxMetadataSettings settings;
        public XboxMetadataSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, BackgroundImageResolution> _backgroundImageResolutions;
        public Dictionary<string, BackgroundImageResolution> BackgroundImageResolutions { get => _backgroundImageResolutions; set => SetValue(ref _backgroundImageResolutions, value); }

        private Dictionary<string, CoverFormat> _preferredCoverFormatItems;
        public Dictionary<string, CoverFormat> PreferredCoverFormatItems { get => _preferredCoverFormatItems; set => SetValue(ref _preferredCoverFormatItems, value); }

        private Dictionary<string, VerticalCoverResolution> _verticalCoverResolutionItems;
        public Dictionary<string, VerticalCoverResolution> VerticalCoverResolutionItems { get => _verticalCoverResolutionItems; set => SetValue(ref _verticalCoverResolutionItems, value); }

        private Dictionary<string, SquareCoverResolution> _squareCoverResolutionItems;
        public Dictionary<string, SquareCoverResolution> SquareCoverResolutionItems { get => _squareCoverResolutionItems; set => SetValue(ref _squareCoverResolutionItems, value); }

        private Dictionary<string, XboxLocale> _localeItems;
        public Dictionary<string, XboxLocale> LocaleItems { get => _localeItems; set => SetValue(ref _localeItems, value); }

        public XboxMetadataSettingsViewModel(XboxMetadata plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<XboxMetadataSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new XboxMetadataSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);

            BackgroundImageResolutions = new Dictionary<string, BackgroundImageResolution>
            {
                [ResourceProvider.GetString("OriginalImageResolutionLabel")] = BackgroundImageResolution.Original,
                ["1920x1080px"] = BackgroundImageResolution.Resolution1920x1080,
            };

            PreferredCoverFormatItems = new Dictionary<string, CoverFormat>
            {
                [ResourceProvider.GetString("ImageVerticalFormatLabel")] = CoverFormat.Vertical,
                [ResourceProvider.GetString("ImageSquareFormatLabel")] = CoverFormat.Square,
            };

            VerticalCoverResolutionItems = new Dictionary<string, VerticalCoverResolution>
            {
                [ResourceProvider.GetString("OriginalImageResolutionLabel")] = VerticalCoverResolution.Original,
                ["720x1080px"] = VerticalCoverResolution.Resolution720x1080,
                ["600x900px"] = VerticalCoverResolution.Resolution600x900
            };

            SquareCoverResolutionItems = new Dictionary<string, SquareCoverResolution>
            {
                [ResourceProvider.GetString("OriginalImageResolutionLabel")] = SquareCoverResolution.Original,
                ["2160x2160px"] = SquareCoverResolution.Resolution2160x2160,
                ["1080x1080px"] = SquareCoverResolution.Resolution1080x1080,
                ["600x600px"] = SquareCoverResolution.Resolution600x600,
                ["300x300px"] = SquareCoverResolution.Resolution300x300
            };

            LocaleItems = new Dictionary<string, XboxLocale>
            {
                // AMERICA
                { "Argentina - Español", XboxLocale.Argentina_Spanish },
                { "Bolivia - Español", XboxLocale.Bolivia_Spanish },
                { "Brasil - Português", XboxLocale.Brazil_Portuguese },
                { "Canada - English", XboxLocale.Canada_English },
                { "Canada - Français", XboxLocale.Canada_French },
                { "Chile - Español", XboxLocale.Chile_Spanish },
                { "Colombia - Español", XboxLocale.Colombia_Spanish },
                { "Costa Rica - Español", XboxLocale.CostaRica_Spanish },
                { "Ecuador - Español", XboxLocale.Ecuador_Spanish },
                { "El Salvador - Español", XboxLocale.ElSalvador_Spanish },
                { "Guatemala - Español", XboxLocale.Guatemala_Spanish },
                { "Honduras - Español", XboxLocale.Honduras_Spanish },
                { "México - Español", XboxLocale.Mexico_Spanish },
                { "Nicaragua - Español", XboxLocale.Nicaragua_Spanish },
                { "Panamá - Español", XboxLocale.Panama_Spanish },
                { "Paraguay - Español", XboxLocale.Paraguay_Spanish },
                { "Perú - Español", XboxLocale.Peru_Spanish },
                { "United States - English", XboxLocale.UnitedStates_English },
                { "Uruguay - Español", XboxLocale.Uruguay_Spanish },

                // EUROPE
                { "België - Nederlands", XboxLocale.Belgium_Dutch },
                { "Belgique - Français", XboxLocale.Belgium_French },
                { "Bosna-Hersek – Bosanski", XboxLocale.BosniaAndHerzegovina_Bosnian },
                { "Česká Republika - Čeština", XboxLocale.CzechRepublic_Czech },
                { "Srbija - Latinica Srpski", XboxLocale.Serbia_LatinSerbian },
                { "Crna Gora - Srpski", XboxLocale.Montenegro_Serbian },
                { "Cyprus - English", XboxLocale.Cyprus_English },
                { "Danmark - Dansk", XboxLocale.Denmark_Danish },
                { "Deutschland - Deutsch", XboxLocale.Germany_German },
                { "España - Español", XboxLocale.Spain_Spanish },
                { "Eesti - Eesti", XboxLocale.Estonia_Estonian },
                { "France - Français", XboxLocale.France_French },
                { "Hrvatska - Hrvatski", XboxLocale.Croatia_Croatian },
                { "Ireland - English", XboxLocale.Ireland_English },
                { "Ísland - Íslenska", XboxLocale.Iceland_Icelandic },
                { "Italia - Italiano", XboxLocale.Italy_Italian },
                { "Latvija - Latviešu", XboxLocale.Latvia_Latvian },
                { "Liechtenstein - Deutsch", XboxLocale.Liechtenstein_German },
                { "Lietuva - Lietuvių", XboxLocale.Lithuania_Lithuanian },
                { "Luxembourg - Français", XboxLocale.Luxembourg_French },
                { "Luxemburg - Deutsch", XboxLocale.Luxembourg_German },
                { "Magyarország - Magyar", XboxLocale.Hungary_Hungarian },
                { "Северна Македонија - Македонија", XboxLocale.NorthMacedonia_Macedonian },
                { "Malta - Maltese", XboxLocale.Malta_Maltese },
                { "Nederland - Nederlands", XboxLocale.Netherlands_Dutch },
                { "Norge - Norsk bokmål", XboxLocale.Norway_Norwegian },
                { "Österreich - Deutsch", XboxLocale.Austria_German },
                { "Polska - Polski", XboxLocale.Poland_Polish },
                { "Portugal - Português", XboxLocale.Portugal_Portuguese },
                { "Republica Moldova - Română", XboxLocale.Moldova_Romanian },
                { "Республика Молдова - Русский", XboxLocale.Moldova_Russian },
                { "România - Română", XboxLocale.Romania_Romanian },
                { "Schweiz - Deutsch", XboxLocale.Switzerland_German },
                { "Shqipëri - Shqip", XboxLocale.Albania_Albanian },
                { "Slovenija - Slovenščina", XboxLocale.Slovenia_Slovenian },
                { "Slovensko - Slovenčina", XboxLocale.Slovakia_Slovak },
                { "Srbija - Srpski", XboxLocale.Serbia_Serbian },
                { "Suisse - Français", XboxLocale.Switzerland_French },
                { "Suomi - Suomi", XboxLocale.Finland_Finnish },
                { "Sverige - Svenska", XboxLocale.Sweden_Swedish },
                { "United Kingdom - English", XboxLocale.UnitedKingdom_English },
                { "Ελλάδα - Ελληνικά", XboxLocale.Greece_Greek },
                { "България - Български", XboxLocale.Bulgaria_Bulgarian },
                { "Россия - Русский", XboxLocale.Russia_Russian },
                { "Україна - Українська", XboxLocale.Ukraine_Ukrainian },
                { "საქართველო - ქართული", XboxLocale.Georgia_Georgian },
                { "Australia - English", XboxLocale.Australia_English },

                // ASIA + PACIFIC
                { "Hong Kong SAR / Macao SAR - English", XboxLocale.HongKongSAR_MacaoSAR_English },
                { "India - English", XboxLocale.India_English },
                { "Indonesia - Bahasa Indonesia", XboxLocale.Indonesia_BahasaIndonesia },
                { "Malaysia - English", XboxLocale.Malaysia_English },
                { "New Zealand - English", XboxLocale.NewZealand_English },
                { "Philippines - English", XboxLocale.Philippines_English },
                { "Singapore - English", XboxLocale.Singapore_English },
                { "Việt Nam - Tiếng việt", XboxLocale.Vietnam_Vietnamese },
                { "ไทย - ไทย", XboxLocale.Thailand_Thai },
                { "대한민국 - 한국어", XboxLocale.SouthKorea_Korean },
                { "中国 - 中文", XboxLocale.China_Chinese },
                { "台灣 - 繁體中文", XboxLocale.Taiwan_TraditionalChinese },
                { "日本 - 日本語", XboxLocale.Japan_Japanese },
                { "香港特別行政區/澳門特別行政區 - 繁體中文", XboxLocale.HongKong_Macau_Chinese },

                // MIDDLE EAST + AFRICA
                { "الجزائر - عربي", XboxLocale.Algeria_Arabic },
                { "المغرب - عربي", XboxLocale.Morocco_Arabic },
                { "South Africa - English", XboxLocale.SouthAfrica_English },
                { "تونس - العربية", XboxLocale.Tunisia_Arabic },
                { "Türkiye - Türkçe", XboxLocale.Turkey_Turkish },
                { "ישראל - עברית", XboxLocale.Israel_Hebrew },
                { "الإمارات العربية المتحدة - العربية", XboxLocale.UnitedArabEmirates_Arabic },
                { "المملكة العربية السعودية - العربية", XboxLocale.SaudiArabia_Arabic },
                { "ليبيا - العربية", XboxLocale.Libya_Arabic },
                { "مصر - العربية", XboxLocale.Egypt_Arabic },
                { "البحرين - عربي", XboxLocale.Bahrain_Arabic },
                { "الكويت - عربي", XboxLocale.Kuwait_Arabic },
                { "عمان - عربي", XboxLocale.Oman_Arabic },
                { "قطر - عربي", XboxLocale.Qatar_Arabic }
            };
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}