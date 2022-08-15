using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesSizeCalculator.GOG
{
    public class GogSizeCalculator : IOnlineSizeCalculator
    {
        public string ServiceName { get; } = "GOG";

        public async Task<ulong?> GetInstallSizeAsync(Game game)
        {
            var url = GetGogGameUrl(game);
            if (url == null)
            {
                return null;
            }

            return 0UL;
        }

        private string GetGogGameUrl(Game game)
        {
            string url = game.Links?.Select(l => l.Url).FirstOrDefault(l => l.StartsWith("https://www.gog.com/game/"));
            return url;
        }
    }
}
