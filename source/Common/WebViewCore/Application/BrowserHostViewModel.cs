using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WebViewCore.Domain.Entities;
using WebViewCore.Domain.Events;
using WebViewCore.Infrastructure;

namespace WebViewCore.Application
{
    public class BrowserHostViewModel : ObservableObject, IDisposable
    {
        private readonly CefSharpWebBrowserHost _cefSharpWebBrowserHost;
        private readonly BookmarksManager _bookmarksManager;
        private bool _disposed = false;
        private string _address = string.Empty;
        public string Address { get => _address; set => SetValue(ref _address, value); }
        public string CurrentAddress => _cefSharpWebBrowserHost.GetCurrentAddress();

        private bool _isLoading = false;
        public bool IsLoading { get => _isLoading; set => SetValue(ref _isLoading, value); }
        private WebBrowserUserInterfaceSettings _userInterfaceSettings;
        public WebBrowserUserInterfaceSettings UserInterfaceSettings { get => _userInterfaceSettings; set => SetValue(ref _userInterfaceSettings, value); }

        public RelayCommand OpenAddressExternallyCommand { get; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand GoForwardCommand { get; }
        public RelayCommand<string> NavigateToAddressCommand { get; }
        public RelayCommand NavigateToCurrentAddressCommand { get; }
        public RelayCommand ReloadCommand { get; }
        public RelayCommand ClearHistoryCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }
        public ObservableCollection<Bookmark> Bookmarks => _bookmarksManager.Bookmarks.ToObservable();

        public BrowserHostViewModel(
            CefSharpWebBrowserHost cefSharpWebBrowserHost,
            BookmarksManager bookmarksManager,
            WebBrowserUserInterfaceSettings uiSettings,
            Action openSettingsAction = null)
        {
            _cefSharpWebBrowserHost = cefSharpWebBrowserHost;
            _bookmarksManager = bookmarksManager;
            UserInterfaceSettings = uiSettings;

            OpenAddressExternallyCommand = new RelayCommand(() => ProcessStarter.StartUrl(Address),() => !Address.IsNullOrEmpty());
            GoBackCommand = new RelayCommand(GoBack, CanGoBack);
            GoForwardCommand = new RelayCommand(GoForward, CanGoForward);
            NavigateToAddressCommand = new RelayCommand<string>((a) => NavigateToAddress(a));
            NavigateToCurrentAddressCommand = new RelayCommand(NavigateToCurrentAddress, CanNavigateToCurrentAddress);
            ReloadCommand = new RelayCommand(Reload);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            OpenSettingsCommand = openSettingsAction != null
                ? new RelayCommand(openSettingsAction)
                : new RelayCommand(() => {}, () => false);

            SubscribeToBrowserHostEvents(_cefSharpWebBrowserHost);
            SubscribeToBookmarksManagerEvents(_bookmarksManager);
        }

        internal Control GetWebViewControl() => _cefSharpWebBrowserHost.GetWebViewControl();

        public void NavigateToFirstBookmark()
        {
            if (_bookmarksManager.Bookmarks.HasItems())
            {
                NavigateToAddress(_bookmarksManager.Bookmarks.First().Address);
            }
        }

        private void SubscribeToBrowserHostEvents(CefSharpWebBrowserHost cefSharpWebBrowserHost)
        {
            cefSharpWebBrowserHost.AddressChanged += CefSharpWebBrowserHost_AddressChanged;
            cefSharpWebBrowserHost.IsLoadingChanged += CefSharpWebBrowserHost_IsLoadingChanged;
        }

        private void UnsubscribeFromBrowserHostEvents(CefSharpWebBrowserHost cefSharpWebBrowserHost)
        {
            cefSharpWebBrowserHost.AddressChanged -= CefSharpWebBrowserHost_AddressChanged;
            cefSharpWebBrowserHost.IsLoadingChanged -= CefSharpWebBrowserHost_IsLoadingChanged;
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

        private void CefSharpWebBrowserHost_IsLoadingChanged(object sender, IsLoadingChangedEventArgs e)
        {
            IsLoading = e.NewIsLoading;
            NotifyPropertyChanged();
            OnPropertyChanged(nameof(Bookmarks));
        }

        private void CefSharpWebBrowserHost_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            Address = e.NewAddress;
            OnPropertyChanged(nameof(Bookmarks));
            NotifyPropertyChanged();
        }

        private void BookmarksManager_BookmarksChanged(object sender, BookmarksChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Bookmarks));
            NotifyPropertyChanged();
        }

        private void BookmarksManager_BookmarkRemoved(object sender, BookmarkRemovedEventArgs e)
        {
            OnPropertyChanged(nameof(Bookmarks));
            NotifyPropertyChanged();
        }

        private void BookmarksManager_BookmarkAdded(object sender, BookmarkAddedEventArgs e)
        {
            OnPropertyChanged(nameof(Bookmarks));
            NotifyPropertyChanged();
        }

        private void NotifyPropertyChanged()
        {
            OnPropertyChanged(nameof(Address));
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(GoBackCommand));
            OnPropertyChanged(nameof(GoForwardCommand));
            OnPropertyChanged(nameof(NavigateToAddressCommand));
            OnPropertyChanged(nameof(ReloadCommand));
            OnPropertyChanged(nameof(ClearHistoryCommand));
            OnPropertyChanged(nameof(OpenAddressExternallyCommand));
        }

        public void AddBookmarkCommand(string name, string address)
        {
            _bookmarksManager.AddBookmark(name, address);
            RefreshBookmarks();
        }

        public void RemoveBookmarkCommand(Bookmark bookmark)
        {
            _bookmarksManager.RemoveBookmark(bookmark);
            RefreshBookmarks();
        }

        private void RefreshBookmarks()
        {
            Bookmarks.Clear();
            foreach (var bookmark in _bookmarksManager.Bookmarks)
            {
                Bookmarks.Add(bookmark);
            }
        }

        public void GoBack()
        {
            _cefSharpWebBrowserHost?.GoBack();
        }

        public bool CanGoBack()
        {
            return _cefSharpWebBrowserHost?.CanGoBack() ?? false;
        }

        private void GoForward()
        {
            _cefSharpWebBrowserHost?.GoForward();
        }

        public bool CanGoForward()
        {
            return _cefSharpWebBrowserHost?.CanGoForward() ?? false;
        }

        private void NavigateToCurrentAddress()
        {
            NavigateToAddress(_address);
        }

        public void NavigateToAddress(string address)
        {
            _cefSharpWebBrowserHost?.NavigateTo(address);
        }

        private bool CanNavigateToCurrentAddress()
        {
            return !string.IsNullOrWhiteSpace(_address);
        }

        public void Reload()
        {
            _cefSharpWebBrowserHost?.Reload();
        }

        public void NavigateToEmptyPage()
        {
            _cefSharpWebBrowserHost?.NavigateToEmptyPage();
        }

        public void ClearHistory()
        {
            _cefSharpWebBrowserHost?.ClearHistory();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                UnsubscribeFromBrowserHostEvents(_cefSharpWebBrowserHost);
                UnsubscribeFromBookmarksManagerEvents(_bookmarksManager);
                _cefSharpWebBrowserHost?.Dispose();
                _disposed = true;
            }
        }
    }
}