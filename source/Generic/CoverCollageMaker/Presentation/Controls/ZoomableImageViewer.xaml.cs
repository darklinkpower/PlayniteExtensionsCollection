using System;
using System.Collections.Generic;
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
    /// Interaction logic for ZoomableImageViewer.xaml
    /// </summary>
    public partial class ZoomableImageViewer : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(ZoomableImageViewer),
                new PropertyMetadata(null, OnImageSourceChanged));

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(ZoomableImageViewer),
                new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnZoomLevelChanged));

        public static readonly DependencyProperty ResolutionProperty =
            DependencyProperty.Register(nameof(Resolution), typeof(string), typeof(ZoomableImageViewer),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MinZoomLevelProperty =
            DependencyProperty.Register(nameof(MinZoomLevel), typeof(double), typeof(ZoomableImageViewer),
                new PropertyMetadata(20.0));

        public static readonly DependencyProperty MaxZoomLevelProperty =
            DependencyProperty.Register(nameof(MaxZoomLevel), typeof(double), typeof(ZoomableImageViewer),
                new PropertyMetadata(400.0));

        public static readonly DependencyPropertyKey ImageWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageWidth), typeof(double), typeof(ZoomableImageViewer),
                new PropertyMetadata(0.0));
        public static readonly DependencyProperty ImageWidthProperty = ImageWidthPropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey ImageHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageHeight), typeof(double), typeof(ZoomableImageViewer),
                new PropertyMetadata(0.0));
        public static readonly DependencyProperty ImageHeightProperty = ImageHeightPropertyKey.DependencyProperty;
        private readonly ScaleTransform _zoomTransform;
        private readonly TranslateTransform _panTransform;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the image to be displayed.
        /// </summary>
        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the zoom level (percentage) of the image. Default is 100.
        /// </summary>
        public double ZoomLevel
        {
            get => (double)GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets the resolution of the loaded image in "Width×Height" format.
        /// </summary>
        public string Resolution
        {
            get => (string)GetValue(ResolutionProperty);
            private set => SetValue(ResolutionProperty, value);
        }

        /// <summary>
        /// Gets or sets the minimum allowed zoom level (percentage).
        /// </summary>
        public double MinZoomLevel
        {
            get => (double)GetValue(MinZoomLevelProperty);
            set => SetValue(MinZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum allowed zoom level (percentage).
        /// </summary>
        public double MaxZoomLevel
        {
            get => (double)GetValue(MaxZoomLevelProperty);
            set => SetValue(MaxZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets the original width of the image.
        /// </summary>
        public double ImageWidth
        {
            get => (double)GetValue(ImageWidthProperty);
            private set => SetValue(ImageWidthPropertyKey, value);
        }

        /// <summary>
        /// Gets the original height of the image.
        /// </summary>
        public double ImageHeight
        {
            get => (double)GetValue(ImageHeightProperty);
            private set => SetValue(ImageHeightPropertyKey, value);
        }

        /// <summary>
        /// Occurs when the zoom level is changed.
        /// </summary>
        public event EventHandler<double> ZoomChanged;

        #endregion

        #region Constructor

        public ZoomableImageViewer()
        {
            InitializeComponent();
            Loaded += (s, e) => FitToView();
            _zoomTransform = new ScaleTransform();
            _panTransform = new TranslateTransform();
            ZoomContainer.LayoutTransform = _zoomTransform;
            PanContainer.RenderTransform = _panTransform;
        }

        #endregion

        #region Mouse Event Handlers

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            double factor = e.Delta > 0 ? 1.1 : 1 / 1.1;
            Point mousePos = e.GetPosition(PanContainer);

            double clampedX = Math.Max(0, Math.Min(mousePos.X, DisplayedImage.ActualWidth * _zoomTransform.ScaleX));
            double clampedY = Math.Max(0, Math.Min(mousePos.Y, DisplayedImage.ActualHeight * _zoomTransform.ScaleY));
            ZoomAt(new Point(clampedX, clampedY), factor);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isPanning = true;
                _lastMousePosition = e.GetPosition(this);
                Cursor = Cursors.Hand;
                DisplayedImage.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.LowQuality);
                CaptureMouse();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isPanning = false;
                Cursor = Cursors.Arrow;
                ReleaseMouseCapture();
                DisplayedImage.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);
                //ClampPan();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isPanning)
            {
                var pos = e.GetPosition(this);
                var delta = pos - _lastMousePosition;
                _lastMousePosition = pos;

                _panTransform.X += delta.X;
                _panTransform.Y += delta.Y;

                ClampPan();
            }
        }

        #endregion

        #region Property Changed Callbacks

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (ZoomableImageViewer)d;
            if (e.NewValue is ImageSource source)
            {
                viewer.DisplayedImage.Source = source;
                viewer.ImageWidth = source.Width;
                viewer.ImageHeight = source.Height;
                viewer.Resolution = $"{source.Width}×{source.Height}";
                viewer.FitToView();
            }
        }

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (ZoomableImageViewer)d;
            double newZoom = Math.Round(Math.Max(viewer.MinZoomLevel, Math.Min(viewer.MaxZoomLevel, (double)e.NewValue)));

            viewer._zoomTransform.ScaleX = newZoom / 100.0;
            viewer._zoomTransform.ScaleY = newZoom / 100.0;

            if (newZoom != (double)e.NewValue)
            {
                viewer.ZoomLevel = newZoom;
            }

            viewer.ClampPan();
        }

        #endregion

        #region Zooming and Panning Helpers

        private void FitToView()
        {
            if (DisplayedImage.Source == null)
            {
                return;
            }

            double imgW = DisplayedImage.Source.Width;
            double imgH = DisplayedImage.Source.Height;
            double cW = ActualWidth;
            double cH = ActualHeight;

            if (cW == 0 || cH == 0)
            {
                return;
            }

            double zoom = Math.Min(cW / imgW, cH / imgH);
            if (zoom >= 1.0) zoom = 1.0;

            _zoomTransform.ScaleX = zoom;
            _zoomTransform.ScaleY = zoom;
            ZoomLevel = zoom * 100;
            ZoomChanged?.Invoke(this, ZoomLevel);

            double scaledImgW = imgW * zoom;
            double scaledImgH = imgH * zoom;
            _panTransform.X = (cW - scaledImgW) / 2.0;
            _panTransform.Y = (cH - scaledImgH) / 2.0;
        }

        private void ClampPan()
        {
            if (ZoomContainer == null)
            {
                return;
            }

            double zoom = _zoomTransform.ScaleX;
            double contentWidth = ZoomContainer.ActualWidth * zoom;
            double contentHeight = ZoomContainer.ActualHeight * zoom;
            double viewportWidth = ActualWidth;
            double viewportHeight = ActualHeight;

            double minX = contentWidth <= viewportWidth ? (viewportWidth - contentWidth) / 2 : viewportWidth - contentWidth;
            double maxX = contentWidth <= viewportWidth ? minX : 0;

            double minY = contentHeight <= viewportHeight ? (viewportHeight - contentHeight) / 2 : viewportHeight - contentHeight;
            double maxY = contentHeight <= viewportHeight ? minY : 0;
            _panTransform.X = Math.Round(MathUtils.Clamp(_panTransform.X, minX, maxX));
            _panTransform.Y = Math.Round(MathUtils.Clamp(_panTransform.Y, minY, maxY));
        }

        private void ZoomAt(Point mousePos, double factor)
        {
            double oldZoom = _zoomTransform.ScaleX;
            double newZoom = Math.Round(MathUtils.Clamp(oldZoom * factor, MinZoomLevel / 100.0, MaxZoomLevel / 100.0), 2);

            if (Math.Abs(newZoom - oldZoom) < 0.001) return;

            _zoomTransform.ScaleX = newZoom;
            _zoomTransform.ScaleY = newZoom;

            _panTransform.X = mousePos.X - (mousePos.X - _panTransform.X) * (newZoom / oldZoom);
            _panTransform.Y = mousePos.Y - (mousePos.Y - _panTransform.Y) * (newZoom / oldZoom);

            ClampPan();
            ZoomLevel = newZoom * 100;
            ZoomChanged?.Invoke(this, ZoomLevel);
        }

        #endregion

        #region Private Fields

        private bool _isPanning = false;
        private Point _lastMousePosition;

        #endregion

        #region Utility Class

        public static class MathUtils
        {
            public static double Clamp(double value, double min, double max)
                => value < min ? min : (value > max ? max : value);

            public static float Clamp(float value, float min, float max)
                => value < min ? min : (value > max ? max : value);

            public static int Clamp(int value, int min, int max)
                => value < min ? min : (value > max ? max : value);
        }

        #endregion
    }

}
