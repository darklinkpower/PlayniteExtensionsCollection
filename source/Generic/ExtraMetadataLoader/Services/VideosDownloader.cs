using ExtraMetadataLoader.Common;
using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
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
        private const string steamAppDetailsUrlTemplate = @"https://store.steampowered.com/api/appdetails?appids={0}";
        private const string steamLogoUriTemplate = @"https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo.png";
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
            var videoPath = extraMetadataHelper.GetGameVideoPath(game, true);
            var videoMicroPath = extraMetadataHelper.GetGameVideoMicroPath(game, true);
            if (File.Exists(videoPath) && !overwrite)
            {
                downloadVideo = false;
            }
            if (File.Exists(videoMicroPath) && !overwrite)
            {
                downloadVideoMicro = false;
            }
            if (!downloadVideo && !downloadVideoMicro)
            {
                return true;
            }

            var steamId = string.Empty;
            if (SteamCommon.IsGameSteamGame(game))
            {
                logger.Debug("Steam id found for Steam game");
                steamId = game.GameId;
            }
            else
            {
                steamId = extraMetadataHelper.GetSteamIdFromSearch(game, isBackgroundDownload);
            }

            if (steamId.IsNullOrEmpty())
            {
                logger.Debug("Steam id not found");
                return false;
            }

            var steamAppDetails = SteamCommon.GetSteamAppDetails(steamId);
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
            if (settings.FfmpegPath.IsNullOrEmpty())
            {
                logger.Debug($"ffmpeg has not been configured");
                playniteApi.Notifications.Add(new NotificationMessage("ffmpegExeNotConfigured", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageFfmpegNotConfigured"), NotificationType.Error));
            }
            else if (!File.Exists(settings.FfmpegPath))
            {
                logger.Debug($"ffmpeg executable not found in {settings.FfmpegPath}");
                playniteApi.Notifications.Add(new NotificationMessage("ffmpegExeNotFound", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageFfmpegNotFound"), NotificationType.Error));
            }

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
                    var result = ProcessStarter.StartProcessWait(settings.FfmpegPath, args, Path.GetDirectoryName(settings.FfmpegPath), false, out var stdOut, out var stdErr);
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
                        success = IoHelper.MoveFile(videoPath, destinationPath, true);
                    }
                    else
                    {
                        success = IoHelper.MoveFile(videoPath, destinationPath, false);
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

            if (deleteSource && File.Exists(videoPath))
            {
                File.Delete(videoPath);
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
            if (settings.FfprobePath.IsNullOrEmpty())
            {
                logger.Debug($"ffprobe has not been configured");
                playniteApi.Notifications.Add(new NotificationMessage("ffprobeExeNotConfigured", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageFfprobeNotConfigured"), NotificationType.Error));
            }
            else if (!File.Exists(settings.FfprobePath))
            {
                logger.Debug($"ffprobe executable not found in {settings.FfprobePath}");
                playniteApi.Notifications.Add(new NotificationMessage("ffprobeExeNotFound", ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageFfprobeNotFound"), NotificationType.Error));
            }

            logger.Debug($"Obtaining video information: {videoPath}");
            var args = $"-v error -select_streams v:0 -show_entries stream=width,height,codec_name_name,pix_fmt,duration -of json \"{videoPath}\"";
            var result = ProcessStarter.StartProcessWait(settings.FfprobePath, args, Path.GetDirectoryName(settings.FfprobePath), true, out var stdOut, out var stdErr);
            if (result != 0)
            {
                logger.Error($"Failed to get video information: {videoPath}, {result}, {stdErr}");
                return null;
            }

            return JsonConvert.DeserializeObject<FfprobeVideoInfoOutput>(stdOut);
        }


    }
}
