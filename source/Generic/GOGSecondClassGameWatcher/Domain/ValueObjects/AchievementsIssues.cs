using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.ValueObjects
{
    [ProtoContract]
    public class AchievementsIssues
    {
        [ProtoMember(1)]
        public string Title { get; set; }
        [ProtoMember(2)]
        public string Id { get; set; }
        [ProtoMember(3)]
        public string Developer { get; set; }
        [ProtoMember(4)]
        public string Publisher { get; set; }
        [ProtoMember(5)]
        public string ReleaseYear { get; set; }
        [ProtoMember(6)]
        public string MissingAllAchievements { get; set; }
        [ProtoMember(7)]
        public string MissingSomeAchievements { get; set; }
        [ProtoMember(8)]
        public List<string> BrokenAchievements { get; set; } = new List<string>();
        [ProtoMember(9)]
        public string AchievementsAskedResponse { get; set; }  // Reflects response from developers if achievements could be implemented
        [ProtoMember(10)]
        public string Source { get; set; }

        public AchievementsIssues()
        {

        }

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
