using SteamShortcuts.Domain.Enums;
using SteamShortcuts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamShortcuts.Application.Models
{
    public class SupportedFeaturesCacheEntry
    {
        public string SteamId { get; set; }

        public SupportedFeatures Features { get; set; }

        public DateTime CreatedAt { get; set; }

        public SupportedFeaturesCacheEntry(string steamId, SupportedFeatures features)
        {
                SteamId = steamId;
                Features = features;
                CreatedAt = DateTime.UtcNow;
        }
    }
}