using JastUsaLibrary.JastUsaIntegration.Domain.Enums;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
    public class UserGameTag
    {
        [SerializationPropertyName("@id")]
        public string ApiEndpoint { get; set; }

        [SerializationPropertyName("@type")]
        public UserGameTagType Type { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("type")]
        public string Name { get; set; }
    }
}