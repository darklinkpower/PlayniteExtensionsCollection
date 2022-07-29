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
        private bool enableWishlistNotifications = true;
        public bool EnableWishlistNotifications { get => enableWishlistNotifications; set => SetValue(ref enableWishlistNotifications, value); }
        private bool openUrlsInBrowser = false;
        public bool OpenUrlsInBrowser { get => openUrlsInBrowser; set => SetValue(ref openUrlsInBrowser, value); }
        private int databaseVersion = 0;
        public int DatabaseVersion { get => databaseVersion; set => SetValue(ref databaseVersion, value); }
        private int wishlistAutoCheckIntervalMins = 60;
        public int WishlistAutoCheckIntervalMins { get => wishlistAutoCheckIntervalMins; set => SetValue(ref wishlistAutoCheckIntervalMins, value); }
        private int notificationMinDiscount = 1;
        public int NotificationMinDiscount { get => notificationMinDiscount; set => SetValue(ref notificationMinDiscount, value); }
        private bool notifyDiscountsTypeGame = true;
        public bool NotifyDiscountsTypeGame { get => notifyDiscountsTypeGame; set => SetValue(ref notifyDiscountsTypeGame, value); }
        private bool notifyDiscountsTypeDlc = true;
        public bool NotifyDiscountsTypeDlc { get => notifyDiscountsTypeDlc; set => SetValue(ref notifyDiscountsTypeDlc, value); }
        private bool notifyDiscountsTypeMusic = true;
        public bool NotifyDiscountsTypeMusic { get => notifyDiscountsTypeMusic; set => SetValue(ref notifyDiscountsTypeMusic, value); }
        private bool notifyDiscountsTypeApplication = true;
        public bool NotifyDiscountsTypeApplication { get => notifyDiscountsTypeApplication; set => SetValue(ref notifyDiscountsTypeApplication, value); }
        private bool notifyDiscountsTypeVideo = true;
        public bool NotifyDiscountsTypeVideo { get => notifyDiscountsTypeVideo; set => SetValue(ref notifyDiscountsTypeVideo, value); }
        private bool notifyDiscountsTypeHardware = true;
        public bool NotifyDiscountsTypeHardware { get => notifyDiscountsTypeHardware; set => SetValue(ref notifyDiscountsTypeHardware, value); }
        private bool notifyDiscountsTypeMod = true;
        public bool NotifyDiscountsTypeMod { get => notifyDiscountsTypeMod; set => SetValue(ref notifyDiscountsTypeMod, value); }
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

        public AuthStatus CheckedStatus = AuthStatus.Checking;
        public AuthStatus AuthStatus
        {
            get
            {
                if (CheckedStatus != AuthStatus.Checking)
                {
                    return CheckedStatus;
                }

                using (var webView = plugin.PlayniteApi.WebViews.CreateOffscreenView())
                {
                    SteamLogin.GetLoggedInSteamId64(webView, out var status, out var steamId);
                    CheckedStatus = status;
                    return status;
                }
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

            CheckedStatus = AuthStatus.Checking;
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
                var status = AuthStatus.AuthRequired;
                using (var view = plugin.PlayniteApi.WebViews.CreateView(675, 440))
                {
                    view.LoadingChanged += async (s, e) =>
                    {
                        var address = view.GetCurrentAddress();
                        if (address.IsNullOrEmpty())
                        {
                            status = AuthStatus.NoConnection;
                            view.Close();
                        }
                        else if (address.Contains(@"steampowered.com"))
                        {
                            var source = await view.GetPageSourceAsync();
                            if (source == @"<html><head></head><body></body></html>")
                            {
                                status = AuthStatus.NoConnection;
                                view.Close();
                            }

                            var idMatch = Regex.Match(source, @"<div class=""youraccount_steamid"">[^\d]+(\d+)");
                            if (idMatch.Success)
                            {
                                status = AuthStatus.Ok;
                                view.Close();
                            }
                        }
                    };

                    view.DeleteDomainCookies(".steamcommunity.com");
                    view.DeleteDomainCookies("steamcommunity.com");
                    view.DeleteDomainCookies("store.steampowered.com");
                    view.Navigate(@"https://store.steampowered.com/login/?redir=account%2F&redir_ssl=1");
                    view.OpenDialog();
                }

                CheckedStatus = status;
                OnPropertyChanged(nameof(AuthStatus));
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}