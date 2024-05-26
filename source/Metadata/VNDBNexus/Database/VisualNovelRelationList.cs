using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel.Entities;
using VndbApiDomain.VisualNovelAggregate;

namespace VNDBNexus.Database
{
    public class VisualNovelRelations : IAggregateRoot
    {
        public string Id { get; set; }
        public List<string> CharacterIds { get; set; }
        public List<string> ReleaseIds { get; set; }
    }
}