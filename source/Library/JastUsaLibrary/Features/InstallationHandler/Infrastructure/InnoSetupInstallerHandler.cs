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
    public class InnoSetupInstallerHandler : IInstallerHandler
    {
        public InstallerType Type => InstallerType.InnoSetup;

        public bool CanHandle(string filePath, string content, ExecutableMetadata executableMetadata)
        {
            if (content.IsNullOrEmpty())
            {
                return false;
            }

            //if (content.Contains("Inno Setup"))
            //{
            //    return true;
            //}

            if (executableMetadata?.ManifestAssemblyIdentity != null &&
                executableMetadata.ManifestAssemblyIdentity.TryGetValue("name", out var value) &&
                value.Equals("JR.Inno.Setup", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public bool Install(InstallRequest request)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = request.FilePath,
                Arguments = $"/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /DIR=\"{request.TargetDirectory}\"",
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
