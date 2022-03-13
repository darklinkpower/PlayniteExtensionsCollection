using GamePassCatalogBrowser.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using PluginsCommon.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GamePassCatalogBrowser
{
    class XboxLibraryHelper
    {
        private IPlayniteAPI PlayniteApi;
        private ILogger logger = LogManager.GetLogger();
        private Guid pluginId;
        private List<Guid> platformsList;
        private Tag gameExpiredTag;
        private Tag gameAddedTag;
        private GameSource source;
        private GameSource sourceXbox;
        public IEnumerable<Game> LibraryGames;
        public HashSet<string> GameIdsInLibrary;

        public XboxLibraryHelper(IPlayniteAPI api)
        {
            PlayniteApi = api;
            pluginId = Guid.Parse("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287");
            RefreshLibraryItems();

            var pcPlatform = PlayniteApi.Database.Platforms.Add("PC (Windows)");
            platformsList = new List<Guid> { pcPlatform.Id };
            gameExpiredTag = PlayniteApi.Database.Tags.Add("Game Pass (Formerly on)");
            gameAddedTag = PlayniteApi.Database.Tags.Add("Game Pass");
            source = PlayniteApi.Database.Sources.Add("Xbox Game Pass");
            sourceXbox = PlayniteApi.Database.Sources.Add("Xbox");
        }

        public void RefreshLibraryItems()
        {
            LibraryGames = PlayniteApi.Database.Games.
                Where(g => g.PluginId.Equals(pluginId));

            var gamesOnLibrary = new HashSet<string>();
            foreach (Game game in LibraryGames)
            {
                gamesOnLibrary.Add(game.GameId);
            }

            GameIdsInLibrary = gamesOnLibrary;
        }

        private List<Guid> arrayToCompanyGuids(string[] array)
        {
            var list = new List<Guid>();
            foreach (string str in array)
            {
                var company = PlayniteApi.Database.Companies.Add(str);
                list.Add(company.Id);
            }

            return list;
        }

        private static string StringToHtml(string s, bool nofollow)
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

        public Game GetLibraryGameFromGamePassGame(GamePassGame gamePassGame)
        {
            return PlayniteApi.Database.Games
                .FirstOrDefault(g => g.PluginId.Equals(pluginId) &&
                g.GameId.Equals(gamePassGame.GameId) &&
                g.SourceId != null &&
                g.SourceId.Equals(source.Id));
        }

        public Game GetLibraryGameFromGamePassGameAnySource(GamePassGame gamePassGame)
        {
            return PlayniteApi.Database.Games
                .FirstOrDefault(g => g.PluginId.Equals(pluginId) &&
                g.GameId.Equals(gamePassGame.GameId));
        }

        public bool RemoveGamePassGame(GamePassGame gamePassGame)
        {
            var game = GetLibraryGameFromGamePassGame(gamePassGame);
            if (game == null)
            {
                return false;
            }

            if (game.Playtime > 0)
            {
                game.SourceId = sourceXbox.Id;
                PlayniteApi.Database.Games.Update(game);
                return false;
            }
            else
            {
                PlayniteApi.Database.Games.Remove(game.Id);
                GameIdsInLibrary.Remove(game.GameId);
                return true;
            }
        }

        public void AddExpiredTag(GamePassGame gamePassGame)
        {
            var game = GetLibraryGameFromGamePassGame(gamePassGame);
            if (game != null)
            {
                PlayniteUtilities.AddTagToGame(PlayniteApi, game, gameExpiredTag);
                game.SourceId = sourceXbox.Id;
                PlayniteApi.Database.Games.Update(game);
            }
        }

        public int AddGamePassListToLibrary (List<GamePassGame> gamePassGamesList)
        {
            var i = 0;
            foreach (GamePassGame game in gamePassGamesList.ToList())
            {
                if (GameIdsInLibrary.Contains(game.GameId) == false)
                {
                    var success = AddGameToLibrary(game, false);
                    if (success == true)
                    {
                        i++;
                    }
                }
            }

            RefreshLibraryItems();

            return i;
        }

        public bool AddGameToLibrary(GamePassGame game, bool showGameAddDialog)
        {
            if (game == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(game.GameId))
            {
                return false;
            }

            if (game.ProductType != ProductType.Game)
            {
                return false;
            }

            var existingGame = GetLibraryGameFromGamePassGameAnySource(game);
            if (existingGame != null)
            {
                return false;
            }

            var newGame = new Game
            {
                Name = game.Name,
                GameId = game.GameId,
                DeveloperIds = arrayToCompanyGuids(game.Developers),
                PublisherIds = arrayToCompanyGuids(game.Publishers),
                TagIds = new List<Guid>() { gameAddedTag.Id },
                PluginId = pluginId,
                PlatformIds = platformsList,
                Description = StringToHtml(game.Description, true),
                SourceId = source.Id,
                ReleaseDate = new ReleaseDate(game.ReleaseDate)
            };


            PlayniteApi.Database.Games.Add(newGame);
            if (FileSystem.FileExists(game.CoverImage))
            {
                var copiedImage = PlayniteApi.Database.AddFile(game.CoverImage, newGame.Id);
                newGame.CoverImage = copiedImage;
            }

            if (FileSystem.FileExists(game.Icon))
            {
                var copiedImage = PlayniteApi.Database.AddFile(game.Icon, newGame.Id);
                newGame.Icon = copiedImage;
            }

            if (!game.BackgroundImageUrl.IsNullOrEmpty())
            {
                var fileName = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                var downloadPath = Path.Combine(PlayniteApi.Database.GetFileStoragePath(newGame.Id), fileName);
                HttpDownloader.DownloadFile($"{game.BackgroundImageUrl}?mode=scale&q=90&h=1080&w=1920", downloadPath);
                if (FileSystem.FileExists(downloadPath))
                {
                    newGame.BackgroundImage = string.Format("{0}/{1}", newGame.Id.ToString(), fileName);
                }
            }

            PlayniteApi.Database.Games.Update(newGame);
            GameIdsInLibrary.Add(game.GameId);

            if (showGameAddDialog)
            {
                PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCGamePass_Catalog_Browser_AddGameResultsMessage"), game.Name));
            }

            return true;
        }
    }
}
