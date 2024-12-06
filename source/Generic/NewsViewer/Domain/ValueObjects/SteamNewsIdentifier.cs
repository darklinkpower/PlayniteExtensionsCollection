using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer.Domain.ValueObjects
{
    public class SteamNewsIdentifier
    {
        public bool IsPermanentLink { get; }
        public string Value { get; }

        public SteamNewsIdentifier(bool isPermanentLink, string value)
        {
            IsPermanentLink = isPermanentLink;
            Value = value;
        }
    }
}
