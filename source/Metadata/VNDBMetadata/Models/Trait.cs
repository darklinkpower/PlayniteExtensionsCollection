using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.Models
{
    public class Trait
    {
        [SerializationPropertyName("aliases")]
        public string[] Aliases { get; set; }

        [SerializationPropertyName("applicable")]
        public bool Applicable { get; set; }

        [SerializationPropertyName("chars")]
        public int Chars { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("meta")]
        public bool Meta { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("parents")]
        public int[] Parents { get; set; }

        [SerializationPropertyName("searchable")]
        public bool Searchable { get; set; }
    }
}