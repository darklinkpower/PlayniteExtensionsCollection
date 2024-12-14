using SteamWishlistDiscountNotifier.Presentation.Converters;
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
using System.Windows.Threading;
using System.Timers;

namespace SteamWishlistDiscountNotifier.Presentation
{
    /// <summary>
    /// Interaction logic for SteamWishlistViewerView.xaml
    /// </summary>
    public partial class SteamWishlistViewerView : UserControl
    {
        private Dictionary<TextBlock, Timer> _timers = new Dictionary<TextBlock, Timer>();
        private readonly Dispatcher _dispatcher;

        public SteamWishlistViewerView()
        {
            InitializeComponent();
            this.Unloaded += SteamWishlistViewerView_Unloaded;
            _dispatcher = System.Windows.Application.Current.Dispatcher;
        }

        private void SteamWishlistViewerView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= SteamWishlistViewerView_Unloaded;
        }

        private void OnRemainingTimeTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock?.DataContext is SteamWishlistViewItem viewItem)
            {
                if (!_timers.ContainsKey(textBlock))
                {
                    var timer = new Timer(1000);
                    timer.Elapsed += TimerElapsedHandler(textBlock);
                    _timers[textBlock] = timer;
                    timer.Start();
                }

                textBlock.Unloaded += TextBlockUnloadedHandler;
            }
        }

        private ElapsedEventHandler TimerElapsedHandler(TextBlock textBlock)
        {
            return (ss, ee) =>
            {
                _dispatcher.Invoke(() =>
                {
                    var binding = textBlock.GetBindingExpression(TextBlock.TextProperty);
                    binding?.UpdateTarget();
                });
            };
        }

        private void TextBlockUnloadedHandler(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock && _timers.ContainsKey(textBlock))
            {
                var timer = _timers[textBlock];
                timer.Elapsed -= TimerElapsedHandler(textBlock);
                timer.Stop();
                _timers.Remove(textBlock);
                textBlock.Unloaded -= TextBlockUnloadedHandler;
            }
        }

    }
}
