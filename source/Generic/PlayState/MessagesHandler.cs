using Microsoft.Toolkit.Uwp.Notifications;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayState.Enums;
using PlayState.Models;
using PlayState.ViewModels;
using PlayState.Views;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PlayState
{
    public class MessagesHandler
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private readonly PlayStateSettingsViewModel settings;
        private Window currentSplashWindow;
        private readonly SplashWindowViewModel splashWindowViewModel;
        private readonly DispatcherTimer timer;

        public MessagesHandler(IPlayniteAPI playniteApi, PlayStateSettingsViewModel playStateSettings, PlayStateManagerViewModel playStateManagerViewModel)
        {
            this.playniteApi = playniteApi;
            settings = playStateSettings;
            splashWindowViewModel = new SplashWindowViewModel();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += (_, __) =>
            {
                timer.Stop();
                if (currentSplashWindow != null)
                {
                    currentSplashWindow.Hide();
                    currentSplashWindow.Topmost = false;
                }
            };

            playStateManagerViewModel.OnGameStatusSwitched += PlayStateManagerViewModel_OnGameStatusSwitched;
            playStateManagerViewModel.PlayStateDataCollection.CollectionChanged += PlayStateDataCollection_CollectionChanged;
        }

        private void PlayStateDataCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null || e.NewItems.Count == 0)
            {
                return;
            }

            foreach (PlayStateData newData in e.NewItems)
            {
                ShowGameStatusNotification(NotificationTypes.DataAdded, newData);
            }
        }

        private void PlayStateManagerViewModel_OnGameStatusSwitched(object sender, Events.OnGameStatusSwitchedArgs e)
        {
            ShowGameStatusNotification(e.NotificationType, e.PlayStateData);
        }

        /// <summary>
        /// Method for showing notifications. It will respect the style (Playnite / Windows) notification settings.<br/><br/>
        /// <param name="status">Status of the game to be notified:<br/>
        /// - "resumed" for resuming process and playtime<br/>
        /// - "playtimeResumed" for resuming playtime<br/>
        /// - "suspended" for suspend process and playtime<br/>
        /// - "playtimeSuspended" for suspend playtime<br/>
        /// - "information" for showing the actual status<br/>
        /// </param>
        /// </summary>
        public void ShowGameStatusNotification(NotificationTypes status, PlayStateData gameData)
        {
            // Used for cases where it makes sense to always show the notification,
            // like manually calling the information hotkey
            var ignoreEnableNotificationSetting = false;
            switch (status)
            {
                case NotificationTypes.DataAdded:
                    ignoreEnableNotificationSetting = true;
                    break;
                case NotificationTypes.Information:
                    ignoreEnableNotificationSetting = true;
                    break;
                default:
                    break;
            }

            if (!ignoreEnableNotificationSetting && !settings.Settings.EnableNotificationMessages)
            {
                return;
            }
            
            var sb = new StringBuilder();
            var canAddCurrentPlaytimeLine = true;
            switch (status)
            {
                case NotificationTypes.Resumed: // for resuming process and playtime
                    sb.Append($"{ResourceProvider.GetString("LOCPlayState_StatusActionMessage")} ");
                    sb.Append(ResourceProvider.GetString("LOCPlayState_StatusResumedMessage"));
                    break;
                case NotificationTypes.PlaytimeResumed: // for resuming playtime
                    sb.Append($"{ResourceProvider.GetString("LOCPlayState_StatusActionMessage")} ");
                    sb.Append(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeResumedMessage"));
                    break;
                case NotificationTypes.Suspended: // for suspend process and playtime
                    sb.Append($"{ResourceProvider.GetString("LOCPlayState_StatusActionMessage")} ");
                    sb.Append(ResourceProvider.GetString("LOCPlayState_StatusSuspendedMessage"));
                    break;
                case NotificationTypes.PlaytimeSuspended: // for suspend playtime
                    sb.Append($"{ResourceProvider.GetString("LOCPlayState_StatusActionMessage")} ");
                    sb.Append(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeSuspendedMessage"));
                    break;
                case NotificationTypes.Information:
                    sb.Append($"{ResourceProvider.GetString("LOCPlayState_Setting_SuspendModeLabel")} ");
                    if (gameData.SuspendMode == SuspendModes.Processes)
                    {
                        sb.AppendLine(ResourceProvider.GetString("LOCPlayState_SuspendModeProcesses"));
                    }
                    else
                    {
                        sb.AppendLine(ResourceProvider.GetString("LOCPlayState_SuspendModePlaytime"));
                    }

                    sb.Append($"{ResourceProvider.GetString("LOCPlayState_Setting_SuspendStatusLabel")} ");
                    if (gameData.IsSuspended)
                    {
                        sb.Append(ResourceProvider.GetString("LOCPlayState_Setting_SuspendStatusSuspendedLabel"));
                    }
                    else
                    {
                        sb.Append(ResourceProvider.GetString("LOCPlayState_Setting_SuspendStatusNotSuspendedLabel"));
                    }
                    break;
                case NotificationTypes.DataAdded: // When game is added to PlayState data
                    sb.AppendLine(ResourceProvider.GetString("LOCPlayState_StatusPlayStateDataAddedMessage"));
                    sb.Append($"{ResourceProvider.GetString("LOCPlayState_Setting_SuspendModeLabel")} ");
                    if (gameData.SuspendMode == SuspendModes.Processes)
                    {
                        sb.Append(ResourceProvider.GetString("LOCPlayState_SuspendModeProcesses"));
                    }
                    else
                    {
                        sb.Append(ResourceProvider.GetString("LOCPlayState_SuspendModePlaytime"));
                    }

                    canAddCurrentPlaytimeLine = false;
                    break;
                default:
                    break;
            }

            if (canAddCurrentPlaytimeLine && settings.Settings.NotificationShowSessionPlaytime)
            {
                sb.Append($"\n{ResourceProvider.GetString("LOCPlayState_SessionPlaytimeLabel")} {GetHoursString(GetRealPlaytime(gameData))}");
            }
            if (settings.Settings.NotificationShowTotalPlaytime)
            {
                sb.Append($"\n{ResourceProvider.GetString("LOCPlayState_TotalPlaytime")} {GetHoursString(GetRealPlaytime(gameData) + gameData.Game.Playtime)}");
            }

            var notificationMessage = sb.ToString();
            if (settings.Settings.NotificationStyle == NotificationStyles.Toast && settings.IsWindows10Or11)
            {
                var toastNotification = new ToastContentBuilder()
                    .AddText(gameData.Game.Name) // First AddText field will act as a title
                    .AddText(notificationMessage);

                if (!gameData.Game.BackgroundImage.IsNullOrEmpty())
                {
                    var backgroundImage = playniteApi.Database.GetFullFilePath(gameData.Game.BackgroundImage);
                    if (!backgroundImage.IsNullOrEmpty() && FileSystem.FileExists(backgroundImage))
                    {
                        toastNotification.AddHeroImage(new Uri(backgroundImage));
                    }
                }

                try
                {
                    toastNotification.Show();
                }
                catch (Exception e)
                {
                    // There was a report of someones PC failing to display the notification
                    // for an unknown reason. Possibly caused by borked system.
                    logger.Error(e, "Failed to display toast notification");
                }
            }
            else
            {
                ShowSplashWindow(gameData.Game.Name, notificationMessage);
            }
        }

        public void ShowGenericNotification(string message)
        {
            new ToastContentBuilder()
            .AddText(message)
            .Show();
        }

        public void HideWindow()
        {
            if (currentSplashWindow != null)
            {
                currentSplashWindow.Hide();
                currentSplashWindow.Topmost = false;
            }
        }

        private void CreateSplashWindow()
        {
            currentSplashWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = false,
                ShowActivated = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInTaskbar = false,
                Focusable = false,
                Content = new SplashWindow(),
                DataContext = splashWindowViewModel
            };

            currentSplashWindow.Closed += WindowClosed;
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            currentSplashWindow.Topmost = false;
            currentSplashWindow.Closed -= WindowClosed;
        }

        private void ShowSplashWindow(string gameName, string notificationMessage)
        {
            if (currentSplashWindow == null)
            {
                CreateSplashWindow();
            }

            splashWindowViewModel.GameName = gameName;
            splashWindowViewModel.NotificationMessage = notificationMessage;
            currentSplashWindow.Topmost = true;
            currentSplashWindow.Show();
            timer.Start();
        }

        /// <summary>
        /// Method for obtaining the real playtime of the actual session, which is the playtime after substracting the paused time.
        /// </summary>
        private ulong GetRealPlaytime(PlayStateData gameData)
        {
            var suspendedTime = gameData.Stopwatch.Elapsed;
            ulong elapsedSeconds = 0;
            if (suspendedTime != null)
            {
                elapsedSeconds = Convert.ToUInt64(suspendedTime.TotalSeconds);
            }

            return Convert.ToUInt64(DateTime.Now.Subtract(gameData.StartDate).TotalSeconds) - elapsedSeconds;
        }

        /// <summary>
        /// Method for obtaining the pertinent "{0} hours {1} minutes" string from playtime in seconds.<br/><br/>
        /// <param name="playtimeSeconds">Playtime in seconds</param>
        /// </summary>
        private string GetHoursString(ulong playtimeSeconds)
        {
            var playtime = TimeSpan.FromSeconds(playtimeSeconds);
            var playtimeHours = playtime.Hours + playtime.Days * 24;
            if (playtimeHours == 1)
            {
                if (playtime.Minutes == 1)
                {
                    return string.Format(ResourceProvider.GetString("LOCPlayState_HourMinutePlayed"), playtimeHours.ToString(), playtime.Minutes.ToString());
                }
                else
                {
                    return string.Format(ResourceProvider.GetString("LOCPlayState_HourMinutesPlayed"), playtimeHours.ToString(), playtime.Minutes.ToString());
                }
            }
            else if (playtimeHours == 0 && playtime.Minutes == 0) // If the playtime is less than a minute, show the seconds instead
            {
                if (playtime.Seconds == 1)
                {
                    return string.Format(ResourceProvider.GetString("LOCPlayState_SecondPlayed"), playtime.Seconds.ToString());
                }
                else
                {
                    return string.Format(ResourceProvider.GetString("LOCPlayState_SecondsPlayed"), playtime.Seconds.ToString());
                }
            }
            else
            {
                if (playtime.Minutes == 1)
                {
                    return string.Format(ResourceProvider.GetString("LOCPlayState_HoursMinutePlayed"), playtimeHours.ToString(), playtime.Minutes.ToString());
                }
                else
                {
                    return string.Format(ResourceProvider.GetString("LOCPlayState_HoursMinutesPlayed"), playtimeHours.ToString(), playtime.Minutes.ToString());
                }
            }
        }
    }
}