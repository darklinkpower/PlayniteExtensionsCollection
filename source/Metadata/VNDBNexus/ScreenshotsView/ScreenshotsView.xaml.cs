using Playnite.SDK;
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
using VNDBNexus.Converters;

namespace VNDBNexus.Screenshots
{
    /// <summary>
    /// Interaction logic for ScreenshotsView.xaml
    /// </summary>
    public partial class ScreenshotsView : UserControl
    {
        public ScreenshotsView(ImageUriToBitmapImageConverter imageUriToBitmapImageConverter)
        {
            Resources.Add("ImageUriToBitmapImageConverter", imageUriToBitmapImageConverter);
            SetControlTextBlockStyle();

            InitializeComponent();
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
    }
}