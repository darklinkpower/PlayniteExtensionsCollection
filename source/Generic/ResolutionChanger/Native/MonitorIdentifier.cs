using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DisplayHelper.Native
{
    /// <summary>
    /// Shows a large label on every display for a few seconds (like Windows' "Identify"),
    /// using Display Helper's own names so users can tell which DISPLAY is which.
    /// </summary>
    public static class MonitorIdentifier
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const int ShowSeconds = 4;
        private const double BoxWidth = 520;
        private const double BoxHeight = 210;

        public static void Show()
        {
            try
            {
                var labels = MonitorNames.GetMonitorFriendlyNames();
                var numbers = MonitorNames.GetWindowsNumbers();
                var windows = new List<Window>();

                // Physical pixels -> WPF units. Only the center matters, so small
                // DPI differences between monitors still land on the right screen.
                var toDip = GetDeviceToDipScale();

                foreach (var b in MonitorLayout.GetBounds())
                {
                    var gdiShort = b.GdiName.Replace(@"\\.\", string.Empty);
                    var bigText = numbers.TryGetValue(b.GdiName, out var number) ? number.ToString() : gdiShort;
                    labels.TryGetValue(b.GdiName, out var label);
                    var detail = b.IsPrimary ? $"{gdiShort} · primary" : gdiShort;

                    var window = CreateWindow(bigText, label ?? gdiShort, detail);
                    window.Left = b.CenterX * toDip.X - BoxWidth / 2;
                    window.Top = b.CenterY * toDip.Y - BoxHeight / 2;
                    window.Show();
                    windows.Add(window);
                }

                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(ShowSeconds) };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    foreach (var w in windows)
                    {
                        w.Close();
                    }
                };
                timer.Start();
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while identifying displays");
            }
        }

        private static Window CreateWindow(string displayNumber, string label, string position)
        {
            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                Text = displayNumber,
                FontSize = 72,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            if (!string.IsNullOrEmpty(position))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = position,
                    FontSize = 15,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xC8, 0xC8)),
                    Margin = new Thickness(0, 4, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            var window = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                ShowActivated = false,
                Topmost = true,
                Width = BoxWidth,
                Height = BoxHeight,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(0xE6, 0x1A, 0x1F, 0x26)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x4F, 0xA3, 0xF7)),
                    BorderThickness = new Thickness(3),
                    CornerRadius = new CornerRadius(12),
                    Child = stack
                }
            };
            window.MouseLeftButtonDown += (s, e) => window.Close();
            return window;
        }

        private static Vector GetDeviceToDipScale()
        {
            try
            {
                var main = System.Windows.Application.Current?.MainWindow;
                var source = main != null ? PresentationSource.FromVisual(main) : null;
                if (source?.CompositionTarget != null)
                {
                    var m = source.CompositionTarget.TransformFromDevice;
                    return new Vector(m.M11, m.M22);
                }
            }
            catch
            {
            }

            return new Vector(1, 1);
        }
    }
}
