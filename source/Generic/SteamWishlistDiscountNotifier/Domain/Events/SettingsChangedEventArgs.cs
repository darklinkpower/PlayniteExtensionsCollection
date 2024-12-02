using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.Events
{
    public class SettingsChangedEventArgs
    {
        public Guid Id { get; }
        public DateTime CreatedAtUtc { get; }
        public SteamWishlistDiscountNotifierSettings OldSettings { get; }
        public SteamWishlistDiscountNotifierSettings NewSettings { get; }

        public SettingsChangedEventArgs(SteamWishlistDiscountNotifierSettings oldSettings, SteamWishlistDiscountNotifierSettings newSettings)
        {
            Id = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            OldSettings = oldSettings;
            NewSettings = newSettings;
        }
    }
}