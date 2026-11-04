using SteamShortcuts.Domain.Models;
using SteamShortcuts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamShortcuts.Application.Interfaces
{
    public interface ISupportedFeaturesProvider
    {
        Task<SupportedFeatures> GetFeaturesAsync(
            string steamId,
            CancellationToken cancellationToken);
    }
}
