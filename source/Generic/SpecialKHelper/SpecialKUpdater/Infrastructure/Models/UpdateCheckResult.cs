using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKUpdater.Infrastructure.Models
{
    public sealed class UpdateCheckResult
    {
        public bool IsUpdateAvailable { get; }

        public string CurrentVersion { get; }

        public string LatestVersion { get; }

        public string ReleaseNotes { get; }

        public string Description { get; }

        public string InstallerUrl { get; }

        public string Sha256 { get; }

        public UpdateCheckResult(
            bool isUpdateAvailable,
            string currentVersion,
            string latestVersion,
            string releaseNotes,
            string description,
            string installerUrl,
            string sha256)
        {
            IsUpdateAvailable = isUpdateAvailable;
            CurrentVersion = currentVersion;
            LatestVersion = latestVersion;
            ReleaseNotes = releaseNotes;
            Description = description;
            InstallerUrl = installerUrl;
            Sha256 = sha256;
        }

        public static UpdateCheckResult NoUpdate(
            string currentVersion)
        {
            return new UpdateCheckResult(
                false,
                currentVersion,
                currentVersion,
                string.Empty,
                string.Empty,
                null,
                null);
        }
    }
}