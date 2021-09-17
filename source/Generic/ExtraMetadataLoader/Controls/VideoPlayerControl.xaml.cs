using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ExtraMetadataLoader
{
    /// <summary>
    /// Interaction logic for VideoPlayerControl.xaml
    /// </summary>
    public partial class VideoPlayerControl : PluginUserControl, INotifyPropertyChanged
    {
        public enum ActiveVideoType { Microtrailer, Trailer, None }; 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        IPlayniteAPI PlayniteApi; public ExtraMetadataLoaderSettingsViewModel SettingsModel { get; set; }

        bool isDragging;
        private bool useMicrovideosSource;
        private bool isPlaying = false;
        private ActiveVideoType activeVideoType;
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;
                OnPropertyChanged();
            }
        }

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

        public VideoPlayerControl(IPlayniteAPI PlayniteApi, ExtraMetadataLoaderSettingsViewModel settings)
        {
            InitializeComponent();
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
                isPlaying = true;
            }, (a) => !IsPlaying && VideoSource != null);
        }

        void MediaPlay()
        {
            player.Play();
            timer.Start();
            IsPlaying = true;
        }

        public RelayCommand<object> VideoPauseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MediaPause();
                isPlaying = false;
            }, (a) => IsPlaying && VideoSource != null);
        }

        void MediaPause()
        {
            player.Pause();
            timer.Stop();
            IsPlaying = false;
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
            });
        }

        void SwitchVideoSource()
        {
            ResetPlayerValues();
            useMicrovideosSource = !useMicrovideosSource;
            playingContextChanged();
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
                player.Position = new TimeSpan(0, 0, 1);
                MediaPlay();
            }
            else
            {
                player.Stop();
                timer.Stop();
                IsPlaying = false;
            }
        }

        private void ResetPlayerValues()
        {

            VideoSource = null;
            IsPlaying = false;
            activeVideoType = ActiveVideoType.None;
            timelineSlider.Value = 0;
            playbackProgressBar.Value = 0;
            PlaybackTimeProgress = "00:00";
            PlaybackTimeTotal = "00:00";
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            ResetPlayerValues();
            currentGame = null;
            if (!SettingsModel.Settings.EnableVideoPlayer)
            {
                ControlGrid.Visibility = Visibility.Collapsed;
                return;
            }

            if (newContext != null)
            {
                currentGame = newContext;
                playingContextChanged();
            }
        }

        public void playingContextChanged()
        {
            SetTrailerPath(currentGame);
            if (videoSource == null)
            {
                ControlGrid.Visibility = Visibility.Collapsed;
                return;
            }

            if (SettingsModel.Settings.AutoPlayVideos)
            {
                MediaPlay();
                isPlaying = true;
            }
            else
            {
                //This is to get the first frame of the video
                MediaPlay();
                MediaPause();
            }
        }

        public void SetTrailerPath(Game game)
        {
            var videoPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString(), "VideoTrailer.mp4");
            var videoMicroPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString(), "VideoMicrotrailer.mp4");
            if (useMicrovideosSource)
            {
                if (File.Exists(videoMicroPath))
                {
                    VideoSource = new Uri(videoMicroPath);
                    activeVideoType = ActiveVideoType.Microtrailer;
                }
                else if (File.Exists(videoPath) && SettingsModel.Settings.FallbackVideoSource)
                {
                    VideoSource = new Uri(videoPath);
                    activeVideoType = ActiveVideoType.Trailer;
                }
            }
            else
            {
                if (File.Exists(videoPath))
                {
                    VideoSource = new Uri(videoPath);
                    activeVideoType = ActiveVideoType.Trailer;
                }
                else if (File.Exists(videoMicroPath) && SettingsModel.Settings.FallbackVideoSource)
                {
                    VideoSource = new Uri(videoMicroPath);
                    activeVideoType = ActiveVideoType.Microtrailer;
                }
            }

            return;
        }
    }
}
