using GameRelations.Interfaces;
using GameRelations.Models;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GameRelations.PlayniteControls
{
    /// <summary>
    /// Interaction logic for GameRelationsBase.xaml
    /// </summary>
    public partial class GameRelationsBase : PluginUserControl, INotifyPropertyChanged
    {
        protected IPlayniteAPI PlayniteApi { get; private set; }
        public GameRelationsSettings Settings { get; private set; }
        public IGameRelationsControlSettings ControlSettings { get; private set; }
        private readonly DispatcherTimer _updateDataTimer;
        private readonly DesktopView _activeViewAtCreation;
        private static readonly ILogger _logger = LogManager.GetLogger();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private IEnumerable<MatchedGameWrapper> matchedGames = Enumerable.Empty<MatchedGameWrapper>();
        private ScrollViewer gamesScrollViewer;

        public IEnumerable<MatchedGameWrapper> MatchedGames
        {
            get => matchedGames;
            set
            {
                matchedGames = value;
                OnPropertyChanged();
                ScrollGamesContainerToStart();
            }
        }

        public GameRelationsBase(IPlayniteAPI playniteApi, GameRelationsSettings settings, IGameRelationsControlSettings controlSettings)
        {
            InitializeComponent();
            PlayniteApi = playniteApi;
            Settings = settings;
            DataContext = this;
            _updateDataTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(220)
            };

            ControlSettings = controlSettings;
            _updateDataTimer.Tick += new EventHandler(UpdateDataEvent);
            if (playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = playniteApi.MainView.ActiveDesktopView;
            }

            SetControlTextBlockStyle();
        }

        private void SetControlTextBlockStyle()
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle && baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                Resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        protected void SetSettings(IGameRelationsControlSettings newSettings)
        {
            ControlSettings = newSettings;
        }

        private async void UpdateDataEvent(object sender, EventArgs e)
        {
            await UpdateDataAsync();
        }

        public void RestartTimer()
        {
            _updateDataTimer.Stop();
            _updateDataTimer.Start();
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            _updateDataTimer.Stop();
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                _activeViewAtCreation != PlayniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            CollapseControl();
            if (!ControlSettings.IsEnabled || newContext is null)
            {
                return;
            }

            RestartTimer();
        }

        public virtual void CollapseControl()
        {
            Visibility = Visibility.Collapsed;
            ControlSettings.IsVisible = false;
        }

        public virtual void DisplayControl()
        {
            Visibility = Visibility.Visible;
            ControlSettings.IsVisible = true;
        }

        public virtual IEnumerable<Game> GetMatchingGames(Game game)
        {
            return Enumerable.Empty<Game>();
        }

        public async Task UpdateDataAsync()
        {
            _updateDataTimer.Stop();
            if (GameContext is null)
            {
                return;
            }

            var contextGame = GameContext;
            var matchedGames = GetMatchingGames(contextGame);
            if (GameContext is null || GameContext.Id != contextGame.Id || !matchedGames.HasItems())
            {
                return;
            }

            var filteredGames = matchedGames
                .Where(g => g.IsInstalled || !ControlSettings.DisplayOnlyInstalled)
                .Take(ControlSettings.MaxItems);

            var gameWrappers = await MatchedGamesUtilities.GetGamesWrappersAsync(filteredGames, Settings);
            if (GameContext is null || GameContext.Id != contextGame.Id || !gameWrappers.HasItems())
            {
                return;
            }

            MatchedGames = gameWrappers;
            DisplayControl();
        }

        private void ScrollGamesContainerToStart()
        {
            if (GamesContainerLb.Items.Count > 0)
            {
                if (gamesScrollViewer is null)
                {
                    gamesScrollViewer = BindingTools.FindVisualChild<ScrollViewer>(GamesContainerLb, "GamesContainerScrollViewer");
                }

                gamesScrollViewer?.ScrollToHome();
            }
        }

        /// <summary>
        /// Calculates the match value between a list of items and a HashSet by determining the percentage of common elements.
        /// </summary>
        /// <param name="listToMatch">The list of items to compare.</param>
        /// <param name="hashSet">The HashSet to compare against.</param>
        /// <returns>A match value between 0 and 1 representing the percentage of common elements.</returns>
        protected static decimal CalculateListHashSetMatchPercentage<T>(IEnumerable<T> listToMatch, HashSet<T> hashSet)
        {
            if (listToMatch is null || !listToMatch.Any())
            {
                if (hashSet.Count == 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            if (hashSet.Count == 0)
            {
                return 0;
            }

            var commonCount = listToMatch.Count(item => hashSet.Contains(item));
            if (commonCount == 0)
            {
                return 0;
            }

            // Rounding is done to prevent errors when doing arithmetic operations
            var matchPercent = Math.Round(commonCount / (decimal)Math.Max(listToMatch.Count(), hashSet.Count), 3);
            return matchPercent;
        }

        /// <summary>
        /// Calculate the Jaccard similarity between a list of items and a HashSet.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collections.</typeparam>
        /// <param name="listToMatch">The list of items to compare.</param>
        /// <param name="hashSet">The HashSet to compare against.</param>
        /// <returns>A match value between 0 and 1 representing the similarity between the elements.</returns>
        protected static double CalculateJaccardSimilarity<T>(IEnumerable<T> listToMatch, HashSet<T> hashSet)
        {
            if (listToMatch is null || !listToMatch.Any())
            {
                //If both games have 0 items in this collection, give some pity similarity
                if (hashSet is null || hashSet.Count == 0)
                {
                    return 0.9D;
                }

                return 0;
            }

            var uniqueCount = hashSet.Count;
            var commonCount = 0;
            foreach (var item in listToMatch)
            {
                if (hashSet.Contains(item))
                {
                    commonCount++;
                }
                else
                {
                    uniqueCount++;
                }
            }

            // Rounding is done to prevent errors when doing arithmetic operations
            var similarity = Math.Round((double)commonCount / uniqueCount, 3);
            return similarity;
        }

        /// <summary>
        /// Checks if there is any common item between a list and a HashSet.
        /// </summary>
        /// <param name="listToMatch">The list to check for common items.</param>
        /// <param name="hashSet">The HashSet to check against.</param>
        /// <returns>Returns the common item if found, default value otherwise</returns>
        protected static T GetAnyCommonItem<T>(List<T> listToMatch, HashSet<T> hashSet)
        {
            if (listToMatch is null || listToMatch.Count == 0 || hashSet.Count == 0)
            {
                return default;
            }

            foreach (var item in listToMatch)
            {
                if (hashSet.Contains(item))
                {
                    return item;
                }
            }

            return default;
        }

        /// <summary>
        /// Checks if any item in the provided list is contained within the HashSet.
        /// </summary>
        /// <typeparam name="T">The type of elements in the lists and the HashSet.</typeparam>
        /// <param name="listToMatch">The list of items to compare against the HashSet.</param>
        /// <param name="hashSet">The HashSet to check for item presence.</param>
        /// <returns>True if any item in the list is found in the HashSet, otherwise false.
        /// Returns false if the list is null or empty.</returns>
        protected static bool HashSetContainsAnyItem<T>(IEnumerable<T> listToMatch, HashSet<T> hashSet)
        {
            if (listToMatch is null || !listToMatch.Any())
            {
                return false;
            }

            return listToMatch.Any(x => hashSet.Contains(x));
        }

        /// <summary>
        /// Retrieves items from the provided list that are not present in the specified HashSet.
        /// </summary>
        /// <typeparam name="T">The type of elements in the lists and the HashSet.</typeparam>
        /// <param name="listToMatch">The list of items to filter against the HashSet.</param>
        /// <param name="hashSet">The HashSet to compare the list against.</param>
        /// <returns>
        /// An IEnumerable containing items from the list that are not present in the HashSet.
        /// Returns an empty IEnumerable if the provided list is null.
        /// </returns>
        protected static IEnumerable<T> GetItemsNotInHashSet<T>(IEnumerable<T> listToMatch, HashSet<T> hashSet)
        {
            return listToMatch?.Where(x => !hashSet.Contains(x)) ?? Enumerable.Empty<T>();
        }

        public RelayCommand<object> OpenGameDetailsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                if (a is MatchedGameWrapper gameWrapper)
                {
                    PlayniteApi.MainView.SelectGame(gameWrapper.Game.Id);
                }
            });
        }


    }
}