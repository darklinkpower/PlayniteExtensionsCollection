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
                OnPropertyChanged("SelectedProfileIsReshadeReady");
                OnPropertyChanged("SelectedProfileReshade32When");
                OnPropertyChanged("SelectedProfileReshade64When");
            }
        }

        public bool SelectedProfileIsReshadeReady
        {
            get
            {
                if (SelectedProfile != null)
                {
                    return SelectedProfile.IsReshadeReady;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (SelectedProfile == null || SelectedProfile.IsReshadeReady == value)
                {
                    return;
                }

                SelectedProfile.IsReshadeReady = value;
                OnPropertyChanged();
                OnPropertyChanged("SelectedProfileReshade32When");
                OnPropertyChanged("SelectedProfileReshade64When");
            }
        }

        public string SelectedProfileReshade32When
        {
            get
            {
                if (SelectedProfile != null)
                {
                    return SelectedProfile.Import.ReShade32.When;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (SelectedProfile == null || SelectedProfile.Import.ReShade32.When == value)
                {
                    return;
                }

                SelectedProfile.Import.ReShade32.When = value;
                OnPropertyChanged();
            }
        }

        public string SelectedProfileReshade64When
        {
            get
            {
                if (SelectedProfile != null)
                {
                    return SelectedProfile.Import.ReShade64.When;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (SelectedProfile == null || SelectedProfile.Import.ReShade64.When == value)
                {
                    return;
                }

                SelectedProfile.Import.ReShade64.When = value;
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
                if (Fuzz.Ratio(SearchString, profile.ProfileName.ToLower()) > 83)
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

                var newProfile = new SpecialKProfile
                {
                    ProfileName = Path.GetFileName(profileDir),
                    ProfileDirectory = profileDir,
                    ProfileIniPath = iniPath
                };

                IniData ini = iniParser.ReadFile(iniPath);
                var iniValue = string.Empty;
                iniValue = ini["Steam.System"]["AppID"];
                if (iniValue != null)
                {
                    newProfile.Steam.System.AppID = iniValue;
                }

                newProfile.Steam.System.AppID = GetStringIfValueFoundOrReturnOriginal(newProfile.Steam.System.AppID, ini, "Steam.System", "AppID");

                newProfile.Steam.System.PreLoadSteamOverlay = GetNullableBoolFromString(
                    ini["Steam.System"]["PreLoadSteamOverlay"]);

                newProfile.Compatibility.General.DisableBloatWare_NVIDIA = GetNullableBoolFromString(
                    ini["Compatibility.General"]["DisableBloatWare_NVIDIA"]);

                newProfile.Render.DXGI.UseFlipDiscard = GetNullableBoolFromString(
                    ini["Render.DXGI"]["UseFlipDiscard"]);

                newProfile.Render.FrameRate.TargetFPS = GetDoubleOrZeroFromString(
                    ini["Render.FrameRate"]["TargetFPS"]);

                newProfile.Render.FrameRate.SleeplessRenderThread = GetNullableBoolFromString(
                    ini["Render.FrameRate"]["SleeplessRenderThread"]);

                newProfile.Render.OSD.ShowInVideoCapture = GetNullableBoolFromString(
                    ini["Render.OSD"]["ShowInVideoCapture"]);

                //newProfile.Import.ReShade64.When = GetStringIfValueFoundOrNull(newProfile.Import.ReShade64.When, ini, "Import.ReShade64", "When");
                //newProfile.Import.ReShade32.When = GetStringIfValueFoundOrNull(newProfile.Import.ReShade32.When, ini, "Import.ReShade64", "When");
                newProfile.UpdateReshadeStatus();
                profiles.Add(newProfile);
            }

            return profiles;
        }

        private string GetStringIfValueFoundOrNull(string originalValue, IniData ini, string key, string subKey)
        {
            var iniValue = ini[key][subKey];
            if (iniValue != null)
            {
                return iniValue;
            }

            return null;
        }

        private string GetStringIfValueFoundOrReturnOriginal(string originalValue, IniData ini, string key, string subKey)
        {
            var iniValue = ini[key][subKey];
            if (iniValue != null)
            {
                return iniValue;
            }

            return originalValue;
        }

        public RelayCommand<object> SetAsReshadeReadyCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectedProfile.ChangeReshadeStatus(true);
            }, a => isProfileSelected && !SelectedProfile.IsReshadeReady);
        }

        public RelayCommand<object> SetAsNotReshadeReadyCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectedProfile.ChangeReshadeStatus(false);
            }, a => isProfileSelected && SelectedProfile.IsReshadeReady);
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

            if (!File.Exists(specialKProfile.ProfileIniPath))
            {
                playniteApi.Dialogs.ShowErrorMessage($"Special K profile file not found in \"{specialKProfile.ProfileIniPath}\".", "Special K Helper");
                return false;
            }

            IniData ini = iniParser.ReadFile(specialKProfile.ProfileIniPath);
            var updatedValues = 0;
            updatedValues += AddStringValueToIni("Steam.System", "AppID", ini, specialKProfile.Steam.System.AppID);
            updatedValues += AddNullableBoolValueToIni("Steam.System", "PreLoadSteamOverlay", ini, specialKProfile.Steam.System.PreLoadSteamOverlay);
            updatedValues += AddNullableBoolValueToIni("Compatibility.General", "DisableBloatWare_NVIDIA", ini, specialKProfile.Compatibility.General.DisableBloatWare_NVIDIA);
            updatedValues += AddNullableBoolValueToIni("Render.DXGI", "UseFlipDiscard", ini, specialKProfile.Render.DXGI.UseFlipDiscard);
            updatedValues += AddNullableBoolValueToIni("Render.FrameRate", "SleeplessRenderThread", ini, specialKProfile.Render.FrameRate.SleeplessRenderThread);
            updatedValues += AddDoubleValueToIni("Render.FrameRate", "TargetFPS", ini, specialKProfile.Render.FrameRate.TargetFPS);
            updatedValues += AddNullableBoolValueToIni("Render.OSD", "ShowInVideoCapture", ini, specialKProfile.Render.OSD.ShowInVideoCapture);
            updatedValues += AddStringValueToIni("Import.ReShade64", "Architecture", ini, specialKProfile.Import.ReShade64.Architecture);
            updatedValues += AddStringValueToIni("Import.ReShade64", "Role", ini, specialKProfile.Import.ReShade64.Role);
            updatedValues += AddStringValueToIni("Import.ReShade64", "When", ini, specialKProfile.Import.ReShade64.When);
            updatedValues += AddStringValueToIni("Import.ReShade64", "Filename", ini, specialKProfile.Import.ReShade64.Filename);
            updatedValues += AddStringValueToIni("Import.ReShade32", "Architecture", ini, specialKProfile.Import.ReShade32.Architecture);
            updatedValues += AddStringValueToIni("Import.ReShade32", "Role", ini, specialKProfile.Import.ReShade32.Role);
            updatedValues += AddStringValueToIni("Import.ReShade32", "When", ini, specialKProfile.Import.ReShade32.When);
            updatedValues += AddStringValueToIni("Import.ReShade32", "Filename", ini, specialKProfile.Import.ReShade32.Filename);
            specialKProfile.UpdateReshadeStatus();

            if (updatedValues > 0)
            {
                iniParser.WriteFile(specialKProfile.ProfileIniPath, ini, Encoding.UTF8);
            }

            playniteApi.Dialogs.ShowMessage($"Special K profile \"{specialKProfile.ProfileName}\" updated.");
            return true;
        }

        private int AddStringValueToIni(string key, string subKey, IniData ini, string newValue)
        {
            if (newValue.IsNullOrEmpty())
            {
                return 0;
            }

            var oldValue = ini[key][subKey];
            if (oldValue.IsNullOrEmpty() || oldValue != newValue)
            {
                ini[key][subKey] = newValue;
                return 1;
            }

            return 0;
        }

        private int AddDoubleValueToIni(string key, string subKey, IniData ini, double newValue)
        {
            var oldValue = ini[key][subKey];
            if (oldValue == null || double.TryParse(ini[key][subKey], out var oldValueParsed) && oldValueParsed != newValue)
            {
                ini[key][subKey] = newValue.ToString();
                return 1;
            }

            return 0;
        }

        private int AddNullableBoolValueToIni(string key, string subKey, IniData ini, bool? newValue)
        {
            if (newValue == null)
            {
                return 0;
            }

            var oldValue = ini[key][subKey];
            var newValueCasted = (bool)newValue;
            if (oldValue == null || bool.TryParse(ini[key][subKey], out var oldValueParsed) && oldValueParsed != newValueCasted)
            {
                ini[key][subKey] = newValueCasted.ToString().ToLower();
                return 1;
            }

            return 0;
        }

        private double GetDoubleOrZeroFromString(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return 0.0;
            }

            if (double.TryParse(str, out var doubleValue))
            {
                return doubleValue;
            }

            return 0.0;
        }

        public bool? GetNullableBoolFromString(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            if (bool.TryParse(str, out var boolValue))
            {
                return boolValue;
            }

            return null;
        }
    }
}