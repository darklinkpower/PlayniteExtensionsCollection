using ExtraMetadataLoader.Models;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private HttpClient client;
        private static readonly Regex steamLinkRegex = new Regex(@"^https?:\/\/store\.steampowered\.com\/app\/(\d+)", RegexOptions.Compiled);
        public enum ActiveVideoType { Microtrailer, Trailer, None }; 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        IPlayniteAPI PlayniteApi; public ExtraMetadataLoaderSettingsViewModel SettingsModel { get; set; }

        
        private bool useMicrovideosSource;
        private readonly string pluginDataPath;
        private readonly DesktopView ActiveViewAtCreation;
        private ActiveVideoType activeVideoType;
        private bool isDragging;
        private Uri microVideoPath;
        private Uri trailerVideoPath;
        private bool multipleSourcesAvailable = false;
        private Game currentGame;
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
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            this.pluginDataPath = pluginDataPath;
            this.PlayniteApi = PlayniteApi;
            SettingsModel = settings;
            DataContext = this;

            useMicrovideosSource = settings.Settings.UseMicrotrailersDefault;
            player.Volume = 1;
            volumeSlider.Value = 0;

            if (settings.Settings.DefaultVolume != 0)
            {
                volumeSlider.Value = settings.Settings.DefaultVolume / 100;
            }
            if (settings.Settings.StartNoSound)
            {
                player.Volume = 0;
            }
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }
            volumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += new EventHandler(timer_Tick);
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
            player.Volume = e.NewValue * e.NewValue;
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

        void MediaPause()
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
            if (player.Volume > 0)
            {
                player.Volume = 0;
            }
            else if (player.Volume == 0)
            {
                player.Volume = volumeSlider.Value * volumeSlider.Value;
            }
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

        private void ResetPlayerValues()
        {
            VideoSource = null;
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

            ResetPlayerValues();
            currentGame = null;
            if (SettingsModel.Settings.EnableVideoPlayer && newContext != null)
            {
                currentGame = newContext;
                UpdateGameVideoSources();
                playingContextChanged();
                return;
            }

            ControlVisibility = Visibility.Collapsed;
            SettingsModel.Settings.NewContextVideoAvailable = false;
        }

        public void playingContextChanged()
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
                //This is to get the first frame of the video
                player.Play();
                player.Pause();
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
            if (File.Exists(videoPath))
            {
                SettingsModel.Settings.IsAnyVideoAvailable = true;
                SettingsModel.Settings.IsTrailerAvailable = true;
                trailerVideoPath = new Uri(videoPath);
            }

            var videoMicroPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString(), "VideoMicrotrailer.mp4");
            if (File.Exists(videoMicroPath))
            {
                SettingsModel.Settings.IsAnyVideoAvailable = true;
                SettingsModel.Settings.IsMicrotrailerAvailable = true;
                microVideoPath = new Uri(videoMicroPath);
            }

            if (SettingsModel.Settings.StreamSteamVideosOnDemand && trailerVideoPath == null)
            {
                var gameDataPath = Path.Combine(pluginDataPath, $"{game.Id}_SteamAppDetails.json");
                if (!File.Exists(gameDataPath))
                {
                    string steamId = GetSteamId(game);
                    if (steamId != null)
                    {
                        var url = string.Format(@"https://store.steampowered.com/api/appdetails?appids={0}", steamId);
                        DownloadFile(url, gameDataPath).GetAwaiter().GetResult();
                    }
                }
                if (File.Exists(gameDataPath))
                {
                    var jsonString = File.ReadAllText(gameDataPath);
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

                            // Checking if micro videos exist takes too long
                            // and some videos have issues playing
                            //if (microVideoPath == null)
                            //{
                            //    var microvideoUrl = string.Format(@"https://steamcdn-a.akamaihd.net/steam/apps/{0}/microtrailer.mp4", response.data.Movies?[0].Id.ToString());
                            //    if (GetResponseCode(microvideoUrl) == HttpStatusCode.OK)
                            //    {
                            //        microVideoPath = new Uri(microvideoUrl);
                            //        SettingsModel.Settings.IsAnyVideoAvailable = true;
                            //        SettingsModel.Settings.IsMicrotrailerAvailable = true;
                            //    }
                            //}
                        }
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

        public async Task DownloadFile(string requestUri, string path)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    string fileToWriteTo = path;
                    using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during file download, url {requestUri}");
            }
        }

        public HttpStatusCode GetResponseCode(string url)
        {
            try
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                return response.StatusCode;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get HTTP response for {url}.");
                return HttpStatusCode.ServiceUnavailable;
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
