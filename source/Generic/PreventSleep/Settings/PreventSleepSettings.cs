using EventsCommon;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreventSleep.Settings
{
    public class PreventSleepSettings : ObservableObject
    {
        private bool _showSwitchModeItemOnTopPanel = true;
        public bool ShowSwitchModeItemOnTopPanel { get => _showSwitchModeItemOnTopPanel; set => SetValue(ref _showSwitchModeItemOnTopPanel, value); }
    }

    public class PreventSleepSettingsViewModel : ObservableObject, ISettings
    {
        private readonly PreventSleep _plugin;
        private readonly EventAggregator _eventAggregator;
        private PreventSleepSettings _editingClone;

        private PreventSleepSettings _settings;
        public PreventSleepSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public PreventSleepSettingsViewModel(PreventSleep plugin, EventAggregator eventAggregator)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            _plugin = plugin;
            _eventAggregator = eventAggregator;
            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<PreventSleepSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new PreventSleepSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            _editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = _editingClone;
        }

        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);
            _eventAggregator.Publish(new OnSettingsChangedEvent(Settings));
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}