using GamesSizeCalculator.SteamSizeCalculation;
using Playnite.SDK.Models;

namespace GamesSizeCalculator.Tests.Steam
{
    public class FakeSteamAppIdUtility : ISteamAppIdUtility
    {
        public FakeSteamAppIdUtility(string appId)
        {
            AppId = appId;
        }

        public FakeSteamAppIdUtility(uint appId)
        {
            AppId = appId.ToString();
        }

        public string AppId { get; }

        public string GetSteamGameId(Game game)
        {
            return AppId;
        }
    }
}
