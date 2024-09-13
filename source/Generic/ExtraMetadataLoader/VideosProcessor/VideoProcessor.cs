using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.VideosProcessor
{
    public class VideoProcessor
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly ExtraMetadataLoaderSettings _settings;
        private readonly ILogger _logger;

        internal VideoProcessor(
            IPlayniteAPI playniteApi,
            ILogger logger,
            ExtraMetadataLoaderSettings settings)
        {
            _playniteApi = playniteApi;
            _settings = settings;
            _logger = logger;      
        }

        public bool ProcessVideo(string videoPath, string destinationPath, bool copyOnly, bool deleteSource, string args = null)
        {
            var ffmpegPath = ExtraMetadataHelper.ExpandVariables(_settings.FfmpegPath);
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

                    var result = ProcessStarter.StartProcessWait(ffmpegPath, args, Path.GetDirectoryName(ffmpegPath), true, out var stdOut, out var stdErr);
                    if (result != 0)
                    {
                        _logger.Error($"Failed to process video in ffmpeg: {result}, {stdErr}");
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
                _logger.Error(e, $"Error while processing video \"{videoPath}\" with process action \"{neededAction}\"");
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
                _logger.Debug($"Video information not obtained");
                return VideoActionNeeded.Invalid;
            }
            else if (videoInformation.Streams == null || videoInformation.Streams.Count() < 1)
            {
                _logger.Debug($"Video streams null or 0");
                return VideoActionNeeded.Invalid;
            }

            var stream = videoInformation.Streams[0];
            if (stream.Width == 0 || stream.Height == 0)
            {
                _logger.Debug($"Video width and height not obtained");
                return VideoActionNeeded.Invalid;
            }
            else if (string.IsNullOrEmpty(stream.PixFmt))
            {
                _logger.Debug($"Video PixFmt is null");
                return VideoActionNeeded.Invalid;
            }
            else if (stream.PixFmt != "yuv420p")
            {
                _logger.Debug($"Video conversion needed, PixFmt is {stream.PixFmt}");
                return VideoActionNeeded.Conversion;
            }

            return VideoActionNeeded.Nothing;
        }

        private FfprobeVideoInfoOutput GetVideoInformation(string videoPath)
        {
            var ffprobePath = ExtraMetadataHelper.ExpandVariables(_settings.FfprobePath);

            _logger.Debug($"Obtaining video information: {videoPath}");
            var args = $"-v error -select_streams v:0 -show_entries stream=width,height,codec_name_name,pix_fmt,duration -of json \"{videoPath}\"";
            var result = ProcessStarter.StartProcessWait(ffprobePath, args, Path.GetDirectoryName(ffprobePath), true, out var stdOut, out var stdErr);
            if (result != 0)
            {
                _logger.Error($"Failed to get video information: {videoPath}, {result}, {stdErr}");
                _playniteApi.Notifications.Add(new NotificationMessage(
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
            var ffmpegPath = ExtraMetadataHelper.ExpandVariables(game, _settings.FfmpegPath);

            var videoPath = ExtraMetadataHelper.GetGameVideoPath(game, true);
            var videoMicroPath = ExtraMetadataHelper.GetGameVideoMicroPath(game, true);
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
                    var result = ProcessStarter.StartProcessWait(ffmpegPath, args, Path.GetDirectoryName(ffmpegPath), true, out var stdOut, out var stdErr);
                    if (result != 0)
                    {
                        _logger.Error($"Failed to process video in ffmpeg: {videoPath}, {result}, {stdErr}");
                        _playniteApi.Notifications.Add(new NotificationMessage(
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
                    var result = ProcessStarter.StartProcessWait(ffmpegPath, args, Path.GetDirectoryName(ffmpegPath), true, out var stdOut, out var stdErr);
                    if (result != 0)
                    {
                        _logger.Error($"Failed to process video in ffmpeg: {videoPath}, {result}, {stdErr}");
                        _playniteApi.Notifications.Add(new NotificationMessage(
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
                var result = ProcessStarter.StartProcessWait(ffmpegPath, args, Path.GetDirectoryName(ffmpegPath), true, out var stdOut, out var stdErr);
                if (result != 0)
                {
                    _logger.Error($"Failed to process video in ffmpeg: {videoPath}, {result}, {stdErr}");
                    _playniteApi.Notifications.Add(new NotificationMessage(
                        Guid.NewGuid().ToString(),
                        string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationErrorFfmpegProcessingFail"), videoPath, result, stdErr),
                        NotificationType.Error)
                    );
                    success = false;
                }
            }

            return success;
        }
    }
}
