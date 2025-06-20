using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Playnite.SDK;
using ReviewViewer.Domain;

namespace ReviewViewer.Presentation.SteamLanguageSelector
{
    public class SteamLanguageSelectorViewModel
    {
        public ObservableCollection<LanguageOptionEntry> Languages { get; }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public bool DialogResult { get; private set; } = false;

        private readonly Window _window;

        public SteamLanguageSelectorViewModel(List<SteamLanguage> selectedLanguages, Window window)
        {
            _window = window;

            Languages = new ObservableCollection<LanguageOptionEntry>(
                Enum.GetValues(typeof(SteamLanguage))
                    .Cast<SteamLanguage>()
                    .Select(lang => new LanguageOptionEntry
                    {
                        Language = lang,
                        DisplayName = lang.ToString(),
                        IsSelected = selectedLanguages.Contains(lang)
                    }));

            ConfirmCommand = new RelayCommand(() => Confirm(selectedLanguages));
            CancelCommand = new RelayCommand(() => Cancel());
        }

        private void Confirm(List<SteamLanguage> selectedLanguages)
        {
            selectedLanguages.Clear();
            selectedLanguages.AddRange(Languages.Where(x => x.IsSelected).Select(x => x.Language));
            DialogResult = true;
            _window.Close();
        }

        private void Cancel()
        {
            _window.Close();
        }


    }
}
