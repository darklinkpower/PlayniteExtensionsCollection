using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NVIDIAGeForceNowEnabler
{
    public class NVIDIAGeForceNowClient : LibraryClient
    {
        // TODO Do this path thing properly
        public override bool IsInstalled => File.Exists(Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "NVIDIA Corporation", "GeForceNOW", "CEF", "GeForceNOW.exe"));
        public override string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");

        public override void Open()
        {
            var gfnPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "NVIDIA Corporation", "GeForceNOW", "CEF", "GeForceNOW.exe");
            if (FileSystem.FileExists(gfnPath))
            {
                ProcessStarter.StartProcess(gfnPath);
            }
        }
    }
}