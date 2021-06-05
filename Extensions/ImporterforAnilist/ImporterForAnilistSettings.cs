using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist
{
    public class ImporterForAnilistSettings : ISettings
    {
        private readonly ImporterForAnilist plugin;

        public string AccountAccessCode { get; set; } = string.Empty;
        public string PropertiesPrefix { get; set; } = string.Empty;
        public bool ImportAnimeLibrary { get; set; } = true;
        public bool ImportMangaLibrary { get; set; } = true;
        public bool UpdateUserScoreOnLibUpdate { get; set; } = true;
        public bool UpdateCompletionStatusOnLibUpdate { get; set; } = true;
        public bool UpdateProgressOnLibUpdate { get; set; } = false;


        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonIgnore` ignore attribute.
        [JsonIgnore]
        public bool OptionThatWontBeSaved { get; set; } = false;

        // Parameterless constructor must exist if you want to use LoadPluginSettings method.
        public ImporterForAnilistSettings()
        {
        }

        public ImporterForAnilistSettings(ImporterForAnilist plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ImporterForAnilistSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                AccountAccessCode = savedSettings.AccountAccessCode;
                PropertiesPrefix = savedSettings.PropertiesPrefix;
                ImportAnimeLibrary = savedSettings.ImportAnimeLibrary;
                ImportMangaLibrary = savedSettings.ImportMangaLibrary;
                UpdateUserScoreOnLibUpdate = savedSettings.UpdateUserScoreOnLibUpdate;
                UpdateCompletionStatusOnLibUpdate = savedSettings.UpdateCompletionStatusOnLibUpdate;
                UpdateProgressOnLibUpdate = savedSettings.UpdateProgressOnLibUpdate;
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Login();
            });
        }

        private void Login()
        {
            string url = string.Format("https://anilist.co/api/v2/oauth/authorize?client_id={0}&response_type=token", "5706");
            System.Diagnostics.Process.Start(url);
        }
    }
}