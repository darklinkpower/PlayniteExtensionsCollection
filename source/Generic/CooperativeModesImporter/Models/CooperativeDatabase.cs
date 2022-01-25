using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CooperativeModesImporter.Models
{
    class CooperativeDatabase
    {
        public long TotalGames { get; set; }

        public long TotalGamesAdded { get; set; }

        public long TotalPages { get; set; }

        public List<CooperativeGame> Games { get; set; }
    }

    public class CooperativeGame
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string System { get; set; }

        public List<string> Modes { get; set; }

        public ModesDetailed ModesDetailed { get; set; }
    }

    public class ModesDetailed
    {
        public string LocalCoop { get; set; }

        public string OnlineCoop { get; set; }

        public string ComboCoop { get; set; }

        public string LanPlay { get; set; }

        public List<string> Extras { get; set; }
    }
}