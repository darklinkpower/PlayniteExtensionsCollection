using Playnite.SDK.Models;

namespace GamesSizeCalculator.Steam
{
    public interface ISteamAppIdUtility
    {
        string GetSteamGameId(Game game);
    }
}