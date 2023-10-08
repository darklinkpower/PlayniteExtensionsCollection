using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer.Models
{
    public class NumberOfPlayersResponse
    {
        [SerializationPropertyName("response")]
        public NumberOfPlayersResponseResponse Response { get; set; }
    }

    public class NumberOfPlayersResponseResponse
    {
        [SerializationPropertyName("player_count")]
        public int PlayerCount { get; set; }

        [SerializationPropertyName("result")]
        public int Result { get; set; }
    }
}