using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.MetadataProviders
{
    public class VideoResult
    {
        public string Url { get; private set; }
        public string FilePath { get; private set; }
        public bool IsUrl => !Url.IsNullOrEmpty();

        private VideoResult() { }

        public static VideoResult FromUrl(string url)
        {
            return new VideoResult { Url = url };
        }

        public static VideoResult FromFilePath(string filePath)
        {
            return new VideoResult { FilePath = filePath };
        }
    }

    public class VideoDownloadOptions
    {
        public string DownloadPath { get; private set; }
        public bool IsBackgroundDownload { get; private set; }

        public VideoDownloadOptions (string downloadPath, bool isBackgroundDownload)
        {
            DownloadPath = downloadPath;
            IsBackgroundDownload = isBackgroundDownload;
        }
    }

    interface IVideoProvider
    {
        string Id { get; }
        Result<VideoResult> GetVideo(Game game, VideoDownloadOptions downloadOptions, CancellationToken cancelToken);
    }
}
