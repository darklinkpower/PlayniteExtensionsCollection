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

namespace VNDBNexus.Converters
{
    public class ImageUriToBitmapImageConverter : IValueConverter
    {
        private readonly string _storageDirectory;
        private static readonly CacheManager<string, BitmapImage> _imagesCacheManager = new CacheManager<string, BitmapImage>(TimeSpan.FromSeconds(60));

        public ImageUriToBitmapImageConverter(string storageDirectory)
        {
            _storageDirectory = storageDirectory;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Uri uri)
            {
                var fileName = Paths.ReplaceInvalidCharacters(uri.ToString().Replace(@"https://", string.Empty));
                var existingCache = _imagesCacheManager.GetCache(fileName, true);
                if (existingCache != null)
                {
                    return existingCache.Item;
                }

                var storagePath = Path.Combine(_storageDirectory, Paths.GetSafePathName(fileName));
                if (!FileSystem.FileExists(storagePath))
                {
                    var request = HttpRequestFactory.GetHttpFileRequest()
                        .WithUrl(uri)
                        .WithDownloadTo(storagePath);
                    var result = request.DownloadFile();
                    if (!result.IsSuccess)
                    {
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
