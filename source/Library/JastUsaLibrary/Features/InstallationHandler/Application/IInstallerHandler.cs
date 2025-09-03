using JastUsaLibrary.Features.InstallationHandler.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Application
{
    public interface IInstallerHandler
    {
        InstallerType Type { get; }
        bool CanHandle(string filePath, string fileContent);
        void Install(InstallRequest request);
    }
}
