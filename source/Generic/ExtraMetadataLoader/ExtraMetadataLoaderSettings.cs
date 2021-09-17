using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExtraMetadataLoader
{
    public class ExtraMetadataLoaderSettings : ObservableObject
    {
        [DontSerialize]
        private bool enableVideoPlayer { get; set; } = true;
        public bool EnableVideoPlayer
        {
            get => enableVideoPlayer;
            set
            {
                enableVideoPlayer = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool autoPlayVideos { get; set; } = false;
        public bool AutoPlayVideos
        {
            get => autoPlayVideos;
            set
            {
                autoPlayVideos = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool repeatTrailerVideos { get; set; } = false;
        public bool RepeatTrailerVideos
        {
            get => repeatTrailerVideos;
            set
            {
                repeatTrailerVideos = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool startNoSound { get; set; } = false;
        public bool StartNoSound
        {
            get => startNoSound;
            set
            {
                startNoSound = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool useMicrotrailersDefault { get; set; } = false;
        public bool UseMicrotrailersDefault
        {
            get => useMicrotrailersDefault;
            set
            {
                useMicrotrailersDefault = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool fallbackVideoSource { get; set; } = true;
        public bool FallbackVideoSource
        {
            get => fallbackVideoSource;
            set
            {
                fallbackVideoSource = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool showControls { get; set; } = true;
        public bool ShowControls
        {
            get => showControls;
            set
            {
                showControls = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private double defaultVolume { get; set; } = 100;
        public double DefaultVolume
        {
            get => defaultVolume;
            set
            {
                defaultVolume = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool enableLogos { get; set; } = true;
        public bool EnableLogos
        {
            get => enableLogos;
            set
            {
                enableLogos = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private double logoMaxWidth { get; set; } = 600;
        public double LogoMaxWidth
        {
            get => logoMaxWidth;
            set
            {
                logoMaxWidth = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private double logoMaxHeight { get; set; } = 200;
        public double LogoMaxHeight
        {
            get => logoMaxHeight;
            set
            {
                logoMaxHeight = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private HorizontalAlignment logoHorizontalAlignment { get; set; } = HorizontalAlignment.Center;
        public HorizontalAlignment LogoHorizontalAlignment
        {
            get => logoHorizontalAlignment;
            set
            {
                logoHorizontalAlignment = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private VerticalAlignment logoVerticalAlignment { get; set; } = VerticalAlignment.Center;
        public VerticalAlignment LogoVerticalAlignment
        {
            get => logoVerticalAlignment;
            set
            {
                logoVerticalAlignment = value;
                OnPropertyChanged();
            }
        }

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        [DontSerialize]
        private bool isLogoAvailable { get; set; } = true;
        [DontSerialize]
        public bool IsLogoAvailable
        {
            get => isLogoAvailable;
            set
            {
                isLogoAvailable = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool isTrailerAvailable { get; set; } = false;
        [DontSerialize]
        public bool IsTrailerAvailable
        {
            get => isTrailerAvailable;
            set
            {
                isTrailerAvailable = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool isMicrotrailerAvailable { get; set; } = false;
        [DontSerialize]
        public bool IsMicrotrailerAvailable
        {
            get => isMicrotrailerAvailable;
            set
            {
                isMicrotrailerAvailable = value;
                OnPropertyChanged();
            }
        }
        [DontSerialize]
        private bool isAnyVideoAvailable { get; set; } = false;
        [DontSerialize]
        public bool IsAnyVideoAvailable
        {
            get => isAnyVideoAvailable;
            set
            {
                isAnyVideoAvailable = value;
                OnPropertyChanged();
            }
        }
    }

    public class ExtraMetadataLoaderSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ExtraMetadataLoader plugin;
        private ExtraMetadataLoaderSettings editingClone { get; set; }

        private ExtraMetadataLoaderSettings settings;
        public ExtraMetadataLoaderSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ExtraMetadataLoaderSettingsViewModel(ExtraMetadataLoader plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ExtraMetadataLoaderSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ExtraMetadataLoaderSettings();
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