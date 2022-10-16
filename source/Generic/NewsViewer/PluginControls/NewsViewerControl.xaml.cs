using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static readonly ILogger logger = LogManager.GetLogger();
        public NewsViewerSettingsViewModel SettingsModel { get; set; }
        private readonly Dictionary<string, string>  headers = new Dictionary<string, string> {["Accept"] = "text/xml", ["Accept-Encoding"] = "utf-8"};

        private readonly XmlDocument xmlDoc;
        private readonly DispatcherTimer updateContextTimer;
        private XmlNode currentNewsNode;
        public XmlNode CurrentNewsNode
        {
            get => currentNewsNode;
            set
            {
                currentNewsNode = value;
                OnPropertyChanged();
                UpdateBindings();
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

        private string newsDate;
        public string NewsDate
        {
            get => newsDate;
            set
            {
                newsDate = value;
                OnPropertyChanged();
            }
        }

        private string newsTitle;
        public string NewsTitle
        {
            get => newsTitle;
            set
            {
                newsTitle = value;
                OnPropertyChanged();
            }
        }

        private string newsText;
        public string NewsText
        {
            get => newsText;
            set
            {
                newsText = value;
                OnPropertyChanged();
            }
        }

        private void UpdateBindings()
        {
            if (currentNewsNode == null)
            {
                NewsTitle = string.Empty;
                NewsText = string.Empty;
                NewsDate = string.Empty;
                return;
            }

            var titleChild = CurrentNewsNode.SelectSingleNode(@"title");
            var descriptionChild = CurrentNewsNode.SelectSingleNode(@"description");
            var dateChild = CurrentNewsNode.SelectSingleNode(@"pubDate");
            if (titleChild != null  && descriptionChild != null)
            {
                NewsTitle = HtmlToPlainText(titleChild.InnerText);
                NewsText = CleanSteamNewsText(descriptionChild.InnerText);
                Dispatcher.Invoke(() => NewsScrollViewer.ScrollToTop());
                NewsDate = Regex.Replace(HtmlToPlainText(dateChild.InnerText), @" \+\d+$", "");
            }
        }

        private static string CleanSteamNewsText(string html)
        {
            return Regex.Replace(html, @"(<div onclick=""javascript:ReplaceWithYouTubeEmbed.*?(?=<\/div>)<\/div>)", "");
        }

        const string steamRssTemplate = @"https://store.steampowered.com/feeds/news/app/{0}/l={1}";
        private readonly string steamLanguage;
        private readonly DesktopView ActiveViewAtCreation;
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

        private XmlNodeList newsNodes;

        private int selectedNewsIndex;
        private bool multipleNewsAvailable;
        private Game currentGame;

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

        public RelayCommand<object> NextNewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                NextNews();
            }, (a) => multipleNewsAvailable);
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
                SelectedNewsIndex = SelectedNewsIndex + 1;
            }
        }

        public RelayCommand<object> PreviousNewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                PreviousNews();
            }, (a) => multipleNewsAvailable);
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
                SelectedNewsIndex = SelectedNewsIndex - 1;
            }
        }

        public RelayCommand<object> OpenSelectedNewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenSelectedNews();
            }, (a) => SettingsModel.Settings.ReviewsAvailable);
        }

        void OpenSelectedNews()
        {
            if (CurrentNewsNode == null)
            {
                return;
            }

            if (SettingsModel.Settings.UseCompactWebNewsViewer)
            {
                OpenNewsOnCompactView();
            }
            else
            {
                var linkChild = CurrentNewsNode.SelectSingleNode(@"link");
                if (linkChild != null)
                {
                    var webView = PlayniteApi.WebViews.CreateView(1024, 700);
                    webView.Navigate(linkChild.InnerText);
                    webView.OpenDialog();
                    webView.Dispose();
                }
            }
        }

        private void OpenNewsOnCompactView()
        {
            var descriptionChild = CurrentNewsNode.SelectSingleNode(@"description");
            if (descriptionChild == null)
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
                Regex.Replace(CurrentNewsNode.SelectSingleNode(@"pubDate")?.InnerText ?? "", @" \+\d+$", ""),
                CurrentNewsNode.SelectSingleNode(@"title")?.InnerText ?? "",
                descriptionChild.InnerText);

            var webView = PlayniteApi.WebViews.CreateView(650, 700);

            // The webview fails to correctly render if it includes reserved
            // characters (See https://datatracker.ietf.org/doc/html/rfc3986#section-2.2)
            // To fix this, we encode to base 64 and load it
            webView.Navigate("data:text/html;base64," + html.Base64Encode());
            webView.OpenDialog();
            webView.Dispose();
        }

        public NewsViewerControl(IPlayniteAPI PlayniteApi, NewsViewerSettingsViewModel settings, string steamLanguage)
        {
            InitializeComponent();
            this.PlayniteApi = PlayniteApi;
            SettingsModel = settings;
            xmlDoc = new XmlDocument();

            this.steamLanguage = steamLanguage;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }

            DataContext = this;

            updateContextTimer = new DispatcherTimer();
            updateContextTimer.Interval = TimeSpan.FromMilliseconds(700);
            updateContextTimer.Tick += new EventHandler(UpdateContextTimer_Tick);
            SetControlTextBlockStyle();
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

            if (currentGame == null || !SettingsModel.Settings.EnableNewsControl)
            {
                updateContextTimer.Stop();
            }
            else
            {
                updateContextTimer.Stop();
                updateContextTimer.Start();
            }
        }

        private void UpdateContextTimer_Tick(object sender, EventArgs e)
        {
            updateContextTimer.Stop();
            UpdateNewsContext();
        }

        private void UpdateNewsContext()
        {
            if (currentGame == null)
            {
                return;
            }

            var contextId = currentGame.Id;
            var steamId = Steam.GetGameSteamId(currentGame, SettingsModel.Settings.ShowSteamNewsNonSteam);
            if (steamId.IsNullOrEmpty())
            {
                return;
            }

            Task.Run(() =>
            {
                var downloadStringResult = HttpDownloader.DownloadStringWithHeaders(string.Format(steamRssTemplate, steamId, steamLanguage), headers);
                if (!downloadStringResult.Success || currentGame == null || currentGame.Id != contextId)
                {
                    return;
                }

                try
                {
                    xmlDoc.LoadXml(downloadStringResult.Result);
                    XmlNodeList nodes = xmlDoc.SelectNodes("/rss/channel/item");
                    if (nodes != null && nodes.Count > 0)
                    {
                        if (nodes.Count == 1)
                        {
                            multipleNewsAvailable = false;
                        }
                        else
                        {
                            multipleNewsAvailable = true;
                            SwitchNewsVisibility = Visibility.Visible;
                        }
                        SettingsModel.Settings.ReviewsAvailable = true;
                        ControlVisibility = Visibility.Visible;
                        newsNodes = nodes;
                        SelectedNewsIndex = 0;
                    }
                }
                catch
                {

                }
            });
        }

        static string HtmlToPlainText(string html)
        {
            if (html.IsNullOrEmpty())
            {
                return string.Empty;
            }

            // From https://stackoverflow.com/a/50363077
            string buf;
            string block = "address|article|aside|blockquote|canvas|dd|div|dl|dt|" +
              "fieldset|figcaption|figure|footer|form|h\\d|header|hr|li|main|nav|" +
              "noscript|ol|output|p|pre|section|table|tfoot|ul|video";

            string patNestedBlock = $"(\\s*?</?({block})[^>]*?>)+\\s*";
            buf = Regex.Replace(html, patNestedBlock, "\n", RegexOptions.IgnoreCase);

            // Replace br tag to newline.
            buf = Regex.Replace(buf, @"<(br)[^>]*>", "\n", RegexOptions.IgnoreCase);

            // (Optional) remove styles and scripts.
            buf = Regex.Replace(buf, @"<(script|style)[^>]*?>.*?</\1>", "", RegexOptions.Singleline);

            // Remove all tags.
            buf = Regex.Replace(buf, @"<[^>]*(>|$)", "", RegexOptions.Multiline);

            // Replace HTML entities.
            buf = WebUtility.HtmlDecode(buf);
            return buf;
        }

    }
}