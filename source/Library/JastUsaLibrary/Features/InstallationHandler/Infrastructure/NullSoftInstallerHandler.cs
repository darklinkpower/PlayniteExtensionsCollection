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
    public class NullSoftInstallerHandler : IInstallerHandler
    {
        public InstallerType Type => InstallerType.NullSoft;

        public bool CanHandle(string filePath, string content, ExecutableMetadata executableMetadata)
        {
            if (content.IsNullOrEmpty())
            {
                return false;
            }

            //if (content.Contains("Nullsoft"))
            //{
            //    return true;
            //}

            if (executableMetadata?.ManifestAssemblyIdentity != null &&
                executableMetadata.ManifestAssemblyIdentity.TryGetValue("name", out var value) &&
                value.Equals("Nullsoft.NSIS.exehead", StringComparison.OrdinalIgnoreCase))
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
                Arguments = $"/S /D={request.TargetDirectory.TrimEnd('\\')}", // /D= must be last, with no quotes and trailing backlash
                UseShellExecute = true, // Must be true for Verb="runas"
                //CreateNoWindow = true, // will not work with UseShellExecute = true
                Verb = "runas", // Seems to require elevation, e.g. Katawa Shoujo
                WorkingDirectory = Path.GetDirectoryName(request.FilePath)
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
