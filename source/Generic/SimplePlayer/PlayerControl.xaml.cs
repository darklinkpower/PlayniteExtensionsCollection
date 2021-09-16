using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace SimplePlayer
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : PluginUserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        
        SimplePlayerSettings Settings;
        IPlayniteAPI PlayniteApi;
        bool isDragging;
        public bool isPlaying;
        Uri videoSource;
        DispatcherTimer timer;
        private string playbackTimeProgress;
        public string PlaybackTimeProgress
        {
            get => playbackTimeProgress;
            set
            {
                playbackTimeProgress = value;
                OnPropertyChanged();
            }
        }
        private string playbackTimeTotal;
        public string PlaybackTimeTotal
        {
            get => playbackTimeTotal;
            set
            {
                playbackTimeTotal = value;
                OnPropertyChanged();
            }
        }

        public PlayerControl(IPlayniteAPI PlayniteApi, SimplePlayerSettingsViewModel PluginSettings)
        {
            isPlaying = false;
            InitializeComponent();
            
            this.PlayniteApi = PlayniteApi;
            Settings = PluginSettings.Settings;
            
            player.Volume = 1;
            volumeSlider.Value = 1;
            volumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += new EventHandler(timer_Tick);
            playbackTimeProgress = "Start";
            playbackTimeTotal = "End";
            DataContext = this;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                timelineSlider.Value = player.Position.TotalSeconds;
            }
            PlaybackTimeProgress = player.Position.TotalSeconds.ToString(@"mm\:ss") ?? "0:00";
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
            }, (a) => !isPlaying && videoSource != null);
        }

        void MediaPlay()
        {
            player.Play();
        }

        public RelayCommand<object> VideoPauseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MediaPause();
                isPlaying = false;
            }, (a) => isPlaying && videoSource != null);
        }

        void MediaPause()
        {
            player.Pause();
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


        private void player_MediaOpened(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = player.NaturalDuration.TimeSpan;
                timelineSlider.SmallChange = 0.25;
                timelineSlider.LargeChange = Math.Min(10, ts.Seconds / 10);
                timelineSlider.Maximum = ts.TotalSeconds;
                playbackProgressBar.Maximum = ts.TotalSeconds;
                PlaybackTimeTotal = ts.TotalSeconds.ToString(@"mm\:ss");
            }

            timer.Start();
        }

        private void player_MediaEnded(object sender, EventArgs e)
        {
            player.Stop();
            isPlaying = false;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            player.Source = null;
            videoSource = null;
            isPlaying = false;
            if (newContext == null)
            {
                return;
            }

            SetTrailerPath(newContext);
            if (videoSource != null)
            {
                player.Source = videoSource;

                //This is to get the first frame of the video
                player.Play();
                player.Pause();
                isPlaying = false;
            }
        }

        public void SetTrailerPath(Game game)
        {
            var videoPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString(), "VideoTrailer.mp4");
            if (File.Exists(videoPath))
            {
                videoSource = new Uri(videoPath);
            }
            return;
        }

        public bool GetIsVideoPlaying()
        {
            if (Utils.GetMediaState(player) == MediaState.Play)
            {
                return true;
            }
            return false;
        }
    }
}

public static class Utils
{
    public static MediaState GetMediaState(this MediaElement myMedia)
    {
        FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
        object helperObject = hlp.GetValue(myMedia);
        FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
        MediaState state = (MediaState)stateField.GetValue(helperObject);
        return state;
    }
}