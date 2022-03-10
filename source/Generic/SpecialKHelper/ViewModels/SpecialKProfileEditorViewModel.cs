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

        private string currentEditSection;
        public string CurrentEditSection
        {
            get
            {
                if (currentEditSection.IsNullOrEmpty())
                {
                    return string.Empty;
                }

                return currentEditSection;
            }
            set
            {
                currentEditSection = value;
                OnPropertyChanged();
            }
        }

        private bool isKeySelected { get; set; } = false;
        public bool IsKeySelected
        {
            get => isKeySelected;
            set
            {
                isKeySelected = value;
                OnPropertyChanged();
            }
        }

        private string currentEditKey;
        public string CurrentEditKey
        {
            get
            {
                if (currentEditKey.IsNullOrEmpty())
                {
                    return string.Empty;
                }

                return currentEditKey;
            }
            set
            {
                currentEditKey = value;
                OnPropertyChanged();
            }
        }

        private string currentEditValue;
        public string CurrentEditValue
        {
            get
            {
                if (currentEditValue.IsNullOrEmpty())
                {
                    isKeySelected = true;
                    return string.Empty;
                }

                isKeySelected = true;
                return currentEditValue;
            }
            set
            {
                currentEditValue = value;
                OnPropertyChanged();
            }
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
                CurrentEditSection = selectedProfileSection?.SectionName ?? string.Empty;
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
                CurrentEditKey = selectedProfileKey.Value?.KeyName ?? string.Empty;
                CurrentEditValue = selectedProfileKey.Value?.Value ?? string.Empty;
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

        public RelayCommand<object> SaveSelectedProfileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SaveProfile(SelectedProfile);
            }, a => isProfileSelected);
        }

        private bool SaveProfile(SpecialKProfile specialKProfile)
        {
            if (specialKProfile == null)
            {
                return false;
            }

            iniParser.WriteFile(specialKProfile.ProfileIniPath, specialKProfile.IniData, Encoding.UTF8);
            playniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_EditorDialogMessageSaveProfile"), specialKProfile.ProfileName, specialKProfile.ProfileIniPath));
            return true;
        }

        public RelayCommand<object> SaveValueCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SaveValue();
            }, a => isKeySelected);
        }

        private bool SaveValue()
        {
            if (AddValueToIni(SelectedProfile.IniData, CurrentEditSection, CurrentEditKey, CurrentEditValue) == 1)
            {
                OnPropertyChanged("SelectedProfile");
                OnPropertyChanged("SelectedProfileKey");
                return true;
            }

            return false;
        }

        private int AddValueToIni(IniData ini, string section, string key, string newValue)
        {
            var currentValue = ini[section][key];
            if (currentValue == null || currentValue != newValue)
            {
                ini[section][key] = newValue;
                return 1;
            }

            return 0;
        }
    }
}