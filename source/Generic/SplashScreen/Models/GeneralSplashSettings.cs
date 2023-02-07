using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SplashScreen.Models
{
    public interface IGeneralSplashSettings
    {
        /// <summary>
        /// Gets or sets if a black splashcreen should be displayed.
        /// </summary>
        bool UseBlackSplashscreen { get; set; }

        /// <summary>
        /// Gets or sets if Fade-In animation for image should be enabled.
        /// </summary>
        bool EnableImageFadeInAnimation { get; set; }

        /// <summary>
        /// Gets or sets if logo should be displayed in Splashscren if available.
        /// </summary>
        bool EnableLogoDisplay { get; set; }

        /// <summary>
        /// Gets or sets if logo should show with a fade in animation.
        /// </summary>
        bool EnableLogoFadeInAnimation { get; set; }

        /// <summary>
        /// Gets or sets if game icon should be displayed in instead of logo.
        /// </summary>
        bool LogoUseIconAsLogo { get; set; }

        /// <summary>
        /// Gets or sets if custom background image should be used for SplashScreen.
        /// </summary>
        bool EnableCustomBackgroundImage { get; set; }

        /// <summary>
        /// Gets or sets if logo should be displayed when using custom background image.
        /// </summary>
        bool EnableLogoDisplayOnCustomBackground { get; set; }

        /// <summary>
        /// Gets or sets custom background image file.
        /// </summary>
        string CustomBackgroundImage { get; set; }

        /// <summary>
        /// Gets or sets logo display horizontal alignment.
        /// </summary>
        HorizontalAlignment LogoHorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets logo display vertical alignment.
        /// </summary>
        VerticalAlignment LogoVerticalAlignment { get; set; }

        /// <summary>
        /// Gets or sets Desktop Mode specific settings.
        /// </summary>
        ModeSplashSettings DesktopModeSettings { get; set; }

        /// <summary>
        /// Gets or sets Fullscreen Mode specific settings.
        /// </summary>
        ModeSplashSettings FullscreenModeSettings { get; set; }
    }

    public class GeneralSplashSettings : IGeneralSplashSettings
    {
        public bool UseBlackSplashscreen { get; set; } = false;
        public bool EnableImageFadeInAnimation { get; set; } = true;
        public bool EnableLogoDisplay { get; set; } = true;
        public bool EnableLogoFadeInAnimation { get; set; } = true;
        public bool LogoUseIconAsLogo { get; set; } = false;
        public bool EnableCustomBackgroundImage { get; set; } = false;
        public bool EnableLogoDisplayOnCustomBackground { get; set; } = true;
        public string CustomBackgroundImage { get; set; } = null;
        public HorizontalAlignment LogoHorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment LogoVerticalAlignment { get; set; } = VerticalAlignment.Bottom;
        public ModeSplashSettings DesktopModeSettings { get; set; } = new ModeSplashSettings();
        public ModeSplashSettings FullscreenModeSettings { get; set; } = new ModeSplashSettings();
    }
}