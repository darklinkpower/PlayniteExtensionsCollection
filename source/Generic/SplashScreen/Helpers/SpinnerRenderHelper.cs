using System;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SplashScreen.Helpers
{
    internal static class SpinnerRenderHelper
    {
        internal static double ClampThickness(double thickness)
        {
            return Math.Max(0.5, thickness);
        }

        internal static double ClampDashLengthPercent(double dashLengthPercent)
        {
            return Math.Max(1.0, Math.Min(100.0, dashLengthPercent));
        }

        internal static int ClampDashCount(int dashCount)
        {
            return Math.Max(1, Math.Min(360, dashCount));
        }

        internal static double ClampSpinnerSize(double spinnerSize)
        {
            return Math.Max(4.0, spinnerSize);
        }

        internal static double CalculateRotationDurationSeconds(double baseSeconds, bool autoSpeed, double spinnerSize)
        {
            var normalizedBaseSeconds = baseSeconds;
            if (normalizedBaseSeconds <= 0)
            {
                normalizedBaseSeconds = 3.0;
            }

            var durationSeconds = normalizedBaseSeconds;
            if (autoSpeed)
            {
                var normalizedSize = spinnerSize / 50.0;
                if (normalizedSize < 0.4)
                {
                    normalizedSize = 0.4;
                }

                durationSeconds = normalizedBaseSeconds * normalizedSize;
            }

            if (durationSeconds < 0.25)
            {
                durationSeconds = 0.25;
            }

            if (durationSeconds > 20.0)
            {
                durationSeconds = 20.0;
            }

            return durationSeconds;
        }

        internal static void ApplySpinnerAppearance(Path spinnerPath, double spinnerSize, double thickness, double dashLengthPercent, int dashCount, bool roundedDashes)
        {
            if (spinnerPath == null)
            {
                return;
            }

            var normalizedThickness = ClampThickness(thickness);
            var normalizedDashLengthPercent = ClampDashLengthPercent(dashLengthPercent);
            var normalizedDashCount = ClampDashCount(dashCount);
            var normalizedSpinnerSize = ClampSpinnerSize(spinnerSize);

            spinnerPath.StrokeThickness = normalizedThickness;
            spinnerPath.StrokeDashArray = null;
            spinnerPath.StrokeDashOffset = 0;
            spinnerPath.StrokeDashCap = PenLineCap.Flat;
            spinnerPath.StrokeStartLineCap = roundedDashes ? PenLineCap.Round : PenLineCap.Flat;
            spinnerPath.StrokeEndLineCap = roundedDashes ? PenLineCap.Round : PenLineCap.Flat;
            spinnerPath.Data = SpinnerGeometryBuilder.BuildDashGeometry(normalizedSpinnerSize, normalizedThickness, normalizedDashLengthPercent, normalizedDashCount, roundedDashes);
        }
    }
}