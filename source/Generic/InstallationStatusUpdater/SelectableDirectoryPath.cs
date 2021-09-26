using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater
{
    public class SelectableDirectory
    {
        private bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
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
