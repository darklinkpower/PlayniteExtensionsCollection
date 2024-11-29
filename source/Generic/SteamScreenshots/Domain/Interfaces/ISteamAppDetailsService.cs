using SteamCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamScreenshots.Domain.Interfaces
{
    public interface ISteamAppDetailsService
    {
        SteamAppDetails GetAppDetails(string id, bool downloadFromNetwork, CancellationToken cancellationToken = default);
        void SaveAppDetails(string id, SteamAppDetails details);
        void DeleteAppDetails(string id);
    }
}