using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.ValueObjects
{
    public class GogSecondClassGame
    {
        public string Title { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Id { get; set; }
        public GeneralIssues GeneralIssues { get; set; }
        public AchievementsIssues AchievementsIssues { get; set; }

        public int TotalIssues => GeneralIssues.GetIssuesCount() + AchievementsIssues.GetIssuesCount();

        // Needed for Serializers
        public GogSecondClassGame() { }

        public GogSecondClassGame(string title, string developer, string publisher, string id, GeneralIssues generalIssues, AchievementsIssues achievementsIssues)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Developer = developer;
            Publisher = publisher;
            Id = id;
            GeneralIssues = generalIssues;
            AchievementsIssues = achievementsIssues;
        }
    }


}