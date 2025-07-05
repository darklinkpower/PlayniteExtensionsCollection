using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewViewer
{
    public sealed class SettingsChangedInfo<T>
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public T OldSettings { get; }
        public T NewSettings { get; }

        public SettingsChangedInfo(T oldSettings, T newSettings)
        {
            if (oldSettings == null) throw new ArgumentNullException(nameof(oldSettings));
            if (newSettings == null) throw new ArgumentNullException(nameof(newSettings));

            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            OldSettings = oldSettings;
            NewSettings = newSettings;
        }
    }
}
