using JastUsaLibrary.Features.InstallationHandler.Application;
using JastUsaLibrary.Features.InstallationHandler.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Infrastructure
{
    public class InstallShieldInstallerHandler : IInstallerHandler
    {
        public InstallerType Type => InstallerType.InstallShield;

        public bool CanHandle(string filePath, string content) => content.Contains("InstallShield");

        public void Install(InstallRequest request)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = request.FilePath,
                Arguments = "/s /v\"/qn\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process?.WaitForExit();
            }
        }
    }
}
