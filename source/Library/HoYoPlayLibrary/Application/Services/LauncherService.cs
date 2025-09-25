using HoYoPlayLibrary.Domain.Interfaces;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Application.Services
{
    internal class LauncherService
    {
        private readonly ILauncherRepository _launcherRepository;

        public LauncherService(ILauncherRepository launcherRepository)
        {
            _launcherRepository = launcherRepository ?? throw new ArgumentNullException(nameof(launcherRepository));
        }

        public bool IsInstalled()
        {
            var launcherInfo = _launcherRepository.FindLauncher();
            return launcherInfo != null && FileSystem.FileExists(launcherInfo.ExePath);
        }

        public void OpenLauncher()
        {
            var launcherInfo = _launcherRepository.FindLauncher();
            if (launcherInfo is null || !FileSystem.FileExists(launcherInfo.ExePath))
            {
                throw new InvalidOperationException("HoYoPlay launcher not found.");
            }

            ProcessStarter.StartProcess(launcherInfo.ExePath, asAdmin: true);
        }

        public void OpenGamePage(string gameId)
        {
            if (gameId.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));
            }

            var launcherInfo = _launcherRepository.FindLauncher();
            if (launcherInfo is null || !FileSystem.FileExists(launcherInfo.ExePath))
            {
                throw new InvalidOperationException("HoYoPlay launcher not found.");
            }

            var args = $"--game={gameId}";
            ProcessStarter.StartProcess(launcherInfo.ExePath, args, asAdmin: true);
        }

        public void UninstallGame(string gameId)
        {
            if (gameId.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));
            }

            var launcherInfo = _launcherRepository.FindLauncher();
            if (launcherInfo is null || !FileSystem.FileExists(launcherInfo.ExePath))
            {
                throw new InvalidOperationException("HoYoPlay launcher not found.");
            }

            var args = $"--uninstall_game={gameId}";
            ProcessStarter.StartProcess(launcherInfo.ExePath, args, asAdmin: true);
        }
    }
}
