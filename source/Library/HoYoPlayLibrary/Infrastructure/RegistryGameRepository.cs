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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Infrastructure
{
    internal class RegistryGameRepository : IHoyoPlayGameRepository
    {
        private readonly ILogger _logger;
        private readonly IRegistryVersionResolver _registryVersionResolver;

        public RegistryGameRepository(ILogger logger, IRegistryVersionResolver registryVersionResolver)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registryVersionResolver = registryVersionResolver ?? throw new ArgumentNullException(nameof(registryVersionResolver));
        }

        public IEnumerable<HoyoPlayGame> GetInstalledGames()
        {
            var games = new List<HoyoPlayGame>();
            var rootKeyPath = _registryVersionResolver.GetActiveRootKeyPath();
            if (rootKeyPath.IsNullOrEmpty())
            {
                _logger.Warn("HoYoPlay registry not found; cannot locate games.");
                return games;
            }

            using (var root = Registry.CurrentUser.OpenSubKey(rootKeyPath))
            {
                if (root is null)
                {
                    _logger.Warn($"Failed to open HoYoPlay registry path: {rootKeyPath}");
                    return games;
                }

                var subkeyNames = root.GetSubKeyNames();
                if (subkeyNames.Length == 0)
                {
                    _logger.Warn($"No subkeys found under HoYoPlay registry path: {rootKeyPath}");
                    return games;
                }

                foreach (var subkeyName in subkeyNames)
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

                if (!games.Any())
                {
                    _logger.Warn($"No valid game entries found under HoYoPlay registry path: {rootKeyPath}");
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
