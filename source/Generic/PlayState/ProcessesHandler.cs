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
        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtSuspendProcess(IntPtr processHandle);
        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtResumeProcess(IntPtr processHandle);

        private static readonly List<string> exclusionList = new List<string>
        {
            "7z.exe",
            "7za.exe",
            "archive.exe",
            "asset_.exe",
            "anetdrop.exe",
            "bat_to_exe_convertor.exe",
            "bssndrpt.exe",
            "bootboost.exe",
            "bootstrap.exe",
            "cabarc.exe",
            "cdkey.exe",
            "cheat engine.exe",
            "cheatengine",
            "civ2map.exe",
            "config",
            "closepw.exe",
            "crashdump",
            "crashreport",
            "crc32.exe",
            "creationkit.exe",
            "creatureupload.exe",
            "easyhook.exe",
            "dgvoodoocpl.exe",
            "dotnet",
            "doc.exe",
            "dxsetup",
            "dw.exe",
            "enbinjector.exe",
            "havokbehaviorpostprocess.exe",
            "help",
            "install",
            "launch_game.exe",
            "langselect.exe",
            "language.exe",
            "launch",
            "loader",
            "mapcreator.exe",
            "master_dat_fix_up.exe",
            "md5sum.exe",
            "mgexegui.exe",
            "modman.exe",
            "modorganizer.exe",
            "notepad++.exe",
            "notification_helper.exe",
            "oalinst.exe",
            "palettestealersuspender.exe",
            "pak",
            "patch",
            "planet_mapgen.exe",
            "papyrus",
            "radtools.exe",
            "readspr.exe",
            "register.exe",
            "sekirofpsunlocker",
            "settings",
            "setup",
            "scuex64.exe",
            "synchronicity.exe",
            "syscheck.exe",
            "systemsurvey.exe",
            "tes construction set.exe",
            "texmod.exe",
            "unins",
            "unitycrashhandler",
            "x360ce",
            "unpack",
            "unx_calibrate",
            "update",
            "unrealcefsubprocess.exe",
            "url.exe",
            "versioned_json.exe",
            "vcredist",
            "xtexconv.exe",
            "xwmaencode.exe",
            "website.exe",
            "wide_on.exe"
        };

        public static List<ProcessItem> GetProcessesWmiQuery(bool filterPaths, string gameInstallDir, string exactPath = null)
        {
            var wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process";
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
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                            };

                var gameProcesses = new List<ProcessItem>();
                if (exactPath != null)
                {
                    foreach (var fItem in query)
                    {
                        if (fItem.Path.IsNullOrEmpty() ||
                            !fItem.Path.Equals(exactPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        gameProcesses.Add(
                           new ProcessItem
                           {
                               ExecutablePath = fItem.Path,
                               Process = fItem.Process
                           }
                       );
                    }
                }
                else
                {
                    foreach (var item in query)
                    {
                        if (item.Path.IsNullOrEmpty() ||
                            !item.Path.StartsWith(gameInstallDir, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        if (filterPaths &&
                            exclusionList.Any(e => Path.GetFileName(item.Path).Contains(e, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            continue;
                        }

                        gameProcesses.Add(
                            new ProcessItem
                            {
                                ExecutablePath = item.Path,
                                Process = item.Process
                            }
                        );
                    }
                }

                return gameProcesses;
            }
        }
    }
}
