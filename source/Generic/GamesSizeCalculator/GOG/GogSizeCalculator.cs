using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesSizeCalculator.GOG
{
    public class GogSizeCalculator : IOnlineSizeCalculator
    {
        public GogSizeCalculator(IHttpDownloader downloader)
        {
            this.ApiClient = new GogApiClient(downloader);
        }

        public string ServiceName { get; } = "GOG";
        private GogApiClient ApiClient { get; }
        public static Guid GogLibraryId = new Guid("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E");
        private static string GameUrlBase = "https://www.gog.com/game/";

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

            return (ulong)data.size * 1024UL; //megabytes -> bytes
        }

        private string GetGogGameUrl(Game game)
        {
            string url = game.Links?.Select(l => l.Url).FirstOrDefault(l => l.StartsWith(GameUrlBase));
            if (url != null)
            {
                return url;
            }

            if (game.PluginId == GogLibraryId)
            {
                var details = ApiClient.GetGameDetails(game.GameId);
                if (!string.IsNullOrWhiteSpace(details?.slug))
                {
                    return GameUrlBase + details.slug;
                }
            }
            return null;
        }
    }
}
