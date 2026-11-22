using SplashScreen.Models;

namespace SplashScreen.Helpers
{
    internal static class SplashSettingsSyncHelper
    {
        internal static void ApplyGlobalIndicatorSettings(GeneralSplashSettings target, GeneralSplashSettings global)
        {
            if (target == null || global == null)
            {
                return;
            }

            target.EnableLoadingSpinner = global.EnableLoadingSpinner;
            target.LoadingSpinnerSize = global.LoadingSpinnerSize;
            target.LoadingSpinnerOpacity = global.LoadingSpinnerOpacity;
            target.LoadingSpinnerThickness = global.LoadingSpinnerThickness;
            target.LoadingSpinnerDashLength = global.LoadingSpinnerDashLength;
            target.LoadingSpinnerDashCount = global.LoadingSpinnerDashCount;
            target.LoadingSpinnerRoundedDashes = global.LoadingSpinnerRoundedDashes;
            target.LoadingSpinnerRotationSeconds = global.LoadingSpinnerRotationSeconds;
            target.EnableLoadingSpinnerAutoSpeed = global.EnableLoadingSpinnerAutoSpeed;
        }
    }
}