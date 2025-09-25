using HoYoPlayLibrary.Domain.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HoYoPlayLibrary
{
    internal class HoYoPlayGameInstallController : InstallController
    {
        private readonly Game _game;
        private readonly HoYoPlayLibraryClient _hoyoPlayClient;
        private readonly ILogger _logger;
        private CancellationTokenSource _watcherToken;

        public HoYoPlayGameInstallController(Game game, HoYoPlayLibraryClient hoyoPlayClient, ILogger logger) : base(game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _hoyoPlayClient = hoyoPlayClient ?? throw new ArgumentNullException(nameof(hoyoPlayClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Name = "Install using HoYoPlay";
        }

        public override void Dispose()
        {
            _watcherToken?.Cancel();
            _watcherToken?.Dispose();
            _watcherToken = null;
            base.Dispose();
        }

        public override void Install(InstallActionArgs args)
        {
            if (!_hoyoPlayClient.IsInstalled)
            {
                throw new InvalidOperationException("HoYoPlay launcher not found.");
            }

            _hoyoPlayClient.OpenGamePage(_game);
            _ = WatchInstallAsync();
        }

        private async Task WatchInstallAsync()
        {
            _watcherToken = new CancellationTokenSource();
            var token = _watcherToken.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var installedGames = _hoyoPlayClient.GetInstalledGames();
                    var installedGame = installedGames.FirstOrDefault(x => x.Id == _game.GameId);

                    if (installedGame != null)
                    {
                        var installInfo = new GameInstallationData
                        {
                            InstallDirectory = installedGame.InstallDirectory
                        };

                        InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
                        return;
                    }

                    await Task.Delay(10_000, token);
                }
            }
            catch (TaskCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in HoYoPlay install watcher: {ex}");
            }
        }
    }
}
