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
        private static readonly HashSet<char> _invalidFileChars = new HashSet<char>(Path.GetInvalidFileNameChars());

        public PathsResolver(IPlayniteAPI playniteApi, InstallationStatusUpdaterSettingsViewModel settings)
        {
            _playniteApi = playniteApi ?? throw new ArgumentNullException(nameof(playniteApi));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string GetInstallDirForDetection(Game game)
        {
            var path = game.InstallDirectory;
            if (path.IsNullOrEmpty())
            {
                return string.Empty;
            }

            // No variables to expand
            if (!path.Contains('{'))
            {
                return path;
            }

            if (path.Contains("{EmulatorDir}"))
            {
                var emulator = GetGameEmulator(game);
                if (emulator != null &&
                    !emulator.InstallDir.IsNullOrEmpty())
                {
                    return _playniteApi.ExpandGameVariables(
                        game,
                        path,
                        emulator.InstallDir);
                }
            }

            return _playniteApi.ExpandGameVariables(game, path);
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

        private bool DetectIsRomInstalled(
            Game game,
            GameRom rom,
            string resolvedInstallationDirectory)
        {
            var path = rom.Path;
            if (path.IsNullOrEmpty())
            {
                return false;
            }

            if (path.Contains('{'))
            {
                var requiresEmulatorDir =
                    path.Contains("{EmulatorDir}") ||
                    game.InstallDirectory?.Contains("{EmulatorDir}") == true;

                if (requiresEmulatorDir)
                {
                    var emulator = GetGameEmulator(game);
                    if (emulator != null &&
                        !emulator.InstallDir.IsNullOrEmpty())
                    {
                        path = _playniteApi.ExpandGameVariables(
                            game,
                            path,
                            emulator.InstallDir);
                    }
                    else
                    {
                        path = _playniteApi.ExpandGameVariables(game, path);
                    }
                }
                else
                {
                    path = _playniteApi.ExpandGameVariables(game, path);
                }
            }

            if (!Path.IsPathRooted(path) &&
                !resolvedInstallationDirectory.IsNullOrEmpty())
            {
                path = Path.Combine(
                    resolvedInstallationDirectory,
                    path);
            }

            return FileSystem.FileExists(path);
        }

        private Emulator GetGameEmulator(Game game)
        {
            if (game.GameActions is null)
            {
                return null;
            }

            foreach (var action in game.GameActions)
            {
                if (action.Type != GameActionType.Emulator || action.EmulatorId == Guid.Empty)
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
                        var isActionPathInstalled = DetectIsFileActionInstalled(game, gameAction, resolvedInstallationDirectory);
                        if (!isActionPathInstalled)
                        {
                            return false;
                        }

                        if (!_settings.Settings.DetectFilesFromLaunchArguments ||  gameAction.Arguments.IsNullOrWhiteSpace())
                        {
                            return true;
                        }

                        var arguments = gameAction.Arguments;
                        if (arguments.Contains('{'))
                        {
                            arguments = _playniteApi.ExpandGameVariables(game, arguments);
                        }

                        if (DetectArePathsInArgumentsInstalled(arguments))
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        private static bool DetectArePathsInArgumentsInstalled(string arguments)
        {
            var pathsInArguments = StringPathsDetector.ExtractPathsFromArguments(arguments);
            foreach (var path in pathsInArguments)
            {
                if (!Path.IsPathRooted(path))
                {
                    continue;
                }

                if (!FileSystem.FileExists(path))
                {
                    return false;
                }
            }

            return true;
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
                if (!_invalidFileChars.Contains(c) || c == '\\' || c == '/' || c == ':')
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
