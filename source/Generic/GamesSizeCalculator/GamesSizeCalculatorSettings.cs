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
        public DateTime LastRefreshOnLibUpdate = DateTime.Now;
        
        private bool calculateNewGamesOnLibraryUpdate = true;
        public bool CalculateNewGamesOnLibraryUpdate { get => calculateNewGamesOnLibraryUpdate; set => SetValue(ref calculateNewGamesOnLibraryUpdate, value); }
        
        private bool calculateModifiedGamesOnLibraryUpdate = true;
        public bool CalculateModifiedGamesOnLibraryUpdate { get => calculateModifiedGamesOnLibraryUpdate; set => SetValue(ref calculateModifiedGamesOnLibraryUpdate, value); }
        private bool getUninstalledGameSizeFromSteam = true;
        public bool GetUninstalledGameSizeFromSteam { get => getUninstalledGameSizeFromSteam; set => SetValue(ref getUninstalledGameSizeFromSteam, value); }
        private bool getSizeFromSteamNonSteamGames = true;
        public bool GetSizeFromSteamNonSteamGames { get => getSizeFromSteamNonSteamGames; set => SetValue(ref getSizeFromSteamNonSteamGames, value); }
        private bool includeDlcInSteamCalculation = false;
        public bool IncludeDlcInSteamCalculation { get => includeDlcInSteamCalculation; set => SetValue(ref includeDlcInSteamCalculation, value); }
        private bool includeOptionalInSteamCalculation = false;
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