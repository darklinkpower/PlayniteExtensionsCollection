using Playnite.SDK;
using SteamScreenshots.Domain.Enums;
using SteamScreenshots.Domain.Interfaces;
using SteamScreenshots.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamScreenshots.Application.Services
{
    public class ScreenshotManagementService : IDisposable
    {
        private bool _disposed = false;
        private readonly IDictionary<ScreenshotServiceType, IScreenshotProvider> _providers;
        private readonly IImageProvider _imageProvider;
        private readonly ILogger _logger;
        private SemaphoreSlim _semaphore;

        public ScreenshotManagementService(IDictionary<ScreenshotServiceType, IScreenshotProvider> providers, IImageProvider imageProvider, ILogger logger)
        {
            _providers = providers;
            _imageProvider = imageProvider;
            _logger = logger;
            _semaphore = new SemaphoreSlim(4);
        }

        public async Task<List<Screenshot>> GetScreenshots(ScreenshotServiceType serviceType,
            string id,
            ScreenshotInitializationOptions screenshotInitializationOptions,
            CancellationToken cancellationToken = default)
        {
            if (!_providers.ContainsKey(serviceType))
            {
                throw new NotSupportedException($"Screenshot service {serviceType} is not supported.");
            }

            var screenshotsData = _providers[serviceType].GetScreenshots(id, cancellationToken);
            if (!screenshotsData.HasItems())
            {
                return new List<Screenshot>();
            }

            var screenshots = screenshotsData
                .Select(s => new Screenshot(s.ThumbnailUrl, s.FullImageUrl, _imageProvider))
                .ToList();

            var initializeTasks = new List<Task>();
            foreach (var screenshot in screenshots)
            {
                if (!screenshotInitializationOptions.LazyLoadThumbnail)
                {
                    initializeTasks.Add(InitializeWithSemaphoreAsync(screenshot.InitializeThumbnail, _semaphore));
                }

                if (!screenshotInitializationOptions.LazyLoadFullImage)
                {
                    initializeTasks.Add(InitializeWithSemaphoreAsync(screenshot.InitializeFullImage, _semaphore));
                }
            }

            if (initializeTasks.Any())
            {
                // Initialize at least the first image so it's not loaded synchronously when first displayed
                if (screenshotInitializationOptions.LazyLoadFullImage)
                {
                    initializeTasks.Add(InitializeWithSemaphoreAsync(screenshots[0].InitializeFullImage, _semaphore));
                }

                await Task.WhenAll(initializeTasks);
            }

            return screenshots;
        }

        private async Task InitializeWithSemaphoreAsync(Action initializeFunc, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                await Task.Run(() => initializeFunc());
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Dispose();
                _semaphore = null;
                _disposed = true;
            }
        }

    }

}