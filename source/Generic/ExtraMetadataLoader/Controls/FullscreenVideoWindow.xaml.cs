using Playnite.SDK;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace EmlFullscreen
{
    /// <summary>
    /// Fullscreen video playback window with transport controls.
    /// Spawned by VideoPlayerControl to display video trailers
    /// in a borderless, maximized window.
    /// </summary>
    public partial class FullscreenVideoWindow : Window
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly TimeSpan _startPosition;
        private readonly bool _startPlaying;
        private readonly bool _shouldLoop;
        private bool _hasAppliedStartPosition;
        private bool _isDragging;
        private bool _isMuted;
        private double _volumeBeforeMute;
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// The playback position at the time the window was closed.
        /// </summary>
        public TimeSpan ExitPosition { get; private set; }

        /// <summary>
        /// Whether the video was actively playing when the window was closed.
        /// </summary>
        public bool WasPlaying { get; private set; }

        /// <summary>
        /// The volume level at the time the window was closed.
        /// </summary>
        public double ExitVolume { get; private set; }

        /// <summary>
        /// Whether the player was muted when the window was closed.
        /// </summary>
        public bool ExitMuted { get; private set; }

        /// <summary>
        /// Creates and initializes the fullscreen video window.
        /// </summary>
        /// <param name="source">Video file URI to play.</param>
        /// <param name="startPosition">Position to seek to after media opens.</param>
        /// <param name="volume">Volume level (0.0 to 1.0).</param>
        /// <param name="startPlaying">Whether to begin playback immediately.</param>
        /// <param name="shouldLoop">Whether the video should loop on completion.</param>
        /// <param name="isMuted">Whether the player should start muted.</param>
        public FullscreenVideoWindow(Uri source, TimeSpan startPosition, double volume, bool startPlaying, bool shouldLoop, bool isMuted)
        {
            InitializeComponent();

            _startPosition = startPosition;
            _startPlaying = startPlaying;
            _shouldLoop = shouldLoop;
            _hasAppliedStartPosition = false;
            _isDragging = false;
            _isMuted = isMuted;
            _volumeBeforeMute = volume;

            // Set up the volume slider and player volume
            VolumeSlider.Value = Math.Sqrt(volume); // Convert quadratic to linear for slider
            if (_isMuted)
            {
                fsPlayer.Volume = 0;
                MuteIcon.Text = "\uE74F"; // Muted icon
            }
            else
            {
                fsPlayer.Volume = volume;
                MuteIcon.Text = "\uE767"; // Unmuted icon
            }

            // Set up the timeline update timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += Timer_Tick;

            try
            {
                fsPlayer.Source = source;

                if (_startPlaying)
                {
                    fsPlayer.Play();
                    WasPlaying = true;
                    PlayPauseIcon.Text = "\uE769"; // Pause icon
                    _timer.Start();
                }
                else
                {
                    // FIX: Explicitly enter Paused state so WPF renders the initial frame
                    // instead of a black screen. (Requires ScrubbingEnabled="True" in XAML)
                    fsPlayer.Pause();
                    PlayPauseIcon.Text = "\uE768"; // Play icon
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize fullscreen video source.");
                ExitPosition = startPosition;
                WasPlaying = false;
                ExitVolume = volume;
                ExitMuted = isMuted;
                Close();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isDragging)
            {
                TimelineSlider.Value = fsPlayer.Position.TotalSeconds;
            }

            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            var current = fsPlayer.Position.ToString(@"mm\:ss") ?? "00:00";
            var total = fsPlayer.NaturalDuration.HasTimeSpan
                ? fsPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss")
                : "00:00";
            TimeDisplay.Text = $"{current} / {total}";
        }

        private void FsPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Seek to the start position once the media is loaded.
            if (!_hasAppliedStartPosition)
            {
                _hasAppliedStartPosition = true;
                fsPlayer.Position = _startPosition;
            }

            // Configure the timeline slider range
            if (fsPlayer.NaturalDuration.HasTimeSpan)
            {
                var ts = fsPlayer.NaturalDuration.TimeSpan;
                TimelineSlider.Maximum = ts.TotalSeconds;
                TimelineSlider.SmallChange = 0.25;
                TimelineSlider.LargeChange = Math.Min(10, ts.TotalSeconds / 10);
            }

            UpdateTimeDisplay();
        }

        private void FsPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (_shouldLoop)
            {
                fsPlayer.Position = TimeSpan.Zero;
                fsPlayer.Play();
            }
            else
            {
                WasPlaying = false;
                PlayPauseIcon.Text = "\uE768"; // Play icon
                _timer.Stop();
            }
        }

        // ── Play/Pause ──────────────────────────────────────────

        private void TogglePlayPause()
        {
            if (WasPlaying)
            {
                fsPlayer.Pause();
                WasPlaying = false;
                PlayPauseIcon.Text = "\uE768"; // Play icon
                _timer.Stop();
            }
            else
            {
                var currentPos = fsPlayer.Position;
                fsPlayer.Play();
                
                // FIX: WPF MediaElement may reset the internal stream to 00:00 when Play() 
                // is called for the first time after it was loaded in a Paused state.
                // Reapplying the previously known valid position immediately after calling Play() prevents this jump.
                if (currentPos != TimeSpan.Zero)
                {
                    fsPlayer.Position = currentPos;
                }

                WasPlaying = true;
                PlayPauseIcon.Text = "\uE769"; // Pause icon
                _timer.Start();
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePlayPause();
        }

        // ── Mute ────────────────────────────────────────────────

        private void ToggleMute()
        {
            _isMuted = !_isMuted;
            if (_isMuted)
            {
                _volumeBeforeMute = fsPlayer.Volume;
                fsPlayer.Volume = 0;
                MuteIcon.Text = "\uE74F"; // Muted icon
            }
            else
            {
                fsPlayer.Volume = _volumeBeforeMute;
                MuteIcon.Text = "\uE767"; // Unmuted icon
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMute();
        }

        // ── Volume Slider ───────────────────────────────────────

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Convert linear slider value to quadratic for perceptual volume
            var linearValue = VolumeSlider.Value;
            var quadraticVolume = linearValue * linearValue;

            if (!_isMuted)
            {
                fsPlayer.Volume = quadraticVolume;
            }

            _volumeBeforeMute = quadraticVolume;
        }

        // ── Timeline Slider ─────────────────────────────────────

        private void TimelineSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void TimelineSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDragging = false;
            fsPlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value);
        }

        private void TimelineSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging)
            {
                var delta = e.GetPosition(TimelineSlider).X / TimelineSlider.ActualWidth;
                if (fsPlayer.NaturalDuration.HasTimeSpan)
                {
                    fsPlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Maximum * delta);
                }
            }
        }

        // ── Keyboard & Mouse ────────────────────────────────────

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseFullscreen();
            }
            else if (e.Key == Key.Space)
            {
                TogglePlayPause();
                e.Handled = true;
            }
            else if (e.Key == Key.M)
            {
                ToggleMute();
                e.Handled = true;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                TogglePlayPause();
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CloseFullscreen();
        }

        /// <summary>
        /// Prevents clicks on the control bar from bubbling up
        /// to the window and triggering play/pause toggle.
        /// </summary>
        private void ControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        // ── Exit ────────────────────────────────────────────────

        private void ExitButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            CloseFullscreen();
        }

        private void CloseFullscreen()
        {
            _timer.Stop();

            try
            {
                ExitPosition = fsPlayer.Position;
                // Capture the actual volume (not muted value)
                ExitVolume = _volumeBeforeMute;
                ExitMuted = _isMuted;
                fsPlayer.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error capturing fullscreen exit state.");
                ExitPosition = TimeSpan.Zero;
                WasPlaying = false;
                ExitVolume = 0.5;
                ExitMuted = false;
            }

            Close();
        }
    }
}
