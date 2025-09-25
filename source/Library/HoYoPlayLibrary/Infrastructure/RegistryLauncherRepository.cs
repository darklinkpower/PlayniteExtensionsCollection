using HoYoPlayLibrary.Domain.Entities;
using HoYoPlayLibrary.Domain.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Infrastructure
{
    internal class RegistryLauncherRepository : ILauncherRepository
    {
        private const string BaseKey = @"Software\Cognosphere\HYP\1_0";
        private const string InstallPathKey = "InstallPath";

        public HoyoPlayLauncherInfo FindLauncher()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(BaseKey))
            {
                if (key is null)
                {
                    return null;
                }

                var installDirectory = key.GetValue(InstallPathKey) as string;
                if (installDirectory.IsNullOrWhiteSpace())
                {
                    return null;
                }

                return new HoyoPlayLauncherInfo(installDirectory);
            }
        }


    }
}
