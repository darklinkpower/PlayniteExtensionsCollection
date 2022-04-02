using Microsoft.Toolkit.Uwp.Notifications;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayState.Enums;
using PlayState.Models;
using PlayState.ViewModels;
using PlayState.Views;
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
        private readonly IPlayniteAPI playniteApi;
        private PlayStateSettingsViewModel settings;
        private Window currentSplashWindow;
        private readonly PlayStateManagerViewModel playStateManager;
        private readonly bool isWindows10Or11;
        private readonly SplashWindowViewModel splashWindowViewModel;
        private readonly DispatcherTimer timer;

        public MessagesHandler(IPlayniteAPI playniteApi, PlayStateSettingsViewModel playStateSettings, PlayStateManagerViewModel playStateManager, bool isWindows10Or11)
        {
            this.playniteApi = playniteApi;
            this.settings = playStateSettings;
            this.playStateManager = playStateManager;
            this.isWindows10Or11 = isWindows10Or11;
            splashWindowViewModel = new SplashWindowViewModel();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += (src, args) =>
            {
                timer.Stop();
                if (currentSplashWindow != null)
                {
                    currentSplashWindow.Hide();
                    currentSplashWindow.Topmost = false;
                }
            };
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
        public void ShowNotification(NotificationTypes status, Game game)
        {
            var gameData = playStateManager.GetCurrentGameData();
            if (gameData == null)
            {
                return;
            }

            var sb = new StringBuilder();
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
                    sb.Append($"{ResourceProvider.GetString("LOCPlayState_StatusInformationMessage")} ");
                    if (gameData.IsSuspended)
                    {
                        if (gameData.ProcessesSuspended)
                        {
                            sb.Append(ResourceProvider.GetString("LOCPlayState_StatusSuspendedMessage"));
                        }
                        else
                        {
                            sb.Append(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeSuspendedMessage"));
                        }
                    }
                    else
                    {
                        if (gameData.ProcessesSuspended)
                        {
                            sb.Append(ResourceProvider.GetString("LOCPlayState_StatusResumedMessage"));
                        }
                        else
                        {
                            sb.Append(ResourceProvider.GetString("LOCPlayState_StatusPlaytimeResumedMessage"));
                        }
                    }
                    break;
                default:
                    break;
            }

            if (settings.Settings.NotificationShowSessionPlaytime)
            {
                sb.Append($"\n{ResourceProvider.GetString("LOCPlayState_Playtime")} {GetHoursString(GetRealPlaytime(gameData))}");
            }
            if (settings.Settings.NotificationShowTotalPlaytime)
            {
                sb.Append($"\n{ResourceProvider.GetString("LOCPlayState_TotalPlaytime")} {GetHoursString(GetRealPlaytime(gameData) + game.Playtime)}");
            }
            var notificationMessage = sb.ToString();

            if (settings.Settings.GlobalShowWindowsNotificationsStyle && isWindows10Or11)
            {
                new ToastContentBuilder()
                    .AddText(game.Name) // First AddText field will act as a title
                    .AddText(notificationMessage)
                    .AddHeroImage(new Uri(playniteApi.Database.GetFullFilePath(game.BackgroundImage))) // Show game image in the notification
                    .Show();
            }
            else
            {
                ShowSplashWindow(game.Name, notificationMessage);
            }
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
