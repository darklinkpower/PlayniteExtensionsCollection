using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUtilitiesCommon.Settings
{
    public class SettingsViewModel<TSettings> : ObservableObject, ISettings
        where TSettings : class, new()
    {
        private readonly SettingsManager<TSettings> _settingsManager;
        private TSettings _editingSettings;

        public TSettings Settings
        {
            get => _editingSettings;
            private set
            {
                _editingSettings = value;
                OnPropertyChanged();
            }
        }

        public SettingsViewModel(SettingsManager<TSettings> settingsManager)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            Settings = _settingsManager.GetEditableSettings();
        }

        public void BeginEdit()
        {

        }

        public void CancelEdit()
        {
            
        }

        public void EndEdit()
        {
            _settingsManager.Save(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        public void RegisterOnSettingsSaved(Action<SettingsChangedEventArgs<TSettings>> callback)
        {
            _settingsManager.OnSettingsSaved += callback;
        }
    }
}
