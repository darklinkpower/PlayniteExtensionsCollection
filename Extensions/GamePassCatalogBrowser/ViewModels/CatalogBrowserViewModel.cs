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

        private GamePassGame selectedGamePassGame;
        public GamePassGame SelectedGamePassGame
        {
            get => selectedGamePassGame;
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
            if (PlayniteApi.Database.Games.Where(g => g.PluginId == BuiltinExtensions.GetIdFromExtension(BuiltinExtension.XboxLibrary)).
                Any(g => g.GameId.Equals(game.GameId)))
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

        public CatalogBrowserViewModel(List<GamePassGame> list, IPlayniteAPI api)
        {
            PlayniteApi = api;

            IList<GamePassGame> gamePassGames = list;

            _gamePassGamesView = CollectionViewSource.GetDefaultView(gamePassGames);

            _gamePassGamesView.CurrentChanged += GamePassGameSelectionChanged;

            void GamePassGameSelectionChanged(object sender, EventArgs e)
            {
                if (sender == null)
                {
                    _addButtonEnabled = false;
                }
            }

            _gamePassGamesView.Filter = GamePassGameFilter;


            bool isGameInCollection (GamePassGame game)
            {
                if (_filterString == "All")
                {
                    return true;
                }
                else if (game.Categories == null)
                {
                    return false;
                }
                else
                {
                    return game.Categories.Any(x => x.Equals(_filterString));
                }
            }

            bool GamePassGameFilter(object item)
            {
                GamePassGame game = item as GamePassGame;

                if (string.IsNullOrEmpty(_searchString))
                {
                    return isGameInCollection(game);
                }
                else if (game.Name.ToLower().Contains(_searchString.ToLower()))
                {
                    return isGameInCollection(game);
                }
                else
                {
                    return false;
                }
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
                InvokeAddGameToLibrary(gamePassGame);
            }, (gamePassGame) => AddButtonEnabled);
        }

        public List<Guid> arrayToCompanyGuids(string [] array)
        {
            var list = new List<Guid>();
            foreach (string str in array)
            {
                var company = PlayniteApi.Database.Companies.Add(str);
                list.Add(company.Id);
            }

            return list;
        }

        public static string StringToHtml(string s, bool nofollow)
        {
            s = WebUtility.HtmlEncode(s);
            string[] paragraphs = s.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
            StringBuilder sb = new StringBuilder();
            foreach (string par in paragraphs)
            {
                sb.AppendLine("<p>");
                string p = par.Replace(Environment.NewLine, "<br />\r\n");
                if (nofollow)
                {
                    p = Regex.Replace(p, @"\[\[(.+)\]\[(.+)\]\]", "<a href=\"$2\" rel=\"nofollow\">$1</a>");
                    p = Regex.Replace(p, @"\[\[(.+)\]\]", "<a href=\"$1\" rel=\"nofollow\">$1</a>");
                }
                else
                {
                    p = Regex.Replace(p, @"\[\[(.+)\]\[(.+)\]\]", "<a href=\"$2\">$1</a>");
                    p = Regex.Replace(p, @"\[\[(.+)\]\]", "<a href=\"$1\">$1</a>");
                    sb.AppendLine(p);
                }
                sb.AppendLine(p);
                sb.AppendLine("</p>");
            }
            return sb.ToString();
        }

        private void InvokeAddGameToLibrary(GamePassGame game)
        {
            if (game != null)
            {
                var newGame = new Game
                {
                    Name = game.Name,
                    GameId = game.GameId,
                    DeveloperIds = arrayToCompanyGuids(game.Developers),
                    PublisherIds = arrayToCompanyGuids(game.Publishers),
                    PluginId = BuiltinExtensions.GetIdFromExtension(BuiltinExtension.XboxLibrary),
                    PlatformId = PlayniteApi.Database.Platforms.Add("PC").Id,
                    Description = StringToHtml(game.Description, true)
                };

                PlayniteApi.Database.Games.Add(newGame);

                if (File.Exists(game.CoverImage))
                {
                    var copiedImage = PlayniteApi.Database.AddFile(game.CoverImage, newGame.Id);
                    newGame.CoverImage = copiedImage;
                }

                if (File.Exists(game.Icon))
                {
                    var copiedImage = PlayniteApi.Database.AddFile(game.Icon, newGame.Id);
                    newGame.Icon = copiedImage;
                }

                if (string.IsNullOrEmpty(game.BackgroundImageUrl) == false)
                {
                    using (var webClient = new WebClient())
                    {
                        try
                        {
                            var fileName = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                            var downloadPath = Path.Combine(PlayniteApi.Database.GetFileStoragePath(newGame.Id), fileName);
                            webClient.DownloadFile(game.BackgroundImageUrl, downloadPath);
                            newGame.BackgroundImage = string.Format("{0}/{1}", newGame.Id.ToString(), fileName);
                        }
                        catch
                        {

                        }
                    }
                }

                PlayniteApi.Database.Games.Update(newGame);
                AddButtonEnabled = false;
                PlayniteApi.Dialogs.ShowMessage($"{game.Name} added to the Playnite library");
            }
        }
    }
}