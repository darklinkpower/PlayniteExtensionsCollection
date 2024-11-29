using SteamCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamScreenshots.Domain.Interfaces
{
    public interface ISteamAppDetailsRepository
    {
        SteamAppDetails GetAppDetails(string id);
        void SaveAppDetails(string id, SteamAppDetails details);
        void DeleteAppDetails(string id);
        DateTime? GetAppDetailsCreationDate(string id);
    }
}