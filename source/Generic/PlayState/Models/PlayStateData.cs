using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PlayState.Enums;
using PlayState.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PlayState.Models
{
    public class PlayStateData : ObservableObject, IDisposable
    {
        // Constants
        private const string _featureSuspendPlaytime = "[PlayState] Suspend Playtime only";
        private const string _featureSuspendProcesses = "[PlayState] Suspend Processes";

        // Fields
        private readonly PlayStateSettingsViewModel _settingsModel;
        private readonly Stopwatch _suspendTimeStopwatch = new Stopwatch();
        private readonly Stopwatch _systemSuspendTimeToDeductSw = new Stopwatch();
        private readonly Game _game;
        private bool _isSuspended = false;
        private SuspendModes _suspendMode;
        private PlayStateAutomaticStateSwitchStatus _gameStatusOverride;
        private static readonly ILogger _logger = LogManager.GetLogger();

        // Properties
        public double SuspendedTime => CalculateEffectiveSuspendTime();
        public bool HasBeenInForeground { get; set; } = false;

        public bool IsGameStatusOverrided { get; set; } = false;
        public Game Game => _game;

        public DateTime StartDate { get; } = DateTime.Now;

        public List<ProcessItem> GameProcesses { get; set; } = new List<ProcessItem>();

        public bool HasProcesses => GameProcesses?.HasItems() == true;

        public bool IsSuspended
        {
            get => _isSuspended;
            set => SetValue(ref _isSuspended, value);
        }

        public SuspendModes SuspendMode
        {
            get => _suspendMode;
            set => SetValue(ref _suspendMode, value);
        }

        public PlayStateAutomaticStateSwitchStatus GameStatusOverride
        {
            get => _gameStatusOverride;
            set => SetValue(ref _gameStatusOverride, value);
        }

        public PlayStateData(Game game, List<ProcessItem> gameProcesses, PlayStateSettingsViewModel settings)
        {
            _game = game;
            GameProcesses = gameProcesses;
            _settingsModel = settings;
            SetSuspendMode();

            Game.PropertyChanged += Game_PropertyChanged;
            _settingsModel.Settings.PropertyChanged += Settings_PropertyChanged;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            _logger.Debug($"OnPowerModeChanged callback with mode \"{e.Mode}\"'");
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    if (_suspendTimeStopwatch.IsRunning)
                    {
                        _systemSuspendTimeToDeductSw.Start();
                    }
                    break;
                case PowerModes.Resume:
                    if (_systemSuspendTimeToDeductSw.IsRunning)
                    {
                        _systemSuspendTimeToDeductSw.Stop();
                    }
                    break;
                case PowerModes.StatusChange:
                    break;
                default:
                    break;
            }
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
            if (e.PropertyName == nameof(_settingsModel.Settings.GlobalSuspendMode))
            {
                SetSuspendMode();
            }
        }
        
        private double CalculateEffectiveSuspendTime()
        {
            var suspendTime = _suspendTimeStopwatch.Elapsed.TotalSeconds;
            var deductTime = _systemSuspendTimeToDeductSw.Elapsed.TotalSeconds;
            var calculatedSuspendTime = _suspendTimeStopwatch.Elapsed.TotalSeconds - _systemSuspendTimeToDeductSw.Elapsed.TotalSeconds;
            _logger.Debug($"Calculated effective suspend time: {calculatedSuspendTime}. Suspend time: {suspendTime}, System deduct time: {deductTime}");
            return calculatedSuspendTime;
        }

        private void SetSuspendMode()
        {
            if (!HasProcesses || PlayniteUtilities.GetGameHasFeature(_game, _featureSuspendPlaytime, true))
            {
                _suspendMode = SuspendModes.Playtime;
                return;
            }
            else if (PlayniteUtilities.GetGameHasFeature(_game, _featureSuspendProcesses, true))
            {
                _suspendMode = SuspendModes.Processes;
                return;
            }

            _suspendMode = _settingsModel.Settings.GlobalSuspendMode;
        }

        public void SetProcesses(List<ProcessItem> gameProcesses)
        {
            GameProcesses = gameProcesses;
        }

        public StateActions SwitchState()
        {
            if (HasProcesses && SuspendMode == SuspendModes.Processes)
            {
                if (IsSuspended)
                {
                    return ResumeProcesses();
                }
                else
                {
                    return SuspendProcesses();
                }
            }
            else if (SuspendMode == SuspendModes.Playtime)
            {
                if (IsSuspended)
                {
                    return StopSuspendTimeCounter();
                }
                else
                {
                    return StartSuspendTimeCounter();
                }
            }

            return StateActions.None;
        }

        public StateActions SuspendProcesses()
        {
            foreach (var gameProcess in GameProcesses)
            {
                if (gameProcess?.Process is null)
                {
                    continue;
                }

                try
                {
                    if (!gameProcess.Process.HasExited)
                    {
                        var handle = gameProcess.Process.Handle;
                        Ntdll.NtSuspendProcess(handle);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Error(ex, "Error on process suspend");
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error on process suspend");
                    continue;
                }
            }

            IsSuspended = true;
            _suspendTimeStopwatch.Start();
            return StateActions.Suspended;
        }

        public StateActions ResumeProcesses()
        {
            foreach (var gameProcess in GameProcesses)
            {
                if (gameProcess?.Process is null)
                {
                    continue;
                }

                try
                {
                    if (!gameProcess.Process.HasExited)
                    {
                        var handle = gameProcess.Process.Handle;
                        Ntdll.NtResumeProcess(handle);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Error(ex, "Error on process resume");
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error on process resume");
                    continue;
                }
            }

            IsSuspended = false;
            _suspendTimeStopwatch.Stop();
            return StateActions.Resumed;
        }

        public StateActions StopSuspendTimeCounter()
        {
            
            _suspendTimeStopwatch.Stop();
            IsSuspended = false;
            return StateActions.PlaytimeResumed;
        }

        public StateActions StartSuspendTimeCounter()
        {
            _suspendTimeStopwatch.Start();
            IsSuspended = true;
            return StateActions.PlaytimeSuspended;
        }

        internal void RemoveProcesses()
        {
            if (GameProcesses.HasItems())
            {
                GameProcesses = null;
            }
        }

        public ProcessItem GetProcessByWindowHandle(IntPtr handle)
        {
            if (!GameProcesses.HasItems())
            {
                return null;
            }

            foreach (var processItem in GameProcesses)
            {
                if (processItem?.Process is null)
                {
                    continue;
                }

                try
                {
                    if (!processItem.Process.HasExited &&
                        processItem.Process.MainWindowHandle == handle)
                    {
                        return processItem;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Warn(ex, $"GetProcessByWindowHandle: Failed to access process {processItem.Process?.Id}.");
                    continue;
                }
            }

            return null;
        }

        public void BringToForeground()
        {
            if (!GameProcesses.HasItems())
            {
                _logger.Debug("BringToForeground: No game processes.");
                return;
            }

            if (IsSuspended)
            {
                _logger.Debug("BringToForeground: Skipped because game is suspended.");
                return;
            }

            if (IsWindowInForeground())
            {
                _logger.Debug("BringToForeground: Already in foreground.");
                return;
            }

            IntPtr? windowHandle = null;
            foreach (var processItem in GameProcesses)
            {
                if (processItem?.Process is null)
                {
                    continue;
                }

                try
                {
                    if (!processItem.Process.HasExited)
                    {
                        var handle = processItem.Process.MainWindowHandle;
                        if (handle != IntPtr.Zero)
                        {
                            windowHandle = handle;
                            break;
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Warn(ex, $"BringToForeground: Process {processItem.Process?.Id} may have exited.");
                }
            }

            if (windowHandle == null)
            {
                _logger.Warn($"BringToForeground: No valid window handle found for game {Game.Name}.");
                return;
            }

            try
            {
                WindowsHelper.RestoreAndFocusWindow(windowHandle.Value);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"BringToForeground: Error while restoring game window {windowHandle} for game {Game.Name}.");
            }
        }

        public void MinimizeWindows()
        {
            if (!GameProcesses.HasItems() || (_suspendMode == SuspendModes.Processes && _isSuspended))
            {
                return;
            }

            List<IntPtr> windowHandles = GetWindowHandles();
            foreach (var windowHandle in windowHandles)
            {
                try
                {
                    if (!WindowsHelper.IsWindowMinimized(windowHandle))
                    {
                        WindowsHelper.MinimizeWindow(windowHandle);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error while minimizing window of {Game.Name}, handlle {windowHandle}");
                }
            }
        }

        public bool IsWindowInForeground()
        {
            if (!GameProcesses.HasItems() || IsSuspended)
            {
                return false;
            }

            var foregroundWindowHandle = WindowsHelper.GetForegroundWindowHandle();
            foreach (var processItem in GameProcesses)
            {
                if (processItem?.Process is null)
                {
                    continue;
                }

                try
                {
                    if (!processItem.Process.HasExited &&
                        processItem.Process.MainWindowHandle == foregroundWindowHandle)
                    {
                        return true;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Error(ex, "IsWindowInForeground: Failed to check process. It may have exited.");
                    continue;
                }
            }

            return false;
        }

        public bool IsWindowMinimized()
        {
            List<IntPtr> windowHandles = GetWindowHandles();
            var anyWindowMinimized = windowHandles.Any(x => WindowsHelper.IsWindowMinimized(x));
            return anyWindowMinimized;
        }

        private List<IntPtr> GetWindowHandles()
        {
            var windowHandles = new List<IntPtr>();
            foreach (var gameProcess in GameProcesses)
            {
                if (gameProcess?.Process is null)
                {
                    continue;
                }

                try
                {
                    if (!gameProcess.Process.HasExited && gameProcess.Process.MainWindowHandle != IntPtr.Zero)
                    {
                        windowHandles.AddMissing(gameProcess.Process.MainWindowHandle);
                    }
                }
                catch (InvalidOperationException e)
                {
                    // Process has likely exited between checks
                    _logger.Error(e, $"Error during while obtaining Handles of {nameof(GameProcesses)}");
                    continue;
                }
            }

            return windowHandles;
        }

        public void Dispose()
        {
            Game.PropertyChanged -= Game_PropertyChanged;
            _settingsModel.Settings.PropertyChanged -= Settings_PropertyChanged;
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        }

    }
}