using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SteamScreenshots.Domain.Interfaces
{
    public interface IImageProvider
    {
        BitmapImage LoadImage(string path);
        BitmapImage LoadImageWithDecodeMaxDimensions(string url, int decodeMaxWidth = 0, int decodeMaxHeight = 0);
    }
}