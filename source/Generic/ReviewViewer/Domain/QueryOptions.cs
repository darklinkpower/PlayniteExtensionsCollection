using System;
using System.Collections.Generic;

namespace ReviewViewer.Domain
{
    public class QueryOptions
    {
        public ReviewType ReviewType { get; set; } = ReviewType.All;
        public PurchaseType PurchaseType { get; set; } = PurchaseType.All;
        public string Language { get; set; } = "english"; // TODO
        public DateRangeMode DateRangeMode { get; set; } = DateRangeMode.Lifetime;
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public PlaytimePreset PlaytimePreset { get; set; } = PlaytimePreset.None;
        public int CustomPlaytimeMinHours { get; set; } = 0;
        public int CustomPlaytimeMaxHours { get; set; } = 0;
        public PlaytimeDevice PlaytimeDevice { get; set; } = PlaytimeDevice.All;
        public DisplayType Display { get; set; } = DisplayType.MostHelpful;
        public LanguageSelectionMode LanguageSelectionMode { get; set; } = LanguageSelectionMode.Custom;
        public HashSet<SteamLanguage> SelectedLanguages { get; set; } = new HashSet<SteamLanguage>();
        public bool UseHelpfulSystem { get; set; } = true;
        public bool FilterOfftopicActivity { get; set; } = true;
    }
}
