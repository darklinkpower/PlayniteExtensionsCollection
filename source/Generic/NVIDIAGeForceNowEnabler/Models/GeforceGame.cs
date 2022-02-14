using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVIDIAGeForceNowEnabler
{
    public class GeforceGame
    {
        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("sortName")]
        public string SortName { get; set; }

        [SerializationPropertyName("isFullyOptimized")]
        public bool IsFullyOptimized { get; set; }

        [SerializationPropertyName("steamUrl")]
        public string SteamUrl { get; set; }

        [SerializationPropertyName("store")]
        public string Store { get; set; }

        [SerializationPropertyName("publisher")]
        public string Publisher { get; set; }

        [SerializationPropertyName("genres")]
        public string[] Genres { get; set; }

        [SerializationPropertyName("status")]
        public string Status { get; set; }
    }
}