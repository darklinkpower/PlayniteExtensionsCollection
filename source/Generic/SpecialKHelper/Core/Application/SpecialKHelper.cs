using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SpecialKHelper.Core.Domain;
using SpecialKHelper.EasyAnticheat.Application;
using SpecialKHelper.EasyAnticheat.Persistence;
using SpecialKHelper.PluginSidebarItem.Application;
using SpecialKHelper.PluginSidebarItem.Presentation;
using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKHandler.Domain.Enums;
using SpecialKHelper.SpecialKHandler.Domain.Exceptions;
using SpecialKHelper.SpecialKProfilesEditorService.Application;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SpecialKHelper
{
    public class SpecialKHelper : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly string _pluginInstallPath;
        private readonly SpecialKServiceManager _specialKServiceManager;
        private const string _globalModeDisableFeatureName = "[SK] Global Mode Disable";
        private const string _selectiveModeEnableFeatureName = "[SK] Selective Mode Enable";
        private readonly SpecialKProfilesEditor _specialKProfilesEditor;
        private readonly SidebarItemSwitcherViewModel _sidebarItemSwitcherViewModel;

        private readonly EasyAnticheatService _easyAnticheatHelper;
        private readonly SteamHelper _steamHelper;

        private SpecialKHelperSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("71349310-9ed8-4bf5-8bf2-e92cdb222748");

        public SpecialKHelper(IPlayniteAPI api) : base(api)
        {
            _specialKServiceManager = new SpecialKServiceManager(_logger);
            settings = new SpecialKHelperSettingsViewModel(this, _specialKServiceManager, _logger);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            _pluginInstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _specialKProfilesEditor = new SpecialKProfilesEditor(_specialKServiceManager, PlayniteApi);
            _sidebarItemSwitcherViewModel = new SidebarItemSwitcherViewModel(true, _pluginInstallPath, _specialKServiceManager, _specialKProfilesEditor);
            _easyAnticheatHelper = new EasyAnticheatService(new EasyAnticheatCache(GetPluginUserDataPath()));
            _steamHelper = new SteamHelper(GetPluginUserDataPath(), PlayniteApi);
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            if (settings.Settings.ShowSidebarItem)
            {
                yield return new SidebarItem
                {
                    Title = ResourceProvider.GetString("LOCSpecial_K_Helper_SidebarTooltip"),
                    Type = SiderbarItemType.Button,
                    Icon = new SidebarItemSwitcherView { DataContext = _sidebarItemSwitcherViewModel },
                    Activated = () => {
                        _sidebarItemSwitcherViewModel.SwitchAllowState();
                    }
                };
            }
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            var game = args.Game;
            var startServices = GetShouldStartService(game);
            if (_steamHelper.IsEnvinronmentVariableSet())
            {
                if (settings.Settings.SteamOverlayForBpm != SteamOverlay.BigPictureMode
                    || Steam.IsGameSteamGame(game)
                    || !SteamClient.GetIsSteamBpmRunning())
                {
                    _steamHelper.RemoveBigPictureModeEnvVariable();
                }
            }
            else if (settings.Settings.SteamOverlayForBpm == SteamOverlay.BigPictureMode && SteamClient.GetIsSteamBpmRunning())
            {
                _steamHelper.SetBigPictureModeEnvVariable();
            }

            if (!startServices)
            {
                StopAllSpecialKServices();
                return;
            }

            var service32Started = false;
            var service64Started = false;
            try
            {
                if (_specialKServiceManager.Service32BitsStatus != SpecialKServiceStatus.Running)
                {
                    service32Started = _specialKServiceManager.Start32BitsService();
                }

                if (_specialKServiceManager.Service64BitsStatus != SpecialKServiceStatus.Running)
                {
                    service64Started = _specialKServiceManager.Start64BitsService();
                }
            }
            catch (SpecialKFileNotFoundException e)
            {
                LogSkFileNotFound(e);
                return;
            }
            catch (SpecialKPathNotFoundException e)
            {
                LogSkPathNotFound(e);
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while starting services OnGameStarting");
                return;
            }

            if (!service32Started && !service64Started)
            {
                _logger.Info("Skipped Special K configuration validation because no services were started.");
                return;
            }

            var skifPath = _specialKServiceManager.GetInstallDirectory();
            SpecialKConfigurationManager.ValidateDefaultProfile(game, skifPath, settings, GetPluginUserDataPath(), PlayniteApi);
            if (settings.Settings.EnableReshadeOnNewProfiles)
            {
                SpecialKConfigurationManager.ValidateReshadeConfiguration(game, skifPath);
            }
        }

        private void LogSkFileNotFound(SpecialKFileNotFoundException e)
        {
            _logger.Error(e, $"Special K file not found");
            PlayniteApi.Notifications.Add(new NotificationMessage(
                Guid.NewGuid().ToString(),
                string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkFileNotFound"), e.Message),
                NotificationType.Error,
                () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper#file-not-found-notification-error")
            ));
        }

        private void LogSkPathNotFound(SpecialKPathNotFoundException e)
        {
            _logger.Error(e, "Special K Path registry key not found");
            PlayniteApi.Notifications.Add(new NotificationMessage(
                "sk_registryNotFound",
                ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkRegistryKeyNotFound"),
                NotificationType.Error,
                () => OpenSettingsView()
            ));
        }

        private bool GetShouldStartService(Game game)
        {
            if (!_sidebarItemSwitcherViewModel.AllowSkUse)
            {
                _logger.Info("Start of services is disabled by sidebar item");
                return false;
            }

            if (settings.Settings.OnlyExecutePcGames && !PlayniteUtilities.IsGamePcGame(game))
            {
                return false;
            }

            if (settings.Settings.StopExecutionIfVac &&
                game.Features?.Any(x => x.Name == "Valve Anti-Cheat Enabled") == true)
            {
                return false;
            }

            var executionMode = settings.Settings.SpecialKExecutionMode;
            if (executionMode == SpecialKExecutionMode.Selective &&
                game.Features?.Any(x => x.Name == _selectiveModeEnableFeatureName) != true)
            {
                return false;
            }

            if (executionMode == SpecialKExecutionMode.Global &&
                game.Features?.Any(x => x.Name == _globalModeDisableFeatureName) == true)
            {
                return false;
            }

            if (settings.Settings.StopIfEasyAntiCheat && _easyAnticheatHelper.IsGameEacEnabled(game))
            {
                _logger.Info($"Start of services disabled due to game {game.Name} using EasyAntiCheat");
                return false;
            }

            return true;
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (_steamHelper.IsEnvinronmentVariableSet())
            {
                _steamHelper.RemoveBigPictureModeEnvVariable();
            }

            StopAllSpecialKServices();
        }

        private void StopAllSpecialKServices()
        {
            try
            {
                if (_specialKServiceManager.Service32BitsStatus == SpecialKServiceStatus.Running)
                {
                    _specialKServiceManager.Stop32BitsService();
                }

                if (_specialKServiceManager.Service64BitsStatus == SpecialKServiceStatus.Running)
                {
                    _specialKServiceManager.Stop64BitsService();
                }
            }
            catch (SpecialKFileNotFoundException e)
            {
                LogSkFileNotFound(e);
            }
            catch (SpecialKPathNotFoundException e)
            {
                LogSkPathNotFound(e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error on StopAllSpecialKServices");
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptionOpenEditor"),
                    MenuSection = "@Special K Helper",
                    Action = (a) => {
                        _specialKProfilesEditor.OpenEditorWindow();
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
                    Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEA30'),
                    Action = o =>
                    {
                        _steamHelper.OpenGameSteamControllerConfig(firstGame);
                    }
                }
            };

            if (settings.Settings.SpecialKExecutionMode == SpecialKExecutionMode.Global)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptiongGlobalModeAddFeature"),
                    MenuSection = $"Special K Helper",
                    Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEFA9'),
                    Action = o =>
                    {
                        PlayniteUtilities.AddFeatureToGames(PlayniteApi, args.Games.Distinct(), _globalModeDisableFeatureName);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSpecial_K_Helper_DoneMessage"));
                    }
                });
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptiongGlobalModeRemoveFeature"),
                    MenuSection = $"Special K Helper",
                    Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEED7'),
                    Action = o => {
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, args.Games.Distinct(), _globalModeDisableFeatureName);
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
                    Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEED7'),
                    Action = o =>
                    {
                        PlayniteUtilities.AddFeatureToGames(PlayniteApi, args.Games.Distinct(), _selectiveModeEnableFeatureName);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSpecial_K_Helper_DoneMessage"));
                    }
                });
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptiongSelectiveModeRemoveFeature"),
                    MenuSection = $"Special K Helper",
                    Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEFA9'),
                    Action = o => {
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, args.Games.Distinct(), _selectiveModeEnableFeatureName);
                        PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSpecial_K_Helper_DoneMessage"));
                    }
                });
            }

            return menuItems;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SpecialKHelperSettingsView();
        }
    }
}