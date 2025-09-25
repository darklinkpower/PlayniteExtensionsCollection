using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Domain.Entities
{
    internal class HoyoPlayLauncherInfo
    {
        public string InstallDirectory { get; }
        public string ExePath => Path.Combine(InstallDirectory, "launcher.exe");

        public HoyoPlayLauncherInfo(string installDirectory)
        {
            if (installDirectory.IsNullOrEmpty())
            {
                throw new ArgumentException("Install directory cannot be null or empty.", nameof(installDirectory));
            }

            InstallDirectory = installDirectory;
        }
    }
}
