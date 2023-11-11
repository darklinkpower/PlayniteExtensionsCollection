using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using WebCommon;
using ReviewViewer.Models;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using CodeKicker.BBCode;

namespace ReviewViewer.Controls
{
    /// <summary>
    /// Interaction logic for ReviewsControl.xaml
    /// </summary>
    public partial class ReviewsControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var caller = name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly DesktopView ActiveViewAtCreation;
        private readonly Dispatcher _applicationDispatcher;
        private readonly DispatcherTimer timer;
        private readonly BbCodeProcessor bbCodeProcessor = new BbCodeProcessor();
        IPlayniteAPI PlayniteApi;
        public ReviewViewerSettingsViewModel SettingsModel { get; }
        
        private Game currentGame;

        public enum ReviewSearchType { All, Positive, Negative };

        private Visibility thumbsUpVisibility = Visibility.Collapsed;
        public Visibility ThumbsUpVisibility
        {
            get => thumbsUpVisibility;
            set
            {
                thumbsUpVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility thumbsDownVisibility = Visibility.Collapsed;
        public Visibility ThumbsDownVisibility
        {
            get => thumbsDownVisibility;
            set
            {
                thumbsDownVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility mainPanelVisibility = Visibility.Collapsed;
        public Visibility MainPanelVisibility
        {
            get => mainPanelVisibility;
            set
            {
                mainPanelVisibility = value;
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

        private ReviewSearchType selectedReviewSearch = ReviewSearchType.All;
        public ReviewSearchType SelectedReviewSearch
        {
            get => selectedReviewSearch;
            set
            {
                selectedReviewSearch = value;
                OnPropertyChanged();
            }
        }

        private ILogger logger = LogManager.GetLogger();
        private string pluginUserDataPath;

        private const string reviewsApiMask = @"https://store.steampowered.com/appreviews/{0}?json=1&purchase_type=all&language={1}&review_type={2}&playtime_filter_min=0&filter=summary";
        private const string reviewUrlFormat = @"https://steamcommunity.com/profiles/{0}/recommended/{1}/";
        private string steamApiLanguage = string.Empty;

        private bool multipleReviewsAvailable = false;

        public Dictionary<string, string> ReviewTypesSource;

        private ReviewsResponse reviews;
        public ReviewsResponse Reviews
        {
            get => reviews;
            set
            {
                reviews = value;
                OnPropertyChanged();
            }
        }

        private string calculatedScore;
        public string CalculatedScore
        {
            get => calculatedScore;
            set
            {
                calculatedScore = value;
                OnPropertyChanged();
            }
        }

        private int selectedReviewDisplayIndex;
        public int SelectedReviewDisplayIndex
        {
            get => selectedReviewDisplayIndex;
            set
            {
                selectedReviewDisplayIndex = value;
                OnPropertyChanged();
            }
        }

        private string selectedReviewText;
        public string SelectedReviewText
        {
            get => selectedReviewText;
            set
            {
                selectedReviewText = value;
                OnPropertyChanged();

                _applicationDispatcher.Invoke(() =>
                {
                    ReviewsScrollViewer.ScrollToTop();
                });
            }
        }

        private string formattedPlaytime;
        public string FormattedPlaytime
        {
            get => formattedPlaytime;
            set
            {
                formattedPlaytime = value;
                OnPropertyChanged();
            }
        }

        private string totalFormattedPlaytime;
        public string TotalFormattedPlaytime
        {
            get => totalFormattedPlaytime;
            set
            {
                totalFormattedPlaytime = value;
                OnPropertyChanged();
            }
        }

        private string reviewHelpfulnessHelpful;
        public string ReviewHelpfulnessHelpful
        {
            get => reviewHelpfulnessHelpful;
            set
            {
                reviewHelpfulnessHelpful = value;
                OnPropertyChanged();
            }
        }

        private string reviewHelpfulnessFunny;
        public string ReviewHelpfulnessFunny
        {
            get => reviewHelpfulnessFunny;
            set
            {
                reviewHelpfulnessFunny = value;
                OnPropertyChanged();
            }
        }

        private string reviewPostedDate;
        public string ReviewPostedDate
        {
            get => reviewPostedDate;
            set
            {
                reviewPostedDate = value;
                OnPropertyChanged();
            }
        }

        private int selectedReviewIndex;
        public int SelectedReviewIndex
        {
            get => selectedReviewIndex;
            set
            {
                selectedReviewIndex = value;
                OnPropertyChanged();
                SelectedReview = reviews.Reviews[selectedReviewIndex];
                SetReviewInfo();
            }
        }

        private long totalReviewsAvailable = 0;
        public long TotalReviewsAvailable
        {
            get => totalReviewsAvailable;
            set
            {
                totalReviewsAvailable = value;
                OnPropertyChanged();
            }
        }

        private string currentSteamId = null;
        public string CurrentSteamId
        {
            get => currentSteamId;
            set
            {
                currentSteamId = value;
                OnPropertyChanged();
            }
        }

        private void SetReviewInfo()
        {
            var time = TimeSpan.FromMinutes(SelectedReview.Author.PlaytimeAtReview);
            FormattedPlaytime = string.Format(ResourceProvider.GetString("LOCReview_Viewer_ReviewPlaytimeFormat"), Math.Floor(time.TotalHours), time.ToString("mm"));

            time = TimeSpan.FromMinutes(SelectedReview.Author.PlaytimeForever);
            TotalFormattedPlaytime = string.Format(ResourceProvider.GetString("LOCReview_Viewer_ReviewTotalPlaytimeFormat"), Math.Floor(time.TotalHours), time.ToString("mm"));

            ReviewHelpfulnessHelpful = string.Format(ResourceProvider.GetString("LOCReview_Viewer_ReviewHelpfulnessHelpfulFormat"), SelectedReview.VotesUp);
            ReviewHelpfulnessFunny = string.Format(ResourceProvider.GetString("LOCReview_Viewer_ReviewHelpfulnessFunnyFormat"), SelectedReview.VotesFunny);

            ReviewPostedDate = string.Format(ResourceProvider.GetString("LOCReview_Viewer_ReviewPostedDateLabel"), UnixTimeStampToFormattedString(SelectedReview.TimestampUpdated));
        }

        public static string UnixTimeStampToFormattedString(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime.ToString("dd MMM yyyy");
        }

        private Review selectedReview;
        public Review SelectedReview
        {
            get => selectedReview;
            set
            {
                selectedReview = value;
                ThumbsUpVisibility = Visibility.Collapsed;
                ThumbsDownVisibility = Visibility.Collapsed;
                if (selectedReview != null)
                {
                    if (selectedReview.VotedUp)
                    {
                        ThumbsUpVisibility = Visibility.Visible;
                    }
                    else
                    {
                        ThumbsDownVisibility = Visibility.Visible;
                    }

                    SelectedReviewText = bbCodeProcessor.ToHtml(SelectedReview.ReviewReview);
                }

                OnPropertyChanged();
            }
        }

        public RelayCommand<object> NextReviewCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                NextReview();
            }, (a) => multipleReviewsAvailable);
        }

        void NextReview()
        {
            if (selectedReviewIndex == (Reviews.QuerySummary.NumReviews - 1))
            {
                SelectedReviewIndex = 0;
            }
            else
            {
                SelectedReviewIndex = selectedReviewIndex + 1;
            }
            SelectedReviewDisplayIndex = SelectedReviewIndex + 1;
        }

        public RelayCommand<object> PreviousReviewCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                PreviousReview();
            }, (a) => multipleReviewsAvailable);
        }

        void PreviousReview()
        {
            if (selectedReviewIndex == 0)
            {
                SelectedReviewIndex = Convert.ToInt32(Reviews.QuerySummary.NumReviews - 1);
            }
            else
            {
                SelectedReviewIndex = selectedReviewIndex - 1;
            }
            SelectedReviewDisplayIndex = SelectedReviewIndex + 1;
        }

        public RelayCommand<object> SwitchAllReviewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SwitchAllReviewsAsync();
            }, (a) => selectedReviewSearch != ReviewSearchType.All);
        }

        private async void SwitchAllReviewsAsync()
        {
            selectedReviewSearch = ReviewSearchType.All;
            await UpdateReviewsContextAsync();
        }

        public RelayCommand<object> SwitchPositiveReviewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SwitchPositiveReviewsAsync();
            }, (a) => selectedReviewSearch != ReviewSearchType.Positive);
        }

        public RelayCommand<object> OpenSelectedReviewCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                if (selectedReview != null && !CurrentSteamId.IsNullOrEmpty())
                {
                    var reviewUrl = string.Format(reviewUrlFormat, selectedReview.Author.Steamid, currentSteamId);
                    ProcessStarter.StartUrl(reviewUrl);
                }
            });
        }

        private async void SwitchPositiveReviewsAsync()
        {
            selectedReviewSearch = ReviewSearchType.Positive;
            MainPanelVisibility = Visibility.Collapsed;
            await UpdateReviewsContextAsync();
        }

        public RelayCommand<object> SwitchNegativeReviewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SwitchNegativeReviewsAsync();
            }, (a) => selectedReviewSearch != ReviewSearchType.Negative);
        }

        private async void SwitchNegativeReviewsAsync()
        {
            selectedReviewSearch = ReviewSearchType.Negative;
            MainPanelVisibility = Visibility.Collapsed;
            await UpdateReviewsContextAsync();
        }

        public ReviewsControl(string pluginUserDataPath, string steamApiLanguage, ReviewViewerSettingsViewModel settings, IPlayniteAPI playniteApi)
        {
            InitializeComponent();
            this.PlayniteApi = playniteApi;
            SettingsModel = settings;
            selectedReviewSearch = ReviewSearchType.All;
            this.pluginUserDataPath = pluginUserDataPath;
            this.steamApiLanguage = steamApiLanguage;
            DataContext = this;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = PlayniteApi.MainView.ActiveDesktopView;
            }

            _applicationDispatcher = Application.Current.Dispatcher;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(220);
            timer.Tick += new EventHandler(TimerUpdateContext);
            SetControlTextBlockStyle();
            bbCodeProcessor = new BbCodeProcessor();
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

        private async void TimerUpdateContext(object sender, EventArgs e)
        {
            timer.Stop();
            await UpdateReviewsContextAsync();
        }

        private void ResetBindingValues()
        {
            ThumbsUpVisibility = Visibility.Collapsed;
            ThumbsDownVisibility = Visibility.Collapsed;
            MainPanelVisibility = Visibility.Collapsed;
            multipleReviewsAvailable = false;
            SelectedReviewDisplayIndex = 0;
            TotalReviewsAvailable = 0;
            FormattedPlaytime = string.Empty;
            TotalFormattedPlaytime = string.Empty;
            SelectedReviewText = string.Empty;
            ReviewHelpfulnessHelpful = string.Empty;
            ReviewHelpfulnessFunny = string.Empty;
            SettingsModel.Settings.IsControlVisible = false;
            CalculatedScore = "-";
            CurrentSteamId = null;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            timer.Stop();
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                ActiveViewAtCreation != PlayniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (newContext == null || (!Steam.IsGameSteamGame(newContext) && !PlayniteUtilities.IsGamePcGame(newContext)))
            {
                ResetBindingValues();
                ControlVisibility = Visibility.Collapsed;
                SettingsModel.Settings.IsControlVisible = false;
                return;
            }

            currentGame = newContext;
            timer.Start();
        }

        public async Task UpdateReviewsContextAsync()
        {
            ResetBindingValues();
            CurrentSteamId = Steam.GetGameSteamId(currentGame, true);
            if (CurrentSteamId.IsNullOrEmpty())
            {
                return;
            }

            ControlVisibility = Visibility.Visible;
            SettingsModel.Settings.IsControlVisible = true;
            switch (selectedReviewSearch)
            {
                case ReviewSearchType.Positive:
                    await UpdateReviewsContextByTypeAsync("positive");
                    break;
                case ReviewSearchType.Negative:
                    await UpdateReviewsContextByTypeAsync("negative");
                    break;
                default:
                    await UpdateReviewsContextByTypeAsync("all");
                    break;
            }
        }

        private async Task UpdateReviewsContextByTypeAsync(string reviewSearchType)
        {
            var contextGameId = currentGame.Id;
            var gameDataPath = Path.Combine(pluginUserDataPath, $"{contextGameId}_{reviewSearchType}.json");
            if (FileSystem.FileExists(gameDataPath))
            {
                await DownloadReviewDataIfOlderAsync(gameDataPath, reviewSearchType);
            }
            else
            {
                if (!SettingsModel.Settings.DownloadDataOnGameSelection)
                {
                    return;
                }

                await DownloadReviewData(gameDataPath, reviewSearchType);
                if (!FileSystem.FileExists(gameDataPath))
                {
                    return;
                }
            }

            // To detect if game changed while downloading data
            if (currentGame is null || currentGame.Id != contextGameId)
            {
                return;
            }

            if (!Serialization.TryFromJsonFile<ReviewsResponse>(gameDataPath, out var data))
            {
                return;
            }

            Reviews = data;
            if (Reviews.Success != 1)
            {
                logger.Debug($"Deserialized json in {gameDataPath} had Success value {Reviews.Success}.");
                return;
            }

            try
            {
                if (Reviews.QuerySummary.NumReviews == 0)
                {
                    logger.Debug($"Deserialized json in {gameDataPath} had 0 reviews.");
                    return;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error obtaining reviews number for file {gameDataPath}. Error: {e.Message}.");
                return;
            }

            multipleReviewsAvailable = Reviews.QuerySummary.NumReviews > 1;
            SelectedReviewIndex = 0;
            SelectedReviewDisplayIndex = SelectedReviewIndex + 1;
            TotalReviewsAvailable = Reviews.QuerySummary.NumReviews;
            CalculateUserScore();
            MainPanelVisibility = Visibility.Visible;
        }

        private async Task DownloadReviewDataIfOlderAsync(string gameDataPath, string reviewSearchType)
        {
            if (!SettingsModel.Settings.DownloadDataIfOlderThanDays)
            {
                return;
            }

            var fi = new FileInfo(FileSystem.FixPathLength(gameDataPath));
            if (fi.LastWriteTime < DateTime.Now.AddDays(-SettingsModel.Settings.DownloadIfOlderThanValue))
            {
                await DownloadReviewData(gameDataPath, reviewSearchType);
            }
        }

        private async Task DownloadReviewData(string gameDataPath, string reviewSearchType)
        {
            CurrentSteamId = Steam.GetGameSteamId(currentGame, true);
            if (CurrentSteamId is null)
            {
                MainPanelVisibility = Visibility.Collapsed;
                return;
            }

            var uri = string.Format(reviewsApiMask, CurrentSteamId, steamApiLanguage, reviewSearchType);
            await HttpDownloader.GetRequestBuilder().WithUrl(uri).WithDownloadTo(gameDataPath).DownloadFileAsync();
        }

        private void CalculateUserScore()
        {
            // From https://steamdb.info/blog/steamdb-rating/
            double totalReviews = Reviews.QuerySummary.TotalReviews;
            double totalPositiveReviews = Reviews.QuerySummary.TotalPositive;
            double reviewScore = totalPositiveReviews / totalReviews;
            var score = reviewScore - (reviewScore - 0.5) * Math.Pow(2, -Math.Log10(totalReviews + 1));
            CalculatedScore = string.Format("{0}%", Math.Round(score * 100, 2).ToString());
        }

    }
}