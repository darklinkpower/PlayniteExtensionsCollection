using ExtraMetadataLoader.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlowHttp;

namespace ExtraMetadataLoader.MetadataProviders
{
    public class SteamGridDBProvider : ILogoProvider
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;
        private readonly ExtraMetadataLoaderSettings _settings;
        private const string sgdbGameSearchUriTemplate = @"https://www.steamgriddb.com/api/v2/search/autocomplete/{0}";
        private const string sgdbLogoRequestEnumUriTemplate = @"https://www.steamgriddb.com/api/v2/logos/{0}/{1}?";
        private const string sgdbLogoRequestIdUriTemplate = @"https://www.steamgriddb.com/api/v2/logos/game/{0}?";

        public string Id => "sgdbProvider";
        public SteamGridDBProvider(IPlayniteAPI playniteApi, ExtraMetadataLoaderSettings settings, ILogger logger)
        {
            _playniteApi = playniteApi;
            _settings = settings;
            _logger = logger;
        }

        public string GetLogoUrl(Game game, LogoDownloadOptions downloadOptions, CancellationToken cancelToken = default)
        {
            if (_settings.SgdbApiKey.IsNullOrEmpty())
            {
                _logger.Debug("SteamGridDB API Key has not been configured in settings.");
                _playniteApi.Notifications.Add(new NotificationMessage("emtSgdbNoApiKey", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageSgdbApiKeyMissing"), NotificationType.Error));
                return null;
            }

            var requestString = GetSgdbRequestUrl(game,downloadOptions.IsBackgroundDownload);
            if (requestString.IsNullOrEmpty())
            {
                return null;
            }

            var headers = new Dictionary<string, string> {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {_settings.SgdbApiKey}" }
            };

            var downloadedString = HttpRequestFactory.GetHttpRequest()
                .WithUrl(requestString).WithHeaders(headers).DownloadString();
            if (!downloadedString.IsSuccess)
            {
                return null;
            }

            var response = Serialization.FromJson<SteamGridDbLogoResponse>(downloadedString.Content);
            if (!response.Success)
            {
                _logger.Debug($"SteamGridDB request failed. Response string: {downloadedString}");
                return null;
            }

            if (!response.Data.HasItems())
            {
                return null;
            }

            if (downloadOptions.IsBackgroundDownload || response.Data.Count == 1)
            {
                return response.Data[0].Url;
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
                    var selectedOption = _playniteApi.Dialogs.ChooseImageFile
                    (
                        imageFileOptions,
                        string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectLogo"),
                        game.Name)
                    );

                    if (selectedOption != null)
                    {
                        // Since the ImageFileOption dialog used the thumb url, the full resolution
                        // image url needs to be retrieved
                        return response.Data.First(x => x.Thumb == selectedOption.Path).Url;
                    }
                }
            }

            return null;
        }

        private string GetSgdbRequestUrl(Game game, bool isBackgroundDownload)
        {
            // Standard steam game AppIDs typically stay within the uint32 range,
            // while Steam mods use randomly generated IDs that exceed this limit
            // for example, 15042072108903675942
            // Source: https://partner.steamgames.com/doc/api/steam_api#AppId_t
            // For this reason source mods should be matched by name
            if (Steam.IsGameSteamGame(game) && uint.TryParse(game.GameId, out _))
            {
                return ApplySgdbLogoFilters(string.Format(sgdbLogoRequestEnumUriTemplate, "steam", game.GameId));
            }

            var gamesList = GetSteamGridDbSearchResults(game.Name);
            // Try to see if there's an exact match, to not prompt the user unless needed
            var matchingGameName = game.Name.Satinize();
            var exactMatches = gamesList.Where(x => x.Name.Satinize() == matchingGameName);
            if (isBackgroundDownload)
            {
                if (exactMatches?.ToList().Count > 0)
                {
                    var url = string.Format(sgdbLogoRequestIdUriTemplate, exactMatches.First().Id.ToString());
                    return ApplySgdbLogoFilters(url);
                }
            }
            else
            {
                if (exactMatches?.ToList().Count == 1)
                {
                    var url = string.Format(sgdbLogoRequestIdUriTemplate, exactMatches.First().Id.ToString());
                    return ApplySgdbLogoFilters(url);
                }

                var selectedResult = _playniteApi.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(),
                    (a) => GetSteamGridDbGenericItemOptions(a),
                    game.Name,
                    ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectGame"));
                if (selectedResult != null)
                {
                    var url = string.Format(sgdbLogoRequestIdUriTemplate, selectedResult.Description);
                    return ApplySgdbLogoFilters(url);
                }
            }

            return null;
        }

        private string ApplySgdbLogoFilters(string uri)
        {
            if (_settings.SgdbIncludeHumor)
            {
                uri += "&nsfw=any";
            }
            else
            {
                uri += "&nsfw=false";
            }

            if (_settings.SgdbIncludeNsfw)
            {
                uri += "&humor=any";
            }
            else
            {
                uri += "&humor=false";
            }

            return uri;
        }

        private List<GenericItemOption> GetSteamGridDbGenericItemOptions(string gameName)
        {
            return new List<GenericItemOption>(
                GetSteamGridDbSearchResults(gameName).Select(x => new GenericItemOption(x.Name, x.Id.ToString()))
            );
        }

        private List<SgdbData> GetSteamGridDbSearchResults(string gameName)
        {
            var searchUrl = string.Format(sgdbGameSearchUriTemplate, Uri.EscapeDataString(gameName));
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {_settings.SgdbApiKey}" }
            };

            var downloadResult = HttpRequestFactory.GetHttpRequest()
                .WithUrl(searchUrl)
                .WithHeaders(headers)
                .DownloadString();
            if (downloadResult.IsSuccess)
            {
                var response = Serialization.FromJson<SteamGridDbGameSearchResponse>(downloadResult.Content);
                if (response.Success)
                {
                    return response.Data;
                }
                else
                {
                    _logger.Debug($"SteamGridDB request failed. Response string: {downloadResult.Content}");
                }
            }

            return new List<SgdbData>();
        }
    }
}