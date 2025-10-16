using HoYoPlayLibrary.Domain.Entities;
using HoYoPlayLibrary.Domain.Interfaces;
using Microsoft.Win32;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Infrastructure
{
    internal class RegistryLauncherRepository : ILauncherRepository
    {
        private const string InstallPathKey = "InstallPath";
        private readonly ILogger _logger;
        private readonly IRegistryVersionResolver _registryVersionResolver;

        public RegistryLauncherRepository(ILogger logger, IRegistryVersionResolver registryVersionResolver)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registryVersionResolver = registryVersionResolver ?? throw new ArgumentNullException(nameof(registryVersionResolver));
        }

        public HoyoPlayLauncherInfo FindLauncher()
        {
            var rootKeyPath = _registryVersionResolver.GetActiveRootKeyPath();
            if (rootKeyPath.IsNullOrEmpty())
            {
                return null;
            }

            using (var key = Registry.CurrentUser.OpenSubKey(rootKeyPath))
            {
                if (key is null)
                {
                    _logger.Warn($"Failed to open HoYoPlay registry path: {rootKeyPath}");
                    return null;
                }

                var installDirectory = key.GetValue(InstallPathKey) as string;
                if (installDirectory.IsNullOrEmpty())
                {
                    _logger.Warn($"'{InstallPathKey}' not found or empty in {rootKeyPath}");
                    return null;
                }

                return new HoyoPlayLauncherInfo(installDirectory);
            }
        }

    }
}
