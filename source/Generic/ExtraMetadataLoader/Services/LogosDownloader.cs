using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using ExtraMetadataLoader.Common;
using ExtraMetadataLoader.Models;
using ExtraMetadataLoader.Helpers;
using System.Text;
using ImageMagick;

namespace ExtraMetadataLoader.Services
{
    public class LogosDownloader
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ExtraMetadataLoaderSettings settings;
        private readonly ExtraMetadataHelper extraMetadataHelper;
        private const string steamLogoUriTemplate = @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo.png";
        private const string sgdbGameSearchUriTemplate = @"https://www.steamgriddb.com/api/v2/search/autocomplete/{0}";
        private const string sgdbLogoRequestEnumUriTemplate = @"https://www.steamgriddb.com/api/v2/logos/{0}/{1}";
        private const string sgdbLogoRequestIdUriTemplate = @"https://www.steamgriddb.com/api/v2/logos/game/{0}";

        public LogosDownloader(IPlayniteAPI playniteApi, ExtraMetadataLoaderSettings settings, ExtraMetadataHelper extraMetadataHelper)
        {
            this.playniteApi = playniteApi;
            this.settings = settings;
            this.extraMetadataHelper = extraMetadataHelper;
        }

        public bool DownloadSteamLogo(Game game, bool overwrite, bool isBackgroundDownload)
        {
            logger.Debug($"DownloadSteamLogo starting for game {game.Name}");
            var logoPath = extraMetadataHelper.GetGameLogoPath(game, true);
            if (File.Exists(logoPath) && !overwrite)
            {
                logger.Debug("Logo exists and overwrite is set to false, skipping");
                return true;
            }

            var steamId = string.Empty;
            if (SteamCommon.IsGameSteamGame(game))
            {
                logger.Debug("Steam id found for Steam game");
                steamId = game.GameId;
            }
            else if (!settings.SteamDlOnlyProcessPcGames || extraMetadataHelper.IsGamePcGame(game))
            {
                steamId = extraMetadataHelper.GetSteamIdFromSearch(game, isBackgroundDownload);
            }
            else
            {
                logger.Debug("Game is not a PC game and execution is only allowed for PC games");
                return false;
            }

            if (steamId.IsNullOrEmpty())
            {
                logger.Debug("Steam id not found");
                return false;
            }

            var steamUri = string.Format(steamLogoUriTemplate, steamId);
            var success = HttpDownloader.DownloadFileAsync(steamUri, logoPath).GetAwaiter().GetResult();
            if (success && settings.ProcessLogosOnDownload)
            {
                ProcessLogoImage(logoPath);
            }

            return success;
        }

        public bool ProcessLogoImage(string logoPath)
        {
            try
            {
                using (var image = new MagickImage(logoPath))
                {
                    var originalWitdh = image.Width;
                    var originalHeight = image.Height;
                    var imageChanged = false;
                    if (settings.LogoTrimOnDownload)
                    {
                        image.Trim();
                        if (originalWitdh != image.Width || originalHeight != image.Height)
                        {
                            imageChanged = true;
                            originalWitdh = image.Width;
                            originalHeight = image.Height;
                        }
                    }

                    if (settings.SetLogoMaxProcessDimensions)
                    {
                        if (settings.MaxLogoProcessWidth < image.Width || settings.MaxLogoProcessHeight < image.Height)
                        {
                            var targetWidth = settings.MaxLogoProcessWidth;
                            var targetHeight = settings.MaxLogoProcessHeight;
                            MagickGeometry size = new MagickGeometry(targetWidth, targetHeight)
                            {
                                IgnoreAspectRatio = false
                            };
                            image.Resize(size);
                            if (originalWitdh != image.Width || originalHeight != image.Height)
                            {
                                imageChanged = true;
                                originalWitdh = image.Width;
                                originalHeight = image.Height;
                            }
                        }
                    }

                    // Only save new image if dimensions changed
                    if (imageChanged)
                    {
                        image.Write(logoPath);
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while processing logo {logoPath}");
                return false;
            }
        }

        public bool DownloadGoogleImage(Game game, string imageUrl, bool overwrite)
        {
            logger.Debug($"DownloadGoogleImage starting for game {game.Name}");
            var logoPath = extraMetadataHelper.GetGameLogoPath(game, true);
            if (File.Exists(logoPath) && !overwrite)
            {
                logger.Debug("Logo exists and overwrite is set to false, skipping");
                return true;
            }

            var success = HttpDownloader.DownloadFileAsync(imageUrl, logoPath).GetAwaiter().GetResult();
            if (success && settings.ProcessLogosOnDownload)
            {
                ProcessLogoImage(logoPath);
            }

            return success;
        }

        public bool DownloadSgdbLogo(Game game, bool overwrite, bool isBackgroundDownload)
        {
            var logoPath = extraMetadataHelper.GetGameLogoPath(game, true);
            if (File.Exists(logoPath) && !overwrite)
            {
                logger.Debug("Logo exists and overwrite is set to false, skipping");
                return true;
            }
            else if (settings.SgdbApiKey.IsNullOrEmpty())
            {
                logger.Debug("SteamGridDB API Key has not been configured in settings.");
                playniteApi.Notifications.Add(new NotificationMessage("emtSgdbNoApiKey", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageSgdbApiKeyMissing"), NotificationType.Error));
                return false;
            }

            var requestString = GetSgdbRequestUrl(game, isBackgroundDownload);
            if (!string.IsNullOrEmpty(requestString))
            {
                var headers = new Dictionary<string, string> {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {settings.SgdbApiKey}" }
                };
                var downloadedString = HttpDownloader.DownloadStringWithHeadersAsync(requestString, headers).GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(downloadedString))
                {
                    var response = JsonConvert.DeserializeObject<SteamGridDbLogoResponse.Response>(downloadedString);
                    if (response.Success && response.Data.Count > 0)
                    {
                        var success = false;
                        if (isBackgroundDownload || response.Data.Count == 1)
                        {
                            success = HttpDownloader.DownloadFileAsync(response.Data[0].Url, logoPath).GetAwaiter().GetResult();
                        }
                        else
                        {
                            var imageFileOptions = new List<ImageFileOption>();
                            foreach (var icon in response.Data)
                            {
                                imageFileOptions.Add(new ImageFileOption
                                {
                                    Path = icon.Thumb,
                                });
                            }
                            if (imageFileOptions.Count > 0)
                            {
                                var selectedOption = playniteApi.Dialogs.ChooseImageFile(
                                imageFileOptions, ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectLogo"));
                                if (selectedOption != null)
                                {
                                    // Since the ImageFileOption dialog used the thumb url, the full resolution
                                    // image url needs to be retrieved
                                    success = HttpDownloader.DownloadFileAsync(response.Data.First(x => x.Thumb == selectedOption.Path).Url, logoPath).GetAwaiter().GetResult();
                                }
                            }
                        }
                        if (success && settings.ProcessLogosOnDownload)
                        {
                            ProcessLogoImage(logoPath);
                        }
                    }
                    else if (!response.Success)
                    {
                        logger.Debug($"SteamGridDB request failed. Response string: {downloadedString}");
                    }
                }
            }

            return false;
        }

        private string GetSgdbRequestUrl(Game game, bool isBackgroundDownload)
        {
            if (SteamCommon.IsGameSteamGame(game))
            {
                return string.Format(sgdbLogoRequestEnumUriTemplate, "steam", game.GameId.ToString());
            }
            else
            {
                var gamesList = GetSteamGridDbSearchResults(game.Name);
                // Try to see if there's an exact match, to not prompt the user unless needed
                var matchingGameName = game.Name.GetMatchModifiedName();
                var exactMatches = gamesList.Where(x => x.Name.GetMatchModifiedName() == matchingGameName);
                if (isBackgroundDownload)
                {
                    if (exactMatches?.ToList().Count > 0)
                    {
                        return string.Format(sgdbLogoRequestIdUriTemplate, exactMatches.First().Id.ToString());
                    }
                }
                else
                {
                    if (exactMatches?.ToList().Count == 1)
                    {
                        return string.Format(sgdbLogoRequestIdUriTemplate, exactMatches.First().Id.ToString());
                    }

                    var selectedGame = playniteApi.Dialogs.ChooseItemWithSearch(
                        new List<GenericItemOption>(),
                        (a) => GetSteamGridDbGenericItemOptions(a),
                        game.Name,
                        ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectGame"));
                    if (selectedGame != null)
                    {
                        return string.Format(sgdbLogoRequestIdUriTemplate, selectedGame.Description);
                    }
                }
            }

            return null;
        }

        private List<GenericItemOption> GetSteamGridDbGenericItemOptions(string gameName)
        {
            return new List<GenericItemOption>(GetSteamGridDbSearchResults(gameName).Select(x => new GenericItemOption(x.Name, x.Id.ToString())));
        }

        private List<SteamGridDbGameSearchResponse.Data> GetSteamGridDbSearchResults(string gameName)
        {
            var searchUrl = string.Format(sgdbGameSearchUriTemplate, Uri.EscapeDataString(gameName));
            var headers = new Dictionary<string, string> {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {settings.SgdbApiKey}" }
                };
            var downloadedString = HttpDownloader.DownloadStringWithHeadersAsync(searchUrl, headers).GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(downloadedString))
            {
                var response = JsonConvert.DeserializeObject<SteamGridDbGameSearchResponse.Response>(downloadedString);
                if (response.Success)
                {
                    return response.Data;
                }
                else
                {
                    logger.Debug($"SteamGridDB request failed. Response string: {downloadedString}");
                }
            }
            return new List<SteamGridDbGameSearchResponse.Data>();
        }
    }
}