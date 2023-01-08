using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PlayState.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PlayState.Models
{
    public class PlayStateData : ObservableObject, IDisposable
    {
        private const string featureSuspendPlaytime = "[PlayState] Suspend Playtime only";
        private const string featureSuspendProcesses = "[PlayState] Suspend Processes";
        private readonly PlayStateSettingsViewModel settingsModel;
        private Game game;
        public Game Game { get => game; set => SetValue(ref game, value); }
        public DateTime StartDate { get; } = DateTime.Now;
        private Stopwatch stopwatch = new Stopwatch();
        public Stopwatch Stopwatch { get => stopwatch; set => SetValue(ref stopwatch, value); }
        private DispatcherTimer suspensionReminderTimer = new DispatcherTimer();
        public DispatcherTimer SuspensionReminderTimer { get => suspensionReminderTimer; set => SetValue(ref suspensionReminderTimer, value); }
        private DispatcherTimer playingReminderTimer = new DispatcherTimer();
        public DispatcherTimer PlayingReminderTimer { get => playingReminderTimer; set => SetValue(ref playingReminderTimer, value); }
        public List<ProcessItem> GameProcesses { get; set; }
        public bool HasProcesses => GameProcesses?.HasItems() == true;

        private bool isSuspended = false;
        public bool IsSuspended { get => isSuspended; set => SetValue(ref isSuspended, value); }

        private SuspendModes suspendMode;
        public SuspendModes SuspendMode { get => suspendMode; set => SetValue(ref suspendMode, value); }
        public bool HasBeenInForeground = false;
        public bool IsGameStatusOverrided = false;
        private PlayStateAutomaticStateSwitchStatus gameStatusOverride;
        public PlayStateAutomaticStateSwitchStatus GameStatusOverride { get => gameStatusOverride; set => SetValue(ref gameStatusOverride, value); }

        public PlayStateData(Game game, List<ProcessItem> gameProcesses, PlayStateSettingsViewModel settings)
        {
            Game = game;
            GameProcesses = gameProcesses;
            settingsModel = settings;
            SetSuspendMode();

            Game.PropertyChanged += Game_PropertyChanged;
            settingsModel.Settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Game.FeatureIds))
            {
                SetSuspendMode();
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(settingsModel.Settings.GlobalSuspendMode))
            {
                SetSuspendMode();
            }
            if (e.PropertyName == nameof(settingsModel.Settings.NotificationPlayingReminderMinutes))
            {
                PlayingReminderTimer.Interval = TimeSpan.FromMinutes(settingsModel.Settings.NotificationPlayingReminderMinutes);
            }
            if (e.PropertyName == nameof(settingsModel.Settings.NotificationSuspensionReminderMinutes))
            {
                SuspensionReminderTimer.Interval = TimeSpan.FromMinutes(settingsModel.Settings.NotificationSuspensionReminderMinutes);
            }
        }

        private void SetSuspendMode()
        {
            if (!HasProcesses || PlayniteUtilities.GetGameHasFeature(game, featureSuspendPlaytime, true))
            {
                suspendMode = SuspendModes.Playtime;
                return;
            }
            else if (PlayniteUtilities.GetGameHasFeature(game, featureSuspendProcesses, true))
            {
                suspendMode = SuspendModes.Processes;
                return;
            }

            suspendMode = settingsModel.Settings.GlobalSuspendMode;
        }

        public void Dispose()
        {
            Game.PropertyChanged -= Game_PropertyChanged;
            settingsModel.Settings.PropertyChanged -= Settings_PropertyChanged;
            PlayingReminderTimer.Stop();
            SuspensionReminderTimer.Stop();
        }

        public void SetProcesses(List<ProcessItem> gameProcesses)
        {
            GameProcesses = gameProcesses;
        }

        internal void RemoveProcesses()
        {
            if (HasProcesses)
            {
                GameProcesses = null;
            }
        }

        public void StartPlayingReminderTimer()
        {
            PlayingReminderTimer = new DispatcherTimer();
            PlayingReminderTimer.Interval = TimeSpan.FromMinutes(settingsModel.Settings.NotificationPlayingReminderMinutes);
            PlayingReminderTimer.Start();
        }

        public void StopPlayingReminderTimer()
        {
            PlayingReminderTimer.Stop();
        }
        public void StartSuspensionReminderTimer()
        {
            SuspensionReminderTimer = new DispatcherTimer();
            SuspensionReminderTimer.Interval = TimeSpan.FromMinutes(settingsModel.Settings.NotificationSuspensionReminderMinutes);
            SuspensionReminderTimer.Start();
        }

        public void StopSuspensionReminderTimer()
        {
            SuspensionReminderTimer.Stop();
        }
    }
}