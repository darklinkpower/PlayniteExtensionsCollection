using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Domain
{

    public class InstallRequest
    {
        public string FilePath { get; set; }
        public string TargetDirectory { get; set; }

        public InstallRequest(string filePath, string targetDirectory = null)
        {
            FilePath = filePath;
            TargetDirectory = targetDirectory;
        }
    }
}
