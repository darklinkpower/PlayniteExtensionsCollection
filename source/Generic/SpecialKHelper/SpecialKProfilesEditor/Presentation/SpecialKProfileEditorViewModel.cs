using FuzzySharp;
using IniParser;
using IniParser.Model;
using Playnite.SDK;
using PluginsCommon;
using SpecialKHelper.SpecialKProfilesEditor.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SpecialKHelper.SpecialKProfilesEditor.Application
{
    public class SpecialKProfileEditorViewModel : ObservableObject
    {
        #region Fields
        private readonly IPlayniteAPI _playniteApi;
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly FileIniDataParser _iniParser;
        private List<SpecialKProfileData> _specialKProfilesCollectionItems;
        private readonly ICollectionView _specialKProfilesDataCollection;

        private string _currentEditSection = string.Empty;
        private string _currentEditKey = string.Empty;
        private string _currentEditValue = string.Empty;
        private string _searchString = string.Empty;
        private bool _isKeySelected = false;
        private bool _useFuzzySearch = false;
        private bool _isProfileSelected = false;
        private SpecialKProfileData _selectedSpecialKProfileData;
        private SpecialKProfile _selectedProfile;
        private Section selectedProfileSection;
        private ProfileKey _selectedProfileKey;
        private string _selectedProfileKeyValue = string.Empty;
        #endregion

        #region Properties
        public List<SpecialKProfileData> SpecialKProfilesCollectionItems
        {
            get => _specialKProfilesCollectionItems;
            set
            {
                _specialKProfilesCollectionItems = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView SpecialKProfilesDataCollection => _specialKProfilesDataCollection;

        public string CurrentEditSection
        {
            get => _currentEditSection;
            set
            {
                _currentEditSection = value;
                OnPropertyChanged();
            }
        }

        public bool IsKeySelected
        {
            get => _isKeySelected;
            set
            {
                _isKeySelected = value;
                OnPropertyChanged();
            }
        }

        public string CurrentEditKey
        {
            get => _currentEditKey;
            set
            {
                _currentEditKey = value;
                OnPropertyChanged();
            }
        }

        public string CurrentEditValue
        {
            get => _currentEditValue;
            set
            {
                _currentEditValue = value;
                OnPropertyChanged();
            }
        }

        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                OnPropertyChanged();
                OnSearchStringChanged();
            }
        }

        public SpecialKProfileData SelectedSpecialKProfileData
        {
            get => _selectedSpecialKProfileData;
            set
            {
                _selectedSpecialKProfileData = value;
                OnPropertyChanged();
                SelectedProfile = GetSpecialKProfileFromData(_selectedSpecialKProfileData);
            }
        }

        public SpecialKProfile SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                _selectedProfile = value;
                IsProfileSelected = _selectedProfile != null;
                OnPropertyChanged();
            }
        }

        public Section SelectedProfileSection
        {
            get => selectedProfileSection;
            set
            {
                selectedProfileSection = value;
                CurrentEditSection = selectedProfileSection?.Name ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public ProfileKey SelectedProfileKey
        {
            get => _selectedProfileKey;
            set
            {
                _selectedProfileKey = value;
                CurrentEditKey = _selectedProfileKey?.Name ?? string.Empty;
                CurrentEditValue = _selectedProfileKey?.Value;
                OnPropertyChanged();
            }
        }

        public string SelectedProfileKeyValue
        {
            get => !_selectedProfileKeyValue.IsNullOrEmpty() ? _selectedProfileKeyValue : string.Empty;
            set
            {
                _selectedProfileKeyValue = value;
                OnPropertyChanged();
            }
        }

        public bool UseFuzzySearch
        {
            get => _useFuzzySearch;
            set
            {
                _useFuzzySearch = value;
                OnPropertyChanged();
                _specialKProfilesDataCollection.Refresh();
            }
        }

        public bool IsProfileSelected
        {
            get => _isProfileSelected;
            set
            {
                _isProfileSelected = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public RelayCommand SaveSelectedProfileCommand
            => new RelayCommand(() => SaveProfile(SelectedProfile), () => _isProfileSelected);

        public RelayCommand SaveValueCommand
            => new RelayCommand(() => SaveValue(), () =>_isKeySelected);

        public RelayCommand DeleteSelectedProfileCommand
            => new RelayCommand(() => DeleteProfile(SelectedProfile), () => _isProfileSelected);
        #endregion

        #region Constructor
        public SpecialKProfileEditorViewModel(IPlayniteAPI playniteApi, string skifPath, string initialSearch = null)
        {
            _playniteApi = playniteApi;
            _iniParser = new FileIniDataParser();
            ConfigureIniParser(_iniParser);

            SpecialKProfilesCollectionItems = GetSpecialKDataProfiles(skifPath);
            _specialKProfilesDataCollection = CollectionViewSource.GetDefaultView(SpecialKProfilesCollectionItems);
            _specialKProfilesDataCollection.Filter = FilterSkProfilesCollection;
            SearchString = !initialSearch.IsNullOrEmpty() ? initialSearch : string.Empty;
        }
        #endregion

        #region Common Methods
        private bool SaveProfile(SpecialKProfile specialKProfile)
        {
            if (specialKProfile is null)
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

            _iniParser.WriteFile(specialKProfile.ConfigurationPath, iniData, Encoding.UTF8);
            _playniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_EditorDialogMessageSaveProfile"), specialKProfile.Name, specialKProfile.ConfigurationPath));

            return true;
        }

        private bool SaveValue()
        {
            return AddValueToIni(SelectedProfile, CurrentEditSection, CurrentEditKey, CurrentEditValue) == true;
        }

        private bool DeleteProfile(SpecialKProfile specialKProfile)
        {
            var selection = _playniteApi.Dialogs.ShowMessage(
                ResourceProvider.GetString("LOCSpecial_K_Helper_EditorLabelDeleteProfileNotice"),
                "Special K Profile Editor",
                MessageBoxButton.YesNo);

            if (selection == MessageBoxResult.Yes)
            {
                var profileDirectory = Path.GetDirectoryName(specialKProfile.ConfigurationPath);
                try
                {
                    if (Directory.Exists(profileDirectory))
                    {
                        Directory.Delete(profileDirectory, true);
                    }

                    SpecialKProfilesCollectionItems.Remove(specialKProfile.Data);
                    _specialKProfilesDataCollection.Refresh();
                    return true;
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error while deleting profile directory in {profileDirectory}");
                    _playniteApi.Dialogs.ShowErrorMessage(
                        string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_EditorDeleteProfileError"), specialKProfile.Name, profileDirectory, e.Message),
                        "Special K Profile Editor");
                }
            }

            return false;
        }
        #endregion

        #region Helper Methods
        private void ConfigureIniParser(FileIniDataParser iniParser)
        {
            iniParser.Parser.Configuration.AssigmentSpacer = string.Empty;
            iniParser.Parser.Configuration.AllowDuplicateKeys = true;
            iniParser.Parser.Configuration.OverrideDuplicateKeys = true;
            iniParser.Parser.Configuration.AllowDuplicateSections = true;
        }

        private List<SpecialKProfileData> GetSpecialKDataProfiles(string skifPath)
        {
            var skProfilesPath = Path.Combine(skifPath, "Profiles");
            return Directory.GetDirectories(skProfilesPath)
                .Select(x => new SpecialKProfileData(x))
                .ToList();
        }

        private SpecialKProfile GetSpecialKProfileFromData(SpecialKProfileData profileData)
        {
            var iniPath = profileData.ConfigurationPath;
            if (!FileSystem.FileExists(iniPath))
            {
                return null;
            }

            var iniData = _iniParser.ReadFile(iniPath);
            var sections = iniData.Sections.Select(iniSection =>
                new Section(iniSection.SectionName, iniSection.Keys.Select(x => new ProfileKey(x.KeyName, x.Value))));

            return new SpecialKProfile(profileData, sections.ToList());
        }

        private void OnSearchStringChanged()
        {
            _specialKProfilesDataCollection.Refresh();
        }

        private bool FilterSkProfilesCollection(object item)
        {
            var profile = item as SpecialKProfile;
            if (SearchString.IsNullOrEmpty())
            {
                return true;
            }

            return UseFuzzySearch
                ? Fuzz.Ratio(SearchString.ToLower(), profile.Name.ToLower()) > 65
                : profile.Name.ToLower().Contains(SearchString);
        }

        private bool AddValueToIni(SpecialKProfile profile, string section, string key, string newValue)
        {
            var profileSection = profile.Sections.FirstOrDefault(x => x.Name == section);
            if (profileSection is null)
            {
                profileSection = new Section(section);
                profile.Sections.Add(profileSection);
            }

            var existingKey = profileSection.Keys.FirstOrDefault(x => x.Name == key);
            if (existingKey is null)
            {
                profileSection.Keys.Add(new ProfileKey(key, newValue));
                return true;
            }

            if (existingKey.Value != newValue)
            {
                existingKey.Value = newValue;
                return true;
            }

            return false;
        }

        #endregion

    }
}