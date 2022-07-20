using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesSizeCalculator.Models
{
    public class DepotInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long FileSize { get; set; }
        public bool IsDLC { get; }
        public bool Optional { get; }

        public DepotInfo(string id, string name, long fileSize, bool isDlc, bool optional)
        {
            Id = id;
            Name = name;
            FileSize = fileSize;
            IsDLC = isDlc;
            Optional = optional;
        }
    }
}