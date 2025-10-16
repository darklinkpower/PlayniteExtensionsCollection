using HoYoPlayLibrary.Application.Services;
using HoYoPlayLibrary.Infrastructure;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HoYoPlayLibrary
{
    public class HoYoPlayLibrary : LibraryPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly HoYoPlayLibraryClient _hoyoPlayClient;
        public override string LibraryIcon =>
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
        private HoYoPlayLibrarySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("faaf4c1f-14ca-487e-8f32-d7a40c6b9c9a");

        public override string Name => "HoYoPlay Library";

        public override LibraryClient Client => _hoyoPlayClient;

        public HoYoPlayLibrary(IPlayniteAPI api) : base(api)
        {
            settings = new HoYoPlayLibrarySettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = false
            };

            var registryVersionResolver = new RegistryVersionResolver(_logger);
            var launcherRepo = new RegistryLauncherRepository(_logger, registryVersionResolver);
            var launcherService = new LauncherService(launcherRepo);

            var gameRepo = new RegistryGameRepository(_logger, registryVersionResolver);
            var gameService = new GameDiscoveryService(gameRepo);

            _hoyoPlayClient = new HoYoPlayLibraryClient(launcherService, gameService);
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var installedHoyoPlayGames = _hoyoPlayClient.GetInstalledGames();
            foreach (var game in installedHoyoPlayGames)
            {
                yield return new GameMetadata
                {
                    Source = new MetadataNameProperty("HoYoPlay"),
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                    Name = game.Name,
                    InstallDirectory = game.InstallDirectory,
                    GameId = game.Id,
                    IsInstalled = true
                };
            }

            // Playnite does not automatically mark previously imported games as uninstalled
            // if they are not returned by GetGames. To reflect the true state of the library,
            // we explicitly find such games and return them with IsInstalled = false.
            var installedIds = new HashSet<string>(installedHoyoPlayGames.Select(g => g.Id));

            var previouslyImportedNotInstalled = PlayniteApi.Database.Games
                .Where(g => g.PluginId == Id && !installedIds.Contains(g.GameId));

            foreach (var game in previouslyImportedNotInstalled)
            {
                yield return new GameMetadata
                {
                    Source = new MetadataNameProperty("HoYoPlay"),
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                    Name = game.Name,
                    GameId = game.GameId,
                    IsInstalled = false
                };
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new HoYoPlayLibrarySettingsView();
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            var game = args.Game;
            if (args.Game.PluginId != Id)
            {
                return base.GetPlayActions(args);
            }

            var installedGame = _hoyoPlayClient
                .GetInstalledGames()
                .FirstOrDefault(x => x.Id == args.Game.GameId);

            if (installedGame is null || string.IsNullOrEmpty(installedGame.ExePath))
            {
                return base.GetPlayActions(args);
            }

            return new List<PlayController>
            {
                new AutomaticPlayController(game)
                {
                    Name = game.Name,
                    Path = installedGame.ExePath,
                    WorkingDir = installedGame.InstallDirectory,
                    TrackingMode = TrackingMode.Default
                }
            };
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            var game = args.Game;
            if (args.Game.PluginId == Id && _hoyoPlayClient.IsInstalled)
            {
                return new List<InstallController>
                {
                    new HoYoPlayGameInstallController(game, _hoyoPlayClient, _logger)
                };
            }

            return base.GetInstallActions(args);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            var game = args.Game;
            if (args.Game.PluginId == Id && _hoyoPlayClient.IsInstalled)
            {
                return new List<UninstallController>
                {
                    new HoYoPlayGameUninstallController(game, _hoyoPlayClient, _logger)
                };
            }

            return base.GetUninstallActions(args);
        }
    }
}