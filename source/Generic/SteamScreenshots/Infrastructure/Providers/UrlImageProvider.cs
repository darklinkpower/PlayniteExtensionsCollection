using FlowHttp;
using Playnite.SDK;
using PluginsCommon;
using PluginsCommon.Converters;
using SteamScreenshots.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TemporaryCache;

namespace SteamScreenshots.Infrastructure.Providers
{
    public class UrlImageProvider : IImageProvider
    {
        private readonly string _storageDirectory;
        private readonly ILogger _logger;
        private static readonly CacheManager<string, BitmapImage> _imagesCacheManager = new CacheManager<string, BitmapImage>(TimeSpan.FromSeconds(60));
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        private static readonly BitmapImage _fallbackImage = CreateTransparentFallbackImage();

        public UrlImageProvider(string storageDirectory, ILogger logger)
        {
            _storageDirectory = storageDirectory ?? throw new ArgumentNullException(nameof(storageDirectory), "Storage directory cannot be null.");
            _logger = logger;
        }

        public bool DownloadUriToStorage(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var storagePath = GetUriStorageLocation(url);
                if (FileSystem.FileExists(storagePath))
                {
                    return true;
                }

                var tempStoragePath = storagePath + ".tmp";
                var request = HttpRequestFactory.GetHttpFileRequest()
                    .WithUrl(url)
                    .WithDownloadTo(tempStoragePath)
                    .WithTimeout(DefaultTimeout);

                var result = request.DownloadFile(cancellationToken);
                if (!result.IsSuccess)
                {
                    // Clean up incomplete download
                    FileSystem.DeleteFileSafe(tempStoragePath);
                    return false;
                }

                FileSystem.MoveFile(tempStoragePath, storagePath);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error downloading file: {url}");
                return false;
            }
        }

        public BitmapImage LoadImage(string url)
        {
            return LoadImageInternal(url);
        }

        public BitmapImage LoadImageWithDecodeMaxDimensions(string url, int decodeMaxWidth = 0, int decodeMaxHeight = 0)
        {
            return LoadImageInternal(url, decodeMaxWidth, decodeMaxHeight);
        }

        private BitmapImage LoadImageInternal(string url, int decodeMaxWidth = 0, int decodeMaxHeight = 0)
        {
            try
            {
                var fileName = GetUriStorageFilename(url);
                var storagePath = GetFilenameStorageLocation(fileName);

                var bitmapImage = LoadImageFromStorage(url, storagePath, decodeMaxWidth, decodeMaxHeight);
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image {url}: {ex.Message}");
                return GetFallbackImage();
            }
        }

        private BitmapImage LoadImageFromStorage(string url, string storagePath, int decodeMaxWidth, int decodeMaxHeight)
        {
            var key = $"{storagePath}_{decodeMaxWidth}_{decodeMaxHeight}";
            if (_imagesCacheManager.TryGetValue(key, out var cachedImage))
            {
                return cachedImage;
            }

            var shouldDownloadImage = true;
            if (FileSystem.FileExists(storagePath))
            {
                shouldDownloadImage = false;
                var fileSize = new FileInfo(storagePath).Length;
                if (fileSize == 0) // There were reports of images downloads being incomplete and stored so we delete those
                {
                    FileSystem.DeleteFileSafe(storagePath);
                    shouldDownloadImage = true;
                }
            }

            if (shouldDownloadImage)
            {
                var success = DownloadUriToStorage(url);
                if (!success)
                {
                    return GetFallbackImage();
                }
            }

            var bitmapImage = CreateResizedBitmapImageFromPath(storagePath, decodeMaxWidth, decodeMaxHeight);
            _imagesCacheManager.Add(key, bitmapImage);
            return bitmapImage;
        }

        private static BitmapImage CreateResizedBitmapImageFromPath(string filePath, int decodeMaxWidth, int decodeMaxHeight)
        {
            try
            {
                if (!FileSystem.FileExists(filePath))
                {
                    throw new FileNotFoundException("File does not exist at path.", filePath);
                }

                using (var fileStream = FileSystem.OpenReadFileStreamSafe(filePath))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        return GetBitmapImageFromBufferedStream(memoryStream, decodeMaxWidth, decodeMaxHeight);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating BitmapImage from path: {filePath} - {ex.Message}");
                FileSystem.DeleteFileSafe(filePath);
                return GetFallbackImage();
            }
        }

        private static BitmapImage GetBitmapImageFromBufferedStream(Stream stream, int decodeMaxWidth, int decodeMaxHeight)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

            if (decodeMaxWidth != 0 || decodeMaxHeight != 0)
            {
                (int originalWidth, int originalHeight) = GetImageDimensions(stream);

                int newWidth = decodeMaxWidth;
                int newHeight = decodeMaxHeight;

                // Case 1: Only decodeMaxHeight is provided
                if (decodeMaxWidth == 0 && decodeMaxHeight != 0)
                {
                    newWidth = (int)(originalWidth * ((double)decodeMaxHeight / originalHeight));
                }
                // Case 2: Only decodeMaxWidth is provided
                else if (decodeMaxHeight == 0 && decodeMaxWidth != 0)
                {
                    newHeight = (int)(originalHeight * ((double)decodeMaxWidth / originalWidth));
                }
                // Case 3: Both values are provided, resize while preserving aspect ratio
                else if (decodeMaxWidth != 0 && decodeMaxHeight != 0)
                {
                    double widthScale = (double)decodeMaxWidth / originalWidth;
                    double heightScale = (double)decodeMaxHeight / originalHeight;

                    double scaleFactor = Math.Min(widthScale, heightScale);

                    newWidth = (int)(originalWidth * scaleFactor);
                    newHeight = (int)(originalHeight * scaleFactor);
                }

                bitmapImage.DecodePixelWidth = newWidth;
                bitmapImage.DecodePixelHeight = newHeight;
            }

            stream.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = stream;
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }


        private static (int width, int height) GetImageDimensions(Stream stream)
        {
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
            return (decoder.Frames[0].PixelWidth, decoder.Frames[0].PixelHeight);
        }

        private static BitmapImage GetFallbackImage()
        {
            return _fallbackImage;
        }

        private string GetUriStorageLocation(string url)
        {
            var fileName = GetUriStorageFilename(url);
            return GetFilenameStorageLocation(fileName);
        }

        private string GetUriStorageFilename(string url)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                return Paths.ReplaceInvalidCharacters(Path.GetFileName(uri.LocalPath));
            }

            throw new UriFormatException("Invalid URL format.");
        }

        private string GetFilenameStorageLocation(string fileName)
        {
            return Path.Combine(_storageDirectory, Paths.GetSafePathName(fileName));
        }

        private static BitmapImage CreateTransparentFallbackImage()
        {
            var width = 160;
            var height = 90;
            double dpiX = 96;
            double dpiY = 96;

            var writeableBitmap = new WriteableBitmap(width, height, dpiX, dpiY, PixelFormats.Pbgra32, null);
            var pixels = new int[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 0x00000000;
            }

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * sizeof(int), 0);
            return WriteableBitmapToBitmapImage(writeableBitmap);
        }

        private static BitmapImage WriteableBitmapToBitmapImage(WriteableBitmap writeableBitmap)
        {
            var bitmapImage = new BitmapImage();
            using (var memoryStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));

                encoder.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }


    }
}