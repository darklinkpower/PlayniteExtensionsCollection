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
    public class MsiInstallerHandler : IInstallerHandler
    {
        public InstallerType Type => InstallerType.Msi;

        public bool CanHandle(string filePath, string content) =>
            Path.GetExtension(filePath).Equals(".msi", StringComparison.OrdinalIgnoreCase);

        public void Install(InstallRequest request)
        {
            var arguments = $"/i \"{request.FilePath}\" /qn /norestart";
            var startInfo = new ProcessStartInfo
            {
                FileName = "msiexec",
                Arguments = arguments,
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
