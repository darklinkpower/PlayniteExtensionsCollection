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
            var rootKeyPaths = _registryVersionResolver.GetActiveRootKeyPaths();
            if (rootKeyPaths is null || rootKeyPaths.Count == 0)
            {
                _logger.Warn("HoYoPlay registry not found; cannot locate games.");
                return games;
            }

            foreach (var rootKeyPath in rootKeyPaths)
            {
                var gamesFoundInRoot = 0;
                using (var root = Registry.CurrentUser.OpenSubKey(rootKeyPath))
                {
                    if (root is null)
                    {
                        _logger.Warn($"Failed to open HoYoPlay registry path: {rootKeyPath}");
                        continue;
                    }

                    var subkeyNames = root.GetSubKeyNames();
                    if (subkeyNames.Length == 0)
                    {
                        _logger.Warn($"No subkeys found under HoYoPlay registry path: {rootKeyPath}");
                        continue;
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
                                _logger.Warn($"GameBiz value is missing for subkey: {subkeyName}");
                                continue;
                            }

                            var installPath = gameKey.GetValue("GameInstallPath") as string;
                            if (installPath.IsNullOrEmpty())
                            {
                                _logger.Warn($"GameInstallPath value is missing for game '{gameBiz}' in subkey: {subkeyName}");
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

                            var existingGame = games.FirstOrDefault(g => g.Id.Equals(gameBiz, StringComparison.OrdinalIgnoreCase));
                            if (existingGame != null)
                            {
                                _logger.Warn($"Duplicate game entry found for '{gameBiz}'. Replacing duplicate.");
                                games.Remove(existingGame);
                            }

                            var name = GameNameResolver.Resolve(subkeyName);
                            games.Add(new HoyoPlayGame(gameBiz, name, installPath, exePath));
                            gamesFoundInRoot++;
                        }
                    }

                    if (gamesFoundInRoot == 0)
                    {
                        _logger.Warn($"No valid game entries found under HoYoPlay registry path: {rootKeyPath}");
                    }
                }
            }

            if (games.Count == 0)
            {
                _logger.Warn("No valid HoYoPlay game installations were found.");
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
