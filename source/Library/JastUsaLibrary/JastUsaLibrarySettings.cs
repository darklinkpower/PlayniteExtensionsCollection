using JastUsaLibrary.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace JastUsaLibrary
{
    public class JastUsaLibrarySettings : ObservableObject
    {
        private string downloadsPath = string.Empty;
        public string DownloadsPath { get => downloadsPath; set => SetValue(ref downloadsPath, value); }
        private bool extractDownloadedZips = true;
        public bool ExtractDownloadedZips { get => extractDownloadedZips; set => SetValue(ref extractDownloadedZips, value); }
        private bool deleteDownloadedZips = false;
        public bool DeleteDownloadedZips { get => deleteDownloadedZips; set => SetValue(ref deleteDownloadedZips, value); }
    }

    public class JastUsaLibrarySettingsViewModel : ObservableObject, ISettings
    {
        private readonly JastUsaLibrary plugin;
        private JastUsaLibrarySettings editingClone { get; set; }

        private JastUsaLibrarySettings settings;
        private readonly IPlayniteAPI playniteApi;
        private readonly JastUsaAccountClient accountClient;

        public JastUsaLibrarySettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private bool? isUserLoggedIn = null;
        public bool? IsUserLoggedIn
        {
            get
            {
                if (isUserLoggedIn == null)
                {
                    isUserLoggedIn = accountClient.GetIsUserLoggedIn();
                }

                return isUserLoggedIn;
            }
            set
            {
                isUserLoggedIn = value;
                OnPropertyChanged();
            }
        }

        private string loginEmail = string.Empty;
        public string LoginEmail
        {
            get => loginEmail;
            set
            {
                loginEmail = value;
                OnPropertyChanged();
            }
        }

        public JastUsaLibrarySettingsViewModel(JastUsaLibrary plugin, IPlayniteAPI api, JastUsaAccountClient accountClient)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<JastUsaLibrarySettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new JastUsaLibrarySettings();
            }

            playniteApi = api;
            this.accountClient = accountClient;
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            isUserLoggedIn = null;
            LoginEmail = string.Empty;
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

        public RelayCommand<PasswordBox> LoginCommand
        {
            get => new RelayCommand<PasswordBox>((a) =>
            {
                Login(a);
            });
        }

        private void Login(PasswordBox passwordBox)
        {
            if (!LoginEmail.IsNullOrEmpty() && !passwordBox.Password.IsNullOrEmpty())
            {
                isUserLoggedIn = null;
                IsUserLoggedIn = accountClient.Login(LoginEmail, passwordBox.Password);
            }
        }

        public RelayCommand SelectDownloadDirectoryCommand
        {
            get => new RelayCommand(() =>
            {
                var selectedDir = playniteApi.Dialogs.SelectFolder();
                if (!selectedDir.IsNullOrEmpty())
                {
                    settings.DownloadsPath = selectedDir;
                }
            });
        }
    }
}