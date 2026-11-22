using System;
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

        /// <summary>
        /// Gets or sets whether loading spinner should be displayed.
        /// </summary>
        bool EnableLoadingSpinner { get; set; }

        /// <summary>
        /// Gets or sets loading spinner size in pixels.
        /// </summary>
        int LoadingSpinnerSize { get; set; }

        /// <summary>
        /// Gets or sets loading spinner opacity.
        /// </summary>
        double LoadingSpinnerOpacity { get; set; }

        /// <summary>
        /// Gets or sets loading spinner stroke thickness in pixels.
        /// </summary>
        double LoadingSpinnerThickness { get; set; }

        /// <summary>
        /// Gets or sets loading spinner dash length as a percentage of each dash slot.
        /// </summary>
        double LoadingSpinnerDashLength { get; set; }

        /// <summary>
        /// Gets or sets loading spinner dash count.
        /// </summary>
        int LoadingSpinnerDashCount { get; set; }

        /// <summary>
        /// Gets or sets whether spinner dashes should have rounded caps.
        /// </summary>
        bool LoadingSpinnerRoundedDashes { get; set; }

        /// <summary>
        /// Gets or sets the base spinner rotation duration in seconds.
        /// </summary>
        double LoadingSpinnerRotationSeconds { get; set; }

        /// <summary>
        /// Gets or sets whether spinner speed should be adjusted by spinner size.
        /// </summary>
        bool EnableLoadingSpinnerAutoSpeed { get; set; }
        
        /// <summary>
        /// Gets or sets the safe area padding in pixels used by logo and loading spinner.
        /// </summary>
        int SafeAreaPadding { get; set; }

        /// <summary>
        /// Gets or sets background image opacity.
        /// </summary>
        double BackgroundImageOpacity { get; set; }
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
        public bool EnableLoadingSpinner { get; set; } = true;
        public int LoadingSpinnerSize { get; set; } = 50;
        public double LoadingSpinnerOpacity { get; set; } = 0.85;
        public double LoadingSpinnerThickness { get; set; } = 2.0;
        public double LoadingSpinnerDashLength { get; set; } = 70.0;
        public int LoadingSpinnerDashCount { get; set; } = 16;
        public bool LoadingSpinnerRoundedDashes { get; set; } = false;
        public double LoadingSpinnerRotationSeconds { get; set; } = 3.0;
        public bool EnableLoadingSpinnerAutoSpeed { get; set; } = true;
        public int SafeAreaPadding { get; set; } = 60;
        public double BackgroundImageOpacity { get; set; } = 0.60;
    }
}