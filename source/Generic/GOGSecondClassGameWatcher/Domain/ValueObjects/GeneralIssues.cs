using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.ValueObjects
{
    [ProtoContract]
    public class GeneralIssues
    {
        [ProtoMember(1)]
        public string Title { get; set; }

        [ProtoMember(2)]
        public string Developer { get; set; }
        [ProtoMember(3)]
        public string Publisher { get; set; }
        [ProtoMember(4)]
        public IReadOnlyList<string> MissingUpdates { get; set; } = new List<string>();
        [ProtoMember(5)]
        public IReadOnlyList<string> MissingLanguages { get; set; } = new List<string>();
        [ProtoMember(6)]
        public IReadOnlyList<string> MissingFreeDlc { get; set; } = new List<string>();
        [ProtoMember(7)]
        public IReadOnlyList<string> MissingPaidDlc { get; set; } = new List<string>();
        [ProtoMember(8)]
        public IReadOnlyList<string> MissingFeatures { get; set; } = new List<string>();
        [ProtoMember(9)]
        public IReadOnlyList<string> MissingSoundtrack { get; set; } = new List<string>();
        [ProtoMember(10)]
        public IReadOnlyList<string> OtherIssues { get; set; } = new List<string>();
        [ProtoMember(11)]
        public IReadOnlyList<string> MissingBuilds { get; set; } = new List<string>();
        [ProtoMember(12)]
        public IReadOnlyList<string> RegionLocking { get; set; } = new List<string>();
        [ProtoMember(13)]
        public IReadOnlyList<string> SourceOne { get; set; } = new List<string>();
        [ProtoMember(14)]
        public IReadOnlyList<string> SourceTwo { get; set; } = new List<string>();
        public GeneralIssues() { }
        public GeneralIssues(
            string title,
            string developer,
            string publisher,
            List<string> missingUpdates,
            List<string> missingLanguages,
            List<string> missingFreeDlc,
            List<string> missingPaidDlc,
            List<string> missingFeatures,
            List<string> missingSoundtrack,
            List<string> otherIssues,
            List<string> missingBuilds,
            List<string> regionLocking,
            List<string> sourceOne,
            List<string> sourceTwo)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Developer = developer ?? throw new ArgumentNullException(nameof(developer));
            Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            MissingUpdates = missingUpdates ?? new List<string>();
            MissingLanguages = missingLanguages ?? new List<string>();
            MissingFreeDlc = missingFreeDlc ?? new List<string>();
            MissingPaidDlc = missingPaidDlc ?? new List<string>();
            MissingFeatures = missingFeatures ?? new List<string>();
            MissingSoundtrack = missingSoundtrack ?? new List<string>();
            OtherIssues = otherIssues ?? new List<string>();
            MissingBuilds = missingBuilds ?? new List<string>();
            RegionLocking = regionLocking ?? new List<string>();
            SourceOne = sourceOne ?? new List<string>();
            SourceTwo = sourceTwo ?? new List<string>();
        }

        public static GeneralIssues Empty => new GeneralIssues(
            string.Empty,
            string.Empty,
            string.Empty,
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>()
        );

        public int GetIssuesCount()
        {
            int count = 0;

            if (MissingUpdates.Count > 0) count++;
            if (MissingLanguages.Count > 0) count++;
            if (MissingFreeDlc.Count > 0) count++;
            if (MissingPaidDlc.Count > 0) count++;
            if (MissingFeatures.Count > 0) count++;
            if (MissingSoundtrack.Count > 0) count++;
            if (OtherIssues.Count > 0) count++;
            if (MissingBuilds.Count > 0) count++;
            if (RegionLocking.Count > 0) count++;

            return count;
        }
    }
}
