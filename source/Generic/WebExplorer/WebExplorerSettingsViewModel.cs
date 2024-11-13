using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Application;
using WebViewCore.Domain.Entities;
using WebViewCore.Domain.Events;

namespace WebExplorer
{
    public class WebExplorerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly WebExplorer _plugin;
        private readonly BookmarksManager _bookmarksManager;
        private readonly BookmarksManager _themesBookmarksManager;

        private WebExplorerSettings _settings;
        private WebExplorerSettings _editingClone;
        private List<Bookmark> _editBookmarksBackup;
        private List<Bookmark> _editThemeBookmarksBackup;

        private Bookmark _selectedBookmark;
        private Bookmark _selectedThemeBookmark;

        public WebExplorerSettings Settings
        {
            get => _settings;
            set => SetValue(ref _settings, value);
        }

        public ObservableCollection<Bookmark> EditingBookmarks => _bookmarksManager.Bookmarks.ToObservable();
        public ObservableCollection<Bookmark> EditingThemeBookmarks => _themesBookmarksManager.Bookmarks.ToObservable();

        public Bookmark SelectedBookmark
        {
            get => _selectedBookmark;
            set => SetValue(ref _selectedBookmark, value);
        }

        public Bookmark SelectedThemeBookmark
        {
            get => _selectedThemeBookmark;
            set => SetValue(ref _selectedThemeBookmark, value);
        }

        public BookmarkFormViewModel SidebarBookmarkFormViewModel { get; }
        public BookmarkFormViewModel ThemeBookmarkFormViewModel { get; }

        public RelayCommand RemoveBookmarkCommand { get; }
        public RelayCommand RemoveThemeBookmarkCommand { get; }
        public RelayCommand RestoreDefaultBookmarksCommand { get; }

        public WebExplorerSettingsViewModel(WebExplorer plugin, BookmarksManager bookmarksManager, BookmarksManager themesBookmarksManager)
        {
            _plugin = plugin;
            _bookmarksManager = bookmarksManager;
            _themesBookmarksManager = themesBookmarksManager;

            Settings = plugin.LoadPluginSettings<WebExplorerSettings>() ?? new WebExplorerSettings();

            RemoveBookmarkCommand = new RelayCommand(RemoveBookmark, () => SelectedBookmark != null);
            RemoveThemeBookmarkCommand = new RelayCommand(RemoveThemeBookmark, () => SelectedThemeBookmark != null);
            RestoreDefaultBookmarksCommand = new RelayCommand(RestoreDefaultBookmarks);

            SubscribeToBookmarksManagerEvents();

            SidebarBookmarkFormViewModel = new BookmarkFormViewModel(bookmarksManager);
            ThemeBookmarkFormViewModel = new BookmarkFormViewModel(themesBookmarksManager);
        }

        private void SubscribeToBookmarksManagerEvents()
        {
            _bookmarksManager.BookmarkAdded += BookmarksManager_BookmarkAdded;
            _bookmarksManager.BookmarkRemoved += BookmarksManager_BookmarkRemoved;
            _bookmarksManager.BookmarksChanged += BookmarksManager_BookmarksChanged;
        }

        private void UnsubscribeToBookmarksManagerEvents()
        {
            _bookmarksManager.BookmarkAdded -= BookmarksManager_BookmarkAdded;
            _bookmarksManager.BookmarkRemoved -= BookmarksManager_BookmarkRemoved;
            _bookmarksManager.BookmarksChanged -= BookmarksManager_BookmarksChanged;
        }

        private void BookmarksManager_BookmarksChanged(object sender, BookmarksChangedEventArgs e)
        {
            OnPropertyChanged(nameof(EditingBookmarks));
        }

        private void BookmarksManager_BookmarkRemoved(object sender, BookmarkRemovedEventArgs e)
        {
            OnPropertyChanged(nameof(EditingBookmarks));
        }

        private void BookmarksManager_BookmarkAdded(object sender, BookmarkAddedEventArgs e)
        {
            OnPropertyChanged(nameof(EditingBookmarks));
        }

        public void BeginEdit()
        {
            _editingClone = Serialization.GetClone(Settings);
            _editBookmarksBackup = _bookmarksManager.Bookmarks.ToList();
            _editThemeBookmarksBackup = _themesBookmarksManager.Bookmarks.ToList();
        }

        public void CancelEdit()
        {
            Settings = _editingClone;
            _bookmarksManager.SetBookmarks(_editBookmarksBackup);
            _themesBookmarksManager.SetBookmarks(_editThemeBookmarksBackup);
            UnsubscribeToBookmarksManagerEvents();
        }

        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);
            UnsubscribeToBookmarksManagerEvents();
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        private void RemoveBookmark()
        {
            RemoveGenericBookmark(_bookmarksManager, ref _selectedBookmark);
        }

        private void RemoveThemeBookmark()
        {
            RemoveGenericBookmark(_themesBookmarksManager, ref _selectedThemeBookmark);
        }

        private void RemoveGenericBookmark(BookmarksManager manager, ref Bookmark selectedBookmark)
        {
            if (selectedBookmark != null)
            {
                manager.RemoveBookmark(selectedBookmark);
                selectedBookmark = null;
            }
        }

        public void RestoreDefaultBookmarks()
        {
            _bookmarksManager.SuppressNotifications(() =>
            {
                _bookmarksManager.ClearBookmarks();
                var defaultBookmarks = new Dictionary<string, string>
                {
                    { "Steam", "https://store.steampowered.com" },
                    { "GOG", "https://www.gog.com" },
                    { "Epic Games Store", "https://store.epicgames.com/" },
                    { "Ubisoft Store", "https://store.ubisoft.com/" },
                    { "EA", "https://www.ea.com/sales/deals/games_onsale" },
                    { "Humble Bundle", "https://www.humblebundle.com/store" }
                };

                Parallel.ForEach(defaultBookmarks, bookmark =>
                {
                    _bookmarksManager.AddBookmark(bookmark.Key, bookmark.Value);
                });
            });
        }
    }


}