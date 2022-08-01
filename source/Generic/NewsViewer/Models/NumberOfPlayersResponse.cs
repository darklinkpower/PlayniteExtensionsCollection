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
        public Response Response { get; set; }
    }

    public class Response
    {
        [SerializationPropertyName("player_count")]
        public long PlayerCount { get; set; }

        [SerializationPropertyName("result")]
        public int Result { get; set; }
    }
}
