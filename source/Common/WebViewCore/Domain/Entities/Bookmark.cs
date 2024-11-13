using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebViewCore.Domain.Entities
{
    public class Bookmark
    {
        public Guid Id { get; }
        public DateTime CreatedAtUtc { get; }
        public string Name { get; }
        public string Address { get; }
        public string IconPath { get; }

        public Bookmark(BookmarkInternal bookmark, string iconPath)
        {
            Id = bookmark.Id;
            CreatedAtUtc = bookmark.CreatedAtUtc;
            Name = bookmark.Name;
            Address = bookmark.Address;
            IconPath = iconPath;
        }
    }

}