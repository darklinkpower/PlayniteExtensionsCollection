using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
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
    public class SteamMetadataProvider : ILogoProvider, IVideoProvider
    {
        private readonly IPlayniteAPI _playniteApi;
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly ExtraMetadataLoaderSettings _settings;
        private const string _steamLogoUriTemplate = @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo.png";
        private const string _steamMicrotrailerUrlTemplate = @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/microtrailer.mp4";

        public string Id => "steamProvider";

        public SteamMetadataProvider(IPlayniteAPI playniteApi, ExtraMetadataLoaderSettings settings)
        {
            _playniteApi = playniteApi;
            _settings = settings;
        }

        public string GetLogoUrl(Game game, LogoDownloadOptions downloadOptions, CancellationToken cancelToken = default)
        {
            var gameSteamId = GetGameSteamId(game, cancelToken);
            if (!gameSteamId.IsNullOrEmpty())
            {
                return string.Format(_steamLogoUriTemplate, gameSteamId);
            }

            return null;
        }

        private string GetGameSteamId(Game game, CancellationToken cancelToken)
        {
            if (Steam.IsGameSteamGame(game) || (!_settings.SteamDlOnlyProcessPcGames || PlayniteUtilities.IsGamePcGame(game)))
            {
                var steamId = Steam.GetGameSteamId(game, true);
                if (!steamId.IsNullOrEmpty())
                {
                    return steamId;
                }
            }

            return SteamWeb.GetSteamIdFromSearch(game.Name, null, cancelToken);
        }

        public Result<VideoResult> GetVideo(Game game, VideoDownloadOptions downloadOptions, CancellationToken cancelToken)
        {
            //if (FileSystem.FileExists(videoPath) && !overwrite)
            //{
            //    downloadVideo = false;
            //}
            //if (FileSystem.FileExists(videoMicroPath) && !overwrite)
            //{
            //    downloadVideoMicro = false;
            //}
            //if (!downloadVideo && !downloadVideoMicro)
            //{
            //    return true;
            //}

            var steamId = string.Empty;
            if (Steam.IsGameSteamGame(game))
            {
                steamId = game.GameId;
            }
            else if (!_settings.SteamDlOnlyProcessPcGames || PlayniteUtilities.IsGamePcGame(game))
            {
                steamId = GetSteamIdFromSearch(game, downloadOptions.IsBackgroundDownload);
            }

            if (steamId.IsNullOrEmpty())
            {   
                return Result<VideoResult>.Failure("Steam id not found");
            }

            var steamAppDetails = SteamWeb.GetSteamAppDetails(steamId);
            if (steamAppDetails is null || steamAppDetails.data.Movies is null || steamAppDetails.data.Movies.Count == 0)
            {
                return Result<VideoResult>.Failure();
            }

            string videoUrl;
            if (downloadOptions.VideoType == VideoType.Trailer)
            {                
                if (_settings.VideoSteamDownloadHdQuality)
                {
                    videoUrl = steamAppDetails.data.Movies[0].Mp4.Max.ToString();
                }
                else
                {
                    videoUrl = steamAppDetails.data.Movies[0].Mp4.Q480.ToString();
                }
            }
            else
            {
                videoUrl = string.Format(_steamMicrotrailerUrlTemplate, steamAppDetails.data.Movies[0].Id);
            }

            var videoResult = VideoResult.FromFilePath(videoUrl);
            return Result<VideoResult>.Success(videoResult);



            //return true;
        }

        private string GetSteamIdFromSearch(Game game, bool isBackgroundDownload)
        {
            var normalizedName = game.Name.Satinize();
            var results = SteamWeb.GetSteamSearchResults(normalizedName);
            results.ForEach(a => a.Name = a.Name.Satinize());

            var matchingGameName = normalizedName.Satinize();
            var exactMatches = results.Where(x => x.Name.Satinize() == matchingGameName);
            if (exactMatches.HasItems() && (isBackgroundDownload || exactMatches.Count() == 1))
            {
                return exactMatches.First().GameId;
            }

            if (!isBackgroundDownload)
            {
                var selectedGame = _playniteApi.Dialogs.ChooseItemWithSearch(
                    results.Select(
                        x => new GenericItemOption(x.Name, x.GameId)).ToList(),
                        (a) => SteamWeb.GetSteamSearchGenericItemOptions(a),
                        game.Name.NormalizeGameName(),
                        ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectGame")
                );

                if (selectedGame != null)
                {
                    return selectedGame.Description;
                }
            }

            return string.Empty;
        }


    }
}