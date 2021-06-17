using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GamePassCatalogBrowser.Models;
using System.Text.RegularExpressions;

namespace GamePassCatalogBrowser.Services
{
    class GamePassCatalogBrowserService
    {
        private IPlayniteAPI playniteApi;
        private ILogger logger = LogManager.GetLogger();
        private HttpClient client;
        private readonly string userDataPath = string.Empty;
        private readonly string cachePath = string.Empty;
        private readonly string imageCachePath = string.Empty;
        private readonly string gameDataCachePath = string.Empty;
        public const string gamepassCatalogApiBaseUrl = @"https://catalog.gamepass.com/sigls/v2?id=fdd9e2a7-0fee-49f6-ad69-4354098401ff&language={0}&market={1}";
        public const string catalogDataApiBaseUrl = @"https://displaycatalog.mp.microsoft.com/v7.0/products?bigIds={0}&market={1}&languages={2}&MS-CV=F.1";
        private readonly string gamepassCatalogApiUrl = string.Empty;
        private readonly string languageCode = string.Empty;
        private readonly string countryCode = string.Empty;
        private readonly bool notifyCatalogUpdates;
        private readonly bool addExpiredTagToGames;
        private readonly bool addNewGames;
        private readonly bool removeExpiredGames;
        public XboxLibraryHelper xboxLibraryHelper;

        public void Dispose()
        {
            client.Dispose();
            xboxLibraryHelper.Dispose();
        }

        private void ClearFolder(string folderPath)
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                ClearFolder(directory.FullName);
                directory.Delete();
            }
        }

        private void DeleteFileFromPath (string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                try
                {
                    fileInfo.Delete();
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error deleting file {filePath}");
                }
            }
        }

        public void DeleteCache()
        {
            ClearFolder(cachePath);
        }

        public GamePassCatalogBrowserService(IPlayniteAPI api, string dataPath, bool _notifyCatalogUpdates, bool _addExpiredTagToGames, bool _addNewGames, bool _removeExpiredGames, string _languageCode = "en-us", string _countryCode = "US")
        {
            playniteApi = api;
            userDataPath = dataPath;
            notifyCatalogUpdates = _notifyCatalogUpdates;
            addExpiredTagToGames = _addExpiredTagToGames;
            addNewGames = _addNewGames;
            removeExpiredGames = _removeExpiredGames;
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            cachePath = Path.Combine(userDataPath, "cache");
            imageCachePath = Path.Combine(cachePath, "images");
            gameDataCachePath = Path.Combine(cachePath, "gamesCache.json");
            languageCode = _languageCode;
            countryCode = _countryCode;
            gamepassCatalogApiUrl = string.Format(gamepassCatalogApiBaseUrl, languageCode, countryCode);
            
            xboxLibraryHelper = new XboxLibraryHelper(api);
        }

        public List<GamePassCatalogProduct> GetGamepassCatalog()
        {
            var gamePassGames = new List<GamePassCatalogProduct>();
            try
            {
                var response = client.GetAsync(gamepassCatalogApiUrl);
                var contents = response.Result.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(contents.Result))
                {
                    return gamePassGames;
                }
                var gamePassCatalog = JsonConvert.DeserializeObject<List<GamePassCatalogProduct>>(contents.Result);
                foreach (GamePassCatalogProduct gamePassProduct in gamePassCatalog)
                {
                    if (gamePassProduct.Id == null)
                    {
                        continue;
                    }
                    gamePassGames.Add(gamePassProduct);
                }
                return gamePassGames;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error in ApiRequest {gamepassCatalogApiUrl}");
                return gamePassGames;
            }
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

        private static string RegexRemoveOnEnd(string input, string search)
        {
            string result = Regex.Replace(
                input,
                string.Format("{0}$", Regex.Escape(search)),
                "",
                RegexOptions.IgnoreCase
            );
            return result;
        }

        public string NormalizeGameName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            str = str.Replace("(PC)", "").
                Replace("(Windows)", "").
                Replace("(Windows 10)", "").
                Replace("for Windows 10", "").
                Replace("- Windows 10", "").
                Replace("Windows 10", "").
                Replace(@"®", "").
                Replace(@"™", "").
                Replace(@"©", "").
                Trim();

            string[] linesToRemove =
            {
                ": Windows Edition",
                " Windows Edition",
                " Windows 10",
                "- PC",
                " PC",
                " Windows",
                " Win10",
                " Win 10"
            };

            foreach (string lineToRemove in linesToRemove)
            {
                str = RegexRemoveOnEnd(str, lineToRemove);
            }

            return str.Trim();
        }

        public string[] companiesStringToArray(string companiesString)
        {
            var companiesList = new List<string>();

            // Replace ", Inc" for ". Inc" and other terms so it doesn't conflict in the split operation
            companiesString = companiesString.
                Replace("Developed by ", "").
                Replace(", Inc", ". Inc").
                Replace(", INC", ". INC").
                Replace(", inc", ". inc").
                Replace(", Llc", ". Llc").
                Replace(", LLC", ". LLC").
                Replace(", Ltd", ". Ltd").
                Replace(", LTD", ". LTD");

            string[] stringSeparators = new string[] { ", ", "|", "/", "+", " and ", " & " };
            var splitArray = companiesString.Split(stringSeparators, StringSplitOptions.None);
            foreach (string splittedString in splitArray)
            {
                companiesList.Add(splittedString.
                    Replace(". Inc", ", Inc").
                    Replace(". INC", ", INC").
                    Replace(". inc", ", inc").
                    Replace(". Llc", ", Llc").
                    Replace(". LLC", ", LLC").
                    Replace(". Ltd", ", Ltd").
                    Replace(". LTD", ", LTD").
                    Trim());
            }

            return companiesList.ToArray();
        }

        public List<GamePassGame> GetGamePassGamesList()
        {
            // Try to create cache directory in case it doesn't exist
            Directory.CreateDirectory(imageCachePath);

            List<GamePassGame> gamePassGamesList = new List<GamePassGame>();
            var gamePassCatalog = GetGamepassCatalog();

            var idsForDataRequest = new List<string>();
            if (File.Exists(gameDataCachePath) == false)
            {
                foreach (GamePassCatalogProduct gamePassProduct in gamePassCatalog)
                {
                    idsForDataRequest.Add(gamePassProduct.Id);
                }
            }
            else
            {
                gamePassGamesList = JsonConvert.DeserializeObject<List<GamePassGame>>(File.ReadAllText(gameDataCachePath));

                // Check for games removed from the service
                var gamesRemoved = false;
                foreach (GamePassGame game in gamePassGamesList.ToList())
                {
                    if (gamePassCatalog.Any(x => x.Id == game.ProductId) == false)
                    {
                        string[] gameFilesPaths =
                        {
                            Path.Combine(imageCachePath, game.BackgroundImage),
                            Path.Combine(imageCachePath, game.CoverImage),
                            Path.Combine(imageCachePath, game.CoverImageLowRes),
                            Path.Combine(imageCachePath, game.Icon)
                        };

                        foreach (string filePath in gameFilesPaths)
                        {
                            DeleteFileFromPath(filePath);
                        }

                        var gameRemoved = false;
                        if (removeExpiredGames == true)
                        {
                            gameRemoved = xboxLibraryHelper.RemoveGamePassGame(game);
                            if (gameRemoved == true)
                            {
                                // Notify user that game has been removed from the library
                                playniteApi.Notifications.Add(new NotificationMessage(
                                Guid.NewGuid().ToString(),
                                    $"{game.Name} has been removed from the Game Pass catalog and Playnite library",
                                    NotificationType.Info));
                            }
                        }
                        else if (addExpiredTagToGames == true)
                        {
                            xboxLibraryHelper.AddExpiredTag(game);
                        }

                        if (notifyCatalogUpdates == true && gameRemoved == false)
                        {
                            // Notify user that game has been removed
                            playniteApi.Notifications.Add(new NotificationMessage(
                            Guid.NewGuid().ToString(),
                                $"{game.Name} has been removed from the Game Pass catalog",
                                NotificationType.Info));
                        }

                        gamePassGamesList.Remove(game);
                        gamesRemoved = true;
                    }
                }

                if (gamesRemoved == true)
                {
                    File.WriteAllText(gameDataCachePath, JsonConvert.SerializeObject(gamePassGamesList));
                }

                foreach (GamePassCatalogProduct gamePassProduct in gamePassCatalog)
                {
                    if (gamePassGamesList.Any(x => x.ProductId.Equals(gamePassProduct.Id)) == false)
                    {
                        idsForDataRequest.Add(gamePassProduct.Id);
                    }
                }
            }

            if (idsForDataRequest.Count > 0)
            {
                var bigIdsParam = string.Join(",", idsForDataRequest);
                string catalogDataApiUrl = string.Format(catalogDataApiBaseUrl, bigIdsParam, countryCode, languageCode);
                try
                {
                    var response = client.GetAsync(string.Format(catalogDataApiUrl));
                    var contents = response.Result.Content.ReadAsStringAsync();
                    if (response.Status == TaskStatus.RanToCompletion)
                    {
                        var catalogData = JsonConvert.DeserializeObject<CatalogData>(contents.Result);
                        foreach (CatalogProduct product in catalogData.Products)
                        {
                            if (product.Properties.PackageFamilyName == null)
                            {
                                continue;
                            }

                            var gamePassGame = new GamePassGame
                            {
                                BackgroundImage = string.Format("{0}.jpg", Guid.NewGuid().ToString()),
                                BackgroundImageUrl = string.Format("https:{0}", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.SuperHeroArt)?.FirstOrDefault()?.Uri),
                                Categories = product.Properties.Categories,
                                CoverImage = string.Format("{0}.jpg", Guid.NewGuid().ToString()),
                                CoverImageUrl = string.Format("https:{0}", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.Poster)?.FirstOrDefault()?.Uri),
                                CoverImageLowRes = string.Format("{0}.jpg", Guid.NewGuid().ToString()),
                                Description = product.LocalizedProperties[0].ProductDescription,
                                GameId = product.Properties.PackageFamilyName,
                                Name = NormalizeGameName(product.LocalizedProperties[0].ProductTitle),
                                ProductId = product.ProductId,
                                Publishers = companiesStringToArray(product.LocalizedProperties[0].PublisherName)
                            };

                            if (string.IsNullOrEmpty(product.LocalizedProperties[0].DeveloperName))
                            {
                                gamePassGame.Developers = gamePassGame.Publishers;
                            }
                            else
                            {
                                gamePassGame.Developers = companiesStringToArray(product.LocalizedProperties[0].DeveloperName);
                            }

                            if (product.LocalizedProperties[0].Images.Any(x => x.ImagePurpose == ImagePurpose.BoxArt) == true)
                            {
                                gamePassGame.Icon = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                                gamePassGame.IconUrl = string.Format("https:{0}", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.BoxArt)?.FirstOrDefault()?.Uri);
                            }
                            else if (product.LocalizedProperties[0].Images.Any(x => x.ImagePurpose == ImagePurpose.Logo) == true)
                            {
                                gamePassGame.Icon = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                                gamePassGame.IconUrl = string.Format("https:{0}", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.Logo)?.FirstOrDefault()?.Uri);
                            }

                            gamePassGamesList.Add(gamePassGame);

                            // Notify user that game has been added
                            if (notifyCatalogUpdates == true)
                            {
                                playniteApi.Notifications.Add(new NotificationMessage(
                                    Guid.NewGuid().ToString(),
                                    $"{gamePassGame.Name} has been added to the Game Pass catalog",
                                    NotificationType.Info));
                            }
                        }

                        File.WriteAllText(gameDataCachePath, JsonConvert.SerializeObject(gamePassGamesList));
                    }
                    else
                    {
                        logger.Info($"Request {catalogDataApiUrl} not completed");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error in ApiRequest {catalogDataApiUrl}");
                }
            }

            return BuildCacheAndSetFullPaths(gamePassGamesList);
        }

        public List<GamePassGame> BuildCacheAndSetFullPaths(List<GamePassGame> cacheGamesList)
        {
            foreach (GamePassGame game in cacheGamesList)
            {
                game.CoverImageLowRes = Path.Combine(imageCachePath, game.CoverImageLowRes);
                if (!File.Exists(game.CoverImageLowRes))
                {
                    DownloadFile(string.Format("{0}?mode=scale&q=90&h=300&w=200", game.CoverImageUrl), game.CoverImageLowRes).GetAwaiter().GetResult();
                }

                game.CoverImage = Path.Combine(imageCachePath, game.CoverImage);
                if (!File.Exists(game.CoverImage))
                {
                    DownloadFile(string.Format("{0}?mode=scale&q=90&h=900&w=600", game.CoverImageUrl), game.CoverImage).GetAwaiter().GetResult();
                }

                if (game.Icon != null)
                {
                    game.Icon = Path.Combine(imageCachePath, game.Icon);
                    if (!File.Exists(game.Icon))
                    {
                        DownloadFile(string.Format("{0}?mode=scale&q=90&h=128&w=128", game.IconUrl), game.Icon).GetAwaiter().GetResult();
                    }
                }
            }

            cacheGamesList.Sort((x, y) => x.Name.CompareTo(y.Name));
            return cacheGamesList;
        }
    }
}