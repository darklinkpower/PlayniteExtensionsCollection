using Microsoft.Win32;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamCommon
{
    public static class SteamClient
    {
        private static ILogger logger = LogManager.GetLogger();
        public static bool GetIsSteamRunning()
        {
            Process[] processes = Process.GetProcessesByName("Steam");
            return processes.Length > 0;
        }

        private static bool GetIsSteamBpmRunning()
        {
            if (!GetIsSteamRunning())
            {
                return false;
            }

            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
            {
                var value = key?.GetValue("BigPictureInForeground")?.ToString();
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }

                var intValue = int.Parse(value);
                if (intValue == 0)
                {
                    return false;
                }
            }

            logger.Info("Steam Big Picture Mode detected as running");
            return true;
        }

        private const string defaultSteamPath = @"C:\Program Files (x86)\Steam\steam.exe";
        public static string GetSteamInstallationPath()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
            {
                if (key?.GetValueNames().Contains("SteamExe") == true)
                {
                    return key.GetValue("SteamExe")?.ToString().Replace('/', '\\') ?? defaultSteamPath;
                }
            }

            return defaultSteamPath;
        }

        public static string GetSteamInstallationDirectory()
        {
            return Path.GetDirectoryName(GetSteamInstallationPath());
        }

        public static void StartSteam(bool restartIfRunning, List<string> argumentsList)
        {
            StartSteam(restartIfRunning, argumentsList.Aggregate((x, b) => x + " " + b));
        }

       public static void StartSteam(bool restartIfRunning, string arguments = "")
        {
            string steamInstallationPath = GetSteamInstallationPath();
            if (!FileSystem.FileExists(steamInstallationPath))
            {
                logger.Error(string.Format("Steam executable not detected in path \"{0}\"", steamInstallationPath));
                return;
            }

            bool isSteamRunning = GetIsSteamRunning();
            if (isSteamRunning && restartIfRunning)
            {
                ProcessStarter.StartProcess(steamInstallationPath, "-shutdown");
                logger.Info("Steam detected running. Closing via command line.");
                for (int i = 0; i < 8; i++)
                {
                    Thread.Sleep(2000);
                    isSteamRunning = GetIsSteamRunning();
                    if (isSteamRunning)
                    {
                        logger.Info("Steam detected running.");
                    }
                    else
                    {
                        logger.Info("Steam has closed.");
                        break;
                    }
                }
            }

            if (!isSteamRunning)
            {
                ProcessStarter.StartProcess(steamInstallationPath, arguments);
                logger.Info("Steam started with arguments {arguments}");
            }
            else
            {
                logger.Warn("Steam was detected as running and was not launched via the extension.");
            }
        }

    }


}
