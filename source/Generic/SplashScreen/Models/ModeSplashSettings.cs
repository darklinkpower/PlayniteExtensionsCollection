using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplashScreen.Models
{
    public interface IModeSplashSettings
    {
        /// <summary>
        /// Gets or sets if execution for mode is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets if background images are enabled for Splashscreen.
        /// </summary>
        bool EnableBackgroundImage { get; set; }

        /// <summary>
        /// Gets or sets if videos are enabled for Splashscreen.
        /// </summary>
        bool EnableVideos { get; set; }

        /// <summary>
        /// Gets or sets if micro trailers should be used for videos.
        /// </summary>
        bool EnableMicroTrailerVideos { get; set; }

        /// <summary>
        /// Gets or sets if Splashscreen window should be closed automatically on game start.
        /// </summary>
        bool CloseSplashscreenAutomatic { get; set; }
    }

    public class ModeSplashSettings : IModeSplashSettings
    {
        public bool IsEnabled { get; set; } = true;
        public bool EnableBackgroundImage { get; set; } = true;
        public bool EnableVideos { get; set; } = true;
        public bool EnableMicroTrailerVideos { get; set; } = false;
        public bool CloseSplashscreenAutomatic { get; set; } = true;
    }
}