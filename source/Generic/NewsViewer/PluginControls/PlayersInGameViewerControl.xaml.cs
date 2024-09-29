using NewsViewer.Models;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using FlowHttp;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using System.Collections.Concurrent;
using TemporaryCache;
using TemporaryCache.Models;

namespace NewsViewer.PluginControls
{
    /// <summary>
    /// Interaction logic for PlayersInGameViewerControl.xaml
    /// </summary>
    public partial class PlayersInGameViewerControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        IPlayniteAPI PlayniteApi;
        private readonly CacheManager<Guid, NumberOfPlayersResponse> playersCountCacheManager;
        private readonly DispatcherTimer updateControlDataDelayTimer;
        private static readonly ILogger logger = LogManager.GetLogger();
        private static string steamApiGetCurrentPlayersMask = @"https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid={0}";
        public NewsViewerSettingsViewModel SettingsModel { get; set; }
        public DesktopView ActiveViewAtCreation { get; }
        private Game currentGame;
        private Guid currentGameId = Guid.Empty;

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
        
        private string steamId = null;
        private int inGamePlayersCount = 0;

        public int InGamePlayersCount
        {
            get => inGamePlayersCount;
            set
            {
                inGamePlayersCount = value;
                OnPropertyChanged();
            }
        }

        public PlayersInGameViewerControl(IPlayniteAPI PlayniteApi, NewsViewerSettingsViewModel settings, CacheManager<Guid, NumberOfPlayersResponse> playersCountCacheManager)
        {
            InitializeComponent();
            this.PlayniteApi = PlayniteApi;
            this.playersCountCacheManager = playersCountCacheManager;
            SettingsModel = settings;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }

            DataContext = this;

            updateControlDataDelayTimer = new DispatcherTimer();
            updateControlDataDelayTimer.Interval = TimeSpan.FromMilliseconds(700);
            updateControlDataDelayTimer.Tick += new EventHandler(UpdateInGameCountAsync);
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
                updateControlDataDelayTimer.Stop();
                return;
            }

            currentGame = newContext;
            ControlVisibility = Visibility.Collapsed;
            InGamePlayersCount = 0;
            steamId = null;
            SettingsModel.Settings.PlayersCountAvailable = false;

            if (currentGame is null || !SettingsModel.Settings.EnablePlayersCountControl)
            {
                currentGameId = Guid.Empty;
                updateControlDataDelayTimer.Stop();
            }
            else
            {
                currentGameId = currentGame.Id;
                updateControlDataDelayTimer.Stop();

                if (playersCountCacheManager.TryGetValue(currentGame.Id, out var cache))
                {
                    UpdatePlayersCount(cache);
                }
                else
                {
                    updateControlDataDelayTimer.Start();
                }
            }
        }

        private async void UpdateInGameCountAsync(object sender, EventArgs e)
        {
            updateControlDataDelayTimer.Stop();
            await UpdateControlAsync();
        }

        private async Task UpdateControlAsync()
        {
            if (currentGame is null)
            {
                return;
            }

            steamId = Steam.GetGameSteamId(currentGame, SettingsModel.Settings.ShowSteamPlayersNonSteam);
            if (steamId.IsNullOrEmpty())
            {
                return;
            }

            var contextGameId = currentGame.Id;
            currentGameId = contextGameId;
            var url = string.Format(steamApiGetCurrentPlayersMask, steamId);
            var downloadStringResult = await HttpRequestFactory.GetHttpRequest().WithUrl(url).DownloadStringAsync();
            if (!downloadStringResult.IsSuccess)
            {
                return;
            }

            if (Serialization.TryFromJson<NumberOfPlayersResponse>(downloadStringResult.Content, out var data))
            {
                if (data.Response.Result != 1)
                {
                    return;
                }

                var savedCache = playersCountCacheManager.Add(contextGameId, data);
                // To detect if game changed while downloading data
                if (currentGameId != null && contextGameId == currentGameId)
                {
                    UpdatePlayersCount(savedCache);
                }
            }
        }

        private void UpdatePlayersCount(NumberOfPlayersResponse cacheItem)
        {
            InGamePlayersCount = cacheItem.Response.PlayerCount;
            ControlVisibility = Visibility.Visible;
            SettingsModel.Settings.PlayersCountAvailable = true;
        }

        private void OpenSteamDbGraphs()
        {
            if (steamId.IsNullOrEmpty())
            {
                return;
            }

            var url = string.Format(@"https://steamdb.info/app/{0}/graphs/", steamId);
            ProcessStarter.StartUrl(url);
        }

        public RelayCommand OpenSteamDbGraphsCommand
        {
            get => new RelayCommand(() =>
            {
                OpenSteamDbGraphs();
            });
        }
    }
}