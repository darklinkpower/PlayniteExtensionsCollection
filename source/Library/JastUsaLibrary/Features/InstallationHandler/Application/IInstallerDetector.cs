using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Application
{
    public interface IInstallerDetector
    {
        string ReadFileAsAscii(string filePath, int maxBytes = 1024 * 1024);
        bool IsSfxArchive(string filePath);
    }
}
