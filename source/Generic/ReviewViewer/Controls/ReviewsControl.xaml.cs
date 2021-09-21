using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using ReviewViewer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        private string currentSteamId;
        private Game currentGame;

        public enum ReviewSearchType { All, Positive, Negative };
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
        private HttpClient client;
        private string pluginUserDataPath;

        string reviewsApiMask = @"https://store.steampowered.com/appreviews/{0}?json=1&purchase_type=all&language={1}&review_type={2}&playtime_filter_min=0&filter=summary";
        private string steamApiLanguage;

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

        private int selectedReviewIndex;
        public int SelectedReviewIndex
        {
            get => selectedReviewIndex;
            set
            {
                selectedReviewIndex = value;
                OnPropertyChanged();
                SelectedReview = reviews.Reviews[selectedReviewIndex];
                SetFormattedPlaytime();
            }
        }

        private void SetFormattedPlaytime()
        {
            char separator = ':';
            var playtime = TimeSpan.FromMinutes(SelectedReview.Author.PlaytimeForever).ToString("hh\\:mm").Split(separator);
            FormattedPlaytime = string.Format(ResourceProvider.GetString("LOCReview_Viewer_ReviewPlaytimeFormat"), playtime[0], playtime[1]);
        }

        private Review selectedReview;
        public Review SelectedReview
        {
            get => selectedReview;
            set
            {
                selectedReview = value;
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
            if ((Reviews.QuerySummary.NumReviews - 1) == selectedReviewIndex)
            {
                SelectedReview = reviews.Reviews[0];
                SelectedReviewIndex = 0;
            }
            else
            {
                var newIndex = selectedReviewIndex + 1;
                SelectedReview = reviews.Reviews[newIndex];
                SelectedReviewIndex = newIndex;
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
                var newIndex = Reviews.QuerySummary.NumReviews - 1;
                SelectedReview = reviews.Reviews[newIndex];
                SelectedReviewIndex = Convert.ToInt32(newIndex);
            }
            else
            {
                var newIndex = selectedReviewIndex - 1;
                SelectedReview = reviews.Reviews[newIndex];
                SelectedReviewIndex = newIndex;
            }
            SelectedReviewDisplayIndex = SelectedReviewIndex + 1;
        }

        public RelayCommand<object> SwitchAllReviewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SwitchAllReviews();
            }, (a) => selectedReviewSearch != ReviewSearchType.All);
        }

        void SwitchAllReviews()
        {
            selectedReviewSearch = ReviewSearchType.All;
            SummaryGrid.Visibility = Visibility.Visible;
            UpdateReviewsContext();
        }

        public RelayCommand<object> SwitchPositiveReviewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SwitchPositiveReviews();
            }, (a) => selectedReviewSearch != ReviewSearchType.Positive);
        }

        void SwitchPositiveReviews()
        {
            selectedReviewSearch = ReviewSearchType.Positive;
            SummaryGrid.Visibility = Visibility.Collapsed;
            UpdateReviewsContext();
        }

        public RelayCommand<object> SwitchNegativeReviewsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SwitchNegativeReviews();
            }, (a) => selectedReviewSearch != ReviewSearchType.Negative);
        }

        void SwitchNegativeReviews()
        {
            selectedReviewSearch = ReviewSearchType.Negative;
            SummaryGrid.Visibility = Visibility.Collapsed;
            UpdateReviewsContext();
        }

        public ReviewsControl(string pluginUserDataPath, string steamApiLanguage)
        {
            InitializeComponent();
            DataContext = this;
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            this.pluginUserDataPath = pluginUserDataPath;
            this.steamApiLanguage = steamApiLanguage;
            selectedReviewSearch = ReviewSearchType.All;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (newContext == null)
            {
                return;
            }
            if (newContext.PluginId != Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                return;
            }

            currentGame = newContext;
            currentSteamId = currentGame.GameId;
            UpdateReviewsContext();
        }

        public void UpdateReviewsContext()
        {
            string reviewSearchType;
            switch (selectedReviewSearch)
            {
                case ReviewSearchType.Positive:
                    reviewSearchType = "positive";
                    break;
                case ReviewSearchType.Negative:
                    reviewSearchType = "negative";
                    break;
                default:
                    reviewSearchType = "all";
                    break;
            }

            var gameDataPath = Path.Combine(pluginUserDataPath, $"{currentGame.Id}_{reviewSearchType}.json");
            if (!File.Exists(gameDataPath))
            {
                var uri = string.Format(reviewsApiMask, currentSteamId, steamApiLanguage, reviewSearchType);
                DownloadFile(uri, gameDataPath).GetAwaiter().GetResult();
                if (!File.Exists(gameDataPath))
                {
                    return;
                }
            }

            var reviews = JsonConvert.DeserializeObject<ReviewsResponse>(File.ReadAllText(gameDataPath));
            Reviews = reviews;
            if (Reviews.QuerySummary.NumReviews == 0)
            {
                return;
            }

            multipleReviewsAvailable = false;
            if (Reviews.QuerySummary.NumReviews > 1)
            {
                multipleReviewsAvailable = true;
            }

            SelectedReviewIndex = 0;
            SelectedReviewDisplayIndex = SelectedReviewIndex + 1;
            CalculateUserScore();
        }

        private void CalculateUserScore()
        {
            // From https://steamdb.info/blog/steamdb-rating/
            
            double totalReviews = Reviews.QuerySummary.TotalReviews;
            double totalPositiveReviews = Reviews.QuerySummary.TotalPositive;
            double reviewScore = totalPositiveReviews / totalReviews;
            var score = reviewScore - (reviewScore - 0.5) * Math.Pow(2, -Math.Log10(totalReviews + 1));
            CalculatedScore = string.Format("{0}{1}", Math.Round(score * 100, 2).ToString(), "%");
        }

        public async Task DownloadFile(string requestUri, string fileToWriteTo)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during file download, url {requestUri}. Error: {e.Message}.");
            }
        }
    }
}
