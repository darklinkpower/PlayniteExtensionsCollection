using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Models
{
    public class GenerateLinkResponse
    {
        [SerializationPropertyName("url")]
        public string Url { get; set; }
    }
}