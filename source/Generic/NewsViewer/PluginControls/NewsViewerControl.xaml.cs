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
using WebCommon;

namespace NewsViewer.PluginControls
{
    /// <summary>
    /// Interaction logic for NewsViewerControl.xaml
    /// </summary>
    public partial class NewsViewerControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        IPlayniteAPI PlayniteApi;
        private readonly CacheManager<Guid, SteamNewsRssFeed> newsCacheManager;
        private static readonly ILogger logger = LogManager.GetLogger();
        public NewsViewerSettingsViewModel SettingsModel { get; set; }
        private readonly Dictionary<string, string>  headers = new Dictionary<string, string> {["Accept"] = "text/xml", ["Accept-Encoding"] = "utf-8"};
        private readonly DispatcherTimer updateContextTimer;
        const string steamRssTemplate = @"https://store.steampowered.com/feeds/news/app/{0}/l={1}";
        private readonly string steamLanguage;
        private readonly DesktopView ActiveViewAtCreation;
        private readonly CultureInfo _dateTimeConvertCulture;
        private readonly List<SteamHtmlTransformDefinition> _descriptionTransformElems;
        private List<RssItem> newsNodes = new List<RssItem>();
        private int selectedNewsIndex;
        private bool multipleNewsAvailable;
        private Game currentGame;


        public string NewsTitle => CurrentNewsNode?.Title ?? string.Empty;
        public string NewsDate => CurrentNewsNode?.PubDate.ToString("ddd, MMMM d yyyy HH:mm", _dateTimeConvertCulture) ?? string.Empty;
        public string NewsText => CleanSteamNewsDescription(CurrentNewsNode?.Description ?? string.Empty);
        private RssItem currentNewsNode;
        public RssItem CurrentNewsNode
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
                CurrentNewsNode = newsNodes[selectedNewsIndex];
            }
        }

        public NewsViewerControl(IPlayniteAPI PlayniteApi, NewsViewerSettingsViewModel settings, string steamLanguage, CacheManager<Guid, SteamNewsRssFeed> newsCacheManager)
        {
            InitializeComponent();
            this.PlayniteApi = PlayniteApi;
            this.newsCacheManager = newsCacheManager;
            SettingsModel = settings;
            this.steamLanguage = steamLanguage;
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

            _descriptionTransformElems = new List<SteamHtmlTransformDefinition>()
            {
                new SteamHtmlTransformDefinition("span", "bb_strike", "strike"),
                new SteamHtmlTransformDefinition("div", "bb_h1", "h1"),
                new SteamHtmlTransformDefinition("div", "bb_h2", "h2"),
                new SteamHtmlTransformDefinition("div", "bb_h3", "h3"),
                new SteamHtmlTransformDefinition("div", "bb_h4", "h4"),
                new SteamHtmlTransformDefinition("div", "bb_h5", "h5")
            };
        }

        private static string CleanSteamNewsDescription(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }
            
            return Regex.Replace(str, @"(<div onclick=""javascript:ReplaceWithYouTubeEmbed.*?(?=<\/div>)<\/div>)", string.Empty);
        }

        void NextNews()
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

        void PreviousNews()
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

        void OpenSelectedNews()
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
                    using (var webView = PlayniteApi.WebViews.CreateView(1024, 700))
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
            if (CurrentNewsNode is null)
            {
                return null;
            }

            return CurrentNewsNode?.Link;
        }

        private void OpenNewsOnCompactView()
        {
            if (CurrentNewsNode is null)
            {
                return;
            }

            var descriptionChild = CurrentNewsNode.Description;
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
                CurrentNewsNode.PubDate,
                CurrentNewsNode.Title,
                CurrentNewsNode.Description);

            using (var webView = PlayniteApi.WebViews.CreateView(650, 700))
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
            var baseStyleName = PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
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
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                ActiveViewAtCreation != PlayniteApi.MainView.ActiveDesktopView)
            {
                updateContextTimer.Stop();
                currentGame = null;
                return;
            }

            currentGame = newContext;
            newsNodes = null;
            multipleNewsAvailable = false;
            CurrentNewsNode = null;
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
            var cache = newsCacheManager.GetCache(currentGame.Id);
            if (cache is null)
            {
                await UpdateNewsContextAsync();
            }
            else
            {
                UpdateControlData(cache);
            }
        }

        private async Task UpdateNewsContextAsync()
        {
            if (currentGame is null)
            {
                return;
            }

            var contextId = currentGame.Id;
            var steamId = Steam.GetGameSteamId(currentGame, SettingsModel.Settings.ShowSteamNewsNonSteam);
            if (steamId.IsNullOrEmpty())
            {
                return;
            }

            var request = HttpDownloader.GetRequestBuilder()
                .WithUrl(string.Format(steamRssTemplate, steamId, steamLanguage))
                .WithHeaders(headers);
            var result = await request.DownloadStringAsync();
            if (!result.IsSuccessful)
            {
                return;
            }

            var newsFeed = ParseRssResponse(result.Response.Content);
            if (newsFeed is null)
            {
                return;
            }

            var savedCache = newsCacheManager.SaveCache(contextId, newsFeed);
            if (currentGame != null && currentGame?.Id == contextId)
            {
                UpdateControlData(savedCache);
            }
        }

        private SteamNewsRssFeed ParseRssResponse(string xmlContent)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(xmlContent);

                var channelNode = document.DocumentNode.SelectSingleNode("//channel");
                var rssFeed = new SteamNewsRssFeed
                {
                    Channel = new Channel
                    {
                        Title = channelNode.SelectSingleNode("title")?.InnerText,
                        Link = channelNode.SelectSingleNode("link")?.InnerText,
                        Description = channelNode.SelectSingleNode("description")?.InnerText,
                        Language = channelNode.SelectSingleNode("language")?.InnerText,
                        Generator = channelNode.SelectSingleNode("generator")?.InnerText,
                        Items = new List<RssItem>(),
                    }
                };

                var itemNodes = channelNode.SelectNodes(".//item");
                if (!itemNodes.HasItems())
                {
                    return rssFeed;
                }

                var descriptionDocument = new HtmlDocument();
                foreach (var itemNode in itemNodes)
                {
                    var rssItem = new RssItem
                    {
                        Title = itemNode.SelectSingleNode(".//title")?.InnerText.HtmlDecode(),
                        Link = itemNode.SelectSingleNode(".//guid")?.InnerText,
                        PubDate = DateTime.ParseExact(itemNode.SelectSingleNode(".//pubdate")?.InnerText, "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                        Author = itemNode.SelectSingleNode(".//author")?.InnerText.HtmlDecode(),
                        Guid = new NewsGuid
                        {
                            IsPermaLink = itemNode.SelectSingleNode(".//guid")?.GetAttributeValue("isPermaLink", string.Empty) == "true",
                            Value = itemNode.SelectSingleNode(".//guid")?.InnerText,
                        }
                    };

                    var descriptionNode = itemNode.SelectSingleNode(".//description");
                    if (!(descriptionNode is null))
                    {
                        descriptionDocument.LoadHtml(descriptionNode.InnerText.HtmlDecode());
                        FixNewsDescriptionElements(descriptionDocument);
                        rssItem.Description = descriptionDocument.DocumentNode.InnerHtml;
                    }

                    rssFeed.Channel.Items.Add(rssItem);
                }

                
                return rssFeed;
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while parsing rss feed");
                return null;
            }
        }

        private void FixNewsDescriptionElements(HtmlDocument descriptionDocument)
        {
            if (!descriptionDocument.DocumentNode.HasChildNodes)
            {
                return;
            }

            foreach (var childNode in descriptionDocument.DocumentNode.ChildNodes)
            {
                foreach (var transformElem in _descriptionTransformElems)
                {
                    if (childNode.Name != transformElem.Name)
                    {
                        continue;
                    }

                    if (childNode.GetAttributeValue("class", string.Empty) != transformElem.ClassName)
                    {
                        continue;
                    }

                    childNode.Name = transformElem.NewName;
                    break;
                }
            }
        }

        private void UpdateControlData(CacheItem<SteamNewsRssFeed> newsCache)
        {
            if (!newsCache.Item.Channel.Items.HasItems())
            {
                SettingsModel.Settings.ReviewsAvailable = false;
                ControlVisibility = Visibility.Collapsed;
                return;
            }

            if (newsCache.Item.Channel.Items.Count > 0)
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
            newsNodes = newsCache.Item.Channel.Items;
            SelectedNewsIndex = 0;

            NotifyCommandsChanged();
        }

        private void NotifyCommandsChanged()
        {
            OnPropertyChanged(nameof(PreviousNewsCommand));
            OnPropertyChanged(nameof(NextNewsCommand));
            OnPropertyChanged(nameof(OpenSelectedNewsCommand));
            OnPropertyChanged(nameof(OpenSelectedNewsInBrowserCommand));
            OnPropertyChanged(nameof(OpenSelectedNewsInSteamCommand));
        }

        public RelayCommand OpenSelectedNewsInBrowserCommand
        {
            get => new RelayCommand(() =>
            {
                OpenSelectedNewsInBrowser();
            }, () => SettingsModel.Settings.ReviewsAvailable);
        }

        public RelayCommand OpenSelectedNewsInSteamCommand
        {
            get => new RelayCommand(() =>
            {
                OpenSelectedNewsInSteam();
            }, () => SettingsModel.Settings.ReviewsAvailable);
        }

        public RelayCommand OpenSelectedNewsCommand
        {
            get => new RelayCommand(() =>
            {
                OpenSelectedNews();
            }, () => SettingsModel.Settings.ReviewsAvailable);
        }

        public RelayCommand NextNewsCommand
        {
            get => new RelayCommand(() =>
            {
                NextNews();
            }, () => multipleNewsAvailable);
        }

        public RelayCommand PreviousNewsCommand
        {
            get => new RelayCommand(() =>
            {
                PreviousNews();
            }, () => multipleNewsAvailable);
        }
    }
}