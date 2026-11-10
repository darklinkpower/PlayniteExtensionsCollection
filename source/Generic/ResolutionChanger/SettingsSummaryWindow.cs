using Playnite.SDK;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DisplayHelper
{
    public enum SummarySource
    {
        None,
        Game,
        Global
    }

    public sealed class SummaryRow
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public SummarySource Source { get; set; }
        public string SourceText { get; set; }
        public bool IsWarning { get; set; }
    }

    public enum SummaryNoteKind
    {
        Action,
        Info,
        Warning
    }

    public sealed class SummaryNote
    {
        public SummaryNoteKind Kind { get; set; }
        public string Text { get; set; }

        public SummaryNote(SummaryNoteKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }
    }

    /// <summary>
    /// Themed window showing a game's effective Display Helper settings.
    /// Uses Playnite theme brushes when available so it matches the user's theme.
    /// </summary>
    public static class SettingsSummaryWindow
    {
        public static void Show(IPlayniteAPI api, string gameName, IList<SummaryRow> rows, IList<SummaryNote> notes)
        {
            var window = api.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowCloseButton = true,
                ShowMaximizeButton = false,
                ShowMinimizeButton = false
            });

            window.Title = "Display Helper";
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.ResizeMode = ResizeMode.NoResize;
            window.Owner = api.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Content = BuildContent(window, gameName, rows, notes, () => window.Close());
            window.ShowDialog();
        }

        public static FrameworkElement BuildContent(FrameworkElement window, string gameName, IList<SummaryRow> rows, IList<SummaryNote> notes, System.Action onClose)
        {
            var text = Brush(window, "TextBrush", Color.FromRgb(0xEE, 0xEE, 0xEE));
            var muted = Brush(window, "TextBrushDarker", Color.FromRgb(0x9A, 0x9A, 0x9A));
            var accent = Brush(window, "GlyphBrush", Color.FromRgb(0x4F, 0xA3, 0xF7));
            var warning = Brush(window, "WarningBrush", Color.FromRgb(0xF0, 0xA0, 0x30));
            var panel = new SolidColorBrush(Color.FromArgb(0x22, 0x80, 0x80, 0x80));
            var border = new SolidColorBrush(Color.FromArgb(0x44, 0x80, 0x80, 0x80));

            var root = new StackPanel { Margin = new Thickness(24, 20, 24, 20), MinWidth = 520, MaxWidth = 680 };

            // Header
            root.Children.Add(new TextBlock
            {
                Text = gameName,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = text,
                TextWrapping = TextWrapping.Wrap
            });
            root.Children.Add(new TextBlock
            {
                Text = "Display settings used when this game starts",
                FontSize = 12,
                Foreground = muted,
                Margin = new Thickness(0, 2, 0, 16)
            });

            // Settings table: Setting | Value | Source
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Header row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var headerBackground = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x33, 0x80, 0x80, 0x80)),
                BorderBrush = border,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            Grid.SetColumnSpan(headerBackground, 3);
            grid.Children.Add(headerBackground);
            var headers = new[] { "SETTING", "VALUE", "SOURCE" };
            for (var c = 0; c < headers.Length; c++)
            {
                var header = new TextBlock
                {
                    Text = headers[c],
                    Foreground = muted,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(12, 7, 12, 7),
                    HorizontalAlignment = c == 2 ? HorizontalAlignment.Right : HorizontalAlignment.Left
                };
                Grid.SetColumn(header, c);
                grid.Children.Add(header);
            }

            for (var r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                var i = r + 1;
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Row background + separator line
                var rowBackground = new Border
                {
                    Background = r % 2 == 1 ? panel : Brushes.Transparent,
                    BorderBrush = border,
                    BorderThickness = new Thickness(0, 0, 0, r == rows.Count - 1 ? 0 : 1)
                };
                Grid.SetRow(rowBackground, i);
                Grid.SetColumnSpan(rowBackground, 3);
                grid.Children.Add(rowBackground);

                var label = new TextBlock
                {
                    Text = row.Label,
                    Foreground = muted,
                    FontSize = 13,
                    Margin = new Thickness(12, 8, 20, 8),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(label, i);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                var isUnset = row.Source == SummarySource.None && !row.IsWarning;
                var value = new TextBlock
                {
                    Text = row.Value,
                    Foreground = row.IsWarning ? warning : (isUnset ? muted : text),
                    FontSize = 14,
                    FontWeight = isUnset ? FontWeights.Normal : FontWeights.SemiBold,
                    FontStyle = isUnset ? FontStyles.Italic : FontStyles.Normal,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 8, 12, 8),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(value, i);
                Grid.SetColumn(value, 1);
                grid.Children.Add(value);

                if (!string.IsNullOrEmpty(row.SourceText))
                {
                    var tagBrush = row.Source == SummarySource.Game ? accent : muted;
                    var tag = new Border
                    {
                        BorderBrush = tagBrush,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(9),
                        Padding = new Thickness(8, 1, 8, 2),
                        Margin = new Thickness(0, 0, 12, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Child = new TextBlock
                        {
                            Text = row.SourceText,
                            Foreground = tagBrush,
                            FontSize = 11
                        }
                    };
                    Grid.SetRow(tag, i);
                    Grid.SetColumn(tag, 2);
                    grid.Children.Add(tag);
                }
            }

            root.Children.Add(new Border
            {
                BorderBrush = border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                ClipToBounds = true,
                Child = grid
            });

            // On launch panel
            root.Children.Add(new TextBlock
            {
                Text = "WHEN YOU LAUNCH",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = muted,
                Margin = new Thickness(0, 20, 0, 8)
            });

            var notesPanel = new StackPanel();
            foreach (var note in notes)
            {
                string glyph;
                Brush glyphBrush;
                switch (note.Kind)
                {
                    case SummaryNoteKind.Warning:
                        glyph = "⚠";
                        glyphBrush = warning;
                        break;
                    case SummaryNoteKind.Info:
                        glyph = "ℹ";
                        glyphBrush = muted;
                        break;
                    default:
                        glyph = "→";
                        glyphBrush = accent;
                        break;
                }

                var line = new DockPanel { Margin = new Thickness(0, 4, 0, 4) };
                var icon = new TextBlock
                {
                    Text = glyph,
                    Foreground = glyphBrush,
                    FontFamily = new FontFamily("Segoe UI Symbol"),
                    FontSize = 14,
                    Width = 24,
                    VerticalAlignment = VerticalAlignment.Top
                };
                DockPanel.SetDock(icon, Dock.Left);
                line.Children.Add(icon);
                line.Children.Add(new TextBlock
                {
                    Text = note.Text,
                    Foreground = note.Kind == SummaryNoteKind.Info ? muted : text,
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                });
                notesPanel.Children.Add(line);
            }

            root.Children.Add(new Border
            {
                Background = panel,
                BorderBrush = border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 10, 14, 10),
                Child = notesPanel
            });

            var close = new Button
            {
                Content = "Close",
                MinWidth = 90,
                Margin = new Thickness(0, 18, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                IsDefault = true,
                IsCancel = true
            };
            close.Click += (s, e) => onClose?.Invoke();
            root.Children.Add(close);
            return root;
        }

        private static Brush Brush(FrameworkElement element, string key, Color fallback)
        {
            return element.TryFindResource(key) as Brush ?? new SolidColorBrush(fallback);
        }
    }
}
