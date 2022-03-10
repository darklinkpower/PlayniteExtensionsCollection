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
    class SpecialKProfile : ObservableObject
    {
        public string ProfileName { get; set; }
        public string ProfileIniPath { get; set; }
        private List<Section> sections = new List<Section>();
        public List<Section> Sections
        {
            get
            {
                return sections;
            }
            set
            {
                sections = value;
                OnPropertyChanged();
            }
        }
    }

    public class Section : ObservableObject
    {
        public string Name { get; set; }
        private List<ProfileKey> keys = new List<ProfileKey>();
        public List<ProfileKey> Keys
        {
            get
            {
                return keys;
            }
            set
            {
                keys = value;
                OnPropertyChanged();
            }
        }
    }

    public class ProfileKey : ObservableObject
    {
        public string Name { get; set; }
        private string value;
        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                OnPropertyChanged();
            }
        }
    }

    public enum PluginLoadMode
    {
        [Description("Early")]
        Early,
        [Description("PlugIn")]
        PlugIn
    }
}