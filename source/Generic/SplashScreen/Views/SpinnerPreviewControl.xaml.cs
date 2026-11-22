using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SplashScreen.Helpers;

namespace SplashScreen.Views
{
    public partial class SpinnerPreviewControl : UserControl
    {
        public static readonly DependencyProperty SpinnerSizeProperty =
            DependencyProperty.Register(nameof(SpinnerSize), typeof(double), typeof(SpinnerPreviewControl),
                new PropertyMetadata(50.0, OnAnimationPropertyChanged));

        public static readonly DependencyProperty SpinnerOpacityProperty =
            DependencyProperty.Register(nameof(SpinnerOpacity), typeof(double), typeof(SpinnerPreviewControl),
                new PropertyMetadata(0.85));

        public static readonly DependencyProperty PreviewBackgroundLevelProperty =
            DependencyProperty.Register(nameof(PreviewBackgroundLevel), typeof(double), typeof(SpinnerPreviewControl),
                new PropertyMetadata(0.5, OnAppearancePropertyChanged));

        public static readonly DependencyProperty SpinnerThicknessProperty =
            DependencyProperty.Register(nameof(SpinnerThickness), typeof(double), typeof(SpinnerPreviewControl),
                new PropertyMetadata(2.0, OnAppearancePropertyChanged));

        public static readonly DependencyProperty SpinnerDashLengthProperty =
            DependencyProperty.Register(nameof(SpinnerDashLength), typeof(double), typeof(SpinnerPreviewControl),
                new PropertyMetadata(70.0, OnAppearancePropertyChanged));

        public static readonly DependencyProperty SpinnerDashCountProperty =
            DependencyProperty.Register(nameof(SpinnerDashCount), typeof(int), typeof(SpinnerPreviewControl),
                new PropertyMetadata(16, OnAppearancePropertyChanged));

        public static readonly DependencyProperty RoundedDashesProperty =
            DependencyProperty.Register(nameof(RoundedDashes), typeof(bool), typeof(SpinnerPreviewControl),
                new PropertyMetadata(false, OnAppearancePropertyChanged));

        public static readonly DependencyProperty RotationSecondsProperty =
            DependencyProperty.Register(nameof(RotationSeconds), typeof(double), typeof(SpinnerPreviewControl),
                new PropertyMetadata(3.0, OnAnimationPropertyChanged));

        public static readonly DependencyProperty AutoSpeedProperty =
            DependencyProperty.Register(nameof(AutoSpeed), typeof(bool), typeof(SpinnerPreviewControl),
                new PropertyMetadata(true, OnAnimationPropertyChanged));

        public double SpinnerSize
        {
            get => (double)GetValue(SpinnerSizeProperty);
            set => SetValue(SpinnerSizeProperty, value);
        }

        public double SpinnerOpacity
        {
            get => (double)GetValue(SpinnerOpacityProperty);
            set => SetValue(SpinnerOpacityProperty, value);
        }

        public double PreviewBackgroundLevel
        {
            get => (double)GetValue(PreviewBackgroundLevelProperty);
            set => SetValue(PreviewBackgroundLevelProperty, value);
        }

        public double SpinnerThickness
        {
            get => (double)GetValue(SpinnerThicknessProperty);
            set => SetValue(SpinnerThicknessProperty, value);
        }

        public double SpinnerDashLength
        {
            get => (double)GetValue(SpinnerDashLengthProperty);
            set => SetValue(SpinnerDashLengthProperty, value);
        }

        public int SpinnerDashCount
        {
            get => (int)GetValue(SpinnerDashCountProperty);
            set => SetValue(SpinnerDashCountProperty, value);
        }

        public bool RoundedDashes
        {
            get => (bool)GetValue(RoundedDashesProperty);
            set => SetValue(RoundedDashesProperty, value);
        }

        public double RotationSeconds
        {
            get => (double)GetValue(RotationSecondsProperty);
            set => SetValue(RotationSecondsProperty, value);
        }

        public bool AutoSpeed
        {
            get => (bool)GetValue(AutoSpeedProperty);
            set => SetValue(AutoSpeedProperty, value);
        }

        private bool _isLoaded;

        public SpinnerPreviewControl()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isLoaded = true;
                ApplyAppearance();
                ApplyAnimation();
            };
        }

        private static void OnAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpinnerPreviewControl control && control._isLoaded)
            {
                control.ApplyAnimation();
            }
        }

        private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpinnerPreviewControl control && control._isLoaded)
            {
                control.ApplyAppearance();
            }
        }

        private void ApplyAppearance()
        {
            if (PreviewPath == null)
            {
                return;
            }

            var previewLevel = PreviewBackgroundLevel;
            if (previewLevel < 0.0)
            {
                previewLevel = 0.0;
            }
            else if (previewLevel > 1.0)
            {
                previewLevel = 1.0;
            }

            var channel = (byte)(previewLevel * 255.0);
            if (PreviewContainer != null)
            {
                PreviewContainer.Background = new SolidColorBrush(Color.FromRgb(channel, channel, channel));
            }

            SpinnerRenderHelper.ApplySpinnerAppearance(
                PreviewPath,
                SpinnerSize,
                SpinnerThickness,
                SpinnerDashLength,
                SpinnerDashCount,
                RoundedDashes);
        }

        private void ApplyAnimation()
        {
            if (PreviewPath == null)
            {
                return;
            }

            ApplyAppearance();

            if (!(PreviewPath.RenderTransform is RotateTransform rotateTransform))
            {
                rotateTransform = new RotateTransform();
                PreviewPath.RenderTransform = rotateTransform;
            }

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            var durationSeconds = SpinnerRenderHelper.CalculateRotationDurationSeconds(RotationSeconds, AutoSpeed, SpinnerSize);

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                RepeatBehavior = RepeatBehavior.Forever
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

    }
}
