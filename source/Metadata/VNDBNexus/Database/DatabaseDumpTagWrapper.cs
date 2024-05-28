using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiInfrastructure.DatabaseDumpTagAggregate;
using VndbApiDomain.SharedKernel.Entities;

namespace VNDBNexus.Database
{
    public class DatabaseDumpTagWrapper : IAggregateRoot
    {
        public string Id { get; set; }

        public DatabaseDumpTag Tag { get; set; }

        public DatabaseDumpTagWrapper(DatabaseDumpTag databaseDumpTag)
        {
            Id = $"g{databaseDumpTag.Id}";
            Tag = databaseDumpTag;
        }

        private DatabaseDumpTagWrapper()
        {

        }
    }
}