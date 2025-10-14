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

        public bool CanHandle(string filePath, string content, ExecutableMetadata executableMetadata) =>
            content.Contains("InstallShield");

        public bool Install(InstallRequest request)
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
                if (process is null)
                {
                    return false; // Process failed to start
                }

                return process.ExitCode == 0;
            }
        }
    }
}
