using GOGSecondClassGameWatcher.Application;
using GOGSecondClassGameWatcher.Domain.ValueObjects;
using GOGSecondClassGameWatcher.Infrastructure;
using GOGSecondClassGameWatcher.Presentation;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GOGSecondClassGameWatcher
{
    public class GOGSecondClassGameWatcher : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        public GOGSecondClassGameWatcherSettingsViewModel Settings { get; private set; }
        private readonly GogSecondClassService _gogSecondClassWatcherService;
        private readonly GogSecondClassGameWindowCreator _gogSecondClassGameWindowCreator;
        private const string _gameWithIssuesTagName = "[GOG] Game with issues";
        private const string _pluginElementsSourceName = "GogSecondClassWatcher";
        private const string _themesControlName = "SecondClassWatcherControl";

        public override Guid Id { get; } = Guid.Parse("2661ddac-946a-4fee-ba80-3ece762cb64b");

        public GOGSecondClassGameWatcher(IPlayniteAPI api) : base(api)
        {
            Settings = new GOGSecondClassGameWatcherSettingsViewModel(this);
            _gogSecondClassWatcherService = new GogSecondClassService(
                _logger,
                new CsvOnlineSource(),
                new GogSecondClassPersistence(GetPluginUserDataPath(), _logger),
                TimeSpan.FromHours(6));
            _gogSecondClassGameWindowCreator = new GogSecondClassGameWindowCreator(PlayniteApi);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = _pluginElementsSourceName,
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = _pluginElementsSourceName,
                ElementList = new List<string> { _themesControlName }
            });
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == _themesControlName)
            {
                return new PlayniteThemesUserControl(PlayniteApi,_gogSecondClassWatcherService, Settings, _gogSecondClassGameWindowCreator);
            }

            return null;
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            _gogSecondClassWatcherService.EnableBackgroundServiceTracker();
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            var game = args.Game;
            if (Settings.Settings.NotifyOnGameInstalling && GogLibraryUtilities.IsGogGame(game))
            {
                var data = _gogSecondClassWatcherService.GetDataForGame(game);
                if (data != null && GogLibraryUtilities.ShouldWatcherNotify(Settings.Settings, data))
                {
                    _gogSecondClassGameWindowCreator.OpenWindow(data, game);
                }
            }

            return base.GetInstallActions(args);
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (!Settings.Settings.AddStatusTagOnLibraryUpdate)
            {
                return;
            }
            
            using (PlayniteApi.Database.BufferedUpdate())
            {
                var issuesTag = PlayniteApi.Database.Tags.Add(_gameWithIssuesTagName);
                foreach (var game in PlayniteApi.Database.Games)
                {
                    if (!GogLibraryUtilities.IsGogGame(game))
                    {
                        continue;
                    }

                    var data = _gogSecondClassWatcherService.GetDataForGame(game);
                    if (data is null || !GogLibraryUtilities.ShouldWatcherNotify(Settings.Settings, data))
                    {
                        PlayniteUtilities.RemoveTagFromGame(PlayniteApi, game, issuesTag);
                    }
                    else
                    {
                        PlayniteUtilities.AddTagToGame(PlayniteApi, game, issuesTag);
                    }
                }
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args.Games.Count != 1)
            {
                return base.GetGameMenuItems(args);
            }

            var game = args.Games[0];
            if (!GogLibraryUtilities.IsGogGame(game))
            {
                return base.GetGameMenuItems(args);
            }

            var data = _gogSecondClassWatcherService.GetDataForGame(game);
            if (data != null)
            {
                return new List<GameMenuItem>
                {
                    new GameMenuItem
                    {
                        Description = ResourceProvider.GetString("LOC_GogSecondClass_OpenInformationWindowLabel"),
                        Icon = PlayniteUtilities.GetIcoFontGlyphResource('\uEF4E'),
                        MenuSection = $"GOG Second Class Game Watcher",
                        Action = _ => _gogSecondClassGameWindowCreator.OpenWindow(data, game)
                    }
                };
            }

            return base.GetGameMenuItems(args);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GOGSecondClassGameWatcherSettingsView();
        }
    }
}