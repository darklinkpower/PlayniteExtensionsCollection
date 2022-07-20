using GamesSizeCalculator.Models;
using Playnite.SDK;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesSizeCalculator.SteamSizeCalculation
{
    public class SteamSizeCalculator
    {
        private ILogger logger = LogManager.GetLogger();
        public ISteamApiClient SteamApiClient { get; }

        public SteamSizeCalculator(ISteamApiClient steamApiClient)
        {
            SteamApiClient = steamApiClient;
        }

        public async Task<long?> GetInstallSizeAsync(uint appId, bool includeDLC = true, bool includeOptional = true)
        {
            var depotData = await GetRelevantDepots(appId);
            if (depotData == null)
            {
                return null;
            }

            RemoveRegionalMainDepots(ref depotData);

            IEnumerable<DepotInfo> filteredDepots = depotData;

            if (!includeDLC)
            {
                filteredDepots = filteredDepots.Where(d => !d.IsDLC);
            }

            if (!includeOptional)
            {
                filteredDepots = filteredDepots.Where(d => !d.Optional);
            }

            return filteredDepots.Sum(d => d.FileSize);
        }

        private void RemoveRegionalMainDepots(ref List<DepotInfo> depots)
        {
            var biggestDepotSize = depots.Max(d => d.FileSize);
            var biggestDepots = depots.Where(d => d.FileSize > biggestDepotSize * 0.95).OrderBy(d => d.FileSize).ToList();
            var biggestDepotsCopy = new List<DepotInfo>(biggestDepots);
            foreach (var depot in biggestDepots)
            {
                var lastWordInName = GetLastWord(depot.Name)?.ToLowerInvariant();
                switch (lastWordInName)
                {
                    case null:
                        break;
                    case "eu":
                    case "europe":
                    case "asia":
                    case "aus":
                    case "australia":
                    case "nz":
                    case "usa":
                    case "us":
                    case "ru":
                    case "russia":
                    case "germany":
                    case "deutschland":
                    case "de":
                    case "es":
                    case "sa":
                    case "cn":
                    case "china":
                    case "row":
                    case "ww":
                        if (biggestDepotsCopy.Count == 1) // Don't remove the last big depot
                        {
                            return;
                        }

                        biggestDepotsCopy.Remove(depot);
                        depots.Remove(depot);
                        logger.Debug($"Removed depot {depot.Name}");
                        break;
                    default:
                        break;
                }
            }
        }

        private static string GetLastWord(string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                return str;
            }

            str = str.Trim();
            var i = str.LastIndexOfAny(new[] { ' ', '_' });
            if (i == -1)
            {
                return str;
            }

            var lastWord = str.Substring(i + 1);
            if (lastWord.Equals("content", StringComparison.InvariantCultureIgnoreCase)
                || lastWord.Equals("depot", StringComparison.InvariantCultureIgnoreCase))
            {
                string retryStr = str.Remove(i).Trim();
                return GetLastWord(retryStr);
            }
            else
            {
                return lastWord;
            }
        }

        private async Task<List<DepotInfo>> GetRelevantDepots(uint appId)
        {
            var productInfo = await SteamApiClient.GetProductInfo(appId);
            var depots = GetKeyValueNode(productInfo, "depots");
            if (depots == null || depots == KeyValue.Invalid)
            {
                logger.Warn($"No depots for {appId}");
                return null;
            }

            var output = new List<DepotInfo>();
            foreach (var depot in depots.Children)
            {
                var id = GetValue(depot);
                var name = GetKeyValueNode(depot, "name")?.Value;

                var maxsizeStr = GetValue(depot, "maxsize");
                if (maxsizeStr == null || !long.TryParse(maxsizeStr, out long maxsize))
                {
                    continue;
                }

                var sharedinstallNode = GetValue(depot, "sharedinstall");
                if (sharedinstallNode == "1")
                {
                    logger.Debug($"Skipped shared depot {name}");
                    continue;
                }

                var language = GetValue(depot, "config", "language");
                if (!language.IsNullOrEmpty() && !"english".Equals(language, StringComparison.InvariantCultureIgnoreCase))
                {
                    logger.Debug($"Skipping depot \"{name}\" because its language is \"{language}\"");
                    continue;
                }

                var oslist = GetValue(depot, "config", "oslist");
                if (!oslist.IsNullOrEmpty() && !oslist.Contains("windows", StringComparison.InvariantCultureIgnoreCase))
                {
                    logger.Debug($"Skipping depot \"{name}\" because its OS list is \"{oslist}\"");
                    continue;
                }

                var optional = GetValue(depot, "optional") == "1";

                var dlcAppId = GetValue(depot, "dlcappid");

                logger.Debug($"Depot {name}, size {maxsize} for appId {appId}, dlc app ID: {dlcAppId}");

                output.Add(new DepotInfo(id, name, maxsize, dlcAppId != null, optional));
            }

            return output;
        }

        private KeyValue GetKeyValueNode(KeyValue kv, params string[] names)
        {
            if (kv == null || kv == KeyValue.Invalid)
            {
                return null;
            }

            KeyValue node = kv;
            foreach (var name in names)
            {
                node = node[name];
                if (node == null || node == KeyValue.Invalid)
                {
                    return null;
                }
            }
            return node;
        }

        private string GetValue(KeyValue kv, params string[] names)
        {
            return GetKeyValueNode(kv, names)?.Value;
        }
    }
}