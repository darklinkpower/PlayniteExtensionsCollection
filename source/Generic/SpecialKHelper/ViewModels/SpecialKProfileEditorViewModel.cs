using FuzzySharp;
using IniParser;
using IniParser.Model;
using Playnite.SDK;
using SpecialKHelper.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SpecialKHelper.ViewModels
{
    class SpecialKProfileEditorViewModel : ObservableObject
    {
        private readonly IPlayniteAPI playniteApi;
        private readonly string skifPath;
        private readonly FileIniDataParser iniParser;
        private readonly string skProfilesPath;
        private ICollectionView sKProfilesCollection;
        public ICollectionView SKProfilesCollection
        {
            get { return sKProfilesCollection; }
        }

        private string searchString;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value.ToLower();
                OnPropertyChanged();
                sKProfilesCollection.Refresh();
            }
        }

        private SpecialKProfile selectedProfile;
        public SpecialKProfile SelectedProfile
        {
            get { return selectedProfile; }
            set
            {
                selectedProfile = value;
                if (selectedProfile != null)
                {
                    IsProfileSelected = true;
                }
                else
                {
                    IsProfileSelected = false;
                }

                OnPropertyChanged();
            }
        }

        private SectionData selectedProfileSection;
        public SectionData SelectedProfileSection
        {
            get => selectedProfileSection;
            set
            {
                selectedProfileSection = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, KeyData> selectedProfileKey;
        public KeyValuePair<string, KeyData> SelectedProfileKey
        {
            get => selectedProfileKey;
            set
            {
                selectedProfileKey = value;
                OnPropertyChanged();
            }
        }

        private string selectedProfileKeyValue;
        public string SelectedProfileKeyValue
        {
            get
            {
                if (selectedProfileKeyValue.IsNullOrEmpty())
                {
                    return string.Empty;
                }
                else
                {
                    return selectedProfileKeyValue;
                }
            }
            set
            {
                selectedProfileKeyValue = value;
                OnPropertyChanged();
            }
        }

        private bool useFuzzySearch { get; set; } = false;
        public bool UseFuzzySearch
        {
            get => useFuzzySearch;
            set
            {
                useFuzzySearch = value;
                OnPropertyChanged();
            }
        }

        private bool isProfileSelected { get; set; } = false;
        public bool IsProfileSelected
        {
            get => isProfileSelected;
            set
            {
                isProfileSelected = value;
                OnPropertyChanged();
            }
        }

        public SpecialKProfileEditorViewModel(IPlayniteAPI playniteApi, FileIniDataParser iniParser, string skifPath, string initialSearch = null)
        {
            this.playniteApi = playniteApi;
            this.skifPath = skifPath;
            this.iniParser = iniParser;
            skProfilesPath = Path.Combine(skifPath, "Profiles");
            sKProfilesCollection = CollectionViewSource.GetDefaultView(GetSkProfiles());
            sKProfilesCollection.Filter = FilterSkProfilesCollection;

            if (initialSearch == null)
            {
                SearchString = string.Empty;
            }
            else
            {
                SearchString = initialSearch;
            }
        }

        bool FilterSkProfilesCollection(object item)
        {
            var profile = item as SpecialKProfile;
            if (SearchString.IsNullOrEmpty())
            {
                return true;
            }

            if (UseFuzzySearch)
            {
                if (Fuzz.Ratio(SearchString, profile.ProfileName.ToLower()) > 80)
                {
                    return true;
                }
            }
            else if (profile.ProfileName.ToLower().Contains(SearchString))
            {
                return true;
            }

            return false;
        }

        private List<SpecialKProfile> GetSkProfiles()
        {
            var profileDirectoriesSearch = Directory.GetDirectories(skProfilesPath).ToList();
            var profiles = new List<SpecialKProfile>();
            foreach (var profileDir in profileDirectoriesSearch)
            {
                var iniPath = Path.Combine(profileDir, "SpecialK.ini");
                if (!File.Exists(iniPath))
                {
                    continue;
                }

                IniData ini = iniParser.ReadFile(iniPath);
                profiles.Add(new SpecialKProfile
                {
                    ProfileName = Path.GetFileName(profileDir),
                    ProfileIniPath = iniPath,
                    IniData = ini
                });
            }

            return profiles;
        }
    }
}