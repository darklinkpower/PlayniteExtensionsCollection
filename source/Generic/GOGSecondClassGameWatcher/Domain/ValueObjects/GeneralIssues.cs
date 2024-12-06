using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.ValueObjects
{
    public class GeneralIssues
    {
        public string Title { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public IReadOnlyList<string> MissingUpdates { get; }
        public IReadOnlyList<string> MissingLanguages { get; }
        public IReadOnlyList<string> MissingFreeDlc { get; }
        public IReadOnlyList<string> MissingPaidDlc { get; }
        public IReadOnlyList<string> MissingFeatures { get; }
        public IReadOnlyList<string> MissingSoundtrack { get; }
        public IReadOnlyList<string> OtherIssues { get; }
        public IReadOnlyList<string> MissingBuilds { get; }
        public IReadOnlyList<string> RegionLocking { get; }
        public IReadOnlyList<string> SourceOne { get; }
        public IReadOnlyList<string> SourceTwo { get; }

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
