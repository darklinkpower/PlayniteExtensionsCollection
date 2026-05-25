using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKUpdater.Infrastructure.Models
{
    public class RepositoryRoot
    {
        public RepositoryMain Main { get; set; }
    }

    public class RepositoryMain
    {
        public List<RepositoryBranch> Branches { get; set; }

        public List<RepositoryVersion> Versions { get; set; }
    }
}
