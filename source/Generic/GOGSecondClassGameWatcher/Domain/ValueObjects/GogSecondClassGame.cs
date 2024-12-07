using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.ValueObjects
{
    [ProtoContract]
    public class GogSecondClassGame
    {
        [ProtoMember(1)]
        public string Title { get; set; }
        [ProtoMember(2)]
        public string Developer { get; set; }
        [ProtoMember(3)]
        public string Publisher { get; set; }
        [ProtoMember(4)]
        public string Id { get; set; }
        [ProtoMember(5)]
        public GeneralIssues GeneralIssues { get; set; }
        [ProtoMember(6)]
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