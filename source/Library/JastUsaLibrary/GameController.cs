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

        public FakeInstallController(Game game) : base(game)
        {

        }


        public override void Install(InstallActionArgs args)
        {
            StartInstallWatcher();
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
