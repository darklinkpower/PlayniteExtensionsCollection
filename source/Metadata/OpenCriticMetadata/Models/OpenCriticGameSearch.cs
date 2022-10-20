using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCriticMetadata.Models
{
    public class OpenCriticGameResult
    {
        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("dist")]
        public double Dist { get; set; }
    }
}
