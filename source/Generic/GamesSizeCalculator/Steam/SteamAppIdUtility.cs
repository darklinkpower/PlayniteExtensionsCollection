using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GamesSizeCalculator.Steam
{
    public class SteamAppIdUtility : ISteamAppIdUtility
    {
        private static readonly Guid SteamLibraryPluginId = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB");
        private static readonly Regex SteamUrlRegex = new Regex(@"^https?://st(ore\.steampowered|eamcommunity)\.com/app/(?<id>[0-9]+)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static readonly Regex NonLetterOrDigitCharacterRegex = new Regex(@"[^\p{L}\p{Nd}]", RegexOptions.Compiled);

        private Dictionary<string, int> _steamIds;
        private Dictionary<string, int> SteamIdsByTitle
        {
            get { return _steamIds ?? (_steamIds = GetSteamIdsByTitle()); }
        }

        public ICachedFile SteamAppList { get; }

        public SteamAppIdUtility(ICachedFile steamAppList)
        {
            SteamAppList = steamAppList;
        }

        private static string NormalizeTitle(string title)
        {
            return NonLetterOrDigitCharacterRegex.Replace(title, string.Empty);
        }

        public string GetSteamGameId(Game game)
        {
            if (game.PluginId == SteamLibraryPluginId)
            {
                return game.GameId;
            }

            if (game.Links != null)
            {
                foreach (var link in game.Links)
                {
                    var match = SteamUrlRegex.Match(link.Url);
                    if (match.Success)
                    {
                        return match.Groups["id"].Value;
                    }
                }
            }

            if (SteamIdsByTitle.TryGetValue(NormalizeTitle(game.Name), out int appId))
            {
                return appId.ToString();
            }

            return null;
        }

        private Dictionary<string, int> GetSteamIdsByTitle()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "SteamAppList.json");
            var file = new FileInfo(tempPath);
            if (!file.Exists || file.LastWriteTime < DateTime.Now.AddHours(-18))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://api.steampowered.com/ISteamApps/GetAppList/v2/", tempPath);
                }
            }
            var jsonStr = File.ReadAllText(tempPath, Encoding.UTF8);
            var jsonContent = Newtonsoft.Json.JsonConvert.DeserializeObject<AppListRoot>(jsonStr);

            Dictionary<string, int> output = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var app in jsonContent.Applist.Apps)
            {
                var normalizedTitle = NormalizeTitle(app.Name);

                if (output.ContainsKey(normalizedTitle))
                {
                    continue;
                }

                output.Add(normalizedTitle, app.Appid);
            }
            return output;
        }

        private class AppListRoot
        {
            public AppList Applist { get; set; }
        }

        private class AppList
        {
            public List<SteamApp> Apps { get; set; } = new List<SteamApp>();
        }

        private class SteamApp
        {
            public int Appid { get; set; }
            public string Name { get; set; }
        }
    }
}
