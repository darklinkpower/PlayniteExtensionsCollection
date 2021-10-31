using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExtraMetadataLoader.Web;
using System.Net;
using Newtonsoft.Json;
using ExtraMetadataLoader.Models;
using ExtraMetadataLoader.Helpers;

namespace ExtraMetadataLoader.Services
{
    public class LogosDownloader
    {
        private readonly IPlayniteAPI playniteApi;
        private readonly ExtraMetadataLoaderSettings settings;
        private readonly ExtraMetadataHelper extraMetadataHelper;
        private readonly Guid steamPluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
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

        public bool DownloadSteamLogo(Game game, bool overwrite, bool isBackgroundDownload, string steamId)
        {
            var logoPath = extraMetadataHelper.GetGameLogoPath(game, true);
            if (File.Exists(logoPath) && !overwrite)
            {
                return true;
            }

            if (steamId == null)
            {
                return false;
            }
            var steamUri = string.Format(steamLogoUriTemplate, steamId);
            return HttpDownloader.DownloadFileAsync(steamUri, logoPath).GetAwaiter().GetResult();
        }

        public bool DownloadSgdbLogo(Game game, bool overwrite, bool isBackgroundDownload)
        {
            var logoPath = extraMetadataHelper.GetGameLogoPath(game, true);
            if (File.Exists(logoPath) && !overwrite)
            {
                return true;
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
                        if (isBackgroundDownload || response.Data.Count == 1)
                        {
                            return HttpDownloader.DownloadFileAsync(response.Data[0].Url, logoPath).GetAwaiter().GetResult();
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
                                    return HttpDownloader.DownloadFileAsync(response.Data.First(x => x.Thumb == selectedOption.Path).Url, logoPath).GetAwaiter().GetResult();
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private string GetSgdbRequestUrl(Game game, bool isBackgroundDownload)
        {
            if (game.PluginId == steamPluginId)
            {
                return string.Format(sgdbLogoRequestEnumUriTemplate, "steam", game.GameId.ToString());
            }
            else
            {
                if (isBackgroundDownload)
                {
                    var gamesList = GetSteamGridDbSearchResults(game.Name);
                    if (gamesList.Count > 0)
                    {
                        return string.Format(sgdbLogoRequestIdUriTemplate, gamesList[0].Id.ToString());
                    }
                }
                else
                {
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
            }
            return new List<SteamGridDbGameSearchResponse.Data>();
        }
    }
}