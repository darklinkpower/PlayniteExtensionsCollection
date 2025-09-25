using HoYoPlayLibrary.Application.Services;
using HoYoPlayLibrary.Domain.Entities;
using HoYoPlayLibrary.Domain.Interfaces;
using Microsoft.Win32;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Infrastructure
{
    internal class RegistryGameRepository : IHoyoPlayGameRepository
    {
        private const string RootKey = @"Software\Cognosphere\HYP\1_0";
        private readonly ILogger _logger;

        public RegistryGameRepository(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<HoyoPlayGame> GetInstalledGames()
        {
            var games = new List<HoyoPlayGame>();
            using (var root = Registry.CurrentUser.OpenSubKey(RootKey))
            {
                if (root is null)
                {
                    return games;
                }

                foreach (var subkeyName in root.GetSubKeyNames())
                {
                    if (subkeyName.Equals("InstallPath", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using (var gameKey = root.OpenSubKey(subkeyName))
                    {
                        if (gameKey is null)
                        {
                            continue;
                        }

                        var gameBiz = gameKey.GetValue("GameBiz") as string;
                        if (gameBiz.IsNullOrEmpty())
                        {
                            continue;
                        }

                        var installPath = gameKey.GetValue("GameInstallPath") as string;
                        if (installPath.IsNullOrEmpty())
                        {
                            continue;
                        }

                        if (!FileSystem.DirectoryExists(installPath))
                        {
                            _logger.Warn($"Install path does not exist for game '{gameBiz}': {installPath}");
                            continue;
                        }

                        var exePath = FindGameExe(installPath);
                        if (exePath.IsNullOrEmpty())
                        {
                            _logger.Warn($"No valid executable found for game '{gameBiz}' in '{installPath}'");
                            continue;
                        }

                        var name = GameNameResolver.Resolve(subkeyName);
                        games.Add(new HoyoPlayGame(gameBiz, name, installPath, exePath));
                    }
                }
            }
            
            return games;
        }

        private string FindGameExe(string installPath)
        {
            if (!FileSystem.DirectoryExists(installPath))
            {
                return null;
            }

            var exeFiles = Directory.GetFiles(installPath, "*.exe", SearchOption.TopDirectoryOnly);
            foreach (var exe in exeFiles)
            {
                var exeName = Path.GetFileName(exe);

                // Skip UnityPlayer and any known launcher executables, e.g. "UnityCrashHandler64.exe"
                if (exeName.StartsWith("Unity", StringComparison.OrdinalIgnoreCase) ||
                    exeName.Equals("launcher.exe", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return exe; // First valid exe
            }

            return null;
        }


    }
}
