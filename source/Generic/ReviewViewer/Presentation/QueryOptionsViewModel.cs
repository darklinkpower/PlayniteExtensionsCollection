using System;
using System.Collections.Generic;
using System.Linq;
using ReviewViewer.Domain;

namespace ReviewViewer.Presentation
{
    public class QueryOptionsViewModel : ObservableObject
    {
        public ReviewType ReviewType
        {
            get => _reviewType;
            set => SetValue(ref _reviewType, value);
        }
        private ReviewType _reviewType = ReviewType.All;

        public PurchaseType PurchaseType
        {
            get => _purchaseType;
            set => SetValue(ref _purchaseType, value);
        }
        private PurchaseType _purchaseType = PurchaseType.All;

        public string Language
        {
            get => _language;
            set => SetValue(ref _language, value);
        }
        private string _language = "english";

        public DateRangeMode DateRangeMode
        {
            get => _dateRangeMode;
            set => SetValue(ref _dateRangeMode, value);
        }
        private DateRangeMode _dateRangeMode = DateRangeMode.Lifetime;

        public DateTime? StartDateUtc
        {
            get => _startDateUtc;
            set => SetValue(ref _startDateUtc, value);
        }
        private DateTime? _startDateUtc;

        public DateTime? EndDateUtc
        {
            get => _endDateUtc;
            set => SetValue(ref _endDateUtc, value);
        }
        private DateTime? _endDateUtc;

        public PlaytimePreset PlaytimePreset
        {
            get => _playtimePreset;
            set => SetValue(ref _playtimePreset, value);
        }
        private PlaytimePreset _playtimePreset = PlaytimePreset.None;

        public int CustomPlaytimeMinHours
        {
            get => _customPlaytimeMinHours;
            set
            {
                value = Clamp(value, 0, 100);
                if (value > CustomPlaytimeMaxHours)
                {
                    CustomPlaytimeMaxHours = value;
                }

                SetValue(ref _customPlaytimeMinHours, value);
            }
        }

        private int _customPlaytimeMinHours = 0;

        public int CustomPlaytimeMaxHours
        {
            get => _customPlaytimeMaxHours;
            set
            {
                value = Clamp(value, 0, 100);
                if (value < CustomPlaytimeMinHours)
                {
                    CustomPlaytimeMinHours = value;
                }

                SetValue(ref _customPlaytimeMaxHours, value);
            }
        }

        private int _customPlaytimeMaxHours = 0;

        public PlaytimeDevice PlaytimeDevice
        {
            get => _playtimeDevice;
            set => SetValue(ref _playtimeDevice, value);
        }
        private PlaytimeDevice _playtimeDevice = PlaytimeDevice.All;

        public DisplayType Display
        {
            get => _display;
            set => SetValue(ref _display, value);
        }
        private DisplayType _display = DisplayType.MostHelpful;

        public LanguageSelectionMode LanguageSelectionMode
        {
            get => _languageSelectionMode;
            set => SetValue(ref _languageSelectionMode, value);
        }
        private LanguageSelectionMode _languageSelectionMode = LanguageSelectionMode.Custom;

        public HashSet<SteamLanguage> SelectedLanguages
        {
            get => _selectedLanguages;
            set => SetValue(ref _selectedLanguages, value);
        }
        private HashSet<SteamLanguage> _selectedLanguages = new HashSet<SteamLanguage> { SteamLanguage.English };

        public bool UseHelpfulSystem
        {
            get => _useHelpfulSystem;
            set => SetValue(ref _useHelpfulSystem, value);
        }
        private bool _useHelpfulSystem = true;

        public bool FilterOfftopicActivity
        {
            get => _filterOfftopicActivity;
            set => SetValue(ref _filterOfftopicActivity, value);
        }
        private bool _filterOfftopicActivity = true;

        public static QueryOptionsViewModel FromDomain(QueryOptions domain)
        {
            return new QueryOptionsViewModel
            {
                ReviewType = domain.ReviewType,
                PurchaseType = domain.PurchaseType,
                Language = domain.Language,
                DateRangeMode = domain.DateRangeMode,
                StartDateUtc = domain.StartDateUtc,
                EndDateUtc = domain.EndDateUtc,
                PlaytimePreset = domain.PlaytimePreset,
                CustomPlaytimeMinHours = domain.CustomPlaytimeMinHours,
                CustomPlaytimeMaxHours = domain.CustomPlaytimeMaxHours,
                PlaytimeDevice = domain.PlaytimeDevice,
                Display = domain.Display,
                LanguageSelectionMode = domain.LanguageSelectionMode,
                SelectedLanguages = domain.SelectedLanguages != null
                    ? new HashSet<SteamLanguage>(domain.SelectedLanguages)
                    : new HashSet<SteamLanguage>(),
                UseHelpfulSystem = domain.UseHelpfulSystem,
                FilterOfftopicActivity = domain.FilterOfftopicActivity
            };
        }

        public QueryOptions ToDomain()
        {
            return new QueryOptions
            {
                ReviewType = this.ReviewType,
                PurchaseType = this.PurchaseType,
                Language = this.Language,
                DateRangeMode = this.DateRangeMode,
                StartDateUtc = this.StartDateUtc,
                EndDateUtc = this.EndDateUtc,
                PlaytimePreset = this.PlaytimePreset,
                CustomPlaytimeMinHours = this.CustomPlaytimeMinHours,
                CustomPlaytimeMaxHours = this.CustomPlaytimeMaxHours,
                PlaytimeDevice = this.PlaytimeDevice,
                Display = this.Display,
                LanguageSelectionMode = this.LanguageSelectionMode,
                SelectedLanguages = this.SelectedLanguages != null
                    ? new HashSet<SteamLanguage>(this.SelectedLanguages)
                    : new HashSet<SteamLanguage>(),
                UseHelpfulSystem = this.UseHelpfulSystem,
                FilterOfftopicActivity = this.FilterOfftopicActivity
            };
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

}
