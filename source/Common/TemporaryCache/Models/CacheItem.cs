using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TemporaryCache.Models
{
    public class CacheItem<T>
    {
        public T Value { get; }
        public DateTime Expiration { get; private set; }
        public bool IsExpired => DateTime.Now >= Expiration;

        public CacheItem(T value, DateTime expiration)
        {
            Value = value;
            Expiration = expiration;
        }

        public void RefreshExpiration(DateTime expirationDate)
        {
            Expiration = Expiration = expirationDate;
        }
    }
}