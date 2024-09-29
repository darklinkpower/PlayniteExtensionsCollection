using GameRelations.Models;
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
using System.Windows.Media.Imaging;
using TemporaryCache;

namespace GameRelations
{
    public static class MatchedGamesUtilities
    {
        private static readonly IPlayniteAPI _playniteApi = API.Instance;
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly CacheManager<string, BitmapImage> _imagesCacheManager = new CacheManager<string, BitmapImage>(TimeSpan.FromSeconds(30));
        private static readonly BitmapImage _defaultCover = new BitmapImage(new Uri("/GameRelations;component/Resources/DefaultCover.png", UriKind.Relative));
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

        public static async Task<IEnumerable<MatchedGameWrapper>> GetGamesWrappersAsync(IEnumerable<Game> games, GameRelationsSettings settings)
        {
            await _bulkGetGamesWrappersLimiter.WaitAsync();
            try
            {
                return games.Select(g => GetGameWrapper(g, settings));
            }
            finally
            {
                _bulkGetGamesWrappersLimiter.Release();
            }
        }

        public static MatchedGameWrapper GetGameWrapper(Game game, GameRelationsSettings settings)
        {
            var bitmapImage = GetGameCoverImage(game, settings);
            return new MatchedGameWrapper(game, bitmapImage);
        }

        private static BitmapImage GetGameCoverImage(Game game, GameRelationsSettings settings)
        {
            if (game.CoverImage.IsNullOrEmpty())
            {
                return _defaultCover;
            }

            var imagePath = _playniteApi.Database.GetFullFilePath(game.CoverImage);
            if (FileSystem.FileExists(imagePath))
            {
                try
                {
                    var coverHeight = settings.CoversHeight;
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