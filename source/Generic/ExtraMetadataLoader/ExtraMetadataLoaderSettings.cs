using ExtraMetadataLoader.Common;
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
        private bool streamSteamVideosOnDemand { get; set; } = true;
        public bool StreamSteamVideosOnDemand
        {
            get => streamSteamVideosOnDemand;
            set
            {
                streamSteamVideosOnDemand = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool streamSteamHighQuality { get; set; } = false;
        public bool StreamSteamHighQuality
        {
            get => streamSteamHighQuality;
            set
            {
                streamSteamHighQuality = value;
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
        private double videoControlsOpacity { get; set; } = 0.3;
        public double VideoControlsOpacity
        {
            get => videoControlsOpacity;
            set
            {
                videoControlsOpacity = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private double videoControlsOpacityMouseOver { get; set; } = 1.0;
        public double VideoControlsOpacityMouseOver
        {
            get => videoControlsOpacityMouseOver;
            set
            {
                videoControlsOpacityMouseOver = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private VerticalAlignment videoControlsVerticalAlignment { get; set; } = VerticalAlignment.Bottom;
        public VerticalAlignment VideoControlsVerticalAlignment
        {
            get => videoControlsVerticalAlignment;
            set
            {
                videoControlsVerticalAlignment = value;
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
        private bool logosEnableShadowEffect { get; set; } = true;
        public bool LogosEnableShadowEffect
        {
            get => logosEnableShadowEffect;
            set
            {
                logosEnableShadowEffect = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private double logosShadowDepth { get; set; } = 0;
        public double LogosShadowDepth
        {
            get => logosShadowDepth;
            set
            {
                logosShadowDepth = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private double logosShadowDirection { get; set; } = 0;
        public double LogosShadowDirection
        {
            get => logosShadowDirection;
            set
            {
                logosShadowDirection = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private double logosBlurRadius { get; set; } = 20;
        public double LogosBlurRadius
        {
            get => logosBlurRadius;
            set
            {
                logosBlurRadius = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private double logosEffectOpacity { get; set; } = 0.75;
        public double LogosEffectOpacity
        {
            get => logosEffectOpacity;
            set
            {
                logosEffectOpacity = value;
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
        private bool isLogoAvailable { get; set; } = false;
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

        [DontSerialize]
        private bool newContextVideoAvailable { get; set; } = false;
        [DontSerialize]
        public bool NewContextVideoAvailable
        {
            get => newContextVideoAvailable;
            set
            {
                newContextVideoAvailable = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool isVideoPlaying { get; set; } = false;
        [DontSerialize]
        public bool IsVideoPlaying
        {
            get => isVideoPlaying;
            set
            {
                isVideoPlaying = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private string sgdbApiKey = string.Empty;
        public string SgdbApiKey
        {
            get => sgdbApiKey;
            set
            {
                sgdbApiKey = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool sgdbIncludeHumor = false;
        public bool SgdbIncludeHumor
        {
            get => sgdbIncludeHumor;
            set
            {
                sgdbIncludeHumor = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool sgdbIncludeNsfw = false;
        public bool SgdbIncludeNsfw
        {
            get => sgdbIncludeNsfw;
            set
            {
                sgdbIncludeNsfw = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool downloadLogosOnLibUpdate = true;
        public bool DownloadLogosOnLibUpdate
        {
            get => downloadLogosOnLibUpdate;
            set
            {
                downloadLogosOnLibUpdate = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool libUpdateSelectLogosAutomatically = false;
        public bool LibUpdateSelectLogosAutomatically
        {
            get => libUpdateSelectLogosAutomatically;
            set
            {
                libUpdateSelectLogosAutomatically = value;
                OnPropertyChanged();
            }
        }

        public DateTime LastAutoLibUpdateAssetsDownload = DateTime.Now;

        [DontSerialize]
        private bool processLogosOnDownload = true;
        public bool ProcessLogosOnDownload
        {
            get => processLogosOnDownload;
            set
            {
                processLogosOnDownload = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool logoTrimOnDownload = true;
        public bool LogoTrimOnDownload
        {
            get => logoTrimOnDownload;
            set
            {
                logoTrimOnDownload = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool setLogoMaxProcessDimensions = true;
        public bool SetLogoMaxProcessDimensions
        {
            get => setLogoMaxProcessDimensions;
            set
            {
                setLogoMaxProcessDimensions = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private int maxLogoProcessWidth { get; set; } = 640;
        public int MaxLogoProcessWidth
        {
            get => maxLogoProcessWidth;
            set
            {
                maxLogoProcessWidth = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private int maxLogoProcessHeight { get; set; } = 640;
        public int MaxLogoProcessHeight
        {
            get => maxLogoProcessHeight;
            set
            {
                maxLogoProcessHeight = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool updateMissingLogoTagOnLibUpdate { get; set; } = false;
        public bool UpdateMissingLogoTagOnLibUpdate
        {
            get => updateMissingLogoTagOnLibUpdate;
            set
            {
                updateMissingLogoTagOnLibUpdate = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool updateMissingVideoTagOnLibUpdate { get; set; } = false;
        public bool UpdateMissingVideoTagOnLibUpdate
        {
            get => updateMissingVideoTagOnLibUpdate;
            set
            {
                updateMissingVideoTagOnLibUpdate = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool updateMissingMicrovideoTagOnLibUpdate { get; set; } = false;
        public bool UpdateMissingMicrovideoTagOnLibUpdate
        {
            get => updateMissingMicrovideoTagOnLibUpdate;
            set
            {
                updateMissingMicrovideoTagOnLibUpdate = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool downloadVideosOnLibUpdate = false;
        public bool DownloadVideosOnLibUpdate
        {
            get => downloadVideosOnLibUpdate;
            set
            {
                downloadVideosOnLibUpdate = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool downloadVideosMicroOnLibUpdate = false;
        public bool DownloadVideosMicroOnLibUpdate
        {
            get => downloadVideosMicroOnLibUpdate;
            set
            {
                downloadVideosMicroOnLibUpdate = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool videoSteamDownloadHdQuality { get; set; } = false;
        public bool VideoSteamDownloadHdQuality
        {
            get => videoSteamDownloadHdQuality;
            set
            {
                videoSteamDownloadHdQuality = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool steamDlOnlyProcessPcGames = true;
        public bool SteamDlOnlyProcessPcGames
        {
            get => steamDlOnlyProcessPcGames;
            set
            {
                steamDlOnlyProcessPcGames = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private string ffmpegPath = string.Empty;
        public string FfmpegPath
        {
            get => ffmpegPath;
            set
            {
                ffmpegPath = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private string ffprobePath = string.Empty;
        public string FfprobePath
        {
            get => ffprobePath;
            set
            {
                ffprobePath = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private string youtubeDlPath = string.Empty;
        public string YoutubeDlPath
        {
            get => youtubeDlPath;
            set
            {
                youtubeDlPath = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private string youtubeCookiesPath = string.Empty;
        public string YoutubeCookiesPath
        {
            get => youtubeCookiesPath;
            set
            {
                youtubeCookiesPath = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool enableYoutubeSearch { get; set; } = true;
        public bool EnableYoutubeSearch
        {
            get => enableYoutubeSearch;
            set
            {
                enableYoutubeSearch = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool enableAlternativeDetailsVideoPlayer { get; set; } = false;
        public bool EnableAlternativeDetailsVideoPlayer
        {
            get => enableAlternativeDetailsVideoPlayer;
            set
            {
                enableAlternativeDetailsVideoPlayer = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool enableAlternativeGridVideoPlayer { get; set; } = false;
        public bool EnableAlternativeGridVideoPlayer
        {
            get => enableAlternativeGridVideoPlayer;
            set
            {
                enableAlternativeGridVideoPlayer = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool enableAlternativeGenericVideoPlayer { get; set; } = false;
        public bool EnableAlternativeGenericVideoPlayer
        {
            get => enableAlternativeGenericVideoPlayer;
            set
            {
                enableAlternativeGenericVideoPlayer = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool showVideoPreviewNotPlayingDetails { get; set; } = true;
        public bool ShowVideoPreviewNotPlayingDetails
        {
            get => showVideoPreviewNotPlayingDetails;
            set
            {
                showVideoPreviewNotPlayingDetails = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool showVideoPreviewNotPlayingGrid { get; set; } = true;
        public bool ShowVideoPreviewNotPlayingGrid
        {
            get => showVideoPreviewNotPlayingGrid;
            set
            {
                showVideoPreviewNotPlayingGrid = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool showVideoPreviewNotPlayingGeneric { get; set; } = true;
        public bool ShowVideoPreviewNotPlayingGeneric
        {
            get => showVideoPreviewNotPlayingGeneric;
            set
            {
                showVideoPreviewNotPlayingGeneric = value;
                OnPropertyChanged();
            }
        }
    }

    public class ExtraMetadataLoaderSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ExtraMetadataLoader plugin;
        private readonly IPlayniteAPI playniteApi;

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

        public ExtraMetadataLoaderSettingsViewModel(ExtraMetadataLoader plugin, IPlayniteAPI playniteApi)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;
            this.playniteApi = playniteApi;
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

        public RelayCommand<object> OpenSgdbApiSiteCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ProcessStarter.StartUrl(@"https://www.steamgriddb.com/profile/preferences/api");
            });
        }

        public RelayCommand<object> DownloadFfmpegCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ProcessStarter.StartUrl(@"https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.7z");
            });
        }

        public RelayCommand<object> DownloadYoutubeDlCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ProcessStarter.StartUrl(@"https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe");
            });
        }

        public RelayCommand<object> OpenCookiesObtainHelpCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ProcessStarter.StartUrl(@"https://github.com/ytdl-org/youtube-dl/blob/master/README.md#how-do-i-pass-cookies-to-youtube-dl");
            });
        }

        public RelayCommand<object> BrowseSelectYoutubeCookiesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var filePath = playniteApi.Dialogs.SelectFile("cookies|cookies.txt");
                if (!filePath.IsNullOrEmpty())
                {
                    settings.YoutubeCookiesPath = filePath;
                }
            });
        }

        public RelayCommand<object> BrowseSelectYoutubeDlCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var filePath = playniteApi.Dialogs.SelectFile("yt-dlp|yt-dlp.exe");
                if (!filePath.IsNullOrEmpty())
                {
                    settings.YoutubeDlPath = filePath;
                }
            });
        }

        public RelayCommand<object> BrowseSelectFfmpegCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var filePath = playniteApi.Dialogs.SelectFile("ffmpeg|ffmpeg.exe");
                if (!filePath.IsNullOrEmpty())
                {
                    settings.FfmpegPath = filePath;
                }
            });
        }

        public RelayCommand<object> BrowseSelectFfprobeCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var filePath = playniteApi.Dialogs.SelectFile("ffProbe|ffProbe.exe");
                if (!filePath.IsNullOrEmpty())
                {
                    settings.FfprobePath = filePath;
                }
            });
        }

        public RelayCommand<object> LoginToYoutubeCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                LoginToYoutube();
            });
        }

        public void LoginToYoutube()
        {
            var webView = playniteApi.WebViews.CreateView(700, 700);
            webView.LoadingChanged += (s, e) =>
            {
                var address = webView.GetCurrentAddress();
                if (address == "https://www.youtube.com/")
                {
                    webView.Close();
                }
            };
            webView.Navigate("https://accounts.google.com/ServiceLogin?service=youtube&uilel=3&passive=true&continue=https%3A%2F%2Fwww.youtube.com%2Fsignin%3Faction_handle_signin%3Dtrue");
            webView.OpenDialog();
            webView.Dispose();
        }
    }
}