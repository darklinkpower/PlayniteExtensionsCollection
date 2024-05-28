using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiDomain.UListAggregate
{
    [Flags]
    public enum UListFields
    {
        None = 0,
        Id = 1 << 0,
        Added = 1 << 1,
        Voted = 1 << 2,
        LastMod = 1 << 3,
        Vote = 1 << 4,
        Started = 1 << 5,
        Finished = 1 << 6,
        Notes = 1 << 7,
        Labels = 1 << 8,
        LabelsId = 1 << 9,
        LabelsLabel = 1 << 10,
        Releases = 1 << 11,
        ReleasesListStatus = 1 << 12
    }
}