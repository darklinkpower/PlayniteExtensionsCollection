using Playnite.SDK;
using Playnite.SDK.Data;
using SteamWishlistDiscountNotifier.Application.Steam.Login;
using SteamWishlistDiscountNotifier.Domain.Enums;
using SteamWishlistDiscountNotifier.Domain.Events;
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
        private bool enablePriceChangesNotifications = true;
        public bool EnablePriceChangesNotifications { get => enablePriceChangesNotifications; set => SetValue(ref enablePriceChangesNotifications, value); }
        private bool enableNewReleasesNotifications = true;
        public bool EnableNewReleasesNotifications { get => enableNewReleasesNotifications; set => SetValue(ref enableNewReleasesNotifications, value); }
        private bool openUrlsInBrowser = false;
        public bool OpenUrlsInBrowser { get => openUrlsInBrowser; set => SetValue(ref openUrlsInBrowser, value); }
        private int databaseVersion = 0;
        public int DatabaseVersion { get => databaseVersion; set => SetValue(ref databaseVersion, value); }

        private bool notificationDisplayOwnedSources = true;
        public bool NotificationDisplayOwnedSources { get => notificationDisplayOwnedSources; set => SetValue(ref notificationDisplayOwnedSources, value); }

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
        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;
        private readonly SteamWishlistDiscountNotifier _plugin;
        private readonly SteamLoginService _steamLoginService;
        private SteamWishlistDiscountNotifierSettings _editingClone;
        private SteamWishlistDiscountNotifierSettings _settings;
        public SteamWishlistDiscountNotifierSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
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

                var accountInfo = _steamLoginService.GetLoggedInStatus();
                CheckedStatus = accountInfo.AuthStatus;
                return accountInfo.AuthStatus;
            }
        }

        public RelayCommand LoginCommand { get; }

        public SteamWishlistDiscountNotifierSettingsViewModel(SteamWishlistDiscountNotifier plugin, SteamLoginService steamLoginService)
        {
            _plugin = plugin;
            _steamLoginService = steamLoginService;
            var savedSettings = plugin.LoadPluginSettings<SteamWishlistDiscountNotifierSettings>();
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new SteamWishlistDiscountNotifierSettings();
            }

            CheckedStatus = AuthStatus.Checking;
            LoginCommand = new RelayCommand(() => Login());
        }

        public void BeginEdit()
        {
            _editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = _editingClone;
        }

        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(_editingClone, Settings));
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        private void Login()
        {
            CheckedStatus = _steamLoginService.Login();
            OnPropertyChanged(nameof(AuthStatus));
        }
    }
}