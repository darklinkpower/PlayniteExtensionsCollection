using Playnite.SDK;
using Playnite.SDK.Data;
using SteamWishlistDiscountNotifier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier
{
    public class SteamWishlistDiscountNotifierSettings : ObservableObject
    {
        private DateTime lastWishlistUpdate = DateTime.MinValue;
        public DateTime LastWishlistUpdate { get => lastWishlistUpdate; set => SetValue(ref lastWishlistUpdate, value); }
        private bool openUrlsInBrowser = false;
        public bool OpenUrlsInBrowser { get => openUrlsInBrowser; set => SetValue(ref openUrlsInBrowser, value); }
        private int wishlistAutoCheckIntervalMins = 60;
        public int WishlistAutoCheckIntervalMins { get => wishlistAutoCheckIntervalMins; set => SetValue(ref wishlistAutoCheckIntervalMins, value); }
        public string SteamId = null;
    }

    public class SteamWishlistDiscountNotifierSettingsViewModel : ObservableObject, ISettings
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly SteamWishlistDiscountNotifier plugin;
        private SteamWishlistDiscountNotifierSettings editingClone { get; set; }

        private SteamWishlistDiscountNotifierSettings settings;
        public SteamWishlistDiscountNotifierSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public AuthStatus AuthStatus
        {
            get
            {
                if (!Settings.SteamId.IsNullOrEmpty())
                {
                    return AuthStatus.Ok;
                }

                return AuthStatus.AuthRequired;
            }
        }

        public SteamWishlistDiscountNotifierSettingsViewModel(SteamWishlistDiscountNotifier plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<SteamWishlistDiscountNotifierSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SteamWishlistDiscountNotifierSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            using (var webView = plugin.PlayniteApi.WebViews.CreateOffscreenView())
            {
                Settings.SteamId = SteamLogin.GetLoggedInSteamId64(webView);
            }
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

        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Login();
            });
        }

        private void Login()
        {
            try
            {
                var steamId = string.Empty;
                using (var view = plugin.PlayniteApi.WebViews.CreateView(675, 440))
                {
                    view.LoadingChanged += async (s, e) =>
                    {
                        var address = view.GetCurrentAddress();
                        if (address.Contains(@"steamcommunity.com"))
                        {
                            var source = await view.GetPageSourceAsync();
                            var idMatch = Regex.Match(source, @"g_steamID = ""(\d+)""");
                            if (idMatch.Success)
                            {
                                steamId = idMatch.Groups[1].Value;
                            }
                            else
                            {
                                idMatch = Regex.Match(source, @"steamid"":""(\d+)""");
                                if (idMatch.Success)
                                {
                                    steamId = idMatch.Groups[1].Value;
                                }
                            }

                            if (idMatch.Success)
                            {
                                view.Close();
                            }
                        }
                    };

                    view.DeleteDomainCookies(".steamcommunity.com");
                    view.DeleteDomainCookies("steamcommunity.com");
                    Settings.SteamId = null;
                    view.Navigate(@"https://steamcommunity.com/login/home/?goto=");
                    view.OpenDialog();
                }

                if (!steamId.IsNullOrEmpty())
                {
                    Settings.SteamId = steamId;
                }

                OnPropertyChanged(nameof(AuthStatus));
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}