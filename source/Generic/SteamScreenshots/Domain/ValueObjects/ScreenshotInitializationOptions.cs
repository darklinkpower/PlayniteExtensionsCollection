using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamScreenshots.Domain.ValueObjects
{
    public class ScreenshotInitializationOptions
    {
        public bool LazyLoadThumbnail { get; }
        public bool LazyLoadFullImage { get; }

        public ScreenshotInitializationOptions(bool lazyLoadThumbnail, bool lazyLoadFullImage)
        {
            LazyLoadThumbnail = lazyLoadThumbnail;
            LazyLoadFullImage = lazyLoadFullImage;
        }
    }
}