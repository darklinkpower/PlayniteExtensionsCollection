using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using SpecialKHelper.Core.Application;
using SpecialKHelper.Core.Domain;
using SpecialKHelper.EasyAnticheat.Application;
using SpecialKHelper.EasyAnticheat.Persistence;
using SpecialKHelper.PluginSidebarItem.Application;
using SpecialKHelper.PluginSidebarItem.Presentation;
using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKHandler.Domain.Enums;
using SpecialKHelper.SpecialKHandler.Domain.Exceptions;
using SpecialKHelper.SpecialKProfilesEditorService.Application;
using SpecialKHelper.SpecialKUpdater.Application;
using SpecialKHelper.SpecialKUpdater.Infrastructure;
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
        private readonly SteamEnvironmentHandler _steamEnvironmentHandler;
        private readonly SpecialKSignalWatcher _signalWatcher;
        private readonly SpecialKGameSessionCoordinator _gameCoordinator;
        private readonly SpecialKUpdateMonitor _specialKUpdateMonitor;

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
            _specialKProfilesEditor = new SpecialKProfilesEditor(
                _specialKServiceManager,
                PlayniteApi);

            _sidebarItemSwitcherViewModel = new SidebarItemSwitcherViewModel(
                true,
                _pluginInstallPath,
                _specialKServiceManager,
                _specialKProfilesEditor);

            _easyAnticheatHelper = new EasyAnticheatService(
                new EasyAnticheatCache(GetPluginUserDataPath()));

            var steamHelper = new SteamHelper(
                GetPluginUserDataPath(),
                PlayniteApi);

            _steamEnvironmentHandler = new SteamEnvironmentHandler(
                settings,
                steamHelper);

            _signalWatcher = new SpecialKSignalWatcher(_logger);
            
            _gameCoordinator = new SpecialKGameSessionCoordinator(
                _specialKServiceManager,
                _signalWatcher,
                _logger,
                PlayniteApi,
                () => this.OpenSettingsView(),
                () => settings.Settings.SpecialKServiceStopMode);

            _specialKUpdateMonitor = new SpecialKUpdateMonitor(
                _logger,
                PlayniteApi,
                new SpecialKUpdateService(_logger, new SpecialKRepositoryClient()),
                _specialKServiceManager,
                new Sha256Validator(),
                settings);
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

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            _signalWatcher.Start();
            _specialKUpdateMonitor.Start();
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            var game = args.Game;

            _steamEnvironmentHandler.OnGameStarting(game);
            if (!GetShouldStartService(game))
            {
                return;
            }

            try
            {
                var session = _gameCoordinator.Start(game);
                if (session is null)
                {
                    _logger.Info(
                        "Skipped configuration validation because services were not started.");
                    return;
                }

                if (!session.Started32BitService &&
                    !session.Started64BitService)
                {
                    _logger.Info(
                        "Skipped configuration validation because services already existed.");

                    return;
                }

                var skifPath = _specialKServiceManager.GetInstallDirectory();

                SpecialKConfigurationManager.ValidateDefaultProfile(
                    game,
                    skifPath,
                    settings,
                    GetPluginUserDataPath(),
                    PlayniteApi);

                if (settings.Settings.EnableReshadeOnNewProfiles)
                {
                    SpecialKConfigurationManager.ValidateReshadeConfiguration(
                        game,
                        skifPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error on game starting.");
            }
        }

        public override void OnGameStartupCancelled(OnGameStartupCancelledEventArgs args)
        {
            _gameCoordinator.RemoveSession(args.Game);
        }
        
        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            _steamEnvironmentHandler.OnGameStopped();
            _gameCoordinator.RemoveSession(args.Game);
        }

        private bool GetShouldStartService(Game game)
        {
            if (!_sidebarItemSwitcherViewModel.AllowSkUse)
            {
                _logger.Info($"Special K services start skipped: disabled by sidebar item.");
                return false;
            }

            if (settings.Settings.OnlyExecutePcGames && !PlayniteUtilities.IsGamePcGame(game))
            {
                _logger.Info($"Special K services start skipped: game '{game.Name}' is not a PC game.");
                return false;
            }

            if (settings.Settings.StopExecutionIfVac &&
                game.Features?.Any(x => x.Name.Equals("Valve Anti-Cheat Enabled", StringComparison.OrdinalIgnoreCase)) == true)
            {
                _logger.Info($"Special K services start skipped: game '{game.Name}' has VAC enabled.");
                return false;
            }

            var executionMode = settings.Settings.SpecialKExecutionMode;
            if (executionMode == SpecialKExecutionMode.Selective &&
                game.Features?.Any(x => x.Name.Equals(_selectiveModeEnableFeatureName, StringComparison.OrdinalIgnoreCase)) != true)
            {
                _logger.Info($"Special K services start skipped: game '{game.Name}' not opted-in for selective execution mode.");
                return false;
            }

            if (executionMode == SpecialKExecutionMode.Global &&
                game.Features?.Any(x => x.Name.Equals(_globalModeDisableFeatureName, StringComparison.OrdinalIgnoreCase)) == true)
            {
                _logger.Info($"Special K services start skipped: game '{game.Name}' opted out of global execution mode.");
                return false;
            }

            if (settings.Settings.StopIfEasyAntiCheat && _easyAnticheatHelper.IsGameEacEnabled(game))
            {
                _logger.Info($"Special K services start skipped: game '{game.Name}' uses EasyAntiCheat.");
                return false;
            }

            _logger.Info($"Special K services will start for game '{game.Name}'.");
            return true;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        private MainMenuItem CreateItem(string resourceKey, Action<MainMenuItemActionArgs> action)
        {
            return new MainMenuItem
            {
                Description = ResourceProvider.GetString(resourceKey),
                MenuSection = "@Special K Helper",
                Action = action
            };
        }

        private MainMenuItem CreateItem(string resourceKey, Action action)
        {
            return CreateItem(resourceKey, _ => action());
        }

        private MainMenuItem Separator() => new MainMenuItem
        {
            Description = "-",
            MenuSection = "@Special K Helper"
        };

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            var items = new List<MainMenuItem>
            {
                CreateItem("LOCSpecial_K_Helper_OpenSpecialK", _specialKServiceManager.OpenSpecialK),
                CreateItem("LOCSpecial_K_Helper_MenuItemDescriptionOpenEditor", () => _specialKProfilesEditor.OpenEditorWindow()),
                Separator(),
                CreateItem("LOCSpecial_K_Helper_MenuItemDescriptionOpenInstallationDirectory", _specialKServiceManager.OpenSpecialKInstallationDirectory),
                Separator()
            };

            var service32Running = _specialKServiceManager.Service32BitsStatus == SpecialKServiceStatus.Running;
            var service64Running = _specialKServiceManager.Service64BitsStatus == SpecialKServiceStatus.Running;

            if (service32Running && service64Running)
            {
                items.Add(CreateItem("LOCSpecial_K_Helper_StopAllServices", () => _specialKServiceManager.StopAllServices()));
                items.Add(Separator());
            }
            else if (!service32Running && !service64Running)
            {
                items.Add(CreateItem("LOCSpecial_K_Helper_StartAllServices", () => _specialKServiceManager.StartAllServices()));
                items.Add(Separator());
            }

            if (service32Running)
            {
                items.Add(CreateItem(
                    "LOCSpecial_K_Helper_Stop32BitsService",
                    () => _specialKServiceManager.Stop32BitsService()));
            }
            else
            {
                items.Add(CreateItem(
                    "LOCSpecial_K_Helper_Start32BitsService",
                    () => _specialKServiceManager.Start32BitsService()));
            }

            if (service64Running)
            {
                items.Add(CreateItem(
                    "LOCSpecial_K_Helper_Stop64BitsService",
                    () => _specialKServiceManager.Stop64BitsService()));
            }
            else
            {
                items.Add(CreateItem(
                    "LOCSpecial_K_Helper_Start64BitsService",
                    () => _specialKServiceManager.Start64BitsService()));
            }

            return items;
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