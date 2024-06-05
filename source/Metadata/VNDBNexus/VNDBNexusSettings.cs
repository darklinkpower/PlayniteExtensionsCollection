using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.SharedKernel;
using VNDBNexus.Database;

namespace VNDBNexus
{
    public class MetadataFieldsConfiguration : ObservableObject
    {
        private bool _enableName = true;
        public bool EnableName { get => _enableName; set => SetValue(ref _enableName, value); }

        private bool _enableDevelopers = true;
        public bool EnableDevelopers { get => _enableDevelopers; set => SetValue(ref _enableDevelopers, value); }

        private bool _enablePublishers = true;
        public bool EnablePublishers { get => _enablePublishers; set => SetValue(ref _enablePublishers, value); }

        private bool _enablePlatform = true;
        public bool EnablePlatform { get => _enablePlatform; set => SetValue(ref _enablePlatform, value); }

        private bool _enableDescription = true;
        public bool EnableDescription { get => _enableDescription; set => SetValue(ref _enableDescription, value); }

        private bool _enableBackgroundImage = true;
        public bool EnableBackgroundImage { get => _enableBackgroundImage; set => SetValue(ref _enableBackgroundImage, value); }

        private bool _enableCoverImage = true;
        public bool EnableCoverImage { get => _enableCoverImage; set => SetValue(ref _enableCoverImage, value); }

        private bool _enableCommunityScore = true;
        public bool EnableCommunityScore { get => _enableCommunityScore; set => SetValue(ref _enableCommunityScore, value); }

        private bool _enableReleaseDate = true;
        public bool EnableReleaseDate { get => _enableReleaseDate; set => SetValue(ref _enableReleaseDate, value); }

        private bool _enableTags = true;
        public bool EnableTags { get => _enableTags; set => SetValue(ref _enableTags, value); }

        private bool _enableLinks = true;
        public bool EnableLinks { get => _enableLinks; set => SetValue(ref _enableLinks, value); }
    }

    public class VNDBNexusSettings : ObservableObject
    {
        private int _settingsVersion = 1;
        public int SettingsVersion { get => _settingsVersion; set => SetValue(ref _settingsVersion, value); }

        private MetadataFieldsConfiguration _metadataFieldsConfiguration = new MetadataFieldsConfiguration();
        public MetadataFieldsConfiguration MetadataFieldsConfiguration { get => _metadataFieldsConfiguration; set => SetValue(ref _metadataFieldsConfiguration, value); }

        private bool _metadataAllowPartialDates = false;
        public bool MetadataAllowPartialDates { get => _metadataAllowPartialDates; set => SetValue(ref _metadataAllowPartialDates, value); }

        private double _tagsMinimumScore = 2.0;
        public double TagsMinimumScore
        {
            get { return _tagsMinimumScore; }
            set
            {
                _tagsMinimumScore = Math.Round(value, 1);
                OnPropertyChanged(nameof(TagsMinimumScore));
            }
        }

        private SpoilerLevelEnum _tagsMaxSpoilerLevel = SpoilerLevelEnum.None;
        public SpoilerLevelEnum TagsMaxSpoilerLevel { get => _tagsMaxSpoilerLevel; set => SetValue(ref _tagsMaxSpoilerLevel, value); }

        private bool _tagsGetContentCat = true;
        public bool TagsImportContentCat { get => _tagsGetContentCat; set => SetValue(ref _tagsGetContentCat, value); }

        private bool _tagsGetTechnicalCat = true;
        public bool TagsImportTechnicalCat { get => _tagsGetTechnicalCat; set => SetValue(ref _tagsGetTechnicalCat, value); }

        private bool _tagsGetSexualCat = true;
        public bool TagsImportSexualCat { get => _tagsGetSexualCat; set => SetValue(ref _tagsGetSexualCat, value); }

        private string _tagsPrefixContentCat = string.Empty;
        public string TagsPrefixContentCat { get => _tagsPrefixContentCat; set => SetValue(ref _tagsPrefixContentCat, value); }

        private string _tagsPrefixTechnicalCat = string.Empty;
        public string TagsPrefixTechnicalCat { get => _tagsPrefixTechnicalCat; set => SetValue(ref _tagsPrefixTechnicalCat, value); }

        private string _tagsPrefixSexualCat = string.Empty;
        public string TagsPrefixSexualCat { get => _tagsPrefixSexualCat; set => SetValue(ref _tagsPrefixSexualCat, value); }

        private ImageSexualityLevelEnum _imagesMaxSexualityLevel = ImageSexualityLevelEnum.Safe;
        public ImageSexualityLevelEnum ImagesMaxSexualityLevel { get => _imagesMaxSexualityLevel; set => SetValue(ref _imagesMaxSexualityLevel, value); }

        private ImageViolenceLevelEnum _imagesMaxViolenceLevel = ImageViolenceLevelEnum.Tame;
        public ImageViolenceLevelEnum ImagesMaxViolenceLevel { get => _imagesMaxViolenceLevel; set => SetValue(ref _imagesMaxViolenceLevel, value); }

        
        private bool _isControlVisible = false;
        [DontSerialize]
        public bool IsControlVisible { get => _isControlVisible; set => SetValue(ref _isControlVisible, value); }

        private bool _enableVnViewControl = true;
        public bool EnableVnViewControl { get => _enableVnViewControl; set => SetValue(ref _enableVnViewControl, value); }

        public DateTime LastDataseDumpsUpdate = DateTime.MinValue;

        private ImageSexualityLevelEnum _controlDefaultImagesMaxSexualityLevel = ImageSexualityLevelEnum.Safe;
        public ImageSexualityLevelEnum ControlDefaultImagesMaxSexualityLevel { get => _controlDefaultImagesMaxSexualityLevel; set => SetValue(ref _controlDefaultImagesMaxSexualityLevel, value); }

        private ImageViolenceLevelEnum _controlDefaultImagesMaxViolenceLevel = ImageViolenceLevelEnum.Tame;
        public ImageViolenceLevelEnum ControlDefaultImagesMaxViolenceLevel { get => _controlDefaultImagesMaxViolenceLevel; set => SetValue(ref _controlDefaultImagesMaxViolenceLevel, value); }

        private bool _controlDefaultEnableContentCategory = true;
        public bool ControlDefaultEnableContentCategory { get => _controlDefaultEnableContentCategory; set => SetValue(ref _controlDefaultEnableContentCategory, value); }

        private bool _controlDefaultEnableTechnicalCategory = true;
        public bool ControlDefaultEnableTechnicalCategory { get => _controlDefaultEnableTechnicalCategory; set => SetValue(ref _controlDefaultEnableTechnicalCategory, value); }

        private bool _controlDefaultEnableSexualCategoryCategory = false;
        public bool ControlDefaultEnableSexualCategoryCategory { get => _controlDefaultEnableSexualCategoryCategory; set => SetValue(ref _controlDefaultEnableSexualCategoryCategory, value); }
    }

    public class VNDBNexusSettingsViewModel : ObservableObject, ISettings
    {
        private string _tagsPrefixSexualCat = string.Empty;
        public string TagsPrefixSexualCat { get => _tagsPrefixSexualCat; set => SetValue(ref _tagsPrefixSexualCat, value); }

        private readonly VNDBNexus _plugin;
        private readonly VndbDatabase _vndbDatabase;

        private VNDBNexusSettings editingClone { get; set; }

        private VNDBNexusSettings _settings;
        public VNDBNexusSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        internal void UpdateSettings()
        {
            if (Settings.SettingsVersion == 1)
            {
                _vndbDatabase.VisualNovels.DeleteAll();
                Settings.SettingsVersion = 2;
                _plugin.SavePluginSettings(Settings);
            }
        }

        public VNDBNexusSettingsViewModel(VNDBNexus plugin, VndbDatabase vndbDatabase)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            _plugin = plugin;
            _vndbDatabase = vndbDatabase;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<VNDBNexusSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new VNDBNexusSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
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
            _plugin.SavePluginSettings(Settings);
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