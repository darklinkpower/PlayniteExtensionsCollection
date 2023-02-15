using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer.Models
{
    public class GamePlayersCountCache
    {
        public readonly DateTime CreationDate;
        public readonly long PlayerCount;

        public GamePlayersCountCache(DateTime creationDate, long playerCount)
        {
            CreationDate = creationDate;
            PlayerCount = playerCount;
        }
    }
}