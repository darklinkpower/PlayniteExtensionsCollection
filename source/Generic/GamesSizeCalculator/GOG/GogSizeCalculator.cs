using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GamesSizeCalculator.GOG
{
    public class GogSizeCalculator : IOnlineSizeCalculator
    {
        public GogSizeCalculator(IHttpDownloader downloader, bool handleNonGogGames)
        {
            this.ApiClient = new GogApiClient(downloader);
            HandleNonGogGames = handleNonGogGames;
        }

        public string ServiceName { get; } = "GOG";
        private GogApiClient ApiClient { get; }
        public bool HandleNonGogGames { get; set; }

        public static Guid GogLibraryId = new Guid("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E");
        private static Regex GameUrlRegex = new Regex(@"^https?://(www\.)?gog.com/([a-z]{2,3}/)?game/", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public async Task<ulong?> GetInstallSizeAsync(Game game)
        {
            var url = GetGogGameUrl(game);
            if (url == null)
            {
                return null;
            }

            var data = ApiClient.GetGameStoreData(url);
            if (data == null)
            {
                return null;
            }

            return (ulong)data.size * 1024UL * 1024UL; //megabytes -> bytes
        }

        private string GetGogGameUrl(Game game)
        {
            if (!HandleNonGogGames && !IsPreferredInstallSizeCalculator(game))
            {
                return null;
            }

            string url = game.Links?.Select(l => l.Url).FirstOrDefault(u => GameUrlRegex.IsMatch(u));
            if (url != null)
            {
                return url;
            }

            if (game.PluginId == GogLibraryId)
            {
                var details = ApiClient.GetGameDetails(game.GameId);
                return details?.links?.product_card;
            }
            return null;
        }

        public bool IsPreferredInstallSizeCalculator(Game game)
        {
            return game.PluginId == GogLibraryId;
        }
    }
}
