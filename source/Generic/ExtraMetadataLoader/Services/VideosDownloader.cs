using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.MetadataProviders;
using ExtraMetadataLoader.VideosProcessor;
using FlowHttp;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.IO;
using System.Threading;

namespace ExtraMetadataLoader.Services
{
    class VideosDownloader
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ExtraMetadataLoaderSettings settings;
        private readonly VideoProcessor videoProcessor;

        private readonly string tempDownloadPath = Path.Combine(Path.GetTempPath(), "VideoTemp.mp4");

        public VideosDownloader(IPlayniteAPI playniteApi, ExtraMetadataLoaderSettings settings)
        {
            this.playniteApi = playniteApi;
            this.settings = settings;
            videoProcessor = new VideoProcessor(playniteApi, logger, settings);
        }

        public bool DownloadSteamVideo(Game game, bool overwrite, bool isBackgroundDownload, CancellationToken cancelToken, bool downloadVideo = false, bool downloadVideoMicro = false)
        {
            var success = false;
            if (downloadVideo)
            {
                success |= DownloadSteamVideo(game, overwrite, isBackgroundDownload, cancelToken, VideoType.Trailer);
            }

            if (downloadVideoMicro)
            {
                success |= DownloadSteamVideo(game, overwrite, isBackgroundDownload, cancelToken, VideoType.Microtrailer);
            }

            return success;
        }

        private bool DownloadSteamVideo(Game game, bool overwrite, bool isBackgroundDownload, CancellationToken cancelToken, VideoType videoType)
        {
            var videoPath = videoType == VideoType.Trailer
                ? ExtraMetadataHelper.GetGameVideoPath(game, true)
                : ExtraMetadataHelper.GetGameVideoMicroPath(game, true);
            if (!overwrite && FileSystem.FileExists(videoPath))
            {
                return true;
            }

            var steamProvider = new SteamMetadataProvider(playniteApi, settings);
            var getResult = steamProvider.GetVideo(game, new VideoDownloadOptions(videoPath, isBackgroundDownload, videoType), cancelToken);
            if (!getResult.IsSuccess)
            {
                return false;
            }

            var result = getResult.Value;
            var sourcePath = result.FilePath;
            var deleteSource = false;
            if (result.IsUrl)
            {
                FileSystem.DeleteFile(tempDownloadPath, true);
                var downloadResult = HttpRequestFactory.GetHttpFileRequest()
                    .WithUrl(result.Url)
                    .WithDownloadTo(tempDownloadPath)
                    .DownloadFile(cancelToken);
                if (!downloadResult.IsSuccess)
                {
                    return false;
                }

                sourcePath = tempDownloadPath;
                deleteSource = true;
            }

            return videoProcessor.ProcessVideo(sourcePath, videoPath, false, deleteSource);
        }

        public bool SelectedDialogFileToVideo(Game game)
        {
            logger.Debug($"SelectedDialogFileToVideo starting for game {game.Name}");
            var videoPath = ExtraMetadataHelper.GetGameVideoPath(game, true);
            var selectedVideoPath = playniteApi.Dialogs.SelectFile("Video file|*.mp4;*.avi;*.mkv;*.webm;*.flv;*.wmv;*.mov;*.m4v");
            if (!selectedVideoPath.IsNullOrEmpty())
            {
                return videoProcessor.ProcessVideo(selectedVideoPath, videoPath, true, false);
            }
            else
            {
                return false;
            }
        }

        public bool DownloadYoutubeVideoById(Game game, string videoId, bool overwrite)
        {
            var youtubeDlPath = ExtraMetadataHelper.ExpandVariables(game, settings.YoutubeDlPath);
            var ffmpegPath = ExtraMetadataHelper.ExpandVariables(game, settings.FfmpegPath);
            var youtubeCookiesPath = ExtraMetadataHelper.ExpandVariables(game, settings.YoutubeCookiesPath);

            var videoPath = ExtraMetadataHelper.GetGameVideoPath(game, true);
            if (FileSystem.FileExists(videoPath) && !overwrite)
            {
                return false;
            }

            var args = string.Format("-v --force-overwrites -o \"{0}\" -f \"mp4\" \"{1}\"", tempDownloadPath, $"https://www.youtube.com/watch?v={videoId}");
            if (!youtubeCookiesPath.IsNullOrEmpty() && FileSystem.FileExists(youtubeCookiesPath))
            {
                args = string.Format("-v --force-overwrites -o \"{0}\" --cookies \"{1}\" -f \"mp4\" \"{2}\"", tempDownloadPath, youtubeCookiesPath, $"https://www.youtube.com/watch?v={videoId}");
            }

            var result = ProcessStarter.StartProcessWait(youtubeDlPath, args, Path.GetDirectoryName(ffmpegPath), true, out var stdOut, out var stdErr);
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
                return videoProcessor.ProcessVideo(tempDownloadPath, videoPath, false, false);
            }
        }

        public bool ConvertVideoToMicro(Game game, bool overwrite)
        {
            return videoProcessor.ConvertVideoToMicro(game, overwrite);
        }
    }
}
