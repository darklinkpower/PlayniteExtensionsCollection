using ImageMagick;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Services
{
    public interface ILogoProcessor
    {
        bool ProcessLogoImage(string logoPath);
    }

    public class LogoProcessor : ILogoProcessor
    {
        private readonly ExtraMetadataLoaderSettings _settings;
        private readonly ILogger _logger;

        public LogoProcessor(ExtraMetadataLoaderSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public bool ProcessLogoImage(string logoPath)
        {
            try
            {
                using (var image = new MagickImage(logoPath))
                {
                    var originalWidth = image.Width;
                    var originalHeight = image.Height;
                    var imageChanged = false;

                    if (_settings.LogoTrimOnDownload)
                    {
                        image.Trim();
                        if (originalWidth != image.Width || originalHeight != image.Height)
                        {
                            imageChanged = true;
                            originalWidth = image.Width;
                            originalHeight = image.Height;
                        }
                    }

                    if (_settings.SetLogoMaxProcessDimensions)
                    {
                        if (_settings.MaxLogoProcessWidth < image.Width || _settings.MaxLogoProcessHeight < image.Height)
                        {
                            var targetWidth = _settings.MaxLogoProcessWidth;
                            var targetHeight = _settings.MaxLogoProcessHeight;
                            MagickGeometry size = new MagickGeometry(targetWidth, targetHeight)
                            {
                                IgnoreAspectRatio = false
                            };

                            image.Resize(size);
                            if (originalWidth != image.Width || originalHeight != image.Height)
                            {
                                imageChanged = true;
                                originalWidth = image.Width;
                                originalHeight = image.Height;
                            }
                        }
                    }

                    if (imageChanged)
                    {
                        image.Write(logoPath);
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while processing logo {logoPath}");
                return false;
            }
        }
    }
}