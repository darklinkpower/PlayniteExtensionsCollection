using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer.Domain.ValueObjects
{
    public class SteamNewsRequestOptions
    {
        public string SteamId { get; }
        public bool ObtainFromCache { get; }
        public bool ObtainFromHttpRequest { get; }

        public SteamNewsRequestOptions(string steamId, bool obtainFromCache, bool obtainFromHttpRequest)
        {
            SteamId = steamId;
            ObtainFromCache = obtainFromCache;
            ObtainFromHttpRequest = obtainFromHttpRequest;
        }
    }
}