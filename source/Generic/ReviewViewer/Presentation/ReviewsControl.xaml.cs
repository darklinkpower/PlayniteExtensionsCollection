using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using ReviewViewer.Application;
using ReviewViewer.Infrastructure;
using ReviewViewer.Presentation.SteamLanguageSelector;
using SteamCommon;

namespace ReviewViewer.Presentation
{
    /// <summary>
    /// Interaction logic for ReviewsControl.xaml
    /// </summary>
    public partial class ReviewsControl : PluginUserControlBase
    {
        // ──────── Dependencies ────────
        private readonly DesktopView _activeViewAtCreation;
        private readonly SteamReviewsCoordinator _steamReviewsCoordinator;
        private readonly BbCodeProcessor _bbCodeProcessor = new BbCodeProcessor();
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;

        protected override TimeSpan UpdateDebounceInterval => TimeSpan.FromMilliseconds(220);

        // ──────── State Fields ────────
        private Game _currentGame;
        private bool _areBindingValuesDefault = false;
        private string _activeRequestQueryKey = string.Empty;
        private int _inProgressDataUpdates = 0;

        // ──────── Settings ViewModel ────────
        public ReviewViewerSettingsViewModel SettingsModel { get; }

        // ──────── Visibility Properties ────────
        private bool _displayProgressPanel;

        public bool DisplayProgressPanel
        {
            get => _displayProgressPanel;
            set { SetValue(ref _displayProgressPanel, value); }
        }

        private bool _displayReviewsPanel;

        public bool DisplayReviewsPanel
        {
            get => _displayReviewsPanel;
            set { SetValue(ref _displayReviewsPanel, value); }
        }

        private bool _displayNoReviewsPanel;

        public bool DisplayNoReviewsPanel
        {
            get => _displayNoReviewsPanel;
            set { SetValue(ref _displayNoReviewsPanel, value); }
        }

        private Visibility _thumbsUpVisibility = Visibility.Collapsed;
        public Visibility ThumbsUpVisibility
        {
            get => _thumbsUpVisibility;
            set => SetValue(ref _thumbsUpVisibility, value);
        }

        private Visibility _thumbsDownVisibility = Visibility.Collapsed;
        public Visibility ThumbsDownVisibility
        {
            get => _thumbsDownVisibility;
            set => SetValue(ref _thumbsDownVisibility, value);
        }

        private Visibility _mainPanelVisibility = Visibility.Collapsed;
        public Visibility MainPanelVisibility
        {
            get => _mainPanelVisibility;
            set => SetValue(ref _mainPanelVisibility, value);
        }

        private Visibility _controlVisibility = Visibility.Collapsed;
        public Visibility ControlVisibility
        {
            get => _controlVisibility;
            set => SetValue(ref _controlVisibility, value);
        }

        // ──────── Data and Selections ────────
        private ReviewsResponseDto _reviews;
        public ReviewsResponseDto Reviews
        {
            get => _reviews;
            set => SetValue(ref _reviews, value);
        }

        private Review _selectedReview;
        public Review SelectedReview
        {
            get => _selectedReview;
            set
            {
                _selectedReview = value;
                ThumbsUpVisibility = Visibility.Collapsed;
                ThumbsDownVisibility = Visibility.Collapsed;

                if (_selectedReview != null)
                {
                    if (_selectedReview.VotedUp)
                    {
                        ThumbsUpVisibility = Visibility.Visible;
                    }
                    else
                    {
                        ThumbsDownVisibility = Visibility.Visible;
                    }

                    SelectedReviewText = _bbCodeProcessor.ToHtml(SelectedReview.ReviewReview);
                }

                OnPropertyChanged();
            }
        }

        private int _selectedReviewIndex;
        public int SelectedReviewIndex
        {
            get => _selectedReviewIndex;
            set
            {
                SetValue(ref _selectedReviewIndex, value);
                SelectedReview = _reviews.Reviews[_selectedReviewIndex];
                SetReviewInfo();
            }
        }

        private int _selectedReviewDisplayIndex;
        public int SelectedReviewDisplayIndex
        {
            get => _selectedReviewDisplayIndex;
            set => SetValue(ref _selectedReviewDisplayIndex, value);
        }

        private long _totalReviewsAvailable = 0;
        public long TotalReviewsAvailable
        {
            get => _totalReviewsAvailable;
            set => SetValue(ref _totalReviewsAvailable, value);
        }

        private string _currentSteamId = null;
        public string CurrentSteamId
        {
            get => _currentSteamId;
            set => SetValue(ref _currentSteamId, value);
        }

        private bool _multipleReviewsAvailable = false;
        public bool MultipleReviewsAvailable
        {
            get => _multipleReviewsAvailable;
            set => SetValue(ref _multipleReviewsAvailable, value);
        }

        // ──────── Display Texts ────────
        private string _selectedReviewText;
        public string SelectedReviewText
        {
            get => _selectedReviewText;
            set
            {
                if (SetValue(ref _selectedReviewText, value))
                {
                    RunOnUI(() => ReviewsScrollViewer.ScrollToTop());
                }
            }
        }

        private string _formattedPlaytime;
        public string FormattedPlaytime
        {
            get => _formattedPlaytime;
            set => SetValue(ref _formattedPlaytime, value);
        }

        private string _totalFormattedPlaytime;
        public string TotalFormattedPlaytime
        {
            get => _totalFormattedPlaytime;
            set => SetValue(ref _totalFormattedPlaytime, value);
        }

        private string _reviewHelpfulnessHelpful;
        public string ReviewHelpfulnessHelpful
        {
            get => _reviewHelpfulnessHelpful;
            set => SetValue(ref _reviewHelpfulnessHelpful, value);
        }

        private string _reviewHelpfulnessFunny;
        public string ReviewHelpfulnessFunny
        {
            get => _reviewHelpfulnessFunny;
            set => SetValue(ref _reviewHelpfulnessFunny, value);
        }

        private string _reviewPostedDate;
        public string ReviewPostedDate
        {
            get => _reviewPostedDate;
            set => SetValue(ref _reviewPostedDate, value);
        }

        // ──────── Query Options ────────
        private QueryOptionsViewModel _queryOptions = new QueryOptionsViewModel();
        public QueryOptionsViewModel QueryOptions
        {
            get => _queryOptions;
            set => SetValue(ref _queryOptions, value);
        }

        // ──────── Popup States ────────
        public bool IsAnyPopupOpen =>
            IsReviewTypePopupOpen ||
            IsPurchaseTypePopupOpen ||
            IsLanguagePopupOpen ||
            IsDateRangePopupOpen ||
            IsPlaytimePopupOpen ||
            IsDisplayPopupOpen;

        private bool _isReviewTypePopupOpen;
        public bool IsReviewTypePopupOpen
        {
            get => _isReviewTypePopupOpen;
            set { if (SetValue(ref _isReviewTypePopupOpen, value)) CheckAllPopupsClosed(); }
        }

        private bool _isPurchaseTypePopupOpen;
        public bool IsPurchaseTypePopupOpen
        {
            get => _isPurchaseTypePopupOpen;
            set { if (SetValue(ref _isPurchaseTypePopupOpen, value)) CheckAllPopupsClosed(); }
        }

        private bool _isLanguagePopupOpen;
        public bool IsLanguagePopupOpen
        {
            get => _isLanguagePopupOpen;
            set { if (SetValue(ref _isLanguagePopupOpen, value)) CheckAllPopupsClosed(); }
        }

        private bool _isDateRangePopupOpen;
        public bool IsDateRangePopupOpen
        {
            get => _isDateRangePopupOpen;
            set { if (SetValue(ref _isDateRangePopupOpen, value)) CheckAllPopupsClosed(); }
        }

        private bool _isPlaytimePopupOpen;
        public bool IsPlaytimePopupOpen
        {
            get => _isPlaytimePopupOpen;
            set { if (SetValue(ref _isPlaytimePopupOpen, value)) CheckAllPopupsClosed(); }
        }

        private bool _isDisplayPopupOpen;
        public bool IsDisplayPopupOpen
        {
            get => _isDisplayPopupOpen;
            set { if (SetValue(ref _isDisplayPopupOpen, value)) CheckAllPopupsClosed(); }
        }

        private void CheckAllPopupsClosed()
        {
            if (!IsAnyPopupOpen)
            {
                OnAllPopupsClosed();
            }
        }

        private void OnAllPopupsClosed()
        {
            base.ScheduleUpdate();
        }

        // ──────── Commands ────────
        public ICommand OpenSteamLanguageSelectorCommand { get; }
        public ICommand NextReviewCommand { get; }
        public ICommand PreviousReviewCommand { get; }
        public ICommand OpenSelectedReviewCommand { get; }

        public ReviewsControl(
            ReviewViewerSettingsViewModel settingsViewModel,
            IPlayniteAPI playniteApi,
            ILogger logger,
            SteamReviewsCoordinator steamReviewsCoordinator) : base(playniteApi)
        {
            InitializeComponent();
            _playniteApi = playniteApi ?? throw new ArgumentNullException(nameof(playniteApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SettingsModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
            _steamReviewsCoordinator = steamReviewsCoordinator ?? throw new ArgumentNullException(nameof(steamReviewsCoordinator));
            _bbCodeProcessor = new BbCodeProcessor() ?? throw new ArgumentNullException(nameof(_bbCodeProcessor));
            _queryOptions = QueryOptionsViewModel.FromDomain(SettingsModel.Settings.LastUsedQuery);
            DataContext = this;
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = _playniteApi.MainView.ActiveDesktopView;
            }
            
            
            OpenSteamLanguageSelectorCommand = new RelayCommand(() => OpenSteamLanguageSelector());
            NextReviewCommand = new RelayCommand(() => NextReview(), () => MultipleReviewsAvailable);
            PreviousReviewCommand = new RelayCommand(() => PreviousReview(), () => MultipleReviewsAvailable);
            OpenSelectedReviewCommand = new RelayCommand(() =>
            {
                if (_selectedReview is null || CurrentSteamId.IsNullOrEmpty())
                {
                    return;
                }

                var reviewUrl = string.Format(
                    @"https://steamcommunity.com/profiles/{0}/recommended/{1}/",
                    _selectedReview.Author.Steamid, _currentSteamId);
                ProcessStarter.StartUrl(reviewUrl);
            });
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.CancelScheduledUpdate();
            if (!_areBindingValuesDefault)
            {
                ResetBindingValues();
                _areBindingValuesDefault = true;
            }

            //The GameContextChanged method is raised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                _activeViewAtCreation != _playniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (newContext is null)
            {
                ControlVisibility = Visibility.Collapsed;
                SettingsModel.Settings.IsControlVisible = false;
                return;
            }

            _currentGame = newContext;
            CurrentSteamId = string.Empty;
            CurrentSteamId = Steam.GetGameSteamId(_currentGame, true);
            if (CurrentSteamId.IsNullOrEmpty())
            {
                ControlVisibility = Visibility.Collapsed;
                SettingsModel.Settings.IsControlVisible = false;
                return;
            }

            base.ScheduleUpdate();
        }

        private void SetReviewInfo()
        {
            var playtimeAtReview = TimeSpan.FromMinutes(SelectedReview.Author.PlaytimeAtReview);
            FormattedPlaytime = string.Format(
                ResourceProvider.GetString("LOCReview_Viewer_ReviewPlaytimeFormat"),
                Math.Floor(playtimeAtReview.TotalHours),
                playtimeAtReview.ToString("mm"));

            var totalPlaytime = TimeSpan.FromMinutes(SelectedReview.Author.PlaytimeForever);
            TotalFormattedPlaytime = string.Format(
                ResourceProvider.GetString("LOCReview_Viewer_ReviewTotalPlaytimeFormat"),
                Math.Floor(totalPlaytime.TotalHours),
                totalPlaytime.ToString("mm"));

            ReviewHelpfulnessHelpful = string.Format(
                ResourceProvider.GetString("LOCReview_Viewer_ReviewHelpfulnessHelpfulFormat"),
                SelectedReview.VotesUp);
            ReviewHelpfulnessFunny = string.Format(
                ResourceProvider.GetString("LOCReview_Viewer_ReviewHelpfulnessFunnyFormat"),
                SelectedReview.VotesFunny);

            ReviewPostedDate = string.Format(
                ResourceProvider.GetString("LOCReview_Viewer_ReviewPostedDateLabel"),
                UnixTimeStampToFormattedString(SelectedReview.TimestampUpdated));
        }

        public static string UnixTimeStampToFormattedString(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime.ToString("dd MMM yyyy");
        }

        void NextReview()
        {
            if (_selectedReviewIndex == (Reviews.QuerySummary.NumReviews - 1))
            {
                SelectedReviewIndex = 0;
            }
            else
            {
                SelectedReviewIndex = _selectedReviewIndex + 1;
            }

            SelectedReviewDisplayIndex = SelectedReviewIndex + 1;
        }

        void PreviousReview()
        {
            if (_selectedReviewIndex == 0)
            {
                SelectedReviewIndex = Convert.ToInt32(Reviews.QuerySummary.NumReviews - 1);
            }
            else
            {
                SelectedReviewIndex = _selectedReviewIndex - 1;
            }
            SelectedReviewDisplayIndex = SelectedReviewIndex + 1;
        }

        private void OpenSteamLanguageSelector()
        {
            var window = _playniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = false,
                ShowCloseButton = false
            });

            window.Height = 450;
            window.Width = 450;
            window.Title = "Steam Languages Selector";

            window.Content = new SteamLanguageSelectorView();
            window.DataContext = new SteamLanguageSelectorViewModel(_queryOptions.SelectedLanguages, window);
            window.SizeToContent = SizeToContent.Width;
            window.Owner = _playniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
            base.ScheduleUpdate();
        }

        protected override async Task OnDebouncedUpdateAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            RunOnUI(() => ControlVisibility = Visibility.Visible);
            await UpdateReviewsContextAsync(token);
        }

        private void ResetBindingValues()
        {
            ThumbsUpVisibility = Visibility.Collapsed;
            ThumbsDownVisibility = Visibility.Collapsed;
            MultipleReviewsAvailable = false;
            SelectedReviewDisplayIndex = 0;
            TotalReviewsAvailable = 0;
            FormattedPlaytime = string.Empty;
            TotalFormattedPlaytime = string.Empty;
            SelectedReviewText = string.Empty;
            ReviewHelpfulnessHelpful = string.Empty;
            ReviewHelpfulnessFunny = string.Empty;
        }

        public async Task UpdateReviewsContextAsync(CancellationToken cancellationToken)
        {
            var contextId = Guid.NewGuid();
            var executingContextId = contextId;

            var requestQuery = _queryOptions.ToDomain();
            var requestKey = $"{CurrentSteamId}_{Serialization.ToJson(requestQuery)}";
            // To prevent making a request unnecessarily
            if (_activeRequestQueryKey == requestKey || CurrentSteamId.IsNullOrEmpty())
            {
                return;
            }

            SettingsModel.Settings.LastUsedQuery = requestQuery;
            _activeRequestQueryKey = requestKey;
            try
            {
                _inProgressDataUpdates++;
                UpdateSectionsVisibility();
                var reviewsResponse = await _steamReviewsCoordinator
                    .GetReviewsAsync(int.Parse(CurrentSteamId), requestQuery, false, cancellationToken: cancellationToken);
                // To detect if game changed while downloading data
                if (contextId != executingContextId || reviewsResponse is null)
                {
                    return;
                }

                base.RunOnUI(() =>
                {
                    ControlVisibility = Visibility.Visible;
                    SettingsModel.Settings.IsControlVisible = true;
                    Reviews = reviewsResponse;
                    if (Reviews.Success != 1)
                    {
                        _logger.Debug($"Response had Success value {Reviews.Success}.");
                        ResetBindingValues();
                        return;
                    }

                    try
                    {
                        if (Reviews.QuerySummary.NumReviews == 0)
                        {
                            _logger.Debug($"Response had 0 reviews.");
                            ResetBindingValues();
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Error obtaining reviews number.");
                        ResetBindingValues();
                        return;
                    }

                    MultipleReviewsAvailable = Reviews.QuerySummary.NumReviews > 1;
                    OnPropertiesChanged(nameof(NextReviewCommand), nameof(PreviousReviewCommand));
                    SelectedReviewIndex = 0;
                    SelectedReviewDisplayIndex = SelectedReviewIndex + 1;
                    TotalReviewsAvailable = Reviews.QuerySummary.NumReviews;
                    MainPanelVisibility = Visibility.Visible;
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while updating reviews data");
            }
            finally
            {
                _inProgressDataUpdates--;
                UpdateSectionsVisibility();
            }
        }

        private void UpdateSectionsVisibility()
        {
            RunOnUI(() =>
            {
                if (_inProgressDataUpdates > 0)
                {
                    DisplayProgressPanel = true;
                    DisplayReviewsPanel = false;
                    DisplayNoReviewsPanel = false;
                }
                else
                {
                    DisplayProgressPanel = false;
                    if (Reviews?.Reviews?.Count > 0 == true)
                    {
                        DisplayReviewsPanel = true;
                        DisplayNoReviewsPanel = false;
                    }
                    else
                    {
                        DisplayReviewsPanel = false;
                        DisplayNoReviewsPanel = true;
                    }
                }
            });
        }

    }
}