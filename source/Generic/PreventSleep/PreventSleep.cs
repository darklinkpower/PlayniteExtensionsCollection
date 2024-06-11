using EventsCommon;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon.Native;
using PreventSleep.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PreventSleep
{
    public class PreventSleep : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly EventAggregator _eventAggregator;
        private readonly PreventSleepSettingsViewModel _settings;
        private readonly TopPanelItem _topPanelModeSwitcher;

        public override Guid Id { get; } = Guid.Parse("d1ccef15-c9aa-4df4-9041-14a2abcb2f13");
        private const string _preventSleepFeatureName = "[PS] Prevent Sleep";
        private bool _isPreventSleepModeSet = false;
        private readonly HashSet<Guid> _gameIdsPreventingSleep = new HashSet<Guid>();

        public PreventSleep(IPlayniteAPI api) : base(api)
        {
            _eventAggregator = new EventAggregator();
            _eventAggregator.Subscribe<OnSettingsChangedEvent>(OnSettingsChanged);
            _settings = new PreventSleepSettingsViewModel(this, _eventAggregator);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
            
            _topPanelModeSwitcher = new TopPanelItem
            {
                Icon = new TextBlock
                {
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily,
                    FontSize = 18
                },
                Visible = _settings.Settings.ShowSwitchModeItemOnTopPanel,
                Activated = () =>
                {
                    if (_isPreventSleepModeSet)
                    {
                        SetAllowSleepState();
                    }
                    else
                    {
                        SetPreventSleepState();
                    }

                    UpdateTopPanelProperties();
                }
            };

            UpdateTopPanelProperties();
        }
        
        public override IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            return new List<TopPanelItem>
            {
                _topPanelModeSwitcher
            };
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            var game = args.Game;
            var gameHasPreventSleepFeature = PlayniteUtilities.GetGameHasFeature(game, _preventSleepFeatureName);
            if (gameHasPreventSleepFeature)
            {
                _gameIdsPreventingSleep.Add(game.Id);
            }

            var shouldPreventSleep = !_isPreventSleepModeSet &&
                (_settings.Settings.AlwaysPreventSleepWhenPlayingGames || gameHasPreventSleepFeature);
            if (shouldPreventSleep)
            {
                SetPreventSleepState();
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            var game = args.Game;
            if (_gameIdsPreventingSleep.Contains(game.Id))
            {
                _gameIdsPreventingSleep.Remove(game.Id);
                _logger.Debug($"Game {game.Name} Id {game.Id} removed from {nameof(_gameIdsPreventingSleep)}. Remaining: {_gameIdsPreventingSleep.Count}");
            }

            var shouldRestoreAllowSleep = _isPreventSleepModeSet &&
                _gameIdsPreventingSleep.Count == 0 &&
                (!_settings.Settings.AlwaysPreventSleepWhenPlayingGames ||
                 !PlayniteApi.Database.Games.Any(x => x.IsRunning));
            if (shouldRestoreAllowSleep)
            {
                SetAllowSleepState();
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PreventSleepSettingsView();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            const string menuSection = "Prevent Sleep";
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOC_PreventSleep_SetGamesPreventSleepPlaying"),
                    MenuSection = menuSection,
                    Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEE81'),
                    Action = a =>
                    {
                        PlayniteUtilities.AddFeatureToGames(PlayniteApi, args.Games.Distinct(), _preventSleepFeatureName);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOC_PreventSleep_RemoveConfiguration"),
                    MenuSection = menuSection,
                    Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEEE1'),
                    Action = a =>
                    {
                        PlayniteUtilities.RemoveFeatureFromGames(PlayniteApi, args.Games.Distinct(), _preventSleepFeatureName);
                    }
                }
            };
        }

        private void SetPreventSleepState()
        {
            Kernel32.SetThreadExecutionState(
                EXECUTION_STATE.ES_CONTINUOUS |
                EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                EXECUTION_STATE.ES_SYSTEM_REQUIRED);
            _isPreventSleepModeSet = true;
            _logger.Debug($"{nameof(SetPreventSleepState)} executed");
            UpdateTopPanelProperties();
        }

        private void SetAllowSleepState()
        {
            Kernel32.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            _isPreventSleepModeSet = false;
            _logger.Debug($"{nameof(SetAllowSleepState)} executed");
            UpdateTopPanelProperties();
        }

        private void OnSettingsChanged(OnSettingsChangedEvent args)
        {
            if (_settings.Settings.ShowSwitchModeItemOnTopPanel)
            {
                _topPanelModeSwitcher.Visible = true;
            }
            else
            {
                _topPanelModeSwitcher.Visible = false;
            }

            if (!_isPreventSleepModeSet &&
                _settings.Settings.AlwaysPreventSleepWhenPlayingGames &&
                PlayniteApi.Database.Games.Any(x => x.IsRunning))
            {
                SetPreventSleepState();
            }
        }

        private void UpdateTopPanelProperties()
        {
            if (_isPreventSleepModeSet)
            {
                if (_topPanelModeSwitcher.Icon is TextBlock textblock)
                {
                    textblock.Text = "\xee81";
                }

                _topPanelModeSwitcher.Title = "Prevent Sleep: " +
                    Environment.NewLine +
                    ResourceProvider.GetString("LOC_PreventSleep_PreventingSystemScreenSleep");
            }
            else
            {
                if (_topPanelModeSwitcher.Icon is TextBlock textblock)
                {
                    textblock.Text = "\xef9e";
                }

                _topPanelModeSwitcher.Title = "Prevent Sleep: " +
                    Environment.NewLine +
                    ResourceProvider.GetString("LOC_PreventSleep_SystemScreenSleepAllowed");
            }
        }


    }
}