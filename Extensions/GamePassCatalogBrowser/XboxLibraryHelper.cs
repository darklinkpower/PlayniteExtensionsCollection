using GamePassCatalogBrowser.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
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
        private HttpClient client;
        private Guid pluginId;
        private Platform pcPlatform;
        private Tag gameExpiredTag;
        private Tag gameAddedTag;
        public IEnumerable<Game> LibraryGames;
        public HashSet<string> GameIdsInLibrary;

        public void Dispose()
        {
            client.Dispose();
        }

        public XboxLibraryHelper(IPlayniteAPI api)
        {
            PlayniteApi = api;
            client = new HttpClient();
            pluginId = BuiltinExtensions.GetIdFromExtension(BuiltinExtension.XboxLibrary);
            RefreshLibraryItems();

            pcPlatform = PlayniteApi.Database.Platforms.Add("PC");
            gameExpiredTag = PlayniteApi.Database.Tags.Add("Game Pass game expired");
            gameAddedTag = PlayniteApi.Database.Tags.Add("Game Pass");
        }

        public async Task DownloadFile(string requestUri, string path)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    string fileToWriteTo = path;
                    using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during file download, url {requestUri}");
            }
        }

        public void RefreshLibraryItems()
        {
            LibraryGames = PlayniteApi.Database.Games.
                Where(g => g.PluginId == BuiltinExtensions.GetIdFromExtension(BuiltinExtension.XboxLibrary));

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

        public void AddGamePassListToLibrary (List<GamePassGame> gamePassGamesList)
        {
            foreach (GamePassGame game in gamePassGamesList.ToList())
            {
                if (GameIdsInLibrary.Contains(game.GameId) == false)
                {
                    AddGameToLibrary(game);
                }
            }

            RefreshLibraryItems();
        }

        public void AddTagToGame(Game game, Tag tag)
        {
            if (game.TagIds == null)
            {
                game.TagIds = new List<Guid>() { tag.Id };
            }
            else
            {
                game.TagIds.AddMissing(tag.Id);
            }
        }

        public bool AddGameToLibrary(GamePassGame game)
        {
            if (game == null)
            {
                return false;
            }

            var newGame = new Game
            {
                Name = game.Name,
                GameId = game.GameId,
                DeveloperIds = arrayToCompanyGuids(game.Developers),
                PublisherIds = arrayToCompanyGuids(game.Publishers),
                PluginId = pluginId,
                PlatformId = pcPlatform.Id,
                Description = StringToHtml(game.Description, true)
            };

            AddTagToGame(newGame, gameAddedTag);

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
                var fileName = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                var downloadPath = Path.Combine(PlayniteApi.Database.GetFileStoragePath(newGame.Id), fileName);
                DownloadFile(game.BackgroundImageUrl, downloadPath).GetAwaiter().GetResult();
                if (File.Exists(downloadPath))
                {
                    newGame.BackgroundImage = string.Format("{0}/{1}", newGame.Id.ToString(), fileName);
                }
            }

            PlayniteApi.Database.Games.Update(newGame);
            PlayniteApi.Dialogs.ShowMessage($"{game.Name} added to the Playnite library");
            GameIdsInLibrary.Add(game.GameId);

            return true;
        }
    }
}
