using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
    public class UserGamesResponseAtribute
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("values")]
        public Value[] Values { get; set; }

        [SerializationPropertyName("code")]
        public int Code { get; set; }

        [SerializationPropertyName("position")]
        public int Position { get; set; }
    }
}