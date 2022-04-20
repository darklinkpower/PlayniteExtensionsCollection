using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVIDIAGeForceNowEnabler
{
    public class NVIDIAGeForceNowEnablerPlayController : PlayController
    {
        private readonly static ILogger logger = LogManager.GetLogger();
        private readonly string supportedGameId;
        private readonly string geforceNowExecutablePath;
        private readonly string geforceNowWorkingPath;
        private Stopwatch stopWatch;

        public NVIDIAGeForceNowEnablerPlayController(Game game, string supportedGameId, string geforceNowExecutablePath, string geforceNowWorkingPath) : base(game)
        {
            Name = ResourceProvider.GetString("LOCNgfn_Enabler_ControllerLaunchInNgfn");
            this.supportedGameId = supportedGameId;
            this.geforceNowExecutablePath = geforceNowExecutablePath;
            this.geforceNowWorkingPath = geforceNowWorkingPath;
        }

        public override void Dispose()
        {
            
        }

        public override void Play(PlayActionArgs args)
        {
            Dispose();
            stopWatch = Stopwatch.StartNew();
            var arguments = string.Format("--url-route=\"#?cmsId={0}&launchSource=External&shortName=game_gfn_pc&parentGameId=\"", supportedGameId);
            ProcessStarter.StartProcess(geforceNowExecutablePath, arguments, geforceNowWorkingPath);
            StartWatching();
        }

        public bool GetIsGeforceNowRunning()
        {
            Process[] processes = Process.GetProcessesByName("GeForceNOW");
            if (processes.Length > 0)
            {
                return true;
            }
            return false;
        }

        public async void StartWatching()
        {
            // TODO Better way to detect game start and stop
            // GeForce NOW leaves leftover processes when closed, which
            // means that it gets detected as running when closed since
            // process tree has not been completely destroyed
            while (true)
            {
                if (GetIsGeforceNowRunning())
                {
                    InvokeOnStarted(new GameStartedEventArgs());
                    break;
                }
                await Task.Delay(15000);
            }

            while (true)
            {
                if (!GetIsGeforceNowRunning())
                {
                    InvokeOnStopped(new GameStoppedEventArgs(Convert.ToUInt64(stopWatch.Elapsed.TotalSeconds)));
                    break;
                }
                await Task.Delay(15000);
            }
        }

    }
}
