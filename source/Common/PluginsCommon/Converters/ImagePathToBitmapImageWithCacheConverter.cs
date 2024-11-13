using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TemporaryCache;

namespace PluginsCommon.Converters
{
    public class ImagePathToBitmapImageWithCacheConverter : IValueConverter
    {
        private static readonly CacheManager<string, BitmapImage> _imagesCacheManager = new CacheManager<string, BitmapImage>(TimeSpan.FromSeconds(60));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconPath && !string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    if (_imagesCacheManager.TryGetValue(iconPath, out var cache))
                    {
                        return cache;
                    }

                    if (FileSystem.FileExists(iconPath))
                    {
                        var iconUri = new Uri(iconPath, UriKind.Absolute);
                        var image = new BitmapImage(iconUri);
                        _imagesCacheManager.Add(iconPath, image);
                        return image;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading icon: {ex.Message}");
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
