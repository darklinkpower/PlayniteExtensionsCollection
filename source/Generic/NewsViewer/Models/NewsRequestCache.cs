using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NewsViewer.Models
{
    public class NewsRequestCache
    {
        public readonly DateTime CreationDate;
        public readonly XmlNodeList NewsNodes;

        public NewsRequestCache(DateTime creationDate, XmlNodeList newsNodes)
        {
            CreationDate = creationDate;
            NewsNodes = newsNodes;
        }
    }
}