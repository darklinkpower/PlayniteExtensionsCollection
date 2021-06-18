using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Web;
using GamePassCatalogBrowser.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections;

namespace GamePassCatalogBrowser.ViewModels
{
    class CatalogBrowserViewModel
    {
        private IPlayniteAPI PlayniteApi;
        private ICollectionView _gamePassGamesView;
        private ICollectionView _collectionsView;
        private string _filterString;
        private string _searchString;
        private bool _storeButtonEnabled;
        private bool _addButtonEnabled;
        private XboxLibraryHelper xboxLibraryHelper;

        private bool showGamesOnLibrary = true;
        public bool ShowGamesOnLibrary
        {
            get { return showGamesOnLibrary; }
            set
            {
                showGamesOnLibrary = value;
                _gamePassGamesView.Refresh();
            }
        }

        private GamePassGame selectedGamePassGame;
        public GamePassGame SelectedGamePassGame
        {
            get { return selectedGamePassGame; }
            set
            {
                selectedGamePassGame = value;
                AddButtonEnabled = GetAddButtonStatus(value);
            }
        }

        public bool GetAddButtonStatus(GamePassGame game)
        {
            if (game == null)
            {
                return false;
            }
            if (xboxLibraryHelper.GameIdsInLibrary.Contains(game.GameId))
            {
                return false;
            }
            if (game.ProductType == ProductType.Collection)
            {
                return false;
            }
            if (game.ProductType == ProductType.EaGame)
            {
                return false;
            }
            return true;
        }

        public bool StoreButtonEnabled
        {
            get { return _storeButtonEnabled; }
            set
            {
                _storeButtonEnabled = value;
            }
        }

        public bool AddButtonEnabled
        {
            get { return _addButtonEnabled; }
            set
            {
                _addButtonEnabled = value;
            }
        }

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                NotifyPropertyChanged("SearchString");
                _gamePassGamesView.Refresh();
            }
        }

        public ICollectionView GamePassGames
        {
            get { return _gamePassGamesView; }
        }

        public ICollectionView Collections
        {
            get { return _collectionsView; }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                NotifyPropertyChanged("FilterString");
                _gamePassGamesView.Refresh();
            }
        }

        private void NotifyPropertyChanged(string v)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            xboxLibraryHelper.Dispose();
        }

        public CatalogBrowserViewModel(List<GamePassGame> list, IPlayniteAPI api)
        {
            PlayniteApi = api;

            xboxLibraryHelper = new XboxLibraryHelper(api);

            IList<GamePassGame> gamePassGames = list;

            _gamePassGamesView = CollectionViewSource.GetDefaultView(gamePassGames);

            _gamePassGamesView.CurrentChanged += GamePassGameSelectionChanged;

            void GamePassGameSelectionChanged(object sender, EventArgs e)
            {
                // Not implemented
            }

            _gamePassGamesView.Filter = GamePassGameFilter;


            bool IsGameInCollection (GamePassGame game)
            {
                if (_filterString == "All")
                {
                    return true;
                }
                else if (string.IsNullOrEmpty(game.Category))
                {
                    return false;
                }
                else if (game.Category == _filterString)
                {
                    return true;
                }

                return false;
            }

            bool GameContainsString (GamePassGame game)
            {
                if (string.IsNullOrEmpty(_searchString))
                {
                    return IsGameInCollection(game);
                }
                else if (game.Name.ToLower().Contains(_searchString.ToLower()))
                {
                    return IsGameInCollection(game);
                }
                else
                {
                    return false;
                }
            }

            bool GamePassGameFilter(object item)
            {
                GamePassGame game = item as GamePassGame;

                if (showGamesOnLibrary == false)
                {
                    if (xboxLibraryHelper.GameIdsInLibrary.Contains(game.GameId))
                    {
                        return false;
                    }
                    else
                    {
                        return GameContainsString(game);
                    } 
                }

                return GameContainsString(game);
            }

            IList<string> collections = GetCollectionsList(list);

            _collectionsView = CollectionViewSource.GetDefaultView(collections);

            _collectionsView.CurrentChanged += CollectionSelectionChanged;

            List<string> GetCollectionsList(List<GamePassGame> collection)
            {
                var categoriesList = new List<string>()
                {
                        {"All"}
                };
                var categories = collection.Select(x => x.Category).
                    Where(a => a != null).Distinct().
                    OrderBy(c => c).ToList();
                foreach (string category in categories)
                {
                    categoriesList.Add(category);
                }
                return categoriesList;
            }

            void CollectionSelectionChanged(object sender, EventArgs e)
            {
                FilterString = Collections.CurrentItem.ToString();
            }
        }

        public RelayCommand<GamePassGame> StoreViewCommand
        {
            get => new RelayCommand<GamePassGame>((gamePassGame) =>
            {
                InvokeStoreView(gamePassGame);
            }, (gamePassGame) => gamePassGame != null);
        }

        private void InvokeStoreView(GamePassGame game)
        {
            if (game != null)
            {
                System.Diagnostics.Process.Start($"ms-windows-store://pdp?productId={game.ProductId}");
            }
        }

        public RelayCommand<GamePassGame> AddGameToLibraryCommand
        {
            get => new RelayCommand<GamePassGame>((gamePassGame) =>
            {
                var success = false;
                success = xboxLibraryHelper.AddGameToLibrary(gamePassGame, true);
                if (success == true)
                {
                    AddButtonEnabled = false;
                    Collections.Refresh();
                }
            }, (gamePassGame) => AddButtonEnabled);
        }
    }
}