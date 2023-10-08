using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NewsViewer.Models
{
    public class CacheItem<T>
    {
        public readonly DateTime CreationDate;
        public readonly T Item;

        public CacheItem(DateTime creationDate, T newsNodes)
        {
            CreationDate = creationDate;
            Item = newsNodes;
        }
    }
}