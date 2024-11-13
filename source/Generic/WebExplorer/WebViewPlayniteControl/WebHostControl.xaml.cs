using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using WebExplorer.WebViewPlayniteControl.Models;
using WebViewCore.Application;
using WebViewCore.Domain.Entities;
using WebViewCore.Domain.Events;
using WebViewCore.Domain.Interfaces;
using WebViewCore.Infrastructure;

namespace WebExplorer.WebViewPlayniteControl
{
    /// <summary>
    /// Interaction logic for ThemesWebHostControl.xaml
    /// </summary>
    public partial class WebHostControl : PluginUserControl, IDisposable, INotifyPropertyChanged
    {
        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        // Constants and Read-only Fields
        private readonly DispatcherTimer _updateControlDataDelayTimer;
        private readonly IPlayniteAPI _playniteApi;
        private readonly BookmarksManager _globalBookmarksManager;
        private readonly BookmarksManager _controlBookmarksManager;
        private readonly BrowserHostViewModel _browserHostViewModel;
        private readonly DesktopView _activeViewAtCreation;
        private readonly Lazy<ThemesWebHostControlCommandsForwarder> _lazyCommandsForwarder;
        private readonly Lazy<ThemesWebHostControlInformationForwarder> _lazyInformationForwarder;

        // Private Fields
        private bool _disposed = false;
        private bool _isValuesDefaultState = true;
        private Game _currentGame;
        private Guid _activeContext = default;

        // Public Properties
        public bool IsControlVisible => Visibility == Visibility.Visible;
        public ThemesWebHostControlCommandsForwarder BrowserCommands => _lazyCommandsForwarder.Value;
        public ThemesWebHostControlInformationForwarder BrowserInformation => _lazyInformationForwarder.Value;
        public List<BookmarksWithCommand> BookmarksWithCommand =>
            _controlBookmarksManager.Bookmarks.Select(x => new BookmarksWithCommand(x, _browserHostViewModel)).ToList();

        public WebHostControl(
            IPlayniteAPI playniteApi,
            WebBrowserUserInterfaceSettings uiSettings,
            IBookmarksIconRepository bookmarksIconRepository,
            BookmarksManager globalBookmarksManager,
            Action openSettingsAction)
        {
            InitializeComponent();
            _playniteApi = playniteApi;
            _globalBookmarksManager = globalBookmarksManager;
            _controlBookmarksManager = new BookmarksManager(
                Guid.NewGuid().ToString(),
                new InMemoryBookmarksRepository(),
                bookmarksIconRepository);

            var browserHost = new CefSharpWebBrowserHost();
            _browserHostViewModel = new BrowserHostViewModel(browserHost, _controlBookmarksManager, uiSettings, openSettingsAction);
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = _playniteApi.MainView.ActiveDesktopView;
            }

            BrowserGrid.Children.Add(new BrowserHostView(_browserHostViewModel));
            SetControlTextBlockStyle(playniteApi);
            SubscribeToBookmarksManagerEvents(_globalBookmarksManager);
            _updateControlDataDelayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1100)
            };

            _updateControlDataDelayTimer.Tick += UpdateControlData;
            IsVisibleChanged += ThemesWebHostControl_IsVisibleChanged;
            _lazyCommandsForwarder =
                new Lazy<ThemesWebHostControlCommandsForwarder>(() => new ThemesWebHostControlCommandsForwarder(_browserHostViewModel));
            _lazyInformationForwarder =
                new Lazy<ThemesWebHostControlInformationForwarder>(() => new ThemesWebHostControlInformationForwarder(_browserHostViewModel, browserHost));
        }

        private void ThemesWebHostControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (newValue == false)
            {
                if (_browserHostViewModel.CurrentAddress != string.Empty)
                {
                    _browserHostViewModel.NavigateToEmptyPage();
                }
            }

            OnPropertyChanged(nameof(IsControlVisible));
        }

        private void SubscribeToBookmarksManagerEvents(BookmarksManager bookmarksManager)
        {
            bookmarksManager.BookmarkAdded += BookmarksManager_BookmarkAdded;
            bookmarksManager.BookmarkRemoved += BookmarksManager_BookmarkRemoved;
            bookmarksManager.BookmarksChanged += BookmarksManager_BookmarksChanged;
        }

        private void UnsubscribeFromBookmarksManagerEvents(BookmarksManager bookmarksManager)
        {
            bookmarksManager.BookmarkAdded -= BookmarksManager_BookmarkAdded;
            bookmarksManager.BookmarkRemoved -= BookmarksManager_BookmarkRemoved;
            bookmarksManager.BookmarksChanged -= BookmarksManager_BookmarksChanged;
        }

        private void BookmarksManager_BookmarksChanged(object sender, BookmarksChangedEventArgs e)
        {
            LoadControlData();
        }

        private void BookmarksManager_BookmarkRemoved(object sender, BookmarkRemovedEventArgs e)
        {
            LoadControlData();
        }

        private void BookmarksManager_BookmarkAdded(object sender, BookmarkAddedEventArgs e)
        {
            LoadControlData();
        }

        private void SetControlTextBlockStyle(IPlayniteAPI playniteApi)
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle &&
                baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                Resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        private async void UpdateControlData(object sender, EventArgs e)
        {
            _updateControlDataDelayTimer.Stop();
            await UpdateControlAsync();
        }

        private void ResetToDefaultValues()
        {
            _activeContext = default;
            _controlBookmarksManager.Bookmarks.Clear();
            _browserHostViewModel.NavigateToEmptyPage();
            Visibility = Visibility.Collapsed;
            OnPropertyChanged(nameof(IsControlVisible));
            _isValuesDefaultState = true;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            _updateControlDataDelayTimer.Stop();
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && _activeViewAtCreation != _playniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (!_isValuesDefaultState)
            {
                ResetToDefaultValues();
            }

            if (newContext is null)
            {
                return;
            }

            _currentGame = newContext;
            _updateControlDataDelayTimer.Start();
        }

        private async Task UpdateControlAsync()
        {
            await LoadControlData(_currentGame).ConfigureAwait(false);
        }

        private void LoadControlData()
        {
            Task.Run(() => LoadControlData(_currentGame)).GetAwaiter().GetResult();
        }

        private async Task LoadControlData(Game game, CancellationToken cancellationToken = default)
        {
            if (_currentGame is null)
            {
                return;
            }

            var scopeContext = Guid.NewGuid();
            _activeContext = scopeContext;
            _isValuesDefaultState = false;
            _controlBookmarksManager.SuppressNotifications(() =>
            {
                var globalBookmarks = _globalBookmarksManager.Bookmarks.ToList();
                _controlBookmarksManager.SetBookmarks(globalBookmarks);
                var localBookmarks = new List<Bookmark>(globalBookmarks.ToList());
                if (game.Links.HasItems())
                {
                    foreach (var link in game.Links)
                    {
                        _controlBookmarksManager.AddBookmark(link.Name, link.Url);
                    }
                }
            });

            await Task.Delay(50);
            OnPropertyChanged(nameof(BookmarksWithCommand));
            Visibility = Visibility.Visible;
            OnPropertyChanged(nameof(IsControlVisible));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _browserHostViewModel.Dispose();
                UnsubscribeFromBookmarksManagerEvents(_globalBookmarksManager);
                if (_lazyInformationForwarder.IsValueCreated)
                {
                    _lazyInformationForwarder.Value.Dispose();
                }

                _updateControlDataDelayTimer.Tick -= UpdateControlData;
                IsVisibleChanged -= ThemesWebHostControl_IsVisibleChanged;
                _disposed = true;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}