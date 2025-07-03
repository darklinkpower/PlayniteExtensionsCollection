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

        public SteamLanguageSelectorViewModel(HashSet<SteamLanguage> selectedLanguages, Window window)
        {
            _window = window;

            Languages = new ObservableCollection<LanguageOptionEntry>(
                Enum.GetValues(typeof(SteamLanguage))
                    .Cast<SteamLanguage>()
                    .Select(lang => new LanguageOptionEntry
                    {
                        Language = lang,
                        DisplayName = lang.ToString().SeparateByCapital(),
                        IsSelected = selectedLanguages.Contains(lang)
                    }));

            ConfirmCommand = new RelayCommand(() => Confirm(selectedLanguages));
            CancelCommand = new RelayCommand(() => Cancel());
        }

        private void Confirm(HashSet<SteamLanguage> selectedLanguages)
        {
            selectedLanguages.Clear();
            foreach (var optionEntry in Languages.Where(x => x.IsSelected))
            {
                selectedLanguages.Add(optionEntry.Language);
            }

            DialogResult = true;
            _window.Close();
        }

        private void Cancel()
        {
            _window.Close();
        }


    }
}
