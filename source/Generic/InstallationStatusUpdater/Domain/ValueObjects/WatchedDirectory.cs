using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Domain.ValueObjects
{
    public class WatchedDirectory
    {
        public bool Enabled { get; set; } = false;
        public bool ScanSubDirs { get; set; } = false;
        public string DirectoryPath { get; set; } = string.Empty;
    }
}