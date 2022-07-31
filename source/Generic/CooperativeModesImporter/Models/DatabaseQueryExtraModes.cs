using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CooperativeModesImporter.Models
{
    public class DatabaseQueryExtraModes
    {
        public int Id { get; set; }
        public string LocalCoop { get; set; }
        public string OnlineCoop { get; set; }
        public string ComboCoop { get; set; }
        public string LanPlay { get; set; }
        public string Extras { get; set; }
    }
}