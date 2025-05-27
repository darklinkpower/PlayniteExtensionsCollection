using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoverCollageMaker.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for ColorPickerControl.xaml
    /// </summary>
    public partial class ColorPickerControl : UserControl, INotifyPropertyChanged
    {

        private WriteableBitmap _colorSpectrumBitmap;
        private bool _isMouseDown = false;
        private Point _lastMousePosition;

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPickerControl),
        new PropertyMetadata(Colors.Black, OnColorChanged));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set
            {
                SetValue(SelectedColorProperty, value);
                OnPropertyChanged(nameof(SelectedColor));
                OnPropertyChanged(nameof(SelectedColorBrush));
                OnPropertyChanged(nameof(SelectedColorHex));
            }
        }

        public SolidColorBrush SelectedColorBrush => new SolidColorBrush(SelectedColor);

        public byte Red
        {
            get => SelectedColor.R;
            set
            {
                SelectedColor = Color.FromArgb(SelectedColor.A, value, SelectedColor.G, SelectedColor.B);
                OnPropertyChanged(nameof(Red));
                OnPropertyChanged(nameof(SelectedColorHex));
            }
        }

        public byte Green
        {
            get => SelectedColor.G;
            set
            {
                SelectedColor = Color.FromArgb(SelectedColor.A, SelectedColor.R, value, SelectedColor.B);
                OnPropertyChanged(nameof(Green));
                OnPropertyChanged(nameof(SelectedColorHex));
            }
        }

        public byte Blue
        {
            get => SelectedColor.B;
            set
            {
                SelectedColor = Color.FromArgb(SelectedColor.A, SelectedColor.R, SelectedColor.G, value);
                OnPropertyChanged(nameof(Blue));
                OnPropertyChanged(nameof(SelectedColorHex));
            }
        }

        public byte Alpha
        {
            get => SelectedColor.A;
            set
            {
                SelectedColor = Color.FromArgb(value, SelectedColor.R, SelectedColor.G, SelectedColor.B);
                OnPropertyChanged(nameof(Alpha));
                OnPropertyChanged(nameof(SelectedColorHex));
            }
        }

        public string SelectedColorHex
        {
            get => $"#{SelectedColor.A:X2}{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                try
                {
                    var str = $"#{value.Trim('#')}";
                    var color = (Color)ColorConverter.ConvertFromString(str);
                    if (color != null)
                    {
                        SelectedColor = color;
                        OnPropertyChanged(nameof(SelectedColorHex));
                    }
                }
                catch
                {

                }
            }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ColorPickerControl)d;
            control.OnPropertyChanged(nameof(Red));
            control.OnPropertyChanged(nameof(Green));
            control.OnPropertyChanged(nameof(Blue));
            control.OnPropertyChanged(nameof(Alpha));
            control.OnPropertyChanged(nameof(SelectedColorBrush));
            control.OnPropertyChanged(nameof(SelectedColorHex));
        }

        public ColorPickerControl()
        {
            InitializeComponent();
            UpdateColorSpectrum();
        }

        private void UpdateColorSpectrum()
        {
            int width = Convert.ToInt32(ColorSpectrum.Width);
            int height = Convert.ToInt32(ColorSpectrum.Height);

            var colorSpectrumBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[width * height * 4];

            // Generate the color spectrum based on selected color's hue
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate the corresponding hue for each x-coordinate
                    double hue = ((double)x / width) * 360;
                    double saturation = 1.0;
                    double value = 1.0 - ((double)y / height); // Use y to control value (brightness)

                    Color hsvColor = FromHsv(hue, saturation, value);

                    int pixelIndex = (y * width + x) * 4;
                    pixels[pixelIndex + 0] = hsvColor.B; // Blue
                    pixels[pixelIndex + 1] = hsvColor.G; // Green
                    pixels[pixelIndex + 2] = hsvColor.R; // Red
                    pixels[pixelIndex + 3] = 255;        // Alpha
                }
            }

            colorSpectrumBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
            ColorSpectrum.Source = colorSpectrumBitmap;
            _colorSpectrumBitmap = colorSpectrumBitmap;
        }

        private void ColorSpectrum_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            ColorSpectrum.CaptureMouse();
            Point currentPosition = e.GetPosition(ColorSpectrum);
            UpdateColorIfPositionChanged(currentPosition);
        }

        private void UpdateColorIfPositionChanged(Point currentPosition)
        {
            if (currentPosition != _lastMousePosition)
            {
                UpdateColorFromMousePosition(currentPosition);
                _lastMousePosition = currentPosition;
            }
        }

        private void ColorSpectrum_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                Point currentPosition = e.GetPosition(ColorSpectrum);
                UpdateColorIfPositionChanged(currentPosition);
            }
        }

        private void ColorSpectrum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            ColorSpectrum.ReleaseMouseCapture();
        }

        private void UpdateColorFromMousePosition(Point position)
        {
            int x = (int)(position.X / ColorSpectrum.ActualWidth * _colorSpectrumBitmap.PixelWidth);
            int y = (int)(position.Y / ColorSpectrum.ActualHeight * _colorSpectrumBitmap.PixelHeight);

            // Ensure the coordinates are within bounds
            if (x >= 0 && x < _colorSpectrumBitmap.PixelWidth && y >= 0 && y < _colorSpectrumBitmap.PixelHeight)
            {
                // Get the color at the clicked pixel
                int stride = _colorSpectrumBitmap.PixelWidth * 4; // 4 bytes per pixel
                byte[] pixel = new byte[4];
                _colorSpectrumBitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, stride, 0);

                // Extract ARGB components
                byte blue = pixel[0];
                byte green = pixel[1];
                byte red = pixel[2];
                byte alpha = pixel[3];

                // Set the selected color
                SelectedColor = Color.FromArgb(alpha, red, green, blue);
            }
        }

        public static (double Hue, double Saturation, double Value) ToHsv(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double hue = 0.0;
            if (delta != 0)
            {
                if (max == r)
                {
                    hue = (g - b) / delta + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    hue = (b - r) / delta + 2;
                }
                else
                {
                    hue = (r - g) / delta + 4;
                }
                hue *= 60;
            }

            double saturation = max == 0 ? 0 : delta / max;
            double value = max;

            return (hue, saturation, value);
        }


        public static Color FromHsv(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0: return Color.FromArgb(255, v, t, p);
                case 1: return Color.FromArgb(255, q, v, p);
                case 2: return Color.FromArgb(255, p, v, t);
                case 3: return Color.FromArgb(255, p, q, v);
                case 4: return Color.FromArgb(255, t, p, v);
                default: return Color.FromArgb(255, v, p, q);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}