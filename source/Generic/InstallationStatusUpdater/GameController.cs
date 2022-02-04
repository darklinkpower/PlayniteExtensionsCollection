using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstallationStatusUpdater
{
    public class StatusUpdaterInstallController : InstallController
    {
        private CancellationTokenSource watcherToken;
        private readonly Game game;
        private readonly bool isGameInstalled;

        public StatusUpdaterInstallController(Game game, bool isGameInstalled) : base(game)
        {
            Name = ResourceProvider.GetString("LOCInstallation_Status_Updater_PlayButtonActionLabel");
            this.game = game;
            this.isGameInstalled = isGameInstalled;
        }

        public override void Dispose()
        {
            watcherToken?.Cancel();
        }

        public override void Install(InstallActionArgs args)
        {
            StartInstallWatcher();
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (isGameInstalled)
                    {
                        var installDirectory = string.Empty;
                        if (!string.IsNullOrEmpty(game.InstallDirectory))
                        {
                            installDirectory = game.InstallDirectory;
                        }

                        var installInfo = new GameInstallationData
                        {
                            InstallDirectory = installDirectory
                        };

                        InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
                    }

                    await Task.Delay(1);
                    watcherToken.Cancel();
                }
            });
        }
    }
}
