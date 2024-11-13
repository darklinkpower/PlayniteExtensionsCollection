using FlowHttp;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Domain.Entities;
using WebViewCore.Domain.Events;
using WebViewCore.Domain.Interfaces;

namespace WebViewCore.Application
{
    public class BookmarksManager
    {
        public string Id { get; }

        public event EventHandler<BookmarkAddedEventArgs> BookmarkAdded;
        public event EventHandler<BookmarkRemovedEventArgs> BookmarkRemoved;
        public event EventHandler<BookmarksChangedEventArgs> BookmarksChanged;

        private int _suppressNotificationsDepth = 0;
        private readonly List<Bookmark> _pendingAddedBookmarks = new List<Bookmark>();
        private readonly List<Bookmark> _pendingRemovedBookmarks = new List<Bookmark>();
        private readonly IBookmarksRepository _bookmarksRepository;
        private readonly IBookmarksIconRepository _bookmarksIconRepository;

        private readonly List<BookmarkInternal> _internalBookmarks;

        private readonly object _lock = new object();
        private readonly object _persistenceRepositoryLock = new object();

        public List<Bookmark> Bookmarks => _internalBookmarks.Select(x => GetBookmarkFromInternalBookmark(x)).ToList();

        public BookmarksManager(string managerId, IBookmarksRepository bookmarksRepository, IBookmarksIconRepository bookmarksIconRepository)
        {
            _bookmarksRepository = bookmarksRepository;
            _bookmarksIconRepository = bookmarksIconRepository;
            _internalBookmarks = new List<BookmarkInternal>();
            Id = managerId;
            LoadBookmarks();
        }

        public Bookmark AddBookmark(string name, string address)
        {
            return AddBookmarkInternal(name, address, null);
        }

        public Bookmark AddBookmark(string name, string address, string iconPath)
        {
            return AddBookmarkInternal(name, address, iconPath);
        }

        private Bookmark AddBookmarkInternal(string name, string address, string iconPath)
        {
            if (TryCreateUri(address, out var addressUri))
            {
                string cachedIconName;
                if (iconPath != null && FileSystem.FileExists(iconPath))
                {
                    cachedIconName = _bookmarksIconRepository.CopyIconToCache(iconPath);
                }
                else
                {
                    cachedIconName = _bookmarksIconRepository.CacheIcon(addressUri);
                }

                var bookmark = new BookmarkInternal(name, address, cachedIconName);
                lock (_lock)
                {
                    _internalBookmarks.Add(bookmark);
                    SaveBookmarks();
                }

                var bookmarkExternal = GetBookmarkFromInternalBookmark(bookmark);
                OnBookmarkAdded(bookmarkExternal);
                return bookmarkExternal;
            }

            return null;
        }

        public bool RemoveBookmark(Bookmark bookmark)
        {
            lock (_lock)
            {
                var internalMatchingBookmark = GetInternalBookmarkById(bookmark.Id);
                if (internalMatchingBookmark is null)
                {
                    return false;
                }

                var removeSuccess = _internalBookmarks.Remove(internalMatchingBookmark);
                if (removeSuccess)
                {
                    SaveBookmarks();
                    OnBookmarkRemoved(bookmark);
                }

                return removeSuccess;
            }
        }

        private BookmarkInternal GetInternalBookmarkById(Guid id)
        {
            return _internalBookmarks.FirstOrDefault(x => x.Id == id);
        }

        public void SetBookmarks(IEnumerable<Bookmark> bookmarks)
        {
            lock (_lock)
            {
                try
                {
                    _internalBookmarks.Clear();
                    var internalBookmarks = bookmarks.Select(x => GetBookmarkFromInternalBookmark(x));
                    _internalBookmarks.AddRange(internalBookmarks);
                    SaveBookmarks();
                    OnBookmarksChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving bookmarks: {ex.Message}");
                }
            }
        }

        public void ClearBookmarks()
        {
            lock (_persistenceRepositoryLock)
            {
                try
                {
                    _internalBookmarks.Clear();
                    SaveBookmarks();
                    OnBookmarksChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing bookmarks: {ex.Message}");
                }
            }
        }

        private void LoadBookmarks()
        {
            lock (_persistenceRepositoryLock)
            {
                _internalBookmarks.Clear();
                var loadedBookmarks = _bookmarksRepository.LoadBookmarks();
                if (loadedBookmarks.HasItems())
                {
                    _internalBookmarks.AddRange(loadedBookmarks);
                }

                OnBookmarksChanged();
            }
        }

        private void SaveBookmarks()
        {
            lock (_persistenceRepositoryLock)
            {
                _bookmarksRepository.SaveBookmarks(_internalBookmarks);
            }
        }

        public void SuppressNotifications(Action action)
        {
            _suppressNotificationsDepth++;
            try
            {
                action();
            }
            finally
            {
                _suppressNotificationsDepth--;
                if (_suppressNotificationsDepth == 0)
                {
                    if (_pendingAddedBookmarks.Any())
                    {
                        OnBookmarkAddedBatch(_pendingAddedBookmarks);
                        _pendingAddedBookmarks.Clear();
                    }

                    if (_pendingRemovedBookmarks.Any())
                    {
                        OnBookmarkRemovedBatch(_pendingRemovedBookmarks);
                        _pendingRemovedBookmarks.Clear();
                    }

                    OnBookmarksChanged();
                }
            }
        }

        protected virtual void OnBookmarkAdded(Bookmark bookmark)
        {
            if (_suppressNotificationsDepth == 0)
            {
                BookmarkAdded?.Invoke(this, new BookmarkAddedEventArgs(Id, new List<Bookmark> { bookmark }));
            }
            else
            {
                _pendingAddedBookmarks.Add(bookmark);
            }
        }

        protected virtual void OnBookmarkRemoved(Bookmark bookmark)
        {
            if (_suppressNotificationsDepth == 0)
            {
                BookmarkRemoved?.Invoke(this, new BookmarkRemovedEventArgs(Id, new List<Bookmark> { bookmark }));
            }
            else
            {
                _pendingRemovedBookmarks.Add(bookmark);
            }
        }

        protected virtual void OnBookmarksChanged()
        {
            if (_suppressNotificationsDepth == 0)
            {
                BookmarksChanged?.Invoke(this, new BookmarksChangedEventArgs(Id, Bookmarks));
            }
        }

        protected virtual void OnBookmarkAddedBatch(List<Bookmark> bookmarks)
        {
            if (_suppressNotificationsDepth == 0)
            {
                BookmarkAdded?.Invoke(this, new BookmarkAddedEventArgs(Id, bookmarks));
            }
        }

        protected virtual void OnBookmarkRemovedBatch(List<Bookmark> bookmarks)
        {
            if (_suppressNotificationsDepth == 0)
            {
                BookmarkRemoved?.Invoke(this, new BookmarkRemovedEventArgs(Id, bookmarks));
            }
        }

        public bool TryCreateUri(string address, out Uri validUri)
        {
            validUri = null;
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                validUri = uri;
                return true;
            }

            return false;
        }

        private Bookmark GetBookmarkFromInternalBookmark(BookmarkInternal internalBookmark)
        {
            var fullIconPath = _bookmarksIconRepository.GetIconPath(internalBookmark.Icon);
            return new Bookmark(internalBookmark, fullIconPath);
        }

        private BookmarkInternal GetBookmarkFromInternalBookmark(Bookmark bookmark)
        {
            return new BookmarkInternal(bookmark.Name, bookmark.Address, Path.GetFileName(bookmark.IconPath));
        }
    }



}