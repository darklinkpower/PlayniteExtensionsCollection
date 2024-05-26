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

namespace VNDBNexus.Converters
{
    public class ImageUriToBitmapImageConverter : IValueConverter
    {
        private readonly string _storageDirectory;

        public ImageUriToBitmapImageConverter(string storageDirectory)
        {
            _storageDirectory = storageDirectory;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Uri uri)
            {
                var fileName = Paths.ReplaceInvalidCharacters(uri.ToString().Replace(@"https://", string.Empty));
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

                return CreateResizedBitmapImageFromPath(storagePath);
            }

            return null;
        }

        public static BitmapImage CreateResizedBitmapImageFromPath(string filePath, int maxWidth = 0, int maxHeight = 0)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
