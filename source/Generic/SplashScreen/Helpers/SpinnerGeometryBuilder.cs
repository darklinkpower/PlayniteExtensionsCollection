using System;
using System.Windows;
using System.Windows.Media;

namespace SplashScreen.Helpers
{
    internal static class SpinnerGeometryBuilder
    {
        internal static Geometry BuildDashGeometry(double spinnerSize, double thickness, double dashLengthPercent, int dashCount, bool roundedDashes)
        {
            var clampedDashCount = Math.Max(1, Math.Min(360, dashCount));
            var innerSize = Math.Max(2.0, spinnerSize - thickness);
            var radius = innerSize / 2.0;
            var slotAngle = (2.0 * Math.PI) / clampedDashCount;
            var clampedDashLengthPercent = Math.Max(1.0, Math.Min(100.0, dashLengthPercent));
            var dashAngle = slotAngle * (clampedDashLengthPercent / 100.0);

            var center = spinnerSize / 2.0;
            var geometry = new PathGeometry();

            for (var i = 0; i < clampedDashCount; i++)
            {
                var centerAngle = (-Math.PI / 2.0) + (i * slotAngle);
                var startAngle = centerAngle - (dashAngle / 2.0);
                var endAngle = centerAngle + (dashAngle / 2.0);
                AddDashFigure(geometry, center, radius, startAngle, endAngle, dashAngle);
            }

            return geometry;
        }

        private static void AddDashFigure(PathGeometry geometry, double center, double radius, double startAngle, double endAngle, double dashAngle)
        {
            var normalizedDashAngle = Math.Max(0.0, dashAngle);
            var fullCircleThreshold = (2.0 * Math.PI) - 0.0001;
            var startPoint = PointOnCircle(center, radius, startAngle);

            var figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false,
                IsFilled = false
            };

            if (normalizedDashAngle >= fullCircleThreshold)
            {
                var middlePoint = PointOnCircle(center, radius, startAngle + Math.PI);

                figure.Segments.Add(new ArcSegment
                {
                    Point = middlePoint,
                    Size = new Size(radius, radius),
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.Clockwise
                });

                figure.Segments.Add(new ArcSegment
                {
                    Point = startPoint,
                    Size = new Size(radius, radius),
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.Clockwise
                });
            }
            else
            {
                var endPoint = PointOnCircle(center, radius, endAngle);
                figure.Segments.Add(new ArcSegment
                {
                    Point = endPoint,
                    Size = new Size(radius, radius),
                    IsLargeArc = normalizedDashAngle > Math.PI,
                    SweepDirection = SweepDirection.Clockwise
                });
            }

            geometry.Figures.Add(figure);
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
