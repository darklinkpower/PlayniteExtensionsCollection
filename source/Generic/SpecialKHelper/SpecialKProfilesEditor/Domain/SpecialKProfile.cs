using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser.Model;
using System.Collections.ObjectModel;
using SpecialKHelper.SpecialKProfilesEditor.Application;

namespace SpecialKHelper.SpecialKProfilesEditor.Domain
{
    public class SpecialKProfile : ObservableObject
    {
        public Guid Id => Data.Id;
        public string Name => Data.Name;
        public string ConfigurationPath => Data.ConfigurationPath;
        public SpecialKProfileData Data { get; }

        public ObservableCollection<Section> Sections { get; }

        public SpecialKProfile(SpecialKProfileData profileData, IEnumerable<Section> sections = null)
        {
            Data = profileData;
            Sections = sections?.ToObservable() ?? new ObservableCollection<Section>();
        }
    }

    public class Section : ObservableObject
    {
        public string Name { get; }
        public ObservableCollection<ProfileKey> Keys { get; }
        public Section (string name, IEnumerable<ProfileKey> keys = null)
        {
            Name = name;
            Keys = keys?.ToObservable() ?? new ObservableCollection<ProfileKey>();
        }
    }

    public class ProfileKey : ObservableObject
    {
        public string Name { get; }
        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public ProfileKey(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}