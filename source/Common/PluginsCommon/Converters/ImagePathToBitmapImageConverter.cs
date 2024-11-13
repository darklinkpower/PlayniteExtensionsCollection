using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PluginsCommon.Converters
{
    public class ImagePathToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconPath && !string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    if (FileSystem.FileExists(iconPath))
                    {
                        var iconUri = new Uri(iconPath, UriKind.Absolute);
                        var image = new BitmapImage(iconUri);
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
