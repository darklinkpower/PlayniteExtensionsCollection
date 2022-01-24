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

        public List<Game> Games { get; set; }

        public partial class Game
        {
            public string Name { get; set; }

            public string System { get; set; }

            public List<string> Modes { get; set; }
        }
    }
}
