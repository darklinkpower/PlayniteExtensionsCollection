using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SplashScreen.Helpers;
using SplashScreen.ViewModels;

namespace SplashScreen.Views
{
    /// <summary>
    /// Interaction logic for SplashScreenImage.xaml
    /// </summary>
    public partial class SplashScreenImage : UserControl
    {
        public SplashScreenImage()
        {
            InitializeComponent();
            Loaded += SplashScreenImage_Loaded;
            DataContextChanged += SplashScreenImage_DataContextChanged;
        }

        private void SplashScreenImage_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigureSpinnerAnimation();
        }

        private void SplashScreenImage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ConfigureSpinnerAnimation();
        }

        private void ConfigureSpinnerAnimation()
        {
            if (!(DataContext is SplashScreenImageViewModel vm) || vm.Settings == null)
            {
                return;
            }

            ConfigureSpinnerAppearance(vm.Settings);

            SpinnerPath?.BeginAnimation(UIElement.OpacityProperty, null);

            if (!(SpinnerPath.RenderTransform is RotateTransform rotateTransform))
            {
                rotateTransform = new RotateTransform();
                SpinnerPath.RenderTransform = rotateTransform;
            }

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);

            var baseSeconds = vm.Settings.LoadingSpinnerRotationSeconds;
            if (baseSeconds <= 0)
            {
                baseSeconds = 3.0;
            }

            var durationSeconds = baseSeconds;
            if (vm.Settings.EnableLoadingSpinnerAutoSpeed)
            {
                var normalizedSize = vm.Settings.LoadingSpinnerSize / 50.0;
                if (normalizedSize < 0.4)
                {
                    normalizedSize = 0.4;
                }

                durationSeconds = baseSeconds * normalizedSize;
            }

            if (durationSeconds < 0.25)
            {
                durationSeconds = 0.25;
            }

            if (durationSeconds > 20.0)
            {
                durationSeconds = 20.0;
            }

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                RepeatBehavior = RepeatBehavior.Forever
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void ConfigureSpinnerAppearance(Models.GeneralSplashSettings settings)
        {
            if (SpinnerPath == null)
            {
                return;
            }

            var thickness = settings.LoadingSpinnerThickness;
            if (thickness < 0.5)
            {
                thickness = 0.5;
            }

            var dashLengthPx = settings.LoadingSpinnerDashLength;
            if (dashLengthPx < 0.5)
            {
                dashLengthPx = 0.5;
            }

            var dashCount = settings.LoadingSpinnerDashCount;
            if (dashCount < 2)
            {
                dashCount = 2;
            }

            var spinnerSize = settings.LoadingSpinnerSize;
            if (spinnerSize < 4)
            {
                spinnerSize = 4;
            }

            var roundedDashes = settings.LoadingSpinnerRoundedDashes;
            SpinnerPath.StrokeThickness = thickness;
            SpinnerPath.StrokeDashArray = null;
            SpinnerPath.StrokeDashOffset = 0;
            SpinnerPath.StrokeDashCap = PenLineCap.Flat;
            SpinnerPath.StrokeStartLineCap = roundedDashes ? PenLineCap.Round : PenLineCap.Flat;
            SpinnerPath.StrokeEndLineCap = roundedDashes ? PenLineCap.Round : PenLineCap.Flat;
            SpinnerPath.Data = SpinnerGeometryBuilder.BuildDashGeometry(spinnerSize, thickness, dashLengthPx, dashCount, roundedDashes);
        }
    }
}