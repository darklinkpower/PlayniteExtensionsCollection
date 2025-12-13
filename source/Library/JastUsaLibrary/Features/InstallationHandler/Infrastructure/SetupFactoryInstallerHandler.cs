using JastUsaLibrary.Features.InstallationHandler.Application;
using JastUsaLibrary.Features.InstallationHandler.Domain;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Infrastructure
{
    // https://www.indigorose.com/webhelp/suf9/Program_Reference/Command_Line_Options.htm
    public class SetupFactoryInstallerHandler : IInstallerHandler
    {
        public InstallerType Type => InstallerType.SetupFactory;

        public bool CanHandle(string filePath, string content, ExecutableMetadata executableMetadata)
        {
            if (content.IsNullOrEmpty())
            {
                return false;
            }

            //if (content.Contains("Setup Factory"))
            //{
            //    return true;
            //}

            if (string.Equals(executableMetadata?.ProductName, "Setup Factory Runtime", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public bool Install(InstallRequest request)
        {
            var tempInstallIni = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ini");
            var iniContent = $@"
[SetupValues]
INSTALLDIR={request.TargetDirectory}";

            FileSystem.WriteStringToFile(tempInstallIni, iniContent.Trim(), true);
            var startInfo = new ProcessStartInfo
            {
                FileName = request.FilePath,
                Arguments = $"/S:{tempInstallIni} /W /NOINIT",
                UseShellExecute = true, // Must be true for Verb="runas"
                //CreateNoWindow = true, // will not work with UseShellExecute = true
                Verb = "runas" // Seems to require elevation, e.g. Hanachirasu
            };

            try
            {
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
            finally
            {
                FileSystem.DeleteFileSafe(tempInstallIni);
            }

        }
    }
}
