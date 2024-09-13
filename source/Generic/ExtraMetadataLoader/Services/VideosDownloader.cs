using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using FlowHttp;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ExtraMetadataLoader.Services
{
    class VideosDownloader
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ExtraMetadataLoaderSettings settings;
        
        private readonly string tempDownloadPath = Path.Combine(Path.GetTempPath(), "VideoTemp.mp4");

        public VideosDownloader(IPlayniteAPI playniteApi, ExtraMetadataLoaderSettings settings)
        {
            this.playniteApi = playniteApi;
            this.settings = settings;
        }

        public bool DownloadSteamVideo(Game game, bool overwrite, bool isBackgroundDownload, CancellationToken cancelToken, bool downloadVideo = false, bool downloadVideoMicro = false)
        {

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
            var youtubeDlPath = extraMetadataHelper.ExpandVariables(game, settings.YoutubeDlPath);
            var ffmpegPath = extraMetadataHelper.ExpandVariables(game, settings.FfmpegPath);
            var youtubeCookiesPath = extraMetadataHelper.ExpandVariables(game, settings.YoutubeCookiesPath);

            var videoPath = extraMetadataHelper.GetGameVideoPath(game, true);
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
                return ProcessVideo(tempDownloadPath, videoPath, false, false);
            }
        }

    }
}