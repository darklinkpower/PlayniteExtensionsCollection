using PluginsCommon;
using SteamScreenshots.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SteamScreenshots.Domain.ValueObjects
{
    public class ScreenshotData
    {
        public string ThumbnailUrl { get; }
        public string FullImageUrl { get; }

        public ScreenshotData(string thumbnailUrl, string fullImageUrl)
        {
            ThumbnailUrl = thumbnailUrl ?? string.Empty;
            FullImageUrl = fullImageUrl ?? string.Empty;
        }
    }

}