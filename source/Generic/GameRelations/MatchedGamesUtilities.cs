using GameRelations.Models;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TemporaryCache;

namespace GameRelations
{
    public static class MatchedGamesUtilities
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly CacheManager<string, BitmapImage> _imagesCacheManager = new CacheManager<string, BitmapImage>().WithItemLifetime(TimeSpan.FromSeconds(30));
        private static readonly BitmapImage _defaultCover = GetDefaultCover();
        private static SemaphoreSlim _bulkGetGamesWrappersLimiter = new SemaphoreSlim(1);

        public static BitmapImage CreateResizedBitmapImageFromPath(string filePath, int maxWidth, int maxHeight)
        {
            using (var fileStream = FileSystem.OpenReadFileStreamSafe(filePath))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return GetBitmapImageFromBufferedStream(memoryStream, maxWidth, maxHeight);
                }
            }
        }

        private static BitmapImage GetBitmapImageFromBufferedStream(Stream stream, int decodeWidth = 0, int decodeHeight = 0)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.DecodePixelWidth = decodeWidth;
            bitmapImage.DecodePixelHeight = decodeHeight;
            bitmapImage.StreamSource = stream;
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        private static BitmapImage GetDefaultCover()
        {
            var bitmapImage = new BitmapImage(new Uri("/GameRelations;component/Resources/DefaultCover.png", UriKind.Relative));
            bitmapImage.Freeze();
            return bitmapImage;
        }

        internal static async Task<List<MatchedGameWrapper>> GetGamesWrappersAsync(IEnumerable<GameRelationSnapshot> games, int coverHeight, CancellationToken cancellationToken)
        {
            await _bulkGetGamesWrappersLimiter.WaitAsync(cancellationToken);
            try
            {
                var gameWrappers = new List<MatchedGameWrapper>();
                foreach (var game in games)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    gameWrappers.Add(GetGameWrapper(game, coverHeight));
                }

                return gameWrappers;
            }
            finally
            {
                _bulkGetGamesWrappersLimiter.Release();
            }
        }

        private static MatchedGameWrapper GetGameWrapper(GameRelationSnapshot game, int coverHeight)
        {
            var bitmapImage = GetGameCoverImage(game.CoverImagePath, coverHeight);
            return new MatchedGameWrapper(game.Game, bitmapImage);
        }

        private static BitmapImage GetGameCoverImage(string imagePath, int coverHeight)
        {
            if (imagePath.IsNullOrEmpty())
            {
                return _defaultCover;
            }

            if (FileSystem.FileExists(imagePath))
            {
                try
                {
                    var cacheKey = $"{imagePath}_{coverHeight}";
                    if (_imagesCacheManager.TryGetValue(cacheKey, out var cachedBitmap))
                    {
                        return cachedBitmap;
                    }

                    var createdBitmap = CreateResizedBitmapImageFromPath(imagePath, 0, coverHeight);
                    _imagesCacheManager.Add(cacheKey, createdBitmap);

                    return createdBitmap;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error while decoding image {imagePath}");
                }
            }

            return _defaultCover;
        }

    }
}
