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
using WebViewCore.Infrastructure;

namespace WebViewCore.Application
{
    /// <summary>
    /// Interaction logic for BrowserHostView.xaml
    /// </summary>
    public partial class BrowserHostView : UserControl
    {
        public BrowserHostView(BrowserHostViewModel browserHostViewModel)
        {
            InitializeComponent();

            var webViewControl = browserHostViewModel.GetWebViewControl();
            BrowserGrid.Children.Add(webViewControl);
            browserHostViewModel.NavigateToFirstBookmark();
            DataContext = browserHostViewModel;
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is BrowserHostViewModel browserHostViewModel)
            {
                var command = browserHostViewModel.NavigateToCurrentAddressCommand;
                command?.Execute(null);
            }
        }

    }
}