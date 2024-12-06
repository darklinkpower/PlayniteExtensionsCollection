using GOGSecondClassGameWatcher.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GOGSecondClassGameWatcher.Presentation
{
    public class GogSecondClassGameDisplayViewModel
    {
        public string ImagePath { get; }
        public string Title { get; }
        public string Developer { get; }
        public string Publisher { get; }

        public string NumberOfIssues { get; }

        // General Issues
        public Visibility GeneralIssuesVisibility { get; }
        public string MissingUpdates { get; }
        public Visibility MissingUpdatesVisibility { get; }
        public string MissingLanguages { get; }
        public Visibility MissingLanguagesVisibility { get; }
        public string MissingFreeDlc { get; }
        public Visibility MissingFreeDlcVisibility { get; }
        public string MissingPaidDlc { get; }
        public Visibility MissingPaidDlcVisibility { get; }
        public string MissingFeatures { get; }
        public Visibility MissingFeaturesVisibility { get; }
        public string MissingSoundtrack { get; }
        public Visibility MissingSoundtrackVisibility { get; }
        public string OtherIssues { get; }
        public Visibility OtherIssuesVisibility { get; }
        public string MissingBuilds { get; }
        public Visibility MissingBuildsVisibility { get; }
        public string RegionLocking { get; }
        public Visibility RegionLockingVisibility { get; }
        public string GeneralIssuesSourceOne { get; }
        public Visibility GeneralIssuesSourceOneVisibility { get; }
        public string GeneralIssuesSourceTwo { get; }
        public Visibility GeneralIssuesSourceTwoVisibility { get; }

        // Achievements Issues
        public Visibility AchievementsIssuesVisibility { get; }
        public string MissingAllAchievements { get; }
        public Visibility MissingAllAchievementsVisibility { get; }
        public string MissingSomeAchievements { get; }
        public Visibility MissingSomeAchievementsVisibility { get; }
        public string BrokenAchievements { get; }
        public Visibility BrokenAchievementsVisibility { get; }
        public string AchievementsAskedResponse { get; }
        public Visibility AchievementsAskedResponseVisibility { get; }
        public string AchievementsIssuesSource { get; }
        public Visibility AchievementsIssuesSourceVisibility { get; }

        public GogSecondClassGameDisplayViewModel(GogSecondClassGame gogSecondClassGame, string imagePath = null)
        {
            ImagePath = imagePath;
            Title = gogSecondClassGame.Title;
            Developer = gogSecondClassGame.Developer;
            Publisher = gogSecondClassGame.Publisher;
            NumberOfIssues = gogSecondClassGame.TotalIssues.ToString();

            // General Issues
            GeneralIssuesVisibility = gogSecondClassGame.GeneralIssues.GetIssuesCount() > 0 ? Visibility.Visible : Visibility.Collapsed;

            MissingUpdates = JoinList(gogSecondClassGame.GeneralIssues.MissingUpdates);
            MissingUpdatesVisibility = GetVisibility(MissingUpdates);

            MissingLanguages = JoinList(gogSecondClassGame.GeneralIssues.MissingLanguages);
            MissingLanguagesVisibility = GetVisibility(MissingLanguages);

            MissingFreeDlc = JoinList(gogSecondClassGame.GeneralIssues.MissingFreeDlc);
            MissingFreeDlcVisibility = GetVisibility(MissingFreeDlc);

            MissingPaidDlc = JoinList(gogSecondClassGame.GeneralIssues.MissingPaidDlc);
            MissingPaidDlcVisibility = GetVisibility(MissingPaidDlc);

            MissingFeatures = JoinList(gogSecondClassGame.GeneralIssues.MissingFeatures);
            MissingFeaturesVisibility = GetVisibility(MissingFeatures);

            MissingSoundtrack = JoinList(gogSecondClassGame.GeneralIssues.MissingSoundtrack);
            MissingSoundtrackVisibility = GetVisibility(MissingSoundtrack);

            OtherIssues = JoinList(gogSecondClassGame.GeneralIssues.OtherIssues);
            OtherIssuesVisibility = GetVisibility(OtherIssues);

            MissingBuilds = JoinList(gogSecondClassGame.GeneralIssues.MissingBuilds);
            MissingBuildsVisibility = GetVisibility(MissingBuilds);

            RegionLocking = JoinList(gogSecondClassGame.GeneralIssues.RegionLocking);
            RegionLockingVisibility = GetVisibility(RegionLocking);

            GeneralIssuesSourceOne = JoinList(gogSecondClassGame.GeneralIssues.SourceOne);
            GeneralIssuesSourceOneVisibility = GetVisibility(GeneralIssuesSourceOne);

            GeneralIssuesSourceTwo = JoinList(gogSecondClassGame.GeneralIssues.SourceTwo);
            GeneralIssuesSourceTwoVisibility = GetVisibility(GeneralIssuesSourceTwo);

            // Achievements Issues
            AchievementsIssuesVisibility = gogSecondClassGame.AchievementsIssues.GetIssuesCount() > 0 ? Visibility.Visible : Visibility.Collapsed;
            MissingAllAchievements = gogSecondClassGame.AchievementsIssues.MissingAllAchievements;
            MissingAllAchievementsVisibility = GetVisibility(MissingAllAchievements);

            MissingSomeAchievements = gogSecondClassGame.AchievementsIssues.MissingSomeAchievements;
            MissingSomeAchievementsVisibility = GetVisibility(MissingSomeAchievements);

            BrokenAchievements = JoinList(gogSecondClassGame.AchievementsIssues.BrokenAchievements);
            BrokenAchievementsVisibility = GetVisibility(BrokenAchievements);

            AchievementsAskedResponse = gogSecondClassGame.AchievementsIssues.AchievementsAskedResponse;
            AchievementsAskedResponseVisibility = GetVisibility(AchievementsAskedResponse);

            AchievementsIssuesSource = gogSecondClassGame.AchievementsIssues.Source;
            AchievementsIssuesSourceVisibility = GetVisibility(AchievementsIssuesSource);
        }

        private static string JoinList(IEnumerable<string> list)
        {
            return list != null && list.Any() ? string.Join("\n", list) : string.Empty;
        }

        private static Visibility GetVisibility(string content)
        {
            return string.IsNullOrEmpty(content) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}