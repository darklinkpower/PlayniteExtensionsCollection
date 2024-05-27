using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VNDBNexus.Converters
{
    public static class ConvertersUtilities
    {
        internal static BitmapImage CreateResizedBitmapImageFromPath(string filePath, int maxWidth = 0, int maxHeight = 0)
        {
            if (!FileSystem.FileExists(filePath))
            {
                return null;
            }
            
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
    }
}
