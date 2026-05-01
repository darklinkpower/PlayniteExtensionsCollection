using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using SteamCommon;
using SteamShortcuts.Application;
using SteamShortcuts.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SteamShortcuts.Presentation
{
    /// <summary>
    /// Interaction logic for SteamGameActionsLauncher.xaml
    /// </summary>
    public partial class SteamGameActionsLauncher : PluginUserControlBase
    {
        private readonly ILogger _logger;
        private readonly DesktopView _activeViewAtCreation;

        public SteamShortcutsSettingsViewModel SettingsModel { get; }

        private readonly SteamUriLauncherService _launcher;
        private string _steamId = string.Empty;
        private bool _areBindingValuesDefault = true;

        public SteamGameActionsLauncher(
            SteamUriLauncherService steamUriLauncherService,
            SteamShortcutsSettingsViewModel settingsViewModel,
            IPlayniteAPI playniteApi,
            ILogger logger) : base(playniteApi)
        {
            InitializeComponent();
            _launcher = steamUriLauncherService ?? throw new ArgumentNullException(nameof(steamUriLauncherService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SettingsModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
            DataContext = this;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (!_areBindingValuesDefault)
            {
                ResetBindingValues();
            }

            //The GameContextChanged method is raised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                _activeViewAtCreation != PlayniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (newContext is null)
            {
                return;
            }

            var foundSteamId = Steam.GetGameSteamId(newContext, false, true);
            if (foundSteamId.IsNullOrEmpty())
            {
                return;
            }
            
            _steamId = foundSteamId;
            Visibility = Visibility.Visible;
            SettingsModel.Settings.IsControlVisible = true;
            _areBindingValuesDefault = false;
        }

        private void ResetBindingValues()
        {
            _steamId = string.Empty;
            Visibility = Visibility.Collapsed;
            SettingsModel.Settings.IsControlVisible = false;
            _areBindingValuesDefault = true;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            PART_ContextMenu.PlacementTarget = PART_Button;
            PART_ContextMenu.IsOpen = true;
        }

        #region Steam Client

        private void OnViewDetails(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamClientUri(SteamClientGameUriType.Details, _steamId);

        private void OnGameProperties(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamClientUri(SteamClientGameUriType.GameProperties, _steamId);

        private void OnControllerConfig(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamClientUri(SteamClientGameUriType.SteamInput, _steamId);

        #endregion

        #region Web

        private void OnStorePage(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamWebUrl(SteamUrlType.StorePage, _steamId);

        private void OnCommunityHub(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamWebUrl(SteamUrlType.CommunityHub, _steamId);

        private void OnDiscussions(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamWebUrl(SteamUrlType.Discussions, _steamId);

        private void OnGuides(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamWebUrl(SteamUrlType.Guides, _steamId);

        private void OnAchievements(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamWebUrl(SteamUrlType.Achievements, _steamId);

        private void OnNews(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamWebUrl(SteamUrlType.News, _steamId);

        private void OnPointsShop(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamWebUrl(SteamUrlType.PointsShop, _steamId);

        #endregion

        #region Global Steam

        private void OnDownloads(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamComponent(SteamComponentType.Downloads);

        private void OnFriends(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamComponent(SteamComponentType.Friends);

        private void OnScreenshots(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamComponent(SteamComponentType.Screenshots);

        private void OnSettings(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamComponent(SteamComponentType.Settings);

        private void OnActivateProduct(object sender, RoutedEventArgs e)
            => _launcher.LaunchSteamComponent(SteamComponentType.ActivateProduct);

        #endregion


    }
}
