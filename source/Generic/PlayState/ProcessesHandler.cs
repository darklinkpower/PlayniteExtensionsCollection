using Playnite.SDK;
using PlayState.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayState
{
    public static class ProcessesHandler
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private const string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process";

        public static List<ProcessItem> GetProcessesWmiQuery(bool useExclusionList, string gameInstallDir, List<string> scanExclusionList, string exactPath = null)
        {
            logger.Debug($"Starting GetProcessesWmiQuery. \"{useExclusionList}\", \"{gameInstallDir}\", \"{exactPath}\"");
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                // Unfortunately due to Playnite being a 32 bits process, the GetProcess()
                // method can't access needed values of 64 bits processes, so it's needed
                // to correlate with data obtained from a WMI query that is exponentially slower.
                // It needs to be done this way until #1199 is done
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new ProcessItem(p, (string)mo["ExecutablePath"]);

                var gameProcesses = new List<ProcessItem>();
                if (!exactPath.IsNullOrEmpty())
                {
                    AddGameProcessesExactPath(exactPath, query, gameProcesses);
                }
                else
                {
                    AddGameProcessesThatStartWithPath(useExclusionList, gameInstallDir, query, gameProcesses, scanExclusionList);
                }

                logger.Debug($"Returning {gameProcesses.Count} items: {string.Join(", ", gameProcesses.Select(x => $"({x.ExecutablePath}|{x.Process.MainWindowHandle})"))}");
                return gameProcesses;
            }
        }

        private static void AddGameProcessesExactPath(string exactPath, IEnumerable<ProcessItem> query, List<ProcessItem> gameProcesses)
        {
            foreach (var queryItem in query)
            {
                if (queryItem.ExecutablePath.IsNullOrEmpty() ||
                    !queryItem.ExecutablePath.Equals(exactPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                gameProcesses.Add(queryItem);
            }
        }

        private static void AddGameProcessesThatStartWithPath(bool useExclusionList, string startPath, IEnumerable<ProcessItem> query, List<ProcessItem> gameProcesses, List<string> exclusionList)
        {
            foreach (var queryItem in query)
            {
                if (queryItem.ExecutablePath.IsNullOrEmpty() ||
                    !queryItem.ExecutablePath.StartsWith(startPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (useExclusionList &&
                    exclusionList.Any(e => Path.GetFileName(queryItem.ExecutablePath).Contains(e, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                gameProcesses.Add(queryItem);
            }
        }

    }
}