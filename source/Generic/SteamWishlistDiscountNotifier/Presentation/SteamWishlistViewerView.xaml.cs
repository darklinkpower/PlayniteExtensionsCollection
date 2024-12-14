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
        private readonly HashSet<TextBlock> _textBlocksToUpdate = new HashSet<TextBlock>();
        private readonly Dispatcher _dispatcher;
        private readonly Timer _timer;

        public SteamWishlistViewerView()
        {
            InitializeComponent();
            
            _dispatcher = System.Windows.Application.Current.Dispatcher;
            _timer = new Timer(1000);
            _timer.Start();
            _timer.Elapsed += TimerElapsedHandler;
            this.Unloaded += SteamWishlistViewerView_Unloaded;
        }

        private void TimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            UpdateTextBlocksText();
        }

        private void UpdateTextBlocksText()
        {
            _dispatcher.Invoke(() =>
            {
                foreach (var textBlock in _textBlocksToUpdate)
                {
                    var binding = textBlock.GetBindingExpression(TextBlock.TextProperty);
                    binding?.UpdateTarget();
                }
            });
        }

        private void SteamWishlistViewerView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= SteamWishlistViewerView_Unloaded;
            _timer.Elapsed -= TimerElapsedHandler;
            _timer.Stop();
            _textBlocksToUpdate.Clear();
        }

        private void TextBlockUnloadedHandler(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                textBlock.Unloaded -= TextBlockUnloadedHandler;
                if (_textBlocksToUpdate.Contains(textBlock))
                {
                    _textBlocksToUpdate.Remove(textBlock);
                }
            }
        }

        private void OnRemainingTimeTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock && !_textBlocksToUpdate.Contains(textBlock))
            {
                _textBlocksToUpdate.Add(textBlock);
                textBlock.Unloaded += TextBlockUnloadedHandler;
            }
        }
    }
}
