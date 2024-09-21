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
        public DateTime ExpirationDate { get; private set; }
        public bool IsExpired => DateTime.Now >= ExpirationDate;
        public T Item { get; }
        private readonly TimeSpan _aliveTime;

        public CacheItem(TimeSpan aliveTime, T item)
        {
            _aliveTime = aliveTime;
            Item = item;
            SetExpirationDate();
        }

        private void SetExpirationDate()
        {
            ExpirationDate = DateTime.Now + _aliveTime;
        }

        public void RefreshExpirationDate()
        {
            SetExpirationDate();
        }
    }
}