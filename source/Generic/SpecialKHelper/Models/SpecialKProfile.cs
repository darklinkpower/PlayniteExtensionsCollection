using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser.Model;

namespace SpecialKHelper.Models
{
    class SpecialKProfile
    {
        public string ProfileName { get; set; }
        public string ProfileIniPath { get; set; }
        public IniData IniData { get; set; }
    }

    public enum PluginLoadMode
    {
        [Description("Early")]
        Early,
        [Description("PlugIn")]
        PlugIn
    }
}