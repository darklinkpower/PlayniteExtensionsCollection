using HtmlAgilityPack;
using NewsViewer.Models;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SteamCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml;
using TemporaryCache;
using TemporaryCache.Models;
using FlowHttp;
using NewsViewer.Infrastructure;
using NewsViewer.Domain.ValueObjects;

namespace NewsViewer.PluginControls
{
    /// <summary>
    /// Interaction logic for NewsViewerControl.xaml
    /// </summary>
    public partial class NewsViewerControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IPlayniteAPI _playniteApi;
        private readonly SteamNewsService _steamNewsService;
        private static readonly ILogger logger = LogManager.GetLogger();
        public NewsViewerSettingsViewModel SettingsModel { get; set; }
        private readonly DispatcherTimer updateContextTimer;
        
        
        private readonly DesktopView ActiveViewAtCreation;
        private readonly CultureInfo _dateTimeConvertCulture;
        
        private IReadOnlyList<SteamNewsArticle> newsNodes = new List<SteamNewsArticle>();
        private int selectedNewsIndex;
        private bool multipleNewsAvailable;
        private Game currentGame;

        public string NewsTitle => CurrentNewsArticle?.Title ?? string.Empty;
        public string NewsDate => CurrentNewsArticle?.PublishedDate.ToString("ddd, MMMM d yyyy HH:mm", _dateTimeConvertCulture) ?? string.Empty;
        public string NewsText => CleanSteamNewsDescription(CurrentNewsArticle?.DescriptionHtmlFormatted ?? string.Empty);
        private SteamNewsArticle currentNewsNode;
        public SteamNewsArticle CurrentNewsArticle
        {
            get => currentNewsNode;
            set
            {
                currentNewsNode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NewsTitle));
                OnPropertyChanged(nameof(NewsDate));
                OnPropertyChanged(nameof(NewsText));
                Dispatcher.Invoke(() => NewsScrollViewer.ScrollToTop());
            }
        }

        private bool isControlVisible = true;
        public bool IsControlVisible
        {
            get => isControlVisible;
            set
            {
                isControlVisible = value;
                OnPropertyChanged();
            }
        }

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

        private Visibility switchNewsVisibility = Visibility.Collapsed;
        public Visibility SwitchNewsVisibility
        {
            get => switchNewsVisibility;
            set
            {
                switchNewsVisibility = value;
                OnPropertyChanged();
            }
        }

        public int SelectedNewsIndex
        {
            get => selectedNewsIndex;
            set
            {
                selectedNewsIndex = value;
                OnPropertyChanged();
                CurrentNewsArticle = newsNodes[selectedNewsIndex];
            }
        }

        public RelayCommand OpenSelectedNewsInBrowserCommand { get; }
        public RelayCommand OpenSelectedNewsInSteamCommand { get; }
        public RelayCommand OpenSelectedNewsCommand { get; }
        public RelayCommand NextNewsCommand { get; }
        public RelayCommand PreviousNewsCommand { get; }
        

        public NewsViewerControl(IPlayniteAPI PlayniteApi, NewsViewerSettingsViewModel settings, SteamNewsService steamNewsService)
        {
            InitializeComponent();
            _playniteApi = PlayniteApi;
            _steamNewsService = steamNewsService;
            SettingsModel = settings;
            
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }

            DataContext = this;

            updateContextTimer = new DispatcherTimer();
            updateContextTimer.Interval = TimeSpan.FromMilliseconds(700);
            updateContextTimer.Tick += new EventHandler(UpdateContextTimer_Tick);

            var dateTimeConvertCulture = PlayniteUtilities.GetPlayniteMatchingLanguageCulture();
            _dateTimeConvertCulture = dateTimeConvertCulture;
            SetControlTextBlockStyle();

            OpenSelectedNewsInBrowserCommand = new RelayCommand(OpenSelectedNewsInBrowser, () => SettingsModel.Settings.ReviewsAvailable);
            OpenSelectedNewsInSteamCommand = new RelayCommand(OpenSelectedNewsInSteam, () => SettingsModel.Settings.ReviewsAvailable);
            OpenSelectedNewsCommand = new RelayCommand(OpenSelectedNews, () => SettingsModel.Settings.ReviewsAvailable);
            NextNewsCommand = new RelayCommand(NextNews, () => multipleNewsAvailable);
            PreviousNewsCommand = new RelayCommand(PreviousNews, () => multipleNewsAvailable);
        }

        private void NextNews()
        {
            if (SelectedNewsIndex == newsNodes.Count -1)
            {
                // index is last item
                SelectedNewsIndex = 0;
            }
            else
            {
                SelectedNewsIndex += 1;
            }
        }

        private void PreviousNews()
        {
            if (SelectedNewsIndex == 0)
            {
                var newIndex = newsNodes.Count - 1;
                SelectedNewsIndex = newIndex;
            }
            else
            {
                SelectedNewsIndex -= 1;
            }
        }

        private void OpenSelectedNews()
        {
            if (SettingsModel.Settings.UseCompactWebNewsViewer)
            {
                OpenNewsOnCompactView();
            }
            else
            {
                var newsLink = GetCurrentNewsLink();
                if (!newsLink.IsNullOrEmpty())
                {
                    using (var webView = _playniteApi.WebViews.CreateView(1024, 700))
                    {
                        webView.Navigate(newsLink);
                        webView.OpenDialog();
                    }
                }
            }
        }

        private void OpenSelectedNewsInBrowser()
        {
            var newsLink = GetCurrentNewsLink();
            if (!newsLink.IsNullOrEmpty())
            {
                ProcessStarter.StartUrl(newsLink);
            }
        }

        private void OpenSelectedNewsInSteam()
        {
            var newsLink = GetCurrentNewsLink();
            if (!newsLink.IsNullOrEmpty())
            {
                var steamUri = string.Format("steam://openurl/{0}", newsLink);
                ProcessStarter.StartUrl(steamUri);
            }
        }

        private string GetCurrentNewsLink()
        {
            if (CurrentNewsArticle is null)
            {
                return null;
            }

            return CurrentNewsArticle?.Url;
        }

        private void OpenNewsOnCompactView()
        {
            if (CurrentNewsArticle is null)
            {
                return;
            }

            var descriptionChild = CurrentNewsArticle.DescriptionHtmlFormatted;
            if (descriptionChild is null)
            {
                return;
            }

            var baseHtml = @"
<head>
    <title>News Viewer</title>
    <meta charset=""UTF-8"">
    <style type=""text/css"">
        html,body
        {{
            color: rgb(207, 210, 211);
            margin: 0;
            padding: 10;
            font-family: ""Arial"";
            font-size: 14px;
            background-color: rgb(51, 54, 60);
        }}
        a {{
            color: rgb(147, 179, 200);
            text-decoration: none;
        }}
        img {{
            max-width: 100%;
        }}
		iframe {{
		  width: 90vw;
		  height: calc(90vw/1.77);
		}}
		img.sharedFilePreviewYouTubeVideo.sizeFull {{
			display: none;
		}}
    </style>
</head>
<body>
    {0}
    <br>
    <h1>{1}</h1>
    <br>
    {2}
</body>";
            var html = string.Format(baseHtml,
                CurrentNewsArticle.PublishedDate,
                CurrentNewsArticle.Title,
                CurrentNewsArticle.DescriptionHtmlFormatted);

            using (var webView = _playniteApi.WebViews.CreateView(650, 700))
            {
                // The webview fails to render correctly if it contains reserved
                // characters (See https://datatracker.ietf.org/doc/html/rfc3986#section-2.2)
                // To fix this, we encode to base 64 and load it
                webView.Navigate("data:text/html;base64," + html.Base64Encode());
                webView.OpenDialog();
            }
        }

        private void SetControlTextBlockStyle()
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = _playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle &&
                baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                Resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                ActiveViewAtCreation != _playniteApi.MainView.ActiveDesktopView)
            {
                updateContextTimer.Stop();
                currentGame = null;
                return;
            }

            currentGame = newContext;
            newsNodes = null;
            multipleNewsAvailable = false;
            CurrentNewsArticle = null;
            ControlVisibility = Visibility.Collapsed;
            SwitchNewsVisibility = Visibility.Collapsed;
            SettingsModel.Settings.ReviewsAvailable = false;

            if (currentGame is null || !SettingsModel.Settings.EnableNewsControl)
            {
                updateContextTimer.Stop();
            }
            else
            {
                updateContextTimer.Stop();
                updateContextTimer.Start();
            }
        }

        private async void UpdateContextTimer_Tick(object sender, EventArgs e)
        {
            updateContextTimer.Stop();
            var steamId = Steam.GetGameSteamId(currentGame, SettingsModel.Settings.ShowSteamNewsNonSteam);
            if (steamId.IsNullOrEmpty())
            {
                return;
            }

            var requestOptions = new SteamNewsRequestOptions(steamId, true, false);
            var cache = await _steamNewsService.GetNewsAsync(requestOptions);
            if (cache != null)
            {
                UpdateControlData(cache);                
            }
            else
            {
                await UpdateNewsContextAsync(steamId);
            }
        }

        private async Task UpdateNewsContextAsync(string steamId)
        {
            if (currentGame is null)
            {
                return;
            }

            var contextId = currentGame.Id;
            var requestOptions = new SteamNewsRequestOptions(steamId, true, true);
            var cache = await _steamNewsService.GetNewsAsync(requestOptions);
            if (cache != null && currentGame != null && currentGame?.Id == contextId)
            {
                UpdateControlData(cache);
            }
        }

        private void UpdateControlData(SteamNewsFeed channel)
        {
            if (!channel.Items.HasItems())
            {
                SettingsModel.Settings.ReviewsAvailable = false;
                ControlVisibility = Visibility.Collapsed;
                return;
            }

            if (channel.Items.Count > 0)
            {
                multipleNewsAvailable = true;
                SwitchNewsVisibility = Visibility.Visible;
            }
            else
            {
                multipleNewsAvailable = false;
            }

            SettingsModel.Settings.ReviewsAvailable = true;
            ControlVisibility = Visibility.Visible;
            newsNodes = channel.Items;
            SelectedNewsIndex = 0;

            NotifyCommandsChanged();
        }

        private static string CleanSteamNewsDescription(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }

            return Regex.Replace(str, @"(<div onclick=""javascript:ReplaceWithYouTubeEmbed.*?(?=<\/div>)<\/div>)", string.Empty);
        }

        private void NotifyCommandsChanged()
        {
            OnPropertyChanged(nameof(PreviousNewsCommand));
            OnPropertyChanged(nameof(NextNewsCommand));
            OnPropertyChanged(nameof(OpenSelectedNewsCommand));
            OnPropertyChanged(nameof(OpenSelectedNewsInBrowserCommand));
            OnPropertyChanged(nameof(OpenSelectedNewsInSteamCommand));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
