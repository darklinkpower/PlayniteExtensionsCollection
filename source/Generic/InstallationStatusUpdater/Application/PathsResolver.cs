using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Application
{
    public class PathsResolver
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly InstallationStatusUpdaterSettingsViewModel _settings;
        private static readonly HashSet<char> invalidFileChars = new HashSet<char>(Path.GetInvalidFileNameChars());

        public PathsResolver(IPlayniteAPI playniteApi, InstallationStatusUpdaterSettingsViewModel settings)
        {
            _playniteApi = playniteApi ?? throw new ArgumentNullException(nameof(playniteApi));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string GetInstallDirForDetection(Game game)
        {
            if (game.InstallDirectory.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var resolvedPath = game.InstallDirectory;
            if (resolvedPath.Contains('{'))
            {
                resolvedPath = _playniteApi.ExpandGameVariables(game, resolvedPath);
            }

            return resolvedPath;
        }

        public bool IsAnyRomInstalled(Game game, string resolvedInstallationDirectory)
        {
            if (game.Roms is null || !game.Roms.Any())
            {
                return false;
            }

            if (_settings.Settings.UseOnlyFirstRomDetection)
            {
                return DetectIsRomInstalled(game, game.Roms.First(), resolvedInstallationDirectory);
            }

            return game.Roms.Any(rom => DetectIsRomInstalled(game, rom, resolvedInstallationDirectory));
        }

        private bool DetectIsRomInstalled(Game game, GameRom rom, string resolvedInstallationDirectory)
        {
            if (rom.Path.IsNullOrEmpty())
            {
                return false;
            }

            var resolvedPath = rom.Path;
            if (resolvedPath.Contains("{EmulatorDir}"))
            {
                var emulator = GetGameEmulator(game);
                if (emulator != null && !emulator.InstallDir.IsNullOrEmpty())
                {
                    resolvedPath = resolvedPath.Replace("{EmulatorDir}", emulator.InstallDir);
                }
            }

            if (resolvedPath.Contains('{'))
            {
                resolvedPath = _playniteApi.ExpandGameVariables(game, resolvedPath);
            }

            if (Path.IsPathRooted(resolvedPath))
            {
                return FileSystem.FileExists(resolvedPath);
            }

            if (!resolvedInstallationDirectory.IsNullOrEmpty())
            {
                resolvedPath = Path.Combine(resolvedInstallationDirectory, resolvedPath);
            }

            return FileSystem.FileExists(resolvedPath);
        }

        private Emulator GetGameEmulator(Game game)
        {
            if (game.GameActions is null)
            {
                return null;
            }

            foreach (var action in game.GameActions)
            {
                if (action.Type != GameActionType.Emulator && action.EmulatorId == Guid.Empty)
                {
                    continue;
                }

                var emulator = _playniteApi.Database.Emulators[action.EmulatorId];
                if (emulator != null)
                {
                    return emulator;
                }
            }

            return null;
        }

        public bool IsAnyActionInstalled(Game game, string resolvedInstallationDirectory)
        {
            if (game.GameActions is null)
            {
                return false;
            }

            foreach (var gameAction in game.GameActions)
            {
                if (!gameAction.IsPlayAction && _settings.Settings.OnlyUsePlayActionsForDetection)
                {
                    continue;
                }

                switch (gameAction.Type)
                {
                    case GameActionType.URL:
                        if (_settings.Settings.UrlActionIsInstalled)
                        {
                            return true;
                        }
                        break;

                    case GameActionType.Script:
                        if (_settings.Settings.ScriptActionIsInstalled)
                        {
                            return true;
                        }
                        break;

                    case GameActionType.File:
                        if (DetectIsFileActionInstalled(game, gameAction, resolvedInstallationDirectory))
                        {
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        private bool DetectIsFileActionInstalled(Game game, GameAction gameAction, string resolvedInstallationDirectory)
        {
            if (gameAction.Path.IsNullOrEmpty())
            {
                return false;
            }

            //Games added as a Microsoft Store Application use explorer and arguments to launch the game
            if (gameAction.Path.Equals("explorer.exe", StringComparison.OrdinalIgnoreCase))
            {
                //If directory has been set, it can be used to detect if game is installed or not
                if (!resolvedInstallationDirectory.IsNullOrEmpty())
                {
                    return FileSystem.DirectoryExists(resolvedInstallationDirectory);
                }
                else
                {
                    return true;
                }
            }

            var resolvedPath = gameAction.Path;
            if (resolvedPath.Contains('{'))
            {
                resolvedPath = _playniteApi.ExpandGameVariables(game, resolvedPath);
            }

            if (!Path.IsPathRooted(resolvedPath) && !resolvedInstallationDirectory.IsNullOrEmpty())
            {
                resolvedPath = Path.Combine(resolvedInstallationDirectory, resolvedPath);
            }

            return FileSystem.FileExists(resolvedPath);
        }

        public static string RemoveInvalidPathChars(string str)
        {
            var result = new StringBuilder(str.Length);
            foreach (var c in str)
            {
                if (!invalidFileChars.Contains(c) || c == '\\' || c == '/' || c == ':')
                {
                    result.Append(c);
                }
            }

            // If no characters were removed, return original string
            if (result.Length == str.Length)
            {
                return str;
            }

            return result.ToString();
        }
    }
}
