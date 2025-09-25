using HoYoPlayLibrary.Application.Services;
using HoYoPlayLibrary.Domain.Entities;
using HoYoPlayLibrary.Domain.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary
{
    internal class HoYoPlayLibraryClient : LibraryClient
    {
        private readonly LauncherService _launcherService;
        private readonly GameDiscoveryService _gameDiscoveryService;
        public override string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");

        public HoYoPlayLibraryClient(LauncherService launcherService, GameDiscoveryService gameDiscoveryService)
        {
            _launcherService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
            _gameDiscoveryService = gameDiscoveryService ?? throw new ArgumentNullException(nameof(gameDiscoveryService));
        }

        public override bool IsInstalled => _launcherService.IsInstalled();

        public List<HoyoPlayGame> GetInstalledGames()
        {
            return _gameDiscoveryService.GetInstalledGames().ToList();
        }

        public override void Open() => _launcherService.OpenLauncher();

        public void OpenGamePage(string gameId) => _launcherService.OpenGamePage(gameId);
        public void OpenGamePage(Game game) => _launcherService.OpenGamePage(game.GameId);

        public void UninstallGame(string gameId) => _launcherService.UninstallGame(gameId);
        public void UninstallGame(Game game) => _launcherService.UninstallGame(game.GameId);
    }
}