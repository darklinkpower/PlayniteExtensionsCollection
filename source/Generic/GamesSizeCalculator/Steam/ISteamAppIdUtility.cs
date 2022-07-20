using Playnite.SDK.Models;

namespace GamesSizeCalculator.SteamSizeCalculation
{
    public interface ISteamAppIdUtility
    {
        string GetSteamGameId(Game game);
    }
}