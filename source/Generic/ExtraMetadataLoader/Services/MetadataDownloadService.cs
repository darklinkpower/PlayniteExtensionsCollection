using EventsCommon;
using ExtraMetadataLoader.Helpers;
using ExtraMetadataLoader.MetadataProviders;
using ExtraMetadataLoader.VideosProcessor;
using FlowHttp;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Services
{
    internal class MetadataDownloadService
    {
        private readonly IEnumerable<ILogoProvider> _logoProviders;
        private readonly IEnumerable<IVideoProvider> _videoProviders;
        private readonly ExtraMetadataLoaderSettings _settings;
        private readonly ILogger _logger;
        private readonly LogoProcessor _logoProcessor;
        private readonly VideoProcessor _videoProcessor;
        private readonly EventAggregator _eventAggregator;

        internal MetadataDownloadService(
            IEnumerable<ILogoProvider> logoProviders,
            IEnumerable<IVideoProvider> videoProviders,
            ExtraMetadataLoaderSettings settings,
            ILogger logger,
            LogoProcessor logoProcessor,
            VideoProcessor videoProcessor,
            EventAggregator eventAggregator)
        {
            _logoProviders = logoProviders;
            _videoProviders = videoProviders;
            _settings = settings;
            _logger = logger;
            _logoProcessor = logoProcessor;
            _videoProcessor = videoProcessor;
            _eventAggregator = eventAggregator;
        }

        public ILogoProvider GetLogoProviderById(string id)
        {
            return _logoProviders.FirstOrDefault(x => x.Id == id);
        }

        public IVideoProvider GetVideoProviderById(string id)
        {
            return _videoProviders.FirstOrDefault(x => x.Id == id);
        }

        public async Task<bool> DownloadLogoAsync(Game game, bool isBackgroundDownload, bool overwrite, CancellationToken cancelToken)
        {
            return await DownloadLogoAsync(_logoProviders, game, isBackgroundDownload, overwrite, cancelToken);
        }

        public async Task<bool> DownloadLogoAsync(Game game, bool isBackgroundDownload, CancellationToken cancelToken)
        {
            return await DownloadLogoAsync(game, isBackgroundDownload, false, cancelToken);
        }

        public async Task<bool> DownloadLogoAsync(ILogoProvider logoProvider, Game game, bool isBackgroundDownload, bool overwrite, CancellationToken cancelToken)
        {
            if (logoProvider is null)
            {
                return false;
            }

            return await DownloadLogoAsync(new[] { logoProvider }, game, isBackgroundDownload, overwrite, cancelToken);
        }

        private async Task<bool> DownloadLogoAsync(IEnumerable<ILogoProvider> logoProviders, Game game, bool isBackgroundDownload, bool overwrite, CancellationToken cancelToken)
        {
            var logoDownloadPath = ExtraMetadataHelper.GetGameLogoPath(game);
            if (!overwrite && FileSystem.FileExists(logoDownloadPath))
            {
                return true;
            }

            var downloadOptions = new LogoDownloadOptions(isBackgroundDownload);            
            foreach (var provider in logoProviders)
            {
                try
                {
                    var logoUrl = provider.GetLogoUrl(game, downloadOptions, cancelToken);
                    if (logoUrl.IsNullOrEmpty())
                    {
                        continue;                        
                    }

                    var downloadIsSuccess = await DownloadFile(logoUrl, logoDownloadPath, cancelToken);
                    if (!downloadIsSuccess)
                    {
                        continue;
                    }

                    if (_settings.ProcessLogosOnDownload)
                    {
                        _logoProcessor.ProcessLogoImage(logoDownloadPath);
                    }

                    OnLogoUpdated(game);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error during logo metadata download for {provider.Id}");
                }
            }

            return false;
        }

        private void OnLogoUpdated(Game game)
        {
            _eventAggregator.Publish(new LogoUpdatedEventArgs(game.Id));
        }

        private void OnVideoUpdated(Game game)
        {
            _eventAggregator.Publish(new LogoUpdatedEventArgs(game.Id));
        }

        private async Task<bool> DownloadFile(string url, string downloadPath, CancellationToken cancelToken)
        {
            FileSystem.DeleteFile(downloadPath);
            var request = HttpRequestFactory.GetHttpFileRequest()
                .WithUrl(url)
                .WithDownloadTo(downloadPath);

            var downloadFileResult = await request.DownloadFileAsync(cancelToken);
            if (!downloadFileResult.IsSuccess)
            {
                return false;
            }
            
            return true;
        }

        public async Task<bool> DownloadVideoAsync(Game game, bool isBackgroundDownload, bool overwrite, CancellationToken cancelToken)
        {
            return await DownloadVideoAsync(_videoProviders, game, isBackgroundDownload, overwrite, cancelToken);
        }

        public async Task<bool> DownloadVideoAsync(IVideoProvider videoProvider, Game game, bool isBackgroundDownload, bool overwrite, CancellationToken cancelToken, bool selectAutomatically = false)
        {
            if (videoProvider is null)
            {
                return false;
            }

            return await DownloadVideoAsync(new[] { videoProvider }, game, isBackgroundDownload, overwrite, cancelToken, selectAutomatically);
        }

        private async Task<bool> DownloadVideoAsync(IEnumerable<IVideoProvider> videoProviders, Game game, bool isBackgroundDownload, bool overwrite, CancellationToken cancelToken, bool selectAutomatically = false)
        {
            var videoDownloadPath = ExtraMetadataHelper.GetGameVideoPath(game);
            if (!overwrite && FileSystem.FileExists(videoDownloadPath))
            {
                return true;
            }

            var downloadOptions = new VideoDownloadOptions(videoDownloadPath, isBackgroundDownload, VideoType.Trailer, selectAutomatically);
            foreach (var provider in videoProviders)
            {
                try
                {
                    var getResult = provider.GetVideo(game, downloadOptions, cancelToken);
                    if (!getResult.IsSuccess)
                    {
                        continue;
                    }

                    var result = getResult.Value;
                    string videoSourcePath;
                    var deleteSource = false;
                    if (result.IsUrl)
                    {
                        videoSourcePath = Path.Combine(Path.GetTempPath(), $"ExtraMetadataLoader_MetadataVideo_{game.Id}_{Guid.NewGuid():N}.mp4");
                        deleteSource = true;
                        var downloadIsSuccess = await DownloadFile(result.Url, videoSourcePath, cancelToken);
                        if (!downloadIsSuccess)
                        {
                            continue;
                        }
                    }
                    else if (result.FilePath.IsNullOrWhiteSpace() || !FileSystem.FileExists(result.FilePath))
                    {
                        continue;
                    }
                    else
                    {
                        videoSourcePath = result.FilePath;
                        deleteSource = true;
                    }

                    ExtraMetadataHelper.GetGameVideoPath(game, true);
                    var processingIsSuccess = _videoProcessor.ProcessVideo(videoSourcePath, videoDownloadPath, false, deleteSource);
                    if (!processingIsSuccess)
                    {
                        FileSystem.DeleteFile(videoSourcePath);
                        continue;
                    }

                    OnVideoUpdated(game);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error during video metadata download for {provider.Id}");
                }
            }

            return false;
        }
    }
}
