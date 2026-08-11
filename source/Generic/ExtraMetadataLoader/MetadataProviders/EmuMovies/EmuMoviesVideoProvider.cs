using Playnite.SDK.Models;
using System.Threading;

namespace ExtraMetadataLoader.MetadataProviders
{
    internal class EmuMoviesVideoProvider : IVideoProvider
    {
        private readonly EmuMoviesVideoService _videoService;

        public string Id => EmuMoviesVideoService.ProviderId;

        public EmuMoviesVideoProvider(EmuMoviesVideoService videoService)
        {
            _videoService = videoService;
        }

        public Result<VideoResult> GetVideo(Game game, VideoDownloadOptions downloadOptions, CancellationToken cancelToken)
        {
            if (downloadOptions.VideoType != VideoType.Trailer)
            {
                return Result<VideoResult>.Failure("EmuMovies only provides full trailer videos.");
            }

            var videoPath = _videoService.DownloadVideoToTempFile(game, downloadOptions.IsBackgroundDownload, downloadOptions.SelectAutomatically, cancelToken);
            return string.IsNullOrWhiteSpace(videoPath)
                ? Result<VideoResult>.Failure("EmuMovies video not found.")
                : Result<VideoResult>.Success(VideoResult.FromFilePath(videoPath));
        }
    }
}
