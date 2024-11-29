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
    public class Screenshot
    {
        private readonly string _thumbnailPath;
        private readonly string _fullImagePath;
        private readonly IImageProvider _imageProvider;

        private readonly Lazy<BitmapImage> _lazyThumbnail;
        private readonly Lazy<BitmapImage> _lazyFullImage;

        public BitmapImage ThumbnailImage => _lazyThumbnail.Value;
        public BitmapImage FullImage => _lazyFullImage.Value;

        public Screenshot(string thumbnailPath, string fullImagePath, IImageProvider imageProvider)
        {
            _thumbnailPath = thumbnailPath;
            _fullImagePath = fullImagePath;
            _imageProvider = imageProvider;
            _lazyThumbnail = new Lazy<BitmapImage>(LoadThumbnail, LazyThreadSafetyMode.ExecutionAndPublication);
            _lazyFullImage = new Lazy<BitmapImage>(LoadFullImage, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private BitmapImage LoadThumbnail()
        {
            if (!_thumbnailPath.IsNullOrEmpty())
            {
                return _imageProvider.LoadImageWithDecodeMaxDimensions(_thumbnailPath, 216, 216);
            }
            else
            {
                return _imageProvider.LoadImageWithDecodeMaxDimensions(_fullImagePath, 216, 216);
            }
        }

        private BitmapImage LoadFullImage()
        {
            return _imageProvider.LoadImage(_fullImagePath);
        }

        public void InitializeThumbnail()
        {
            if (_lazyThumbnail.IsValueCreated)
            {
                return;
            }

            _ = _lazyThumbnail.Value;
        }

        public void InitializeFullImage()
        {
            if (_lazyFullImage.IsValueCreated)
            {
                return;
            }

            _ = _lazyFullImage.Value;
        }

        public void InitializeImages()
        {
            InitializeThumbnail();
            InitializeFullImage();
        }
    }

}