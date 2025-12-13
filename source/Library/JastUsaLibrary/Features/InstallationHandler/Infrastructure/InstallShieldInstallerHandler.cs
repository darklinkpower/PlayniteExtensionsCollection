using JastUsaLibrary.Features.InstallationHandler.Application;
using JastUsaLibrary.Features.InstallationHandler.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            // Arguments:
            // /s          = InstallShield silent mode
            // /v"..."     = passes parameters to MSI
            // /qn         = MSI quiet no UI
            // /l*v log    = verbose logging (optional, helpful for debugging)

            var escapedPath = request.TargetDirectory.Replace("\"", "\\\"");

            var logPath = Path.Combine(Path.GetTempPath(), "installshield.log");
            var arguments = $"/s /v\"/qn INSTALLDIR=\\\"{escapedPath}\\\" /l*v \\\"{logPath}\\\"\"";
            var startInfo = new ProcessStartInfo
            {
                FileName = request.FilePath,
                Arguments = $"/s /v\"/qn INSTALLDIR=\\\"{escapedPath}\\\"\"",
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
