using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using PluginsCommon.Web;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Services
{
    class VideosDownloader
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ExtraMetadataLoaderSettings settings;
        private readonly ExtraMetadataHelper extraMetadataHelper;
        private const string steamMicrotrailerUrlTemplate = @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/microtrailer.mp4";
        private readonly string tempDownloadPath = Path.Combine(Path.GetTempPath(), "VideoTemp.mp4");

        public VideosDownloader(IPlayniteAPI playniteApi, ExtraMetadataLoaderSettings settings, ExtraMetadataHelper extraMetadataHelper)
        {
            this.playniteApi = playniteApi;
            this.settings = settings;
            this.extraMetadataHelper = extraMetadataHelper;
        }

        public bool DownloadSteamVideo(Game game, bool overwrite, bool isBackgroundDownload, bool downloadVideo = false, bool downloadVideoMicro = false)
        {
            logger.Debug($"DownloadSteamVideo starting for game {game.Name}");
            var videoPath = extraMetadataHelper.GetGameVideoPath(game, true);
            var videoMicroPath = extraMetadataHelper.GetGameVideoMicroPath(game, true);
            if (FileSystem.FileExists(videoPath) && !overwrite)
            {
                downloadVideo = false;
            }
            if (FileSystem.FileExists(videoMicroPath) && !overwrite)
            {
                downloadVideoMicro = false;
            }
            if (!downloadVideo && !downloadVideoMicro)
            {
                return true;
            }

            var steamId = string.Empty;
            if (Steam.IsGameSteamGame(game))
            {
                logger.Debug("Steam id found for Steam game");
                steamId = game.GameId;
            }
            else if (!settings.SteamDlOnlyProcessPcGames || PlayniteUtilities.IsGamePcGame(game))
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

            var steamAppDetails = SteamWeb.GetSteamAppDetails(steamId);
            if (steamAppDetails == null || steamAppDetails.data.Movies == null || steamAppDetails.data.Movies.Count == 0)
            {
                return false;
            }

            if (downloadVideo)
            {
                var videoUrl = steamAppDetails.data.Movies[0].Mp4.Q480;
                if (settings.VideoSteamDownloadHdQuality)
                {
                    videoUrl = steamAppDetails.data.Movies[0].Mp4.Max;
                }

                var success = HttpDownloader.DownloadFileAsync(videoUrl.ToString(), tempDownloadPath).GetAwaiter().GetResult();
                if (success)
                {
                    GetVideoInformation(tempDownloadPath);
                    ProcessVideo(tempDownloadPath, videoPath, false, true);
                }
            }
            if (downloadVideoMicro)
            {
                var videoUrl = string.Format(steamMicrotrailerUrlTemplate, steamAppDetails.data.Movies[0].Id);
                var success = HttpDownloader.DownloadFileAsync(videoUrl.ToString(), tempDownloadPath).GetAwaiter().GetResult();
                if (success)
                {
                    ProcessVideo(tempDownloadPath, videoMicroPath, false, true);
                }
            }

            return true;
        }

        private bool ProcessVideo(string videoPath, string destinationPath, bool copyOnly, bool deleteSource, string args = null)
        {
            var videoInfo = GetVideoInformation(videoPath);
            var neededAction = GetIsConversionNeeded(videoInfo);
            var success = true;
            try
            {
                if (neededAction == VideoActionNeeded.Conversion || Path.GetExtension(videoPath) != ".mp4")
                {
                    if (args.IsNullOrEmpty())
                    {
                        args = $"-y -i \"{videoPath}\" -c:v libx264 -c:a mp3 -vf scale=trunc(iw/2)*2:trunc(ih/2)*2 -pix_fmt yuv420p \"{destinationPath}\"";
                    }
                    var result = ProcessStarter.StartProcessWait(settings.FfmpegPath, args, Path.GetDirectoryName(settings.FfmpegPath), true, out var stdOut, out var stdErr);
                    if (result != 0)
                    {
                        logger.Error($"Failed to process video in ffmpeg: {result}, {stdErr}");
                        success = false;
                    }
                }
                else if (neededAction == VideoActionNeeded.Nothing)
                {
                    if (copyOnly)
                    {
                        success = FileSystem.CopyFile(videoPath, destinationPath);
                    }
                    else
                    {
                        success = FileSystem.MoveFile(videoPath, destinationPath);
                    }
                }
                else if (neededAction == VideoActionNeeded.Invalid)
                {
                    success = false;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while processing video \"{videoPath}\" with process action \"{neededAction}\"");
                success = false;
            }

            if (deleteSource && FileSystem.FileExists(videoPath))
            {
                FileSystem.DeleteFile(videoPath);
            }

            return success;
        }

        private VideoActionNeeded GetIsConversionNeeded(FfprobeVideoInfoOutput videoInformation)
        {
            if (videoInformation == null)
            {
                logger.Debug($"Video information not obtained");
                return VideoActionNeeded.Invalid;
            }
            else if (videoInformation.Streams == null || videoInformation.Streams.Count() < 1)
            {
                logger.Debug($"Video streams null or 0");
                return VideoActionNeeded.Invalid;
            }

            var stream = videoInformation.Streams[0];
            if (stream.Width == 0 || stream.Height == 0)
            {
                logger.Debug($"Video width and height not obtained");
                return VideoActionNeeded.Invalid;
            }
            else if (string.IsNullOrEmpty(stream.PixFmt))
            {
                logger.Debug($"Video PixFmt is null");
                return VideoActionNeeded.Invalid;
            }
            else if (stream.PixFmt != "yuv420p")
            {
                logger.Debug($"Video conversion needed, PixFmt is {stream.PixFmt}");
                return VideoActionNeeded.Conversion;
            }

            return VideoActionNeeded.Nothing;
        }

        private FfprobeVideoInfoOutput GetVideoInformation(string videoPath)
        {
            logger.Debug($"Obtaining video information: {videoPath}");
            var args = $"-v error -select_streams v:0 -show_entries stream=width,height,codec_name_name,pix_fmt,duration -of json \"{videoPath}\"";
            var result = ProcessStarter.StartProcessWait(settings.FfprobePath, args, Path.GetDirectoryName(settings.FfprobePath), true, out var stdOut, out var stdErr);
            if (result != 0)
            {
                logger.Error($"Failed to get video information: {videoPath}, {result}, {stdErr}");
                playniteApi.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationErrorFfprobeGetInfoFail"), videoPath, result, stdErr),
                    NotificationType.Error)
                );
                return null;
            }

            return JsonConvert.DeserializeObject<FfprobeVideoInfoOutput>(stdOut);
        }

        public bool ConvertVideoToMicro(Game game, bool overwrite)
        {
            var videoPath = extraMetadataHelper.GetGameVideoPath(game, true);
            var videoMicroPath = extraMetadataHelper.GetGameVideoMicroPath(game, true);
            if (!FileSystem.FileExists(videoPath) || (FileSystem.FileExists(videoMicroPath) && !overwrite))
            {
                return false;
            }

            var videoInfo = GetVideoInformation(videoPath);
            if (videoInfo == null)
            {
                return false;
            }
            
            // It's needed to use invariant culture when parsing because ffprobe output durantion
            // uses a dot as decimal separator and some regions use other symbols for this.
            var videoDuration = double.Parse(videoInfo.Streams[0].Duration, CultureInfo.InvariantCulture);
            var success = true;
            if (videoDuration < 14)
            {
                var actionNeeded = GetIsConversionNeeded(videoInfo);
                if (actionNeeded == VideoActionNeeded.Invalid)
                {
                    success = false;
                }
                else if (actionNeeded == VideoActionNeeded.Conversion)
                {
                    // Scale parameter needs to be used because otherwise ffmpeg
                    // will fail if a dimension is not divisible by 2.
                    var args = $"-y -i \"{videoPath}\" -c:v libx264 -c:a mp3 -vf scale=trunc(iw/2)*2:trunc(ih/2)*2 -pix_fmt yuv420p -an \"{videoMicroPath}\"";
                    var result = ProcessStarter.StartProcessWait(settings.FfmpegPath, args, Path.GetDirectoryName(settings.FfmpegPath), true, out var stdOut, out var stdErr);
                    if (result != 0)
                    {
                        logger.Error($"Failed to process video in ffmpeg: {videoPath}, {result}, {stdErr}");
                        playniteApi.Notifications.Add(new NotificationMessage(
                            Guid.NewGuid().ToString(),
                            string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationErrorFfmpegProcessingFail"), videoPath, result, stdErr),
                            NotificationType.Error)
                        );
                        success = false;
                    }
                }
                else
                {
                    // Scale parameter needs to be used because otherwise ffmpeg
                    // will fail if a dimension is not divisible by 2.
                    var args = $"-y -i \"{videoPath}\" -c:v copy -an \"{videoMicroPath}\"";
                    var result = ProcessStarter.StartProcessWait(settings.FfmpegPath, args, Path.GetDirectoryName(settings.FfmpegPath), true, out var stdOut, out var stdErr);
                    if (result != 0)
                    {
                        logger.Error($"Failed to process video in ffmpeg: {videoPath}, {result}, {stdErr}");
                        playniteApi.Notifications.Add(new NotificationMessage(
                            Guid.NewGuid().ToString(),
                            string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationErrorFfmpegProcessingFail"), videoPath, result, stdErr),
                            NotificationType.Error)
                        );
                        success = false;
                    }
                }
            }
            else
            {
                var rangeStringList = new List<string>();
                var clipDuration = 1;
                int[] startPercentageVideo = {
                    15,
                    25,
                    35,
                    45,
                    55,
                    65
                };

                foreach (var percentage in startPercentageVideo)
                {
                    double clipStart = (percentage * videoDuration) / 100;
                    double clipEnd = clipStart + clipDuration;
                    rangeStringList.Add(string.Format("between(t,{0:N2},{1:N2})", clipStart.ToString(CultureInfo.InvariantCulture), clipEnd.ToString(CultureInfo.InvariantCulture)));
                }

                var selectString = $"\"select = '{string.Join("+", rangeStringList)}', setpts = N / FRAME_RATE / TB, scale = trunc(iw / 2) * 2:trunc(ih / 2) * 2\"";
                var args = $"-y -i \"{videoPath}\" -vf {selectString} -c:v libx264 -pix_fmt yuv420p -an \"{videoMicroPath}\"";
                var result = ProcessStarter.StartProcessWait(settings.FfmpegPath, args, Path.GetDirectoryName(settings.FfmpegPath), true, out var stdOut, out var stdErr);
                if (result != 0)
                {
                    logger.Error($"Failed to process video in ffmpeg: {videoPath}, {result}, {stdErr}");
                    playniteApi.Notifications.Add(new NotificationMessage(
                        Guid.NewGuid().ToString(),
                        string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationErrorFfmpegProcessingFail"), videoPath, result, stdErr),
                        NotificationType.Error)
                    );
                    success = false;
                }
            }

            return success;
        }

        public bool SelectedDialogFileToVideo(Game game)
        {
            logger.Debug($"SelectedDialogFileToVideo starting for game {game.Name}");
            var videoPath = extraMetadataHelper.GetGameVideoPath(game, true);
            var selectedVideoPath = playniteApi.Dialogs.SelectFile("Video file|*.mp4;*.avi;*.mkv;*.webm;*.flv;*.wmv;*.mov;*.m4v");
            if (!selectedVideoPath.IsNullOrEmpty())
            {
                return ProcessVideo(selectedVideoPath, videoPath, true, false);
            }
            else
            {
                return false;
            }
        }

        public bool DownloadYoutubeVideoById(Game game, string videoId, bool overwrite)
        {
            var videoPath = extraMetadataHelper.GetGameVideoPath(game, true);
            if (FileSystem.FileExists(videoPath) && !overwrite)
            {
                return false;
            }

            var args = string.Format("-v --force-overwrites -o \"{0}\" -f \"mp4\" \"{1}\"", tempDownloadPath, $"https://www.youtube.com/watch?v={videoId}");
            if (!settings.YoutubeCookiesPath.IsNullOrEmpty() && FileSystem.FileExists(settings.YoutubeCookiesPath))
            {
                args = string.Format("-v --force-overwrites -o \"{0}\" --cookies \"{1}\" -f \"mp4\" \"{2}\"", tempDownloadPath, settings.YoutubeCookiesPath, $"https://www.youtube.com/watch?v={videoId}");
            }
            var result = ProcessStarter.StartProcessWait(settings.YoutubeDlPath, args, Path.GetDirectoryName(settings.FfmpegPath), true, out var stdOut, out var stdErr);
            if (result != 0)
            {
                logger.Error($"Failed to download video in youtube-dlp: {videoId}, {result}, {stdErr}");
                playniteApi.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationErrorYtdlpDownloadFail"), videoId, result, stdErr),
                    NotificationType.Error)
                );
                return false;
            }
            else
            {
                return ProcessVideo(tempDownloadPath, videoPath, false, false);
            }
        }

    }
}