﻿using FuzzySharp;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SpecialKHelper.Models;
using SpecialKHelper.ViewModels;
using SpecialKHelper.Views;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Process = System.Diagnostics.Process;

namespace SpecialKHelper
{
    public class SpecialKHelper : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string emptyReshadePreset;
        private readonly FileIniDataParser iniParser;
        private readonly string pluginInstallPath;
        private bool steamBpmEnvVarSet = false;
        private static readonly Regex reshadeTechniqueRegex = new Regex(@"technique ([^\s]+)", RegexOptions.None);

        private SidebarItemSwitcherViewModel sidebarItemSwitcherViewModel { get; }
        private SpecialKHelperSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("71349310-9ed8-4bf5-8bf2-e92cdb222748");

        public SpecialKHelper(IPlayniteAPI api) : base(api)
        {
            pluginInstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            emptyReshadePreset = Path.Combine(pluginInstallPath, "Resources", "ReshadeDefaultPreset.ini");

            iniParser = new FileIniDataParser();
            iniParser.Parser.Configuration.AssigmentSpacer = string.Empty;
            iniParser.Parser.Configuration.AllowDuplicateKeys = true;
            iniParser.Parser.Configuration.OverrideDuplicateKeys = true;
            iniParser.Parser.Configuration.AllowDuplicateSections = true;

            settings = new SpecialKHelperSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            sidebarItemSwitcherViewModel = new SidebarItemSwitcherViewModel(true, pluginInstallPath);

            AddTextIcoFontResource("skHelperControllerIcon", "\xEA30");
            AddTextIcoFontResource("skHelperNotAllowedIcon", "\xEFA9");
            AddTextIcoFontResource("skHelperCheckCircledIcon", "\xEED7");
        }

        private void AddTextIcoFontResource(string key, string text)
        {
            Application.Current.Resources.Add(key, new TextBlock
            {
                Text = text,
                FontSize = 20,
                FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
            });
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            if (settings.Settings.ShowSidebarItem)
            {
                yield return new SidebarItem
                {
                    Title = ResourceProvider.GetString("LOCSpecial_K_Helper_SidebarTooltip"),
                    Type = SiderbarItemType.Button,
                    Icon = new SidebarItemSwitcherView { DataContext = sidebarItemSwitcherViewModel },
                    Activated = () => {
                        sidebarItemSwitcherViewModel.SwitchAllowState();
                    }
                };
            }
        }

        private string GetSpecialKPath()
        {
            if (!settings.Settings.CustomSpecialKPath.IsNullOrEmpty())
            {
                if (FileSystem.FileExists(settings.Settings.CustomSpecialKPath))
                {
                    return Path.GetDirectoryName(settings.Settings.CustomSpecialKPath);
                }
                else
                {
                    logger.Warn($"Special K Registry Directory not found in {settings.Settings.CustomSpecialKPath}");
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        "sk_customExeNotFound",
                        string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkCustomExecutablePathNotFound"), settings.Settings.CustomSpecialKPath),
                        NotificationType.Error,
                        () => OpenSettingsView()
                    ));
                }
            }
            
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Kaldaien\Special K"))
            {
                if (key == null)
                {
                    logger.Debug("Special K Registry subkey not found");
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        "sk_registryNotFound",
                        ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkRegistryKeyNotFound"),
                        NotificationType.Error,
                        () => OpenSettingsView()
                    ));

                    return null;
                }

                var pathValue = key.GetValue("Path");
                if (pathValue == null)
                {
                    logger.Debug("Special K Path registry key not found");
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        "sk_registryNotFound",
                        ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkRegistryKeyNotFound"),
                        NotificationType.Error,
                        () => OpenSettingsView()
                    ));

                    return null;
                }

                var directory = pathValue.ToString();
                if (FileSystem.DirectoryExists(directory))
                {
                    return directory;
                }
                else
                {
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        "sk_directoryNotFound",
                        string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkDirectoryNotFound"), directory),
                        NotificationType.Error,
                        () => OpenSettingsView()
                    ));

                    return null;
                }
            }
        }

        public string GetReshadeTechniqueSorting(string reshadeBase)
        {
            var shadersDirectory = Path.Combine(reshadeBase, "reshade-shaders", "Shaders");
            if (!Directory.Exists(shadersDirectory))
            {
                logger.Warn($"Reshade Shaders directory not found in {shadersDirectory}");
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

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            var game = args.Game;
            var startServices = GetShouldStartService(game);

            if (steamBpmEnvVarSet)
            {
                if (settings.Settings.SteamOverlayForBpm != SteamOverlay.BigPictureMode
                    || Steam.IsGameSteamGame(game)
                    || !SteamClient.GetIsSteamBpmRunning())
                {
                    RemoveBpmEnvVariable();
                }
            }
            else if (settings.Settings.SteamOverlayForBpm == SteamOverlay.BigPictureMode && SteamClient.GetIsSteamBpmRunning())
            {
                SetBpmEnvVariable();
            }

            var startSuccess32 = false;
            var startSuccess64 = false;
            var skifPath = GetSpecialKPath();
            if (skifPath.IsNullOrEmpty())
            {
                return;
            }

            if (startServices)
            {
                startSuccess32 = StartSpecialkService(skifPath, "32");
                startSuccess64 = StartSpecialkService(skifPath, "64");
            }
            else
            {
                //Check if leftover service is running and close it
                StopSpecialkService(skifPath, "32");
                StopSpecialkService(skifPath, "64");
            }

            if (!startServices || !startSuccess32 || !startSuccess64)
            {
                logger.Info("Execution stopped due to services not started");
                return;
            }

            ValidateDefaultProfile(game, skifPath);
            ValidateReshadeConfiguration(game, skifPath);
        }

        private void RemoveBpmEnvVariable()
        {
            var variable = Environment.GetEnvironmentVariable("SteamTenfoot", EnvironmentVariableTarget.Process);
            if (!variable.IsNullOrEmpty())
            {
                Environment.SetEnvironmentVariable("SteamTenfoot", string.Empty, EnvironmentVariableTarget.Process);
            }

            steamBpmEnvVarSet = false;
        }

        private void SetBpmEnvVariable()
        {
            // Setting "SteamTenfoot" to "1" forces to use the Steam BPM overlay
            // but it will still not work if Steam BPM is not running
            var variable = Environment.GetEnvironmentVariable("SteamTenfoot", EnvironmentVariableTarget.Process);
            if (variable.IsNullOrEmpty() || variable != "1")
            {
                Environment.SetEnvironmentVariable("SteamTenfoot", "1", EnvironmentVariableTarget.Process);
            }

            steamBpmEnvVarSet = true;
        }

        private void ValidateReshadeConfiguration(Game game, string skifPath)
        {
            var reshadeBase = Path.Combine(skifPath, @"PlugIns\ThirdParty\ReShade");
            var reshadeIniPath = Path.Combine(reshadeBase, "ReShade.ini");

            if (!Directory.Exists(reshadeBase))
            {
                logger.Warn($"Reshade directory not found in {reshadeBase}");
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
                    FileSystem.CopyFile(emptyReshadePreset, gameReshadePreset, true);
                }
            }

            if (FileSystem.FileExists(reshadeIniPath))
            {
                IniData ini = iniParser.ReadFile(reshadeIniPath);
                var updatedValues = 0;
                updatedValues += ValidateIniValue(ini, "GENERAL", "PresetPath", ".\\" + gameReshadePresetSubPath);
                updatedValues += ValidateIniValue(ini, "APP", "ForceVSync", "0");
                updatedValues += ValidateIniValue(ini, "APP", "ForceWindowed", "0");
                updatedValues += ValidateIniValue(ini, "APP", "ForceFullscreen", "0");
                updatedValues += ValidateIniValue(ini, "APP", "ForceResolution", "0,0");
                updatedValues += ValidateIniValue(ini, "APP", "Force10BitFormat", "0");

                if (updatedValues > 0)
                {
                    iniParser.WriteFile(reshadeIniPath, ini, Encoding.UTF8);
                }
            }
        }

        private void ValidateDefaultProfile(Game game, string skifPath)
        {
            var defaultConfigPath = Path.Combine(skifPath, "Global", "default_SpecialK.ini");
            if (!FileSystem.FileExists(defaultConfigPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(defaultConfigPath));
                FileSystem.CreateFile(defaultConfigPath);
                logger.Info($"Created default profile file blank file in {defaultConfigPath} since it was missing");
            }

            IniData ini = iniParser.ReadFile(defaultConfigPath);
            var updatedValues = 0;
            if (settings.Settings.EnableStOverlayOnNewProfiles)
            {
                updatedValues += ValidateIniValue(ini, "Steam.System", "PreLoadSteamOverlay", "true");
                var steamId = ConfigureSteamApiInject(game);
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
                iniParser.WriteFile(defaultConfigPath, ini, Encoding.UTF8);
                logger.Info($"Default ini validated and updated {updatedValues} new values");
            }
        }

        private int ValidateIniValue(IniData ini, string key, string subKey, string newValue)
        {
            var currentValue = ini[key][subKey];
            if (currentValue == null || currentValue != newValue)
            {
                ini[key][subKey] = newValue;
                logger.Info($"Default ini validated and updated value in [{key}]{subKey} to \"{newValue}\"");
                return 1;
            }

            return 0;
        }

        private bool GetIsGameEacEnabled(Game game)
        {
            var cachePath = Path.Combine(GetPluginUserDataPath(), game.Id.ToString() + "_cache.json");
            if (!FileSystem.FileExists(cachePath))
            {
                var newCache = new GameDataCache
                {
                    Id = game.Id,
                    EasyAnticheatStatus = GetGameEasyAnticheatStatus(game)
                };

                FileSystem.WriteStringToFile(cachePath, Serialization.ToJson(newCache));
            }

            var cache = Serialization.FromJsonFile<GameDataCache>(cachePath);
            if (cache.EasyAnticheatStatus == EasyAnticheatStatus.Detected)
            {
                return true;
            }

            return false;
        }

        private bool GetShouldStartService(Game game)
        {
            if (!sidebarItemSwitcherViewModel.AllowSkUse)
            {
                logger.Info("Start of services is disabled by sidebar item");
                return false;
            }

            if (settings.Settings.StopIfEasyAntiCheat && GetIsGameEacEnabled(game))
            {
                logger.Info($"Start of services disabled due to game {game.Name} using EasyAntiCheat");
                return false;
            }

            if (game.Features != null)
            {
                if (settings.Settings.StopExecutionIfVac && game.Features.Any(x => x.Name == "Valve Anti-Cheat Enabled"))
                {
                    return false;
                }

                if (settings.Settings.SpecialKExecutionMode == SpecialKExecutionMode.Global)
                {
                    if (game.Features.Any(x => x.Name == "[SK] Global Mode Disable"))
                    {
                        return false;
                    }
                }
                else if (settings.Settings.SpecialKExecutionMode == SpecialKExecutionMode.Selective)
                {
                    if (!game.Features.Any(x => x.Name == "[SK] Selective Mode Enable"))
                    {
                        return false;
                    }
                }
            }

            if (settings.Settings.OnlyExecutePcGames && !PlayniteUtilities.IsGamePcGame(game))
            {
                return false;
            }

            return true;
        }

        private EasyAnticheatStatus GetGameEasyAnticheatStatus(Game game)
        {
            if (!PlayniteUtilities.IsGamePcGame(game))
            {
                return EasyAnticheatStatus.NotDetected;
            }

            if (!PlayniteUtilities.GetIsInstallDirectoryValid(game))
            {
                return EasyAnticheatStatus.Unknown;
            }

            try
            {
                var eacFile = Directory
                     .EnumerateFiles(game.InstallDirectory, "EasyAntiCheat*", SearchOption.AllDirectories)
                     .FirstOrDefault();
                if (eacFile != null)
                {
                    logger.Info($"EasyAntiCheat file {eacFile} detected for {game.Name}");
                    return EasyAnticheatStatus.Detected;
                }
                else
                {
                    logger.Info($"EasyAntiCheat not detected for {game.Name}");
                    return EasyAnticheatStatus.NotDetected;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during EAC enumeration for {game.Name} with dir {game.InstallDirectory}");
                return EasyAnticheatStatus.ErrorOnDetection;
            }
        }

        private string ConfigureSteamApiInject(Game game)
        {
            if (Steam.IsGameSteamGame(game))
            {
                return null;
            }

            var appIdTextPath = string.Empty;
            var isInstallDirValid = PlayniteUtilities.GetIsInstallDirectoryValid(game);
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
            var historyFlagFile = Path.Combine(GetPluginUserDataPath(), "SteamId_" + game.Id.ToString());
            if (FileSystem.FileExists(historyFlagFile))
            {
                previousId = FileSystem.ReadStringFromFile(historyFlagFile).Trim();
                logger.Info($"Detected attempt flag file for game {game.Name} in {historyFlagFile}. Previous Id: {previousId}");
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
                if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
                {
                    isBackgroundDownload = true;
                }

                var steamIdSearch = GetSteamIdFromSearch(game, isBackgroundDownload, true);
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
                    logger.Error(e, $"Error while creating steam id file in {appIdTextPath}");
                }
            }

            // Flag file so we don't attempt to search again in future startups.
            if (!FileSystem.FileExists(historyFlagFile))
            {
                FileSystem.WriteStringToFile(historyFlagFile, steamId, true);
            }
            
            return steamId;
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (steamBpmEnvVarSet)
            {
                RemoveBpmEnvVariable();
            }
            var skifPath = GetSpecialKPath();
            if (skifPath.IsNullOrEmpty())
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "sk_registryNotFound",
                    ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkRegistryKeyNotFound"),
                    NotificationType.Error
                ));
                return;
            }

            var cpuArchitectures = new string[] { "32", "64" };
            foreach (var cpuArchitecture in cpuArchitectures)
            {
                StopSpecialkService(skifPath, cpuArchitecture);
            }
        }

        private bool StartSpecialkService(string skifPath, string cpuArchitecture)
        {
            var servletPid = Path.Combine(skifPath, "Servlet", "SpecialK" + cpuArchitecture + ".pid");
            if (FileSystem.FileExists(servletPid))
            {
                logger.Info($"Servlet Pid file in {servletPid} detected so it was not started");
                return true;
            }

            var allFilesDetected = true;
            var dllPath = Path.Combine(skifPath, "SpecialK" + cpuArchitecture + ".dll");
            if (!FileSystem.FileExists(dllPath))
            {
                allFilesDetected = false;
                logger.Info($"Special K dll not found in {dllPath}");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "sk_dll_notfound" + cpuArchitecture,
                    string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkFileNotFound"), dllPath),
                    NotificationType.Error,
                    () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper#file-not-found-notification-error")
                ));
            }

            var servletExe = Path.Combine(skifPath, "Servlet", "SKIFsvc" + cpuArchitecture +".exe");
            if (!FileSystem.FileExists(servletExe))
            {
                allFilesDetected = false;
                logger.Info($"Special K servlet exe not found in {servletExe}");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "sk_servletExe_notfound" + cpuArchitecture,
                    string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkFileNotFound"), servletExe),
                    NotificationType.Error,
                    () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper#file-not-found-notification-error")
                ));
            }

            if (!allFilesDetected)
            {
                return false;
            }

            var info = new ProcessStartInfo(servletExe)
            {
                WorkingDirectory = Path.GetDirectoryName(servletExe),
                UseShellExecute = true,
                Arguments = "Start",
            };
            Process.Start(info);

            var i = 0;
            while (i < 12)
            {
                Thread.Sleep(100);
                if (FileSystem.FileExists(servletPid))
                {
                    logger.Info($"Special K global service for \"{cpuArchitecture}\" started. Pid file detected in {servletPid}");
                    return true;
                }
                i++;
            }

            logger.Info($"Special K global service for \"{cpuArchitecture}\" could not be opened");
            PlayniteApi.Notifications.Add(new NotificationMessage(
                "SkNotStarted" + cpuArchitecture,
                string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkErrorOnStart"), cpuArchitecture),
                NotificationType.Error,
                () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper#special-k-service-could-not-be-started-notification-error")
            ));
            return false;
        }

        private bool StopSpecialkService(string skifPath, string cpuArchitecture)
        {
            var servletPid = Path.Combine(skifPath, "Servlet", "SpecialK" + cpuArchitecture + ".pid");
            if (!FileSystem.FileExists(servletPid))
            {
                logger.Info($"Servlet Pid file in {servletPid} not detected so closing was not needed");
                return true;
            }

            var allFilesDetected = true;
            var dllPath = Path.Combine(skifPath, "SpecialK" + cpuArchitecture + ".dll");
            if (!FileSystem.FileExists(dllPath))
            {
                allFilesDetected = false;
                logger.Info($"Special K dll not found in {dllPath}");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "sk_dll_notfound" + cpuArchitecture,
                    string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkFileNotFound"), dllPath),
                    NotificationType.Error,
                    () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper#file-not-found-notification-error")
                ));
            }

            var servletExe = Path.Combine(skifPath, "Servlet", "SKIFsvc" + cpuArchitecture + ".exe");
            if (!FileSystem.FileExists(servletExe))
            {
                allFilesDetected = false;
                logger.Info($"Special K servlet exe not found in {servletExe}");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "sk_servletExe_notfound" + cpuArchitecture,
                    string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkFileNotFound"), servletExe),
                    NotificationType.Error,
                    () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper#file-not-found-notification-error")
                ));
            }

            if (!allFilesDetected)
            {
                return false;
            }

            var info = new ProcessStartInfo(servletExe)
            {
                WorkingDirectory = Path.GetDirectoryName(servletExe),
                UseShellExecute = true,
                Arguments = "Stop",
            };

            Process.Start(info);
            return true;
        }

        public string GetSteamIdFromSearch(Game game, bool isBackgroundDownload, bool matchFuzzyMethods = false)
        {
            var normalizedName = game.Name.NormalizeGameName();
            var results = SteamWeb.GetSteamSearchResults(normalizedName);
            results.ForEach(a => a.Name = a.Name.NormalizeGameName());

            // Try to see if there's an exact match, to not prompt the user unless needed
            var matchingGameName = normalizedName.GetMatchModifiedName();
            var exactMatch = results.FirstOrDefault(x => x.Name.GetMatchModifiedName() == matchingGameName);

            // Automatic match method 1: Remove all symbols
            if (exactMatch != null)
            {
                logger.Info($"Found steam id for game {game.Name} via method 1, Id: {exactMatch.GameId}, Match: {exactMatch.Name}");
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
                    logger.Info($"Found steam id for game {game.Name} via method 2, Id: {currentFuzzyId}, Proximity: {currentFuzzyValue}");
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
                    logger.Info($"Found steam id for game {game.Name} via method 3, Id: {currentLevenshteinId}, Distance: {currentDistance}");
                    return currentLevenshteinId;
                }
            }

            if (!isBackgroundDownload)
            {
                var selectedGame = PlayniteApi.Dialogs.ChooseItemWithSearch(
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
                logger.Info($"Found steam id for game {game.Name} via method 4, Id: {currentLevenshteinId}, Distance: {currentDistance}");
                return currentLevenshteinId;
            }

            logger.Info($"Steam id for game {game.Name} not found");
            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        private void OpenEditorWindow(string searchTerm = null)
        {
            var skifPath = GetSpecialKPath();
            if (skifPath.IsNullOrEmpty())
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "sk_registryNotFound",
                    ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkRegistryKeyNotFound"),
                    NotificationType.Error
                ));
                return;
            }

            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 700;
            window.Width = 900;
            window.Title = ResourceProvider.GetString("LOCSpecial_K_Helper_WindowTitleSkProfileEditor");

            window.Content = new SpecialKProfileEditorView();
            window.DataContext = new SpecialKProfileEditorViewModel(PlayniteApi, iniParser, skifPath, searchTerm);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptionOpenEditor"),
                    MenuSection = "@Special K Helper",
                    Action = o => {
                        OpenEditorWindow();
                    }
                },
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var firstGame = args.Games.Last();
            var menuItems = new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptionOpenSteamControllerConfig"), firstGame.Name),
                    MenuSection = $"Special K Helper",
                    Icon = "skHelperControllerIcon",
                    Action = o =>
                    {
                        OpenGameSteamControllerConfig(firstGame);
                    }
                }
            };

            if (settings.Settings.SpecialKExecutionMode == SpecialKExecutionMode.Global)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptiongGlobalModeAddFeature"),
                    MenuSection = $"Special K Helper",
                    Icon = "skHelperNotAllowedIcon",
                    Action = o =>
                    {
                        PlayniteUtilities.AddFeatureToGames(PlayniteApi, args.Games.Distinct(), "[SK] Global Mode Disable");
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSpecial_K_Helper_DoneMessage"));
                    }
                });
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptiongGlobalModeRemoveFeature"),
                    MenuSection = $"Special K Helper",
                    Icon = "skHelperCheckCircledIcon",
                    Action = o => {
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, args.Games.Distinct(), "[SK] Global Mode Disable");
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSpecial_K_Helper_DoneMessage"));
                    }
                });
            }
            else if (settings.Settings.SpecialKExecutionMode == SpecialKExecutionMode.Selective)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptiongSelectiveModeAddFeature"),
                    MenuSection = $"Special K Helper",
                    Icon = "skHelperCheckCircledIcon",
                    Action = o =>
                    {
                        PlayniteUtilities.AddFeatureToGames(PlayniteApi, args.Games.Distinct(), "[SK] Selective Mode Enable");
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSpecial_K_Helper_DoneMessage"));
                    }
                });
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptiongSelectiveModeRemoveFeature"),
                    MenuSection = $"Special K Helper",
                    Icon = "skHelperNotAllowedIcon",
                    Action = o => {
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, args.Games.Distinct(), "[SK] Selective Mode Enable");
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSpecial_K_Helper_DoneMessage"));
                    }
                });
            }

            return menuItems;
        }

        private string GetConfiguredSteamId(Game game)
        {
            if (Steam.IsGameSteamGame(game))
            {
                return game.GameId;
            }

            if (!game.InstallDirectory.IsNullOrEmpty())
            {
                var appIdTextPath = Path.Combine(game.InstallDirectory, "steam_appid.txt");
                if (FileSystem.FileExists(appIdTextPath))
                {
                    return FileSystem.ReadStringFromFile(appIdTextPath);
                }
            }

            var historyFlagFile = Path.Combine(GetPluginUserDataPath(), "SteamId_" + game.Id.ToString());
            if (FileSystem.FileExists(historyFlagFile))
            {
                return FileSystem.ReadStringFromFile(historyFlagFile);
            }

            return string.Empty;
        }

        private void OpenGameSteamControllerConfig(Game game)
        {
            var steamId = GetConfiguredSteamId(game);
            if (steamId.IsNullOrEmpty())
            {
                PlayniteApi.Dialogs.ShowErrorMessage(
                    string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_DialogMessageSteamControlIdNotFound"), game.Name),
                    "Special K Helper");
                return;
            }

            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_DialogMessageSteamControlNotice"), game.Name, steamId),
                "Special K Helper");

            ProcessStarter.StartUrl($"steam://currentcontrollerconfig/{steamId}");
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SpecialKHelperSettingsView();
        }
    }
}