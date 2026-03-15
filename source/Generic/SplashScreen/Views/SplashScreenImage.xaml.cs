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
            var durationSeconds = SpinnerRenderHelper.CalculateRotationDurationSeconds(
                vm.Settings.LoadingSpinnerRotationSeconds,
                vm.Settings.EnableLoadingSpinnerAutoSpeed,
                vm.Settings.LoadingSpinnerSize);

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
            SpinnerRenderHelper.ApplySpinnerAppearance(
                SpinnerPath,
                settings.LoadingSpinnerSize,
                settings.LoadingSpinnerThickness,
                settings.LoadingSpinnerDashLength,
                settings.LoadingSpinnerDashCount,
                settings.LoadingSpinnerRoundedDashes);
        }
    }
}
