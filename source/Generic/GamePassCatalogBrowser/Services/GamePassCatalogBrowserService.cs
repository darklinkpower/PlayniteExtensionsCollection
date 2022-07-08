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
using PluginsCommon.Web;
using PluginsCommon;

namespace GamePassCatalogBrowser.Services
{
    class GamePassCatalogBrowserService
    {
        private IPlayniteAPI playniteApi;
        private ILogger logger = LogManager.GetLogger();
        public List<GamePassGame> gamePassGamesList = new List<GamePassGame>();
        private readonly string userDataPath = string.Empty;
        private readonly string cachePath = string.Empty;
        private readonly string imageCachePath = string.Empty;
        private readonly string gameDataCachePath = string.Empty;
        public const string gamepassCatalogApiBaseUrl = @"https://catalog.gamepass.com/sigls/v2?id=fdd9e2a7-0fee-49f6-ad69-4354098401ff&language={0}&market={1}";
        public const string gamepassEaCatalogApiBaseUrl = @"https://catalog.gamepass.com/sigls/v2?id=1d33fbb9-b895-4732-a8ca-a55c8b99fa2c&language={0}&market={1}";
        public const string catalogDataApiBaseUrl = @"https://displaycatalog.mp.microsoft.com/v7.0/products?bigIds={0}&market={1}&languages={2}&MS-CV=F.1";
        private readonly string gamepassCatalogApiUrl = string.Empty;
        private readonly string gamepassEaCatalogApiUrl = string.Empty;
        private readonly string languageCode = string.Empty;
        private readonly string countryCode = string.Empty;
        private readonly bool notifyCatalogUpdates;
        private readonly bool addExpiredTagToGames;
        private readonly bool addNewGames;
        private readonly bool removeExpiredGames;
        public XboxLibraryHelper xboxLibraryHelper;

        public void DeleteCache()
        {
            FileSystem.ClearDirectory(cachePath);
        }

        public GamePassCatalogBrowserService(IPlayniteAPI api, string dataPath, bool _notifyCatalogUpdates, bool _addExpiredTagToGames, bool _addNewGames, bool _removeExpiredGames, string _countryCode, string _languageCode = "en-us")
        {
            playniteApi = api;
            userDataPath = dataPath;
            notifyCatalogUpdates = _notifyCatalogUpdates;
            addExpiredTagToGames = _addExpiredTagToGames;
            addNewGames = _addNewGames;
            removeExpiredGames = _removeExpiredGames;

            cachePath = Path.Combine(userDataPath, "cache");
            imageCachePath = Path.Combine(cachePath, "images");
            gameDataCachePath = Path.Combine(cachePath, "gamesCache.json");
            languageCode = _languageCode;
            countryCode = _countryCode;
            gamepassCatalogApiUrl = string.Format(gamepassCatalogApiBaseUrl, languageCode, countryCode);
            gamepassEaCatalogApiUrl = string.Format(gamepassEaCatalogApiBaseUrl, languageCode, countryCode);

            xboxLibraryHelper = new XboxLibraryHelper(api);
        }

        public List<GamePassCatalogProduct> GetGamepassCatalog(string catalogUrl)
        {
            var gamePassGames = new List<GamePassCatalogProduct>();
            try
            {
                var downloadedString = HttpDownloader.DownloadString(catalogUrl);
                if (downloadedString.IsNullOrEmpty())
                {
                    return null;
                }
                
                var gamePassCatalog = JsonConvert.DeserializeObject<List<GamePassCatalogProduct>>(downloadedString);
                foreach (var gamePassProduct in gamePassCatalog)
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
                return null;
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

        private string NormalizeGameName(string str)
        {
            if (str.IsNullOrEmpty())
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
                " - Windows Edition",
                " Windows Edition",
                " Windows 10",
                "- PC",
                " PC",
                " Windows",
                " Win10",
                " Win 10"
            };

            foreach (var lineToRemove in linesToRemove)
            {
                str = RegexRemoveOnEnd(str, lineToRemove);
            }

            return str.Trim();
        }


        private string[] CompaniesStringToArray(string companiesString)
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

        private void AddGamesFromCatalogData(CatalogData catalogData, bool addChildProducts, ProductType gameProductType, bool isChildProduct, string parentProductId)
        {
            foreach (CatalogProduct product in catalogData.Products)
            {
                if (product.ProductBSchema == "ProductAddOn;3")
                {
                    continue;
                }

                if (gamePassGamesList.Any(g => g.ProductId.Equals(product.ProductId)))
                {
                    continue;
                }

                if (!product.Properties.PackageFamilyName.IsNullOrEmpty())
                {
                    continue;
                }

                if (!addChildProducts)
                {
                    continue;
                }

                var marketProperties = product.MarketProperties.FirstOrDefault();
                if (marketProperties == null)
                {
                    continue;
                }

                var idsForDataRequest = new List<string>();
                var childSubproductsList = new List<string>();
                foreach (RelatedProduct relatedProduct in marketProperties.RelatedProducts)
                {
                    if (relatedProduct.RelationshipType == "Bundle" || relatedProduct.RelationshipType == "Parent")
                    {
                        childSubproductsList.Add(relatedProduct.RelatedProductId);
                        if (!gamePassGamesList.Any(g => g.ProductId.Equals(relatedProduct.RelatedProductId)))
                        {
                            idsForDataRequest.Add(relatedProduct.RelatedProductId);
                        }
                    }
                }

                if (idsForDataRequest.Count == 0)
                {
                    continue;
                }

                var bigIdsParam = string.Join(",", idsForDataRequest);
                var catalogDataApiUrl = string.Format(catalogDataApiBaseUrl, bigIdsParam, countryCode, languageCode);
                try
                {
                    var response = HttpDownloader.DownloadString(catalogDataApiUrl);
                    if (!response.IsNullOrEmpty())
                    {
                        AddGamesFromCatalogData(JsonConvert.DeserializeObject<CatalogData>(response), false, gameProductType, true, product.ProductId);
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

                AddGamePassGameFromProduct(gameProductType, isChildProduct, parentProductId, product, childSubproductsList);
            }
        }

        private void AddGamePassGameFromProduct(ProductType gameProductType, bool isChildProduct, string parentProductId, CatalogProduct product, List<string> childSubproductsList)
        {
            var gamePassGame = GetGamePassGameFromProduct(gameProductType, isChildProduct, parentProductId, product, childSubproductsList);
            gamePassGamesList.Add(gamePassGame);
            DownloadGamePassGameCache(gamePassGame);

            var gameAdded = false;
            if (addNewGames == true && gamePassGame.ProductType == ProductType.Game)
            {
                xboxLibraryHelper.AddGameToLibrary(gamePassGame, false);
                if (gameAdded == true)
                {
                    playniteApi.Notifications.Add(new NotificationMessage(
                        Guid.NewGuid().ToString(),
                        $"{gamePassGame.Name} has been added to the Game Pass catalog and Playnite library",
                        NotificationType.Info));
                }
            }

            // Notify user that game has been added
            if (notifyCatalogUpdates == true && gameAdded == false)
            {
                playniteApi.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    $"{gamePassGame.Name} has been added to the Game Pass catalog",
                    NotificationType.Info));
            }

            RestoreMediaPaths(gamePassGame);
        }

        private GamePassGame GetGamePassGameFromProduct(ProductType gameProductType, bool isChildProduct, string parentProductId, CatalogProduct product, List<string> childSubproductsList)
        {
            var gamePassGame = new GamePassGame
            {
                BackgroundImage = string.Format("{0}.jpg", Guid.NewGuid().ToString()),
                BackgroundImageUrl = string.Format("https:{0}", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.SuperHeroArt)?.FirstOrDefault()?.Uri),
                Category = product.Properties.Category,
                Categories = product.Properties.Categories,
                CoverImage = string.Format("{0}.jpg", Guid.NewGuid().ToString()),
                CoverImageUrl = string.Format("https:{0}", product.LocalizedProperties[0].Images.Where(x => x.ImagePurpose == ImagePurpose.Poster)?.FirstOrDefault()?.Uri),
                CoverImageLowRes = string.Format("{0}.jpg", Guid.NewGuid().ToString()),
                Description = product.LocalizedProperties[0].ProductDescription,
                Name = NormalizeGameName(product.LocalizedProperties[0].ProductTitle),
                ProductId = product.ProductId,
                Publishers = CompaniesStringToArray(product.LocalizedProperties[0].PublisherName),
                ReleaseDate = product.MarketProperties.FirstOrDefault().OriginalReleaseDate.UtcDateTime,
                ChildProducts = childSubproductsList,
                IsChildProduct = isChildProduct,
                ParentProductId = parentProductId
            };

            if (product.Properties.PackageFamilyName.IsNullOrEmpty())
            {
                gamePassGame.ProductType = ProductType.Collection;
            }
            else
            {
                gamePassGame.GameId = product.Properties.PackageFamilyName;
                gamePassGame.ProductType = gameProductType;
            }

            if (product.LocalizedProperties[0].DeveloperName.IsNullOrEmpty())
            {
                gamePassGame.Developers = gamePassGame.Publishers;
            }
            else
            {
                gamePassGame.Developers = CompaniesStringToArray(product.LocalizedProperties[0].DeveloperName);
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

            return gamePassGame;
        }

        public List<GamePassGame> GetGamePassGamesList()
        {
            // Try to create cache directory in case it doesn't exist
            Directory.CreateDirectory(imageCachePath);

            if (FileSystem.FileExists(gameDataCachePath))
            {
                gamePassGamesList = JsonConvert.DeserializeObject<List<GamePassGame>>(File.ReadAllText(gameDataCachePath));
            }

            var gamePassCatalogDownload = GetGamepassCatalog(gamepassCatalogApiUrl);
            if (gamePassCatalogDownload != null)
            {
                ProcessGamePassCatalog(gamePassCatalogDownload, ProductType.Game);
            }

            var gamePassEaCatalogDownload = GetGamepassCatalog(gamepassEaCatalogApiUrl);
            if (gamePassEaCatalogDownload != null)
            {
                ProcessGamePassCatalog(gamePassEaCatalogDownload, ProductType.EaGame);
            }
            
            return SetGamePassListFullPaths(gamePassGamesList);
        }

        private void ProcessGamePassCatalog (List<GamePassCatalogProduct> gamePassCatalog, ProductType gameProductType)
        {
            var idsForDataRequest = new List<string>();
            // Check for games removed from the service
            var gamesRemoved = false;
            foreach (GamePassGame game in gamePassGamesList.ToList())
            {
                if (game.ProductType == ProductType.Game || game.ProductType == ProductType.Collection)
                {
                    if (gameProductType == ProductType.EaGame)
                    {
                        continue;
                    }
                }
                else if (game.ProductType == ProductType.EaGame)
                {
                    if (gameProductType == ProductType.Game)
                    {
                        continue;
                    }
                }

                var gameMatched = false;
                if (game.IsChildProduct == false)
                {
                    gameMatched = gamePassCatalog.Any(x => x.Id == game.ProductId);
                }
                else
                {
                    gameMatched = gamePassCatalog.Any(x => x.Id == game.ParentProductId);
                }

                if (gameMatched)
                {
                    continue;
                }

                var gameFilesPaths = new List<string>()
                {
                    Path.Combine(imageCachePath, game.BackgroundImage),
                    Path.Combine(imageCachePath, game.CoverImage),
                    Path.Combine(imageCachePath, game.CoverImageLowRes)
                };

                if (game.Icon != null)
                {
                    gameFilesPaths.AddMissing(Path.Combine(imageCachePath, game.Icon));
                }

                foreach (string filePath in gameFilesPaths)
                {
                    FileSystem.DeleteFileSafe(filePath);
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

            if (gamesRemoved)
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

            if (idsForDataRequest.Count == 0)
            {
                return;
            }

            var bigIdsParam = string.Join(",", idsForDataRequest);
            var catalogDataApiUrl = string.Format(catalogDataApiBaseUrl, bigIdsParam, countryCode, languageCode);
            try
            {
                var response = HttpDownloader.DownloadString(catalogDataApiUrl);
                if (!response.IsNullOrEmpty())
                {
                    AddGamesFromCatalogData(JsonConvert.DeserializeObject<CatalogData>(response), true, gameProductType, false, string.Empty);
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

        public void DownloadGamePassGameCache(GamePassGame game)
        {
            game.CoverImageLowRes = Path.Combine(imageCachePath, game.CoverImageLowRes);
            HttpDownloader.DownloadFile(string.Format("{0}?mode=scale&q=90&h=300&w=200", game.CoverImageUrl), game.CoverImageLowRes);
            
            game.CoverImage = Path.Combine(imageCachePath, game.CoverImage);
            HttpDownloader.DownloadFile(string.Format("{0}?mode=scale&q=90&h=900&w=600", game.CoverImageUrl), game.CoverImage);

            if (game.Icon != null)
            {
                game.Icon = Path.Combine(imageCachePath, game.Icon);
                HttpDownloader.DownloadFile(string.Format("{0}?mode=scale&q=90&h=128&w=128", game.IconUrl), game.Icon);
            }
        }

        public GamePassGame RestoreMediaPaths(GamePassGame game)
        {
            game.CoverImageLowRes = Path.GetFileName(game.CoverImageLowRes);
            game.CoverImage = Path.GetFileName(game.CoverImage);
            if (game.Icon != null)
            {
                game.Icon = Path.GetFileName(game.Icon);
            }

            return game;
        }

        public List<GamePassGame> SetGamePassListFullPaths(List<GamePassGame> cacheGamesList)
        {
            foreach (GamePassGame game in cacheGamesList)
            {
                game.CoverImageLowRes = Path.Combine(imageCachePath, game.CoverImageLowRes);
                game.CoverImage = Path.Combine(imageCachePath, game.CoverImage);
                if (game.Icon != null)
                {
                    game.Icon = Path.Combine(imageCachePath, game.Icon);
                }
            }

            cacheGamesList.Sort((x, y) => x.Name.CompareTo(y.Name));
            return cacheGamesList;
        }
    }
}