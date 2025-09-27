using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUtilitiesCommon.Settings
{
    public class SettingsManager<T> where T : class, new()
    {
        private readonly Plugin _plugin;
        private readonly Func<T, T, bool> _equalsFunc;
        private T _settings;
        private int _deferredSaveNestingLevel = 0;
        private bool _hasPendingChanges = false;

        /// <summary>
        /// Returns current live settings for safe read-only access.
        /// Do not mutate; use GetEditableSettings and Save().
        /// </summary>
        public T CurrentSettings => _settings;

        public SettingsManager(Plugin plugin, Func<T, T, bool> equalsFunc = null)
        {
            _plugin = plugin;
            _equalsFunc = equalsFunc ?? ((a, b) => Equals(a, b));
            _settings = LoadSettings();
        }

        private T LoadSettings()
        {
            var loaded = _plugin.LoadPluginSettings<T>();
            return loaded ?? Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Returns a deep clone of the current settings instance that can be safely modified.
        /// <para>
        /// After making changes, pass the modified clone to <see cref="Save(T)"/> to persist and apply updates.
        /// </para>
        /// </summary>
        public T GetEditableSettings()
        {
            return Serialization.GetClone(_settings);
        }

        /// <summary>
        /// Saves the provided <paramref name="newSettings"/> instance if it differs from the current settings.
        /// <para>
        /// This will persist the changes, update the internal settings reference, and trigger the
        /// <see cref="OnSettingsSaved"/> event if the settings were modified.
        /// </para>
        /// </summary>
        /// <param name="newSettings">A modified settings instance, typically obtained from <see cref="GetEditableSettings"/>.</param>
        public void Save(T newSettings)
        {
            var oldSettings = _settings;
            if (!_equalsFunc(oldSettings, newSettings))
            {
                _plugin.SavePluginSettings(newSettings);
                _settings = newSettings;
                OnSettingsSaved?.Invoke(new SettingsChangedEventArgs<T>(oldSettings, newSettings));
            }
        }

        public void MutateWithoutSave(Action<T> mutate)
        {
            mutate(_settings);
        }

        public void ForceSave()
        {
            _plugin.SavePluginSettings(_settings);
        }

        public void BeginDeferredSave()
        {
            _deferredSaveNestingLevel++;
        }

        public void EndDeferredSave()
        {
            if (_deferredSaveNestingLevel <= 0)
            {
                throw new InvalidOperationException($"{nameof(EndDeferredSave)} called without matching {nameof(BeginDeferredSave)}");
            }

            _deferredSaveNestingLevel--;
            if (_deferredSaveNestingLevel == 0 && _hasPendingChanges)
            {
                Save(_settings);
                _hasPendingChanges = false;
            }
        }

        public IDisposable OpenBufferedEdit(out T editableSettings)
        {
            BeginDeferredSave();
            editableSettings = _settings;
            return new DeferredEditScope(this);
        }

        private class DeferredEditScope : IDisposable
        {
            private readonly SettingsManager<T> _manager;
            private bool _disposed;

            public DeferredEditScope(SettingsManager<T> manager)
            {
                _manager = manager;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _manager.EndDeferredSave();
            }
        }

        public event Action<SettingsChangedEventArgs<T>> OnSettingsSaved;

        private class DeferSaveToken : IDisposable
        {
            private readonly SettingsManager<T> _manager;
            private bool _disposed;

            public DeferSaveToken(SettingsManager<T> manager)
            {
                _manager = manager;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _manager.EndDeferredSave();
                    _disposed = true;
                }
            }
        }
    }
}
