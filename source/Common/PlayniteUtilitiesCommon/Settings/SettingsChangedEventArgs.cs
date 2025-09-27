using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUtilitiesCommon.Settings
{
    public class SettingsChangedEventArgs<T>
    {
        public T OldSettings { get; }
        public T NewSettings { get; }
        public DateTime ChangeTime { get; }
        public Guid Id { get; }

        public SettingsChangedEventArgs(T oldSettings, T newSettings)
        {
            OldSettings = oldSettings;
            NewSettings = newSettings;
            ChangeTime = DateTime.UtcNow;
            Id = Guid.NewGuid();
        }
    }
}
