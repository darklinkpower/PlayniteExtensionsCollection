using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKUpdater.Infrastructure.Models
{
    public class RepositoryVersion
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string ReleaseNotes { get; set; }

        public string Installer { get; set; }

        public string SHA256 { get; set; }

        public List<string> Branches { get; set; }
    }
}
