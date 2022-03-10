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
using System.Windows;
using System.Windows.Data;

namespace SpecialKHelper.ViewModels
{
    class SpecialKProfileEditorViewModel : ObservableObject
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string skifPath;
        private readonly FileIniDataParser iniParser;
        private readonly string skProfilesPath;
        private List<SpecialKProfile> sKProfilesCollectionItems;
        public List<SpecialKProfile> SKProfilesCollectionItems
        {
            get
            {
                return sKProfilesCollectionItems;
            }
            set
            {
                sKProfilesCollectionItems = value;
                OnPropertyChanged();
            }
        }

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

        private Section selectedProfileSection;
        public Section SelectedProfileSection
        {
            get => selectedProfileSection;
            set
            {
                selectedProfileSection = value;
                CurrentEditSection = selectedProfileSection.Name;
                OnPropertyChanged();
            }
        }

        private ProfileKey selectedProfileKey;
        public ProfileKey SelectedProfileKey
        {
            get => selectedProfileKey;
            set
            {
                selectedProfileKey = value;
                CurrentEditKey = selectedProfileKey.Name;
                CurrentEditValue = selectedProfileKey.Value;
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
            SKProfilesCollectionItems = GetSkProfiles();
            sKProfilesCollection = CollectionViewSource.GetDefaultView(SKProfilesCollectionItems);
            sKProfilesCollection.Filter = FilterSkProfilesCollection;

            if (initialSearch == null)
            {
                SearchString = string.Empty;
            }
            else
            {
                SearchString = initialSearch;
            }

            //TODO Rework this for something that makes more sense
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

                var profile = new SpecialKProfile
                {
                    ProfileName = Path.GetFileName(profileDir),
                    ProfileIniPath = iniPath
                };

                foreach (var iniSection in ini.Sections)
                {
                    var section = new Section
                    {
                        Name = iniSection.SectionName
                    };

                    foreach (var iniKey in iniSection.Keys)
                    {
                        section.Keys.Add(new ProfileKey
                        {
                            Name = iniKey.KeyName,
                            Value = iniKey.Value
                        });
                    }

                    profile.Sections.Add(section);
                }

                profiles.Add(profile);
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

            var iniData = new IniData();
            iniData.Configuration.AssigmentSpacer = string.Empty;
            foreach (var section in specialKProfile.Sections)
            {
                foreach (var key in section.Keys)
                {
                    iniData[section.Name][key.Name] = key.Value;
                }
            }
            
            iniParser.WriteFile(specialKProfile.ProfileIniPath, iniData, Encoding.UTF8);
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
            if (AddValueToIni(SelectedProfile, CurrentEditSection, CurrentEditKey, CurrentEditValue) == 1)
            {
                return true;
            }

            return false;
        }

        public RelayCommand<object> DeleteSelectedProfileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                DeleteProfile(SelectedProfile);
            }, a => isProfileSelected);
        }

        private bool DeleteProfile(SpecialKProfile profile)
        {
            var selection = playniteApi.Dialogs.ShowMessage(
                ResourceProvider.GetString("LOCSpecial_K_Helper_EditorLabelDeleteProfileNotice"),
                "Special K Profile Editor",
                MessageBoxButton.YesNo);
            if (selection == MessageBoxResult.Yes)
            {
                var profileDirectory = Path.GetDirectoryName(profile.ProfileIniPath);
                try
                {
                    if (Directory.Exists(profileDirectory))
                    {
                        Directory.Delete(profileDirectory, true);
                    }
                    SKProfilesCollectionItems.Remove(profile);
                    sKProfilesCollection.Refresh();
                    return true;
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error while deleting profile directory in {profileDirectory}");
                    playniteApi.Dialogs.ShowErrorMessage(
                        string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_EditorDeleteProfileError"), profile.ProfileName, profileDirectory, e.Message),
                        "Special K Profile Editor");
                }
            }
            
            return false;
        }

        private int AddValueToIni(SpecialKProfile profile, string section, string key, string newValue)
        {
            var profileSection = profile.Sections.FirstOrDefault(x => x.Name == section);
            if (profileSection == null)
            {
                profile.Sections.Add(new Section { Name = section });
            }

            var existingSection = profile.Sections.FirstOrDefault(x => x.Name == section);
            var existingKey = existingSection.Keys.FirstOrDefault(x => x.Name == key);
            if (existingKey == null)
            {
                existingSection.Keys.Add(new ProfileKey { Name = key, Value = newValue});
                return 1;
            }
            else if (existingKey.Value != newValue)
            {
                existingKey.Value = newValue;
                return 1;
            }

            return 0;
        }
    }
}