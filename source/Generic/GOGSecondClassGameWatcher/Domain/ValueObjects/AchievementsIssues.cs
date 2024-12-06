using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.ValueObjects
{
    public class AchievementsIssues
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string ReleaseYear { get; set; }
        public string MissingAllAchievements { get; set; }
        public string MissingSomeAchievements { get; set; }
        public List<string> BrokenAchievements { get; set; }
        public string AchievementsAskedResponse { get; set; }  // Reflects response from developers if achievements could be implemented
        public string Source { get; set; }

        public AchievementsIssues(
            string title,
            string id,
            string developer,
            string publisher,
            string releaseYear,
            string missingAllAchievements,
            string missingSomeAchievements,
            List<string> brokenAchievements,
            string achievementsAskedResponse,
            string source)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Developer = developer ?? throw new ArgumentNullException(nameof(developer));
            Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            ReleaseYear = releaseYear;
            MissingAllAchievements = missingAllAchievements;
            MissingSomeAchievements = missingSomeAchievements;
            BrokenAchievements = brokenAchievements ?? new List<string>();
            AchievementsAskedResponse = achievementsAskedResponse ?? throw new ArgumentNullException(nameof(achievementsAskedResponse));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public static AchievementsIssues Empty => new AchievementsIssues(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            new List<string>(),
            string.Empty,
            string.Empty
        );

        public int GetIssuesCount()
        {
            int count = 0;
            if (!string.IsNullOrEmpty(MissingAllAchievements)) count++;
            if (!string.IsNullOrEmpty(MissingSomeAchievements)) count++;
            if (BrokenAchievements.Count > 0) count++;
            return count;
        }
    }
}
