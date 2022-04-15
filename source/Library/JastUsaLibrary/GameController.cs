using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JastUsaLibrary
{
    public class FakeInstallController : InstallController
    {
        private string installDir;

        public FakeInstallController(Game game, string installDir) : base(game)
        {
            this.installDir = installDir;
        }

        public override void Install(InstallActionArgs args)
        {
            var installInfo = new GameInstallationData()
            {
                InstallDirectory = installDir
            };

            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
            return;
        }

        public async void StartInstallWatcher()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);
                    return;
                }
            });
        }

    }
}
