using Playnite.SDK;
using PluginsCommon.Converters;
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

namespace SteamScreenshots.Screenshots
{
    /// <summary>
    /// Interaction logic for ScreenshotsView.xaml
    /// </summary>
    public partial class ScreenshotsView : UserControl
    {
        private DateTime _lastClickTime = DateTime.MinValue;
        private const int DoubleClickTimeLimit = 500;

        public ScreenshotsView()
        {
            SetControlTextBlockStyle();
            Loaded += ScreenshotsView_Loaded;
            Unloaded += ScreenshotsView_Unloaded;
            Focusable = true;
            InitializeComponent();
        }

        private void ScreenshotsView_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ScreenshotsView_Loaded;
            Unloaded -= ScreenshotsView_Unloaded;
        }

        private void ScreenshotsView_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }

        private void SetControlTextBlockStyle()
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = API.Instance.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle && baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                Resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var currentTime = DateTime.Now;
            var elapsedMilliseconds = (currentTime - _lastClickTime).TotalMilliseconds;

            if (elapsedMilliseconds < DoubleClickTimeLimit)
            {
                if (DataContext is ScreenshotsViewModel viewModel && viewModel.CloseWindowCommand.CanExecute(null))
                {
                    viewModel.CloseWindowCommand.Execute(null);
                }
            }

            _lastClickTime = currentTime;
        }
    }
}