using MdXaml;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PlayNotes
{
    public class PlayNotesSettings : ObservableObject
    {
        private bool _isControlVisible = false;
        public bool IsControlVisible { get => _isControlVisible; set => SetValue(ref _isControlVisible, value); }

        private Style _markdownStyle;
        public Style MarkdownStyle { get => _markdownStyle; set => SetValue(ref _markdownStyle, value); }
    }

    public class PlayNotesSettingsViewModel : ObservableObject, ISettings
    {
        private readonly PlayNotes plugin;
        private PlayNotesSettings editingClone { get; set; }

        private PlayNotesSettings settings;
        public PlayNotesSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public PlayNotesSettingsViewModel(PlayNotes plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<PlayNotesSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new PlayNotesSettings();
            }

            //var Styles = new List<StyleInfo>
            //{
            //    new StyleInfo("Plain", null),
            //    new StyleInfo("Standard", MarkdownStyle.Standard),
            //    new StyleInfo("Compact", MarkdownStyle.Compact),
            //    new StyleInfo("GithubLike", MarkdownStyle.GithubLike),
            //    new StyleInfo("Sasabune", MarkdownStyle.Sasabune),
            //    new StyleInfo("SasabuneStandard", MarkdownStyle.SasabuneStandard),
            //    new StyleInfo("SasabuneCompact", MarkdownStyle.SasabuneCompact)
            //};

            var markdownStyle = MarkdownStyle.Sasabune;
            if (markdownStyle.Resources[typeof(Image)] is Style imageStyle)
            {
                markdownStyle.Resources.Remove(typeof(Image));

                // For some reason nothing of this works to stretch the image so a max width dependency
                /// needs to be set in xaml
                //imageStyle.Setters.Add(new Setter(Image.StretchProperty, Stretch.Uniform));
                //imageStyle.Setters.Add(new Setter(Image.StretchDirectionProperty, StretchDirection.DownOnly));
                //imageStyle.Setters.Add(new Setter(FrameworkElement.MaxWidthProperty, double.PositiveInfinity));
            }

            settings.MarkdownStyle = markdownStyle;
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
    }
}