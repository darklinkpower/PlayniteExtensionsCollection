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

        public bool CanHandle(string filePath, string content) => content.Contains("Inno Setup");

        public void Install(InstallRequest request)
        {
            var info = new ProcessStartInfo
            {
                FileName = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART",
                Arguments = request.FilePath,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(info))
            {
                process?.WaitForExit();
            } 
        }
    }
}
