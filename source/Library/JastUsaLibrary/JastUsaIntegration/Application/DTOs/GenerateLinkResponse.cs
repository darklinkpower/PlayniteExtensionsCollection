using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Application.DTOs
{
    public class GenerateLinkResponse
    {
        [SerializationPropertyName("url")]
        public Uri Url { get; set; }
    }
}