using ExtraMetadataLoader.Models;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using PluginsCommon.Web;
using SteamCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ExtraMetadataLoader
{
    /// <summary>
    /// Interaction logic for VideoPlayerControl.xaml
    /// </summary>
    public partial class VideoPlayerControl : PluginUserControl, INotifyPropertyChanged
    {
        private static readonly ILogger logger = LogManager.GetLogger(); 
        private static readonly Regex steamLinkRegex = new Regex(@"^https?:\/\/store\.steampowered\.com\/app\/(\d+)", RegexOptions.None);
        public enum ActiveVideoType { Microtrailer, Trailer, None }; 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        IPlayniteAPI PlayniteApi; public ExtraMetadataLoaderSettingsViewModel SettingsModel { get; set; }

        
        private bool useMicrovideosSource;
        private readonly string pluginDataPath;
        private readonly DispatcherTimer updatePlayerTimer;
        private ActiveVideoType activeVideoType;
        private bool isDragging;
        private Uri microVideoPath;
        private Uri trailerVideoPath;
        private bool multipleSourcesAvailable = false;
        private Game currentGame;

        private DesktopView activeViewAtCreation;
        public DesktopView ActiveViewAtCreation
        {
            get => activeViewAtCreation;
            set
            {
                activeViewAtCreation = value;
                OnPropertyChanged();
            }
        }

        private Uri videoSource;
        public Uri VideoSource
        {
            get => videoSource;
            set
            {
                videoSource = value;
                OnPropertyChanged();
            }
        }

        private bool isPlayerMuted = false;
        public bool IsPlayerMuted
        {
            get => isPlayerMuted;
            set
            {
                isPlayerMuted = value;
                OnPropertyChanged();
            }
        }

        private double videoPlayerVolume;
        public double VideoPlayerVolume
        {
            get => videoPlayerVolume;
            set
            {
                videoPlayerVolume = value * value;
                OnPropertyChanged();
            }
        }

        DispatcherTimer timer;
        private string playbackTimeProgress = "00:00";
        public string PlaybackTimeProgress
        {
            get => playbackTimeProgress;
            set
            {
                playbackTimeProgress = value;
                OnPropertyChanged();
            }
        }
        private string playbackTimeTotal = "00:00";
        public string PlaybackTimeTotal
        {
            get => playbackTimeTotal;
            set
            {
                playbackTimeTotal = value;
                OnPropertyChanged();
            }
        }

        private Visibility controlVisibility = Visibility.Collapsed;
        public Visibility ControlVisibility
        {
            get => controlVisibility;
            set
            {
                controlVisibility = value;
                OnPropertyChanged();
            }
        }

        public VideoPlayerControl(IPlayniteAPI PlayniteApi, ExtraMetadataLoaderSettingsViewModel settings, string pluginDataPath)
        {
            InitializeComponent();
            this.pluginDataPath = pluginDataPath;
            this.PlayniteApi = PlayniteApi;
            SettingsModel = settings;
            DataContext = this;

            useMicrovideosSource = settings.Settings.UseMicrotrailersDefault;
            if (settings.Settings.StartNoSound)
            {
                IsPlayerMuted = true;
            }

            var initialVolumeValue = settings.Settings.DefaultVolume / 100;
            volumeSlider.Value = initialVolumeValue;
            VideoPlayerVolume = initialVolumeValue;

            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }

            volumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += new EventHandler(timer_Tick);

            updatePlayerTimer = new DispatcherTimer();
            updatePlayerTimer.Interval = TimeSpan.FromMilliseconds(220);
            updatePlayerTimer.Tick += new EventHandler(UpdaterPlayerTimer_Tick);
        }

        private void UpdaterPlayerTimer_Tick(object sender, EventArgs e)
        {
            updatePlayerTimer.Stop();
            if (SettingsModel.Settings.EnableVideoPlayer && currentGame != null)
            {
                UpdateGameVideoSources();
                playingContextChanged();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                timelineSlider.Value = player.Position.TotalSeconds;
            }

            PlaybackTimeProgress = player.Position.ToString(@"mm\:ss") ?? "00:00";
            playbackProgressBar.Value = player.Position.TotalSeconds;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VideoPlayerVolume = e.NewValue;
        }

        private void timelineSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void timelineSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDragging = false;
            player.Position = TimeSpan.FromSeconds(timelineSlider.Value);
        }

        public RelayCommand<object> VideoPlayCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MediaPlay();
            }, (a) => !SettingsModel.Settings.IsVideoPlaying && VideoSource != null);
        }

        void MediaPlay()
        {
            player.Play();
            timer.Start();
            SettingsModel.Settings.IsVideoPlaying = true;
        }

        public RelayCommand<object> VideoPauseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MediaPause();
            }, (a) => SettingsModel.Settings.IsVideoPlaying && VideoSource != null);
        }

        public void MediaPause()
        {
            player.Pause();
            timer.Stop();
            SettingsModel.Settings.IsVideoPlaying = false;
        }

        public RelayCommand<object> VideoMuteCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MediaMute();
            });
        }

        void MediaMute()
        {
            IsPlayerMuted = !IsPlayerMuted;
        }

        public RelayCommand<object> SwitchVideoSourceCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SwitchVideoSource();
            }, (a) => multipleSourcesAvailable == true);
        }

        void SwitchVideoSource()
        {
            var activeVideoTypeSender = activeVideoType;
            var sourceSwitched = false;
            ResetPlayerValues();

            // Paths need to be revaluated in case videos were deleted since video playing started
            UpdateGameVideoSources();
            if (activeVideoTypeSender == ActiveVideoType.Trailer && microVideoPath != null)
            {
                VideoSource = microVideoPath;
                activeVideoType = ActiveVideoType.Microtrailer;
                sourceSwitched = true;
            }
            else if (activeVideoTypeSender == ActiveVideoType.Microtrailer && trailerVideoPath != null)
            {
                VideoSource = trailerVideoPath;
                activeVideoType = ActiveVideoType.Trailer;
                sourceSwitched = true;
            }

            if (sourceSwitched)
            {
                useMicrovideosSource = !useMicrovideosSource;
                playingContextChanged();
            }
        }

        private void player_MediaOpened(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = player.NaturalDuration.TimeSpan;
                timelineSlider.SmallChange = 0.25;
                timelineSlider.LargeChange = Math.Min(10, ts.Seconds / 10);
                timelineSlider.Maximum = ts.TotalSeconds;
                playbackProgressBar.Maximum = ts.TotalSeconds;
                PlaybackTimeTotal = ts.ToString(@"mm\:ss");
            }
        }

        private void player_MediaEnded(object sender, EventArgs e)
        {
            if (activeVideoType == ActiveVideoType.Trailer && SettingsModel.Settings.RepeatTrailerVideos
                || activeVideoType == ActiveVideoType.Microtrailer)
            {
                player.Position = new TimeSpan(0, 0, 0);
                MediaPlay();
            }
            else
            {
                player.Stop();
                timer.Stop();
                SettingsModel.Settings.IsVideoPlaying = false;
            }
        }

        public void ResetPlayerValues()
        {
            VideoSource = null;
            player.Stop();
            SettingsModel.Settings.IsVideoPlaying = false;
            timelineSlider.Value = 0;
            playbackProgressBar.Value = 0;
            PlaybackTimeProgress = "00:00";
            PlaybackTimeTotal = "00:00";
            activeVideoType = ActiveVideoType.None;
            SettingsModel.Settings.IsAnyVideoAvailable = false;
            SettingsModel.Settings.IsTrailerAvailable = false;
            SettingsModel.Settings.IsMicrotrailerAvailable = false;
            microVideoPath = null;
            trailerVideoPath = null;
            multipleSourcesAvailable = false;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                ActiveViewAtCreation != PlayniteApi.MainView.ActiveDesktopView)
            {
                VideoSource = null;
                return;
            }

            currentGame = null;
            if (newContext != null)
            {
                currentGame = newContext;
            }

            RefreshPlayer();
        }

        public void RefreshPlayer()
        {
            ResetPlayerValues();
            ControlVisibility = Visibility.Collapsed;
            SettingsModel.Settings.NewContextVideoAvailable = false;

            // Used to prevent using processing while quickly changing games
            updatePlayerTimer.Stop();
            updatePlayerTimer.Start();
        }

        private void playingContextChanged()
        {
            if (videoSource == null)
            {
                SettingsModel.Settings.NewContextVideoAvailable = false;
                ControlVisibility = Visibility.Collapsed;
                return;
            }

            SettingsModel.Settings.NewContextVideoAvailable = true;
            if (SettingsModel.Settings.AutoPlayVideos)
            {
                MediaPlay();
            }
            else
            {
                SettingsModel.Settings.IsVideoPlaying = false;
            }
            ControlVisibility = Visibility.Visible;
        }

        public void UpdateGameVideoSources()
        {
            if (currentGame == null)
            {
                return;
            }

            var game = currentGame;
            var videoPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString(), "VideoTrailer.mp4");
            if (FileSystem.FileExists(videoPath))
            {
                SettingsModel.Settings.IsAnyVideoAvailable = true;
                SettingsModel.Settings.IsTrailerAvailable = true;
                trailerVideoPath = new Uri(videoPath);
            }

            var videoMicroPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString(), "VideoMicrotrailer.mp4");
            if (FileSystem.FileExists(videoMicroPath))
            {
                SettingsModel.Settings.IsAnyVideoAvailable = true;
                SettingsModel.Settings.IsMicrotrailerAvailable = true;
                microVideoPath = new Uri(videoMicroPath);
            }

            if (SettingsModel.Settings.StreamSteamVideosOnDemand && trailerVideoPath == null)
            {
                var gameDataPath = Path.Combine(pluginDataPath, $"{game.Id}_SteamAppDetails.json");
                var jsonDownloadValid = true;
                if (!FileSystem.FileExists(gameDataPath))
                {
                    string steamId = GetSteamId(game);
                    if (steamId != null)
                    {
                        var url = string.Format(@"https://store.steampowered.com/api/appdetails?appids={0}", steamId);
                        jsonDownloadValid = HttpDownloader.DownloadFileAsync(url, gameDataPath).GetAwaiter().GetResult();
                    }
                }

                if (FileSystem.FileExists(gameDataPath) && jsonDownloadValid)
                {
                    var jsonString = FileSystem.ReadStringFromFile(gameDataPath);
                    try
                    {
                        var parsedData = Serialization.FromJson<Dictionary<string, SteamAppDetails>>(jsonString);
                        if (parsedData.Keys?.Any() == true)
                        {
                            var response = parsedData[parsedData.Keys.First()];
                            if (response.success == true && response.data != null)
                            {
                                if (trailerVideoPath == null)
                                {
                                    if (SettingsModel.Settings.StreamSteamHighQuality)
                                    {
                                        trailerVideoPath = response.data.Movies?[0].Mp4.Max;
                                    }
                                    else
                                    {
                                        trailerVideoPath = response.data.Movies?[0].Mp4.Q480;
                                    }
                                    SettingsModel.Settings.IsAnyVideoAvailable = true;
                                    SettingsModel.Settings.IsTrailerAvailable = true;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // According to #169, it seems that for some reason the uri redirects to
                        // another page and a html source gets downloaded so it needs to be deleted
                        logger.Error($"Error deserializing steam appdetails json file in {gameDataPath}");
                        FileSystem.DeleteFile(gameDataPath);
                    }
                }
            }

            if (trailerVideoPath != null && microVideoPath != null)
            {
                multipleSourcesAvailable = true;
            }

            if (useMicrovideosSource)
            {
                if (microVideoPath != null)
                {
                    VideoSource = microVideoPath;
                    activeVideoType = ActiveVideoType.Microtrailer;
                }
                else if (trailerVideoPath != null && SettingsModel.Settings.FallbackVideoSource)
                {
                    VideoSource = trailerVideoPath;
                    activeVideoType = ActiveVideoType.Trailer;
                }
            }
            else
            {
                if (trailerVideoPath != null)
                {
                    VideoSource = trailerVideoPath;
                    activeVideoType = ActiveVideoType.Trailer;
                }
                else if (microVideoPath != null && SettingsModel.Settings.FallbackVideoSource)
                {
                    VideoSource = microVideoPath;
                    activeVideoType = ActiveVideoType.Microtrailer;
                }
            }
        }

        private string GetSteamId(Game game)
        {
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                return game.GameId;
            }
            else
            {
                if (game.Links == null)
                {
                    return null;
                }

                foreach (Link gameLink in game.Links)
                {
                    var linkMatch = steamLinkRegex.Match(gameLink.Url);
                    if (linkMatch.Success)
                    {
                        return linkMatch.Groups[1].Value;
                    }
                }

                return null;
            }
        }
    }
}
