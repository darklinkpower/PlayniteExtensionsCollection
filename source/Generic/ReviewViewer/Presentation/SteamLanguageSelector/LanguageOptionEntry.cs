using System.Collections.Generic;
using ReviewViewer.Domain;

namespace ReviewViewer.Presentation.SteamLanguageSelector
{
    public class LanguageOptionEntry : ObservableObject
    {
        public SteamLanguage Language { get; set; }
        public string DisplayName { get; set; }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetValue(ref isSelected, value);
        }
    }
}
