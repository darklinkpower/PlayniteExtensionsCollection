using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesSizeCalculator
{
    public class GamesSizeCalculatorSettings : ObservableObject
    {
        private bool calculateOnGameClose = true;

        public bool CalculateOnGameClose { get => calculateOnGameClose; set => SetValue(ref calculateOnGameClose, value); }

        public DateTime LastRefreshOnLibUpdate = DateTime.Now;
        private bool calculateNewGamesOnLibraryUpdate = true;

        public bool CalculateNewGamesOnLibraryUpdate { get => calculateNewGamesOnLibraryUpdate; set => SetValue(ref calculateNewGamesOnLibraryUpdate, value); }
        private bool calculateModifiedGamesOnLibraryUpdate = true;

        public bool CalculateModifiedGamesOnLibraryUpdate { get => calculateModifiedGamesOnLibraryUpdate; set => SetValue(ref calculateModifiedGamesOnLibraryUpdate, value); }

        private bool getUninstalledGameSizeFromSteam = true;
        private bool includeDlcInSteamCalculation = false;
        private bool includeOptionalInSteamCalculation = false;
        public bool GetUninstalledGameSizeFromSteam { get => getUninstalledGameSizeFromSteam; set => SetValue(ref getUninstalledGameSizeFromSteam, value); }
        public bool IncludeDlcInSteamCalculation { get => includeDlcInSteamCalculation; set => SetValue(ref includeDlcInSteamCalculation, value); }
        public bool IncludeOptionalInSteamCalculation { get => includeOptionalInSteamCalculation; set => SetValue(ref includeOptionalInSteamCalculation, value); }
    }

    public class GamesSizeCalculatorSettingsViewModel : ObservableObject, ISettings
    {
        private readonly GamesSizeCalculator plugin;
        private GamesSizeCalculatorSettings editingClone { get; set; }

        private GamesSizeCalculatorSettings settings;
        public GamesSizeCalculatorSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public GamesSizeCalculatorSettingsViewModel(GamesSizeCalculator plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<GamesSizeCalculatorSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new GamesSizeCalculatorSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}