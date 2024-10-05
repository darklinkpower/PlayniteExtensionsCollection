using FuzzySharp;
using IniParser;
using IniParser.Model;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKHandler.Application
{
    public static class SpecialKConfigurationManager
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly FileIniDataParser _iniParser;
        private static readonly Regex reshadeTechniqueRegex = new Regex(@"technique ([^\s]+)", RegexOptions.None);
        private static readonly string _pluginInstallPath;
        private static readonly string _emptyReshadePresetPath;

        static SpecialKConfigurationManager()
        {
            _iniParser = new FileIniDataParser();
            _iniParser.Parser.Configuration.AssigmentSpacer = string.Empty;
            _iniParser.Parser.Configuration.AllowDuplicateKeys = true;
            _iniParser.Parser.Configuration.OverrideDuplicateKeys = true;
            _iniParser.Parser.Configuration.AllowDuplicateSections = true;

            _pluginInstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _emptyReshadePresetPath = Path.Combine(_pluginInstallPath, "Resources", "ReshadeDefaultPreset.ini");
        }
        
        public static void ValidateReshadeConfiguration(Game game, string skifPath)
        {
            var reshadeBase = Path.Combine(skifPath, @"PlugIns\ThirdParty\ReShade");
            var reshadeIniPath = Path.Combine(reshadeBase, "ReShade.ini");

            if (!FileSystem.DirectoryExists(reshadeBase))
            {
                _logger.Warn($"Reshade directory not found in {reshadeBase}");
                return;
            }

            var gameReshadePresetSubPath = Path.Combine("reshade-presets", game.Id.ToString() + ".ini");
            var gameReshadePreset = Path.Combine(reshadeBase, gameReshadePresetSubPath);

            if (!FileSystem.FileExists(gameReshadePreset))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(gameReshadePreset));
                var techniqueSortingLine = GetReshadeTechniqueSorting(reshadeBase);
                if (!techniqueSortingLine.IsNullOrEmpty())
                {
                    FileSystem.WriteStringToFile(gameReshadePreset, $"PreprocessorDefinitions=\nTechniques=\nTechniqueSorting={techniqueSortingLine}", true);
                }
                else
                {
                    FileSystem.CopyFile(_emptyReshadePresetPath, gameReshadePreset, true);
                }
            }

            if (FileSystem.FileExists(reshadeIniPath))
            {
                var ini = _iniParser.ReadFile(reshadeIniPath);
                var updatedValues = 0;
                updatedValues += ValidateIniValue(ini, "GENERAL", "PresetPath", ".\\" + gameReshadePresetSubPath);
                updatedValues += ValidateIniValue(ini, "APP", "ForceVSync", "0");
                updatedValues += ValidateIniValue(ini, "APP", "ForceWindowed", "0");
                updatedValues += ValidateIniValue(ini, "APP", "ForceFullscreen", "0");
                updatedValues += ValidateIniValue(ini, "APP", "ForceResolution", "0,0");
                updatedValues += ValidateIniValue(ini, "APP", "Force10BitFormat", "0");

                if (updatedValues > 0)
                {
                    _iniParser.WriteFile(reshadeIniPath, ini, Encoding.UTF8);
                }
            }
        }

        private static string GetReshadeTechniqueSorting(string reshadeBase)
        {
            var shadersDirectory = Path.Combine(reshadeBase, "reshade-shaders", "Shaders");
            if (!Directory.Exists(shadersDirectory))
            {
                _logger.Warn($"Reshade Shaders directory not found in {shadersDirectory}");
                return string.Empty;
            }

            var fxFiles = Directory.GetFiles(shadersDirectory, "*.fx", SearchOption.AllDirectories);
            var techniqueList = new List<string>();
            foreach (var fxFile in fxFiles)
            {
                var fxContent = FileSystem.ReadStringFromFile(fxFile);
                var result = reshadeTechniqueRegex.Match(fxContent);
                if (result.Success)
                {
                    techniqueList.Add($"{result.Groups[1]}@{Path.GetFileName(fxFile)}");
                }
            }

            if (techniqueList.Count == 0)
            {
                return string.Empty;
            }

            techniqueList.Sort((x, y) => x.CompareTo(y));
            return string.Join(",", techniqueList);
        }

        public static void ValidateDefaultProfile(
            Game game,
            string skifPath,
            SpecialKHelperSettingsViewModel settings,
            string configurationDirectoryPath,
            IPlayniteAPI playniteApi)
        {
            var defaultConfigPath = Path.Combine(skifPath, "Global", "default_SpecialK.ini");
            if (!FileSystem.FileExists(defaultConfigPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(defaultConfigPath));
                FileSystem.CreateFile(defaultConfigPath);
            }

            var ini = _iniParser.ReadFile(defaultConfigPath);
            var updatedValues = 0;
            if (settings.Settings.EnableStOverlayOnNewProfiles)
            {
                updatedValues += ValidateIniValue(ini, "Steam.System", "PreLoadSteamOverlay", "true");
                var steamId = ConfigureSteamApiInject(game, configurationDirectoryPath, playniteApi);
                if (!steamId.IsNullOrEmpty())
                {
                    updatedValues += ValidateIniValue(ini, "Steam.System", "AppID", "steamId");
                }
            }
            else
            {
                updatedValues += ValidateIniValue(ini, "Steam.System", "PreLoadSteamOverlay", "false");
            }

            if (settings.Settings.EnableReshadeOnNewProfiles)
            {
                updatedValues += ValidateIniValue(ini, "Render.FrameRate", "SleeplessRenderThread", "false");
                updatedValues += ValidateIniValue(ini, "Render.OSD", "ShowInVideoCapture", "false");

                updatedValues += ValidateIniValue(ini, "Import.ReShade64", "Architecture", "x64");
                updatedValues += ValidateIniValue(ini, "Import.ReShade64", "Role", "ThirdParty");
                updatedValues += ValidateIniValue(ini, "Import.ReShade64", "When", "PlugIn");
                updatedValues += ValidateIniValue(ini, "Import.ReShade64", "Filename", @"..\..\PlugIns\ThirdParty\ReShade\ReShade64.dll");

                updatedValues += ValidateIniValue(ini, "Import.ReShade32", "Architecture", "Win32");
                updatedValues += ValidateIniValue(ini, "Import.ReShade32", "Role", "ThirdParty");
                updatedValues += ValidateIniValue(ini, "Import.ReShade32", "When", "PlugIn");
                updatedValues += ValidateIniValue(ini, "Import.ReShade32", "Filename", @"..\..\PlugIns\ThirdParty\ReShade\ReShade32.dll");
            }
            else
            {
                updatedValues += ValidateIniValue(ini, "Import.ReShade64", "Filename", string.Empty);
                updatedValues += ValidateIniValue(ini, "Import.ReShade32", "Filename", string.Empty);
            }

            if (settings.Settings.SetDefaultFpsOnNewProfiles && settings.Settings.DefaultFpsLimit != 0)
            {
                updatedValues += ValidateIniValue(ini, "Render.FrameRate", "TargetFPS", settings.Settings.DefaultFpsLimit.ToString());
            }
            else
            {
                updatedValues += ValidateIniValue(ini, "Render.FrameRate", "TargetFPS", "0.0");
            }

            if (settings.Settings.DisableNvidiaBlOnNewProfiles)
            {
                updatedValues += ValidateIniValue(ini, "Compatibility.General", "DisableBloatWare_NVIDIA", "true");
            }
            else
            {
                updatedValues += ValidateIniValue(ini, "Compatibility.General", "DisableBloatWare_NVIDIA", "false");
            }

            if (settings.Settings.UseFlipModelOnNewProfiles)
            {
                updatedValues += ValidateIniValue(ini, "Render.DXGI", "UseFlipDiscard", "true");
            }
            else
            {
                updatedValues += ValidateIniValue(ini, "Render.DXGI", "UseFlipDiscard", "false");
            }

            if (updatedValues > 0)
            {
                _iniParser.WriteFile(defaultConfigPath, ini, Encoding.UTF8);
            }
        }

        private static int ValidateIniValue(IniData ini, string key, string subKey, string newValue)
        {
            var currentValue = ini[key][subKey];
            if (currentValue is null || currentValue != newValue)
            {
                ini[key][subKey] = newValue;
                return 1;
            }

            return 0;
        }

        private static string ConfigureSteamApiInject(Game game, string configurationDirectoryPath, IPlayniteAPI playniteApi)
        {
            if (Steam.IsGameSteamGame(game))
            {
                return null;
            }

            var appIdTextPath = string.Empty;
            var isInstallDirValid = PlayniteUtilities.IsGameInstallDirectoryValid(game);
            if (isInstallDirValid)
            {
                appIdTextPath = Path.Combine(game.InstallDirectory, "steam_appid.txt");
                if (FileSystem.FileExists(appIdTextPath))
                {
                    return null;
                }
            }

            var previousId = string.Empty;

            // TODO move to game data cache...
            var historyFlagFile = Path.Combine(configurationDirectoryPath, "SteamId_" + game.Id.ToString());
            if (FileSystem.FileExists(historyFlagFile))
            {
                previousId = FileSystem.ReadStringFromFile(historyFlagFile).Trim();
                _logger.Info($"Detected attempt flag file for game {game.Name} in {historyFlagFile}. Previous Id: {previousId}");
            }

            var steamId = "0";
            if (!previousId.IsNullOrEmpty())
            {
                // We use the previously found Id to not have to search again
                steamId = previousId;
            }
            else if (PlayniteUtilities.IsGamePcGame(game))
            {
                var isBackgroundDownload = false;
                if (playniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
                {
                    isBackgroundDownload = true;
                }

                var steamIdSearch = GetSteamIdFromSearch(game, isBackgroundDownload, playniteApi, true);
                if (!steamId.IsNullOrEmpty())
                {
                    steamId = steamIdSearch;
                }
            }

            if (isInstallDirValid && !appIdTextPath.IsNullOrEmpty())
            {
                try
                {
                    FileSystem.WriteStringToFileSafe(appIdTextPath, steamId);
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error while creating steam id file in {appIdTextPath}");
                }
            }

            // Flag file so we don't attempt to search again in future startups.
            if (!FileSystem.FileExists(historyFlagFile))
            {
                FileSystem.WriteStringToFile(historyFlagFile, steamId, true);
            }

            return steamId;
        }

        public static string GetSteamIdFromSearch(Game game, bool isBackgroundDownload, IPlayniteAPI playniteApi, bool matchFuzzyMethods = false)
        {
            var normalizedName = game.Name.NormalizeGameName();
            var results = SteamWeb.GetSteamSearchResults(normalizedName);
            results.ForEach(a => a.Name = a.Name.NormalizeGameName());

            // Try to see if there's an exact match, to not prompt the user unless needed
            var matchingGameName = normalizedName.Satinize();
            var exactMatch = results.FirstOrDefault(x => x.Name.Satinize() == matchingGameName);

            // Automatic match method 1: Remove all symbols
            if (exactMatch != null)
            {
                _logger.Info($"Found steam id for game {game.Name} via method 1, Id: {exactMatch.GameId}, Match: {exactMatch.Name}");
                return exactMatch.GameId;
            }

            var currentLevenshteinId = string.Empty;
            var currentDistance = 99;
            if (matchFuzzyMethods)
            {
                // Automatic match method 2: Fuzzy search
                var currentFuzzyValue = 0;
                var currentFuzzyId = string.Empty;
                foreach (var result in results)
                {
                    var proximity = Fuzz.Ratio(normalizedName.ToLower(), result.Name.ToLower());
                    if (proximity > currentFuzzyValue)
                    {
                        currentFuzzyValue = proximity;
                        currentFuzzyId = result.GameId;
                    }
                }

                if (!currentFuzzyId.IsNullOrEmpty() && currentFuzzyValue > 88)
                {
                    _logger.Info($"Found steam id for game {game.Name} via method 2, Id: {currentFuzzyId}, Proximity: {currentFuzzyValue}");
                    return currentFuzzyId;
                }

                // Automatic match method 3: LevenshteinDistance
                foreach (var result in results)
                {
                    var distance = normalizedName.ToLower().GetLevenshteinDistance(result.Name.ToLower());
                    if (distance < currentDistance)
                    {
                        currentDistance = distance;
                        currentLevenshteinId = result.GameId;
                    }
                }

                if (!currentLevenshteinId.IsNullOrEmpty() && currentDistance < 3)
                {
                    _logger.Info($"Found steam id for game {game.Name} via method 3, Id: {currentLevenshteinId}, Distance: {currentDistance}");
                    return currentLevenshteinId;
                }
            }

            if (!isBackgroundDownload)
            {
                var selectedGame = playniteApi.Dialogs.ChooseItemWithSearch(
                    results.Select(x => new GenericItemOption(x.Name, x.GameId)).ToList(),
                    (a) => SteamWeb.GetSteamSearchGenericItemOptions(a),
                    normalizedName,
                    ResourceProvider.GetString("LOCSpecial_K_Helper_DialogMessageSelectSteamGameOption"));
                if (selectedGame != null)
                {
                    return selectedGame.Description;
                }
            }

            // As a last resort, if the search was background download and fuzzy methods were used,
            // return the best Levenshtein result. Better than nothing I guess
            // and will probably be the appropiate result
            if (isBackgroundDownload && !currentLevenshteinId.IsNullOrEmpty())
            {
                _logger.Info($"Found steam id for game {game.Name} via method 4, Id: {currentLevenshteinId}, Distance: {currentDistance}");
                return currentLevenshteinId;
            }

            _logger.Info($"Steam id for game {game.Name} not found");
            return null;
        }
    }
}