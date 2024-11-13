using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Domain.Entities;
using WebViewCore.Domain.Interfaces;

namespace WebViewCore.Infrastructure
{
    public class InMemoryBookmarksRepository : IBookmarksRepository
    {
        private readonly List<BookmarkInternal> _inMemoryBookmarks;
        private readonly object _lock = new object();

        public InMemoryBookmarksRepository()
        {
            _inMemoryBookmarks = new List<BookmarkInternal>();
        }

        public List<BookmarkInternal> LoadBookmarks()
        {
            lock (_lock)
            {
                return _inMemoryBookmarks.ToList();
            }
        }

        public void SaveBookmarks(List<BookmarkInternal> bookmarks)
        {
            lock (_lock)
            {
                _inMemoryBookmarks.Clear();
                _inMemoryBookmarks.AddRange(bookmarks.ToList());
            }
        }

        public void ClearBookmarks()
        {
            lock (_lock)
            {
                _inMemoryBookmarks.Clear();
            }
        }
    }

}