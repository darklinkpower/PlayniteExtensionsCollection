using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TemporaryCache.Models
{
    public sealed class CacheItem<T>
    {
        public T Value { get; }
        public DateTime CreatedAtUtc { get; }
        public DateTime LastAccessedUtc { get; private set; }

        public CacheItem(T value, DateTime createdAtUtc)
        {
            Value = value;
            CreatedAtUtc = createdAtUtc;
        }

        public void Touch(DateTime nowUtc)
        {
            LastAccessedUtc = nowUtc;
        }
    }
}