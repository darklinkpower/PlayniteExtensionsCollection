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

        public void Dispose()
        {
            client.Dispose();
        }

        public GamePassCatalogBrowserService(IPlayniteAPI api, string dataPath, string _languageCode = "en-us", string _countryCode = "US")
        {
            playniteApi = api;
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            userDataPath = dataPath;
            cachePath = Path.Combine(userDataPath, "cache");
            imageCachePath = Path.Combine(cachePath, "images");
            gameDataCachePath = Path.Combine(cachePath, "gamesCache.json");
            languageCode = _languageCode;
            countryCode = _countryCode;
            gamepassCatalogApiUrl = string.Format(gamepassCatalogApiBaseUrl, languageCode, countryCode);

            // Try to create cache directory in case it doesn't exist
            Directory.CreateDirectory(imageCachePath);
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
                playniteApi.Notifications.Add(new NotificationMessage(
                    "Error",
                    $"Game Pass Browser. Error downloading Game Pass catalog: {e.Message}",
                    NotificationType.Info));
            }
        }

        public void BuildCache (List<GamePassGame> cacheGamesList)
        {
            foreach (GamePassGame game in cacheGamesList)
            {
                var coverLowResPath = Path.Combine(imageCachePath, game.CoverLowRes);
                if (!File.Exists(coverLowResPath))
                {
                    DownloadFile(game.CoverLowResUrl, coverLowResPath).GetAwaiter().GetResult();
                }

                var coverPath = Path.Combine(imageCachePath, game.Cover);
                if (!File.Exists(coverPath))
                {
                    DownloadFile(game.CoverUrl, coverPath).GetAwaiter().GetResult();
                }
                if (game.Icon != null)
                {
                    var iconPath = Path.Combine(imageCachePath, game.Icon);
                    if (!File.Exists(iconPath))
                    {
                        DownloadFile(game.IconUrl, iconPath).GetAwaiter().GetResult();
                    }
                }
            }
        }

        public string CleanGameName(string str)
        {
            return str.Replace("(PC)", "").
                Replace("(Windows)", "").
                Replace("for Windows 10", "").
                Replace("- Windows 10", "").
                Replace(@"®", "").
                Replace(@"™", "").
                Replace(@"©", "").
                Trim();
        }

        public List<GamePassGame> GetGamePassGamesList()
        {
            List<GamePassGame> gamePassGamesList = new List<GamePassGame>();
            var idsForDataRequest = new List<string>();
            var gamePassCatalog = GetGamepassCatalog();
            if (File.Exists(gameDataCachePath))
            {
                gamePassGamesList = JsonConvert.DeserializeObject<List<GamePassGame>>(File.ReadAllText(gameDataCachePath));

                // Check for games removed from the service
                foreach (GamePassGame game in gamePassGamesList)
                {
                    bool gamesRemoved = false;
                    if (gamePassCatalog.Any(x => x.Id == game.ProductId) == false)
                    {
                        // Notify user that game has been removed
                        playniteApi.Notifications.Add(new NotificationMessage(
                            "CatalogRemoved",
                            $"{game.GameName} has been removed from the Game Pass catalog",
                            NotificationType.Info));

                        string[] gameFilesPaths =
                        {
                            Path.Combine(imageCachePath, game.Cover),
                            Path.Combine(imageCachePath, game.CoverLowRes),
                            Path.Combine(imageCachePath, game.Icon)
                        };

                        foreach (string filePath in gameFilesPaths)
                        {
                            try
                            {
                                File.Delete(filePath);
                            }
                            catch {}
                        }

                        gamePassGamesList.Remove(game);
                        gamesRemoved = true;
                    }

                    if (gamesRemoved == true)
                    {
                        File.WriteAllText(gameDataCachePath, JsonConvert.SerializeObject(gamePassGamesList));
                    }
                }

                foreach (GamePassCatalogProduct gamePassProduct in gamePassCatalog)
                {
                    if (gamePassGamesList.Any(x => x.ProductId == gamePassProduct.Id) == false)
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
                                ProductId = product.ProductId,
                                DeveloperName = product.LocalizedProperties[0].DeveloperName.Replace("Developed by ", ""),
                                PublisherName = product.LocalizedProperties[0].PublisherName,
                                ProductDescription = product.LocalizedProperties[0].ProductDescription,
                                BackgroundImage = string.Format("https:{0}?mode=scale&q=90&h=128&w=128", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.SuperHeroArt)?.FirstOrDefault()?.Uri),
                                CoverUrl = string.Format("https:{0}?mode=scale&q=90&h=900&w=600", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.Poster)?.FirstOrDefault()?.Uri),
                                CoverLowResUrl = string.Format("https:{0}?mode=scale&q=90&h=300&w=200", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.Poster)?.FirstOrDefault()?.Uri),
                                Cover = string.Format("{0}.jpg", Guid.NewGuid().ToString()),
                                CoverLowRes = string.Format("{0}.jpg", Guid.NewGuid().ToString()),
                                GameName = product.LocalizedProperties[0].ProductTitle,
                                GameId = product.Properties.PackageFamilyName,
                                Categories = product.Properties.Categories,
                                MsStoreLaunchUri = string.Format("ms-windows-store://pdp?productId={0}", product.ProductId)
                            };

                            if (product.LocalizedProperties[0].Images.Any(x => x.ImagePurpose == ImagePurpose.Logo) == true)
                            {
                                gamePassGame.Icon = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                                gamePassGame.IconUrl = string.Format("https:{0}?mode=scale&q=90&h=128&w=128", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.Logo)?.FirstOrDefault()?.Uri);
                            }
                            else if (product.LocalizedProperties[0].Images.Any(x => x.ImagePurpose == ImagePurpose.BoxArt) == true)
                            {
                                gamePassGame.Icon = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                                gamePassGame.IconUrl = string.Format("https:{0}?mode=scale&q=90&h=128&w=128", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.BoxArt)?.FirstOrDefault()?.Uri);
                            }

                            gamePassGamesList.Add(gamePassGame);
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

            if (gamePassGamesList.Count > 0)
            {
                BuildCache(gamePassGamesList);
            }

            foreach (GamePassGame game in gamePassGamesList)
            {
                game.GameName = CleanGameName(game.GameName);
                game.Cover = Path.Combine(imageCachePath, game.Cover);
                game.CoverLowRes = Path.Combine(imageCachePath, game.CoverLowRes);
                if (game.Icon != null)
                {
                    game.Icon = Path.Combine(imageCachePath, game.Icon);
                }
            }
            return gamePassGamesList;
        }
    }
}