using ExtraMetadataLoader.Models;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using YouTubeCommon;

namespace ExtraMetadataLoader.Controls
{
    /// <summary>
    /// Interaction logic for YouTubeEmbeddedVideoControl.xaml
    /// </summary>
    public partial class YouTubeEmbeddedVideoControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly string pluginDataPath;
        IPlayniteAPI PlayniteApi;
        private Game currentGame;
        private string htmlSource = string.Empty;
        private readonly DispatcherTimer timer;
        public ExtraMetadataLoaderSettingsViewModel SettingsModel { get; set; }
        public DesktopView ActiveViewAtCreation { get; }

        public YouTubeEmbeddedVideoControl(string pluginDataPath, IPlayniteAPI PlayniteApi, ExtraMetadataLoaderSettingsViewModel settings)
        {
            InitializeComponent();
            this.pluginDataPath = pluginDataPath;
            this.PlayniteApi = PlayniteApi;
            SettingsModel = settings;
            DataContext = this;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(800);
            timer.Tick += new EventHandler(UpdateVideo);
            cefWebView.IsVisibleChanged += CefWebView_IsVisibleChanged;
        }

        private void CefWebView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;
            if (isVisible)
            {
                LoadCurrentHtmlSource();
            }
            else
            {
                LoadBlankPage();
            }
        }

        private void UpdateVideo(object sender, EventArgs e)
        {
            timer.Stop();

            var gameDataPath = Path.Combine(pluginDataPath, $"{currentGame.Id}_YouTubeControlSearchCache.json");
            if (!FileSystem.FileExists(gameDataPath))
            {
                var cacheObtained = SearchAndStoreSearchCache(currentGame, gameDataPath);
                if (cacheObtained.IsNullOrEmpty())
                {
                    return;
                }

                UpdateWebViewSource(cacheObtained);
                return;
            }

            var cache = Serialization.FromJsonFile<YouTubeControlSearchCache>(gameDataPath);
            UpdateWebViewSource(cache.VideoId);
        }

        private void UpdateWebViewSource(string videoId)
        {
            var youtubeLink = string.Format("https://www.youtube.com/embed/{0}", videoId);
            htmlSource = string.Format(@"data:text/html,
                    <head>
                        <title>Extra Metadata</title>
                        <meta http-equiv='refresh' content='0; url={0}'>
                    </head>
                    <body style='margin:0'>
                    </body>", youtubeLink);

            LoadCurrentHtmlSource();
        }

        private string SearchAndStoreSearchCache(Game currentGame, string gameDataPath)
        {
            var searchResults = YouTube.GetYoutubeSearchResults(GetYouTubeSearchTerm(currentGame), true);
            if (!searchResults.HasItems())
            {
                return null;
            }

            var cache = new YouTubeControlSearchCache { VideoId = searchResults[0].VideoId };
            FileSystem.WriteStringToFile(gameDataPath, Serialization.ToJson(cache));
            return cache.VideoId;
        }

        private string GetYouTubeSearchTerm(Game game)
        {
            var initialSearch = $"{game.Name} trailer";
            var platform = game.Platforms?.FirstOrDefault(x => x.Name != null);
            if (platform != null)
            {
                initialSearch = $"{game.Name} {platform.Name} trailer";
            }

            // Using PC only in the search provides better results
            return initialSearch.Replace("PC (Windows)", "PC");
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            timer.Stop();
            if (htmlSource != null)
            {
                htmlSource = null;
            }
            
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                ActiveViewAtCreation != PlayniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (newContext == null)
            {
                return;
            }

            currentGame = newContext;
            timer.Start();
        }

        private const string blankPageAddress = "data:text/html,";
        private void LoadBlankPage()
        {
            if (cefWebView.Address != blankPageAddress)
            {
                cefWebView.Load(blankPageAddress);
            }
        }

        private void LoadCurrentHtmlSource()
        {
            if (htmlSource == null)
            {
                cefWebView.Visibility = Visibility.Collapsed;
                return;
            }

            cefWebView.Load(htmlSource);
        }
    }
}