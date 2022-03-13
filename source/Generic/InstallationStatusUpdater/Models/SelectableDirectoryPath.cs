using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Models
{
    public class SelectableDirectory
    {
        private bool enabled = false;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
            }
        }

        private bool scanSubDirs = false;
        public bool ScanSubDirs
        {
            get => scanSubDirs;
            set
            {
                scanSubDirs = value;
            }
        }

        private string directoryPath = string.Empty;
        public string DirectoryPath
        {
            get => directoryPath;
            set
            {
                directoryPath = value;
            }
        }
    }
}
