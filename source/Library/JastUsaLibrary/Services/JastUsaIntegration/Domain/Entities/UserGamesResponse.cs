using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
    public class UserGamesResponse
    {
        [SerializationPropertyName("@context")]
        public string Context { get; set; }

        [SerializationPropertyName("@id")]
        public string Id { get; set; }

        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("products")]
        public List<JastProduct> Products { get; set; }

        [SerializationPropertyName("attributes")]
        public UserGamesResponseAtribute[][] Attributes { get; set; }

        [SerializationPropertyName("total")]
        public int Total { get; set; }

        [SerializationPropertyName("pages")]
        public int Pages { get; set; }
    }

}