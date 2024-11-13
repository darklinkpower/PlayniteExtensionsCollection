using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebViewCore.Domain.Entities
{
    public class BookmarkInternal
    {
        public Guid Id { get; }
        public DateTime CreatedAtUtc { get; }
        public string Name { get; }
        public string Address { get; }
        public string Icon { get; }

        public BookmarkInternal(string name, string address, string icon)
        {
            Id = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            Name = name;
            Address = address;
            Icon = icon;
        }
    }

}