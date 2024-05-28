using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.DatabaseDumpTraitAggregate;
using VndbApiDomain.SharedKernel.Entities;

namespace VNDBNexus.Database
{
    public class DatabaseDumpTraitWrapper : IAggregateRoot
    {
        public string Id { get; set; }

        public DatabaseDumpTrait Trait { get; set; }

        public DatabaseDumpTraitWrapper(DatabaseDumpTrait databaseDumpTrait)
        {
            Id = $"i{databaseDumpTrait.Id}";
            Trait = databaseDumpTrait;
        }

        private DatabaseDumpTraitWrapper()
        {

        }
    }
}