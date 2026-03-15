using System;
using System.Windows;
using System.Windows.Media;

namespace SplashScreen.Helpers
{
    internal static class SpinnerGeometryBuilder
    {
        internal static Geometry BuildDashGeometry(double spinnerSize, double thickness, double dashLengthPx, int dashCount, bool roundedDashes)
        {
            var clampedDashCount = Math.Max(2, Math.Min(360, dashCount));
            var innerSize = Math.Max(2.0, spinnerSize - thickness);
            var radius = innerSize / 2.0;
            var circumference = 2.0 * Math.PI * radius;
            var slotPx = circumference / clampedDashCount;
            var slotAngle = (2.0 * Math.PI) / clampedDashCount;

            var centerlineDashPx = Math.Max(0.25, dashLengthPx);
            if (roundedDashes)
            {
                centerlineDashPx = Math.Max(0.25, centerlineDashPx - thickness);
            }

            if (centerlineDashPx >= slotPx)
            {
                centerlineDashPx = slotPx * 0.9;
            }

            var dashAngle = centerlineDashPx / radius;
            var maxDashAngle = slotAngle * 0.9;
            if (dashAngle > maxDashAngle)
            {
                dashAngle = maxDashAngle;
            }

            var center = spinnerSize / 2.0;
            var geometry = new PathGeometry();

            for (var i = 0; i < clampedDashCount; i++)
            {
                var centerAngle = (-Math.PI / 2.0) + (i * slotAngle);
                var startAngle = centerAngle - (dashAngle / 2.0);
                var endAngle = centerAngle + (dashAngle / 2.0);

                var startPoint = PointOnCircle(center, radius, startAngle);
                var endPoint = PointOnCircle(center, radius, endAngle);

                var figure = new PathFigure
                {
                    StartPoint = startPoint,
                    IsClosed = false,
                    IsFilled = false
                };

                figure.Segments.Add(new ArcSegment
                {
                    Point = endPoint,
                    Size = new Size(radius, radius),
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.Clockwise
                });

                geometry.Figures.Add(figure);
            }

            return geometry;
        }

        private static Point PointOnCircle(double center, double radius, double angle)
        {
            return new Point(
                center + (radius * Math.Cos(angle)),
                center + (radius * Math.Sin(angle))
            );
        }
    }
}
