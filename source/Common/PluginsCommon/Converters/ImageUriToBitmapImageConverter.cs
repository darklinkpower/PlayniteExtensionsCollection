using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using FlowHttp;
using System.Windows.Media.Imaging;
using TemporaryCache;

namespace PluginsCommon.Converters
{
    public class ImageUriToBitmapImageConverter : IValueConverter
    {
        private readonly string _storageDirectory;
        private readonly bool _useLocalPathForFilenames;
        private static readonly CacheManager<string, BitmapImage> _imagesCacheManager = new CacheManager<string, BitmapImage>(TimeSpan.FromSeconds(60));

        public ImageUriToBitmapImageConverter(string storageDirectory, bool useLocalPathForFilenames = false)
        {
            _storageDirectory = storageDirectory;
            _useLocalPathForFilenames = useLocalPathForFilenames;
        }

        public async Task<bool> DownloadUriToStorageAsync(Uri uri)
        {
            var storagePath = GetUriStorageLocation(uri);
            if (FileSystem.FileExists(storagePath))
            {
                return true;
            }

            var request = HttpRequestFactory.GetHttpFileRequest()
                .WithUrl(uri)
                .WithDownloadTo(storagePath);
            var result = await request.DownloadFileAsync();
            if (!result.IsSuccess)
            {
                if (FileSystem.FileExists(storagePath))
                {
                    FileSystem.DeleteFileSafe(storagePath);
                }

                return false;
            }

            return true;
        }

        private string GetUriStorageLocation(Uri uri)
        {
            var fileName = GetUriStorageFilename(uri);
            return GetFilenameStorageLocation(fileName);
        }

        private string GetUriStorageFilename(Uri uri)
        {
            if (_useLocalPathForFilenames)
            {
                return Paths.ReplaceInvalidCharacters(Path.GetFileName(uri.LocalPath));
            }
            else
            {
                return Paths.ReplaceInvalidCharacters(uri.ToString().Replace(@"https://", string.Empty));
            }
        }

        private string GetFilenameStorageLocation(string fileName)
        {
            return Path.Combine(_storageDirectory, Paths.GetSafePathName(fileName));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Uri uri)
            {
                var fileName = GetUriStorageFilename(uri);
                var existingCache = _imagesCacheManager.GetCache(fileName, true);
                if (existingCache != null)
                {
                    return existingCache.Item;
                }

                var storagePath = GetFilenameStorageLocation(fileName);
                if (!FileSystem.FileExists(storagePath))
                {
                    var request = HttpRequestFactory.GetHttpFileRequest()
                        .WithUrl(uri)
                        .WithDownloadTo(storagePath);
                    var result = request.DownloadFile();
                    if (!result.IsSuccess)
                    {
                        if (FileSystem.FileExists(storagePath))
                        {
                            FileSystem.DeleteFileSafe(storagePath);
                        }

                        return null;
                    }
                }

                var createdBitmapImage = ConvertersUtilities.CreateResizedBitmapImageFromPath(storagePath);
                _imagesCacheManager.SaveCache(fileName, createdBitmapImage);
                return createdBitmapImage;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
