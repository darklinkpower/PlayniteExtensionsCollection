using SteamShortcuts.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamShortcuts.Application.Interfaces
{
    public interface ISupportedFeaturesCacheRepository
    {
        SupportedFeaturesCacheEntry Get(string steamId);

        void Save(SupportedFeaturesCacheEntry entry);
    }
}
