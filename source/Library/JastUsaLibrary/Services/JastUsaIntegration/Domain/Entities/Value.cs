using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Entities
{
    public class Value
    {
        [SerializationPropertyName("@type")]
        public Enums.ValueType Type { get; set; }

        [SerializationPropertyName("label")]
        public string Label { get; set; }

        [SerializationPropertyName("code")]
        public string Code { get; set; }

        [SerializationPropertyName("counter")]
        public int Counter { get; set; }
    }
}