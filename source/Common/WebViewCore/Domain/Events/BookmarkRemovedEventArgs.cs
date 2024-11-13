using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Domain.Entities;

namespace WebViewCore.Domain.Events
{
    public class BookmarkRemovedEventArgs : EventArgs
    {
        public Guid EventId { get; }
        public DateTime TimestampUtc { get; }
        public string ManagerId { get; }
        public IReadOnlyCollection<Bookmark> Bookmarks { get; }

        public BookmarkRemovedEventArgs(string managerId, List<Bookmark> bookmarks)
        {
            EventId = Guid.NewGuid();
            TimestampUtc = DateTime.UtcNow;
            Bookmarks = bookmarks.AsReadOnly();
            ManagerId = managerId;
        }
    }
}
