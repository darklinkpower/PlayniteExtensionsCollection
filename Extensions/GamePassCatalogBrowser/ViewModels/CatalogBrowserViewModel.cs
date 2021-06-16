using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using GamePassCatalogBrowser.Models;
using Playnite.SDK;

namespace GamePassCatalogBrowser.ViewModels
{
    class CatalogBrowserViewModel
    {
        private ICollectionView _gamePassGamesView;
        private ICollectionView _collectionsView;
        private string _filterString;

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

        public CatalogBrowserViewModel(List<GamePassGame> list)
        {
            IList<GamePassGame> gamePassGames = list;

            _gamePassGamesView = CollectionViewSource.GetDefaultView(gamePassGames);

            _gamePassGamesView.CurrentChanged += GamePassGameSelectionChanged;

            void GamePassGameSelectionChanged(object sender, EventArgs e)
            {
                // React to the changed selection
            }

            _gamePassGamesView.Filter = GamePassGameFilter;

            bool GamePassGameFilter(object item)
            {
                GamePassGame game = item as GamePassGame;

                if (_filterString == "All")
                {
                    return true;
                }
                else if (game.Categories == null)
                {
                    return false;
                }
                else if (game.Categories.Any(x => x.Equals(_filterString)))
                {
                    return true;
                }

                return false;
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
                var categories = collection.Select(x => x.Categories).
                    Where(a => a != null && a.Any()).SelectMany(a => a).
                    Distinct().OrderBy(c => c).ToList();
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

        public void GetCollectionsLists(List<GamePassGame> collection)
        {
            var CategoriesList = new Dictionary<string, string>()
                    {
                        { "All", "All" }
                    };
            var categories = collection.Select(x => x.Categories).
                Where(a => a != null && a.Any()).SelectMany(a => a).
                Distinct().OrderBy(c => c).ToList();
            foreach (string category in categories)
            {
                CategoriesList.Add(category, category);
            }
        }

        public RelayCommand<object> StoreViewCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                InvokeStoreView();
            });
        }

        private void InvokeStoreView()
        {
            var game = GamePassGames.CurrentItem as GamePassGame;
            if (game != null)
            {
                var uri = string.Format("ms-windows-store://pdp?productId={0}", game.ProductId);
                System.Diagnostics.Process.Start(uri);
            }
        }
    }
}