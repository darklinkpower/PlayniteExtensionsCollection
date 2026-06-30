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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private CancellationTokenSource _updateDataCancellationTokenSource;
        private long _updateDataVersion;
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
            CancelPendingUpdate();
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

        internal virtual object CreateMatchingSettingsSnapshot()
        {
            return null;
        }

        internal virtual IEnumerable<GameRelationSnapshot> GetMatchingGames(GameRelationSnapshot game, List<GameRelationSnapshot> libraryGames, object matchingSettings)
        {
            return Enumerable.Empty<GameRelationSnapshot>();
        }

        internal static IEnumerable<GameRelationSnapshot> GetMatchingGamesByRelation(
            GameRelationSnapshot game,
            List<GameRelationSnapshot> libraryGames,
            Func<GameRelationSnapshot, List<GameRelationValueSnapshot>> relationSelector)
        {
            var sourceRelations = relationSelector(game);
            if (!sourceRelations.HasItems())
            {
                return Enumerable.Empty<GameRelationSnapshot>();
            }

            var sourceHashSet = sourceRelations.Select(x => x.Id).ToHashSet();
            var similarGamesDict = new Dictionary<GameRelationSnapshot, string>();
            foreach (var otherGame in libraryGames)
            {
                if (otherGame.Id == game.Id)
                {
                    continue;
                }

                if (!game.Hidden && otherGame.Hidden)
                {
                    continue;
                }

                var commonItem = relationSelector(otherGame).FirstOrDefault(x => sourceHashSet.Contains(x.Id));
                if (commonItem != null)
                {
                    similarGamesDict.Add(otherGame, commonItem.Name);
                }
            }

            return similarGamesDict
                .OrderBy(pair => pair.Value)
                .ThenBy(x => x.Key.SortName)
                .Select(x => x.Key);
        }

        public async Task UpdateDataAsync()
        {
            _updateDataTimer.Stop();
            if (GameContext is null)
            {
                return;
            }

            CancelPendingUpdate();
            var updateVersion = Interlocked.Increment(ref _updateDataVersion);
            var updateCancellationTokenSource = new CancellationTokenSource();
            _updateDataCancellationTokenSource = updateCancellationTokenSource;
            var cancellationToken = updateCancellationTokenSource.Token;
            var controlName = GetType().Name;

            GameRelationSnapshot contextGame;
            List<GameRelationSnapshot> librarySnapshot;
            GameRelationsUpdateSettings updateSettings;
            try
            {
                contextGame = CreateGameSnapshot(GameContext);
                librarySnapshot = PlayniteApi.Database.Games.Select(CreateGameSnapshot).ToList();
                updateSettings = new GameRelationsUpdateSettings
                {
                    MaxItems = ControlSettings.MaxItems,
                    DisplayOnlyInstalled = ControlSettings.DisplayOnlyInstalled,
                    CoversHeight = Settings.CoversHeight,
                    MatchingSettings = CreateMatchingSettingsSnapshot()
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"GameRelations {controlName}: Failed to create game relation snapshot.");
                ClearCompletedUpdate(updateCancellationTokenSource);
                return;
            }

            var contextGameId = contextGame.Id;
            GameRelationsUpdateResult updateResult;
            try
            {
                updateResult = await Task.Run(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var matchedGames = GetMatchingGames(contextGame, librarySnapshot, updateSettings.MatchingSettings).ToList();

                    cancellationToken.ThrowIfCancellationRequested();
                    var filteredGames = matchedGames
                        .Where(g => g.IsInstalled || !updateSettings.DisplayOnlyInstalled)
                        .Take(updateSettings.MaxItems)
                        .ToList();

                    var gameWrappers = await MatchedGamesUtilities.GetGamesWrappersAsync(filteredGames, updateSettings.CoversHeight, cancellationToken);

                    return new GameRelationsUpdateResult
                    {
                        GameWrappers = gameWrappers
                    };
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                ClearCompletedUpdate(updateCancellationTokenSource);
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"GameRelations {controlName}: Failed to prepare related games for {contextGame.Name}.");
                ClearCompletedUpdate(updateCancellationTokenSource);
                return;
            }

            if (cancellationToken.IsCancellationRequested ||
                updateVersion != Interlocked.Read(ref _updateDataVersion) ||
                GameContext is null ||
                GameContext.Id != contextGameId ||
                !updateResult.GameWrappers.HasItems())
            {
                ClearCompletedUpdate(updateCancellationTokenSource);
                return;
            }

            MatchedGames = updateResult.GameWrappers;
            DisplayControl();
            ClearCompletedUpdate(updateCancellationTokenSource);
        }

        private void CancelPendingUpdate()
        {
            Interlocked.Increment(ref _updateDataVersion);
            var cancellationTokenSource = _updateDataCancellationTokenSource;
            if (cancellationTokenSource is null)
            {
                return;
            }

            _updateDataCancellationTokenSource = null;
            cancellationTokenSource.Cancel();
        }

        private void ClearCompletedUpdate(CancellationTokenSource updateCancellationTokenSource)
        {
            if (_updateDataCancellationTokenSource != updateCancellationTokenSource)
            {
                updateCancellationTokenSource.Dispose();
                return;
            }

            _updateDataCancellationTokenSource = null;
            updateCancellationTokenSource.Dispose();
        }

        private GameRelationSnapshot CreateGameSnapshot(Game game)
        {
            return new GameRelationSnapshot
            {
                Game = game,
                Id = game.Id,
                Name = game.Name,
                SortingName = game.SortingName,
                Hidden = game.Hidden,
                IsInstalled = game.IsInstalled,
                CoverImagePath = game.CoverImage.IsNullOrEmpty() ? null : PlayniteApi.Database.GetFullFilePath(game.CoverImage),
                TagIds = game.TagIds?.ToList() ?? new List<Guid>(),
                GenreIds = game.GenreIds?.ToList() ?? new List<Guid>(),
                CategoryIds = game.CategoryIds?.ToList() ?? new List<Guid>(),
                SeriesIds = game.SeriesIds?.ToList() ?? new List<Guid>(),
                Series = SnapshotRelationValues(game.Series),
                Developers = SnapshotRelationValues(game.Developers),
                Publishers = SnapshotRelationValues(game.Publishers)
            };
        }

        private static List<GameRelationValueSnapshot> SnapshotRelationValues<T>(IEnumerable<T> values) where T : DatabaseObject
        {
            if (values is null)
            {
                return new List<GameRelationValueSnapshot>();
            }

            return values.Select(x => new GameRelationValueSnapshot
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
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
        /// Calculate the Jaccard similarity between a list of items and a HashSet.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collections.</typeparam>
        /// <param name="listToMatch">The list of items to compare.</param>
        /// <param name="hashSet">The HashSet to compare against.</param>
        /// <returns>A match value between 0 and 1 representing the similarity between the elements.</returns>
        protected static double CalculateJaccardSimilarity<T>(IEnumerable<T> listToMatch, HashSet<T> hashSet)
        {
            if (listToMatch is null || !listToMatch.Any() || hashSet is null || hashSet.Count == 0)
            {
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
