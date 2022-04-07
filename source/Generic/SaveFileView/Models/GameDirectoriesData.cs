using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Data;

namespace SaveFileView.Models
{
    class GameDirectoriesData
    {
        [SerializationPropertyName("SaveDirectories")]
        public string[] SaveDirectories { get; set; }

        [SerializationPropertyName("ConfigDirectories")]
        public string[] ConfigDirectories { get; set; }
    }
}
