using GOGSecondClassGameWatcher.Domain.ValueObjects;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Application
{
    public static class GogLibraryUtilities
    {
        private static Guid _gogPluginId = Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E");
        private static Guid _gogOssPluginId = Guid.Parse("03689811-3F33-4DFB-A121-2EE168FB9A5C");

        public static bool IsGogGame(Game game)
        {
            if (game.PluginId == _gogPluginId || game.PluginId == _gogOssPluginId)
            {
                return true;
            }

            return false;
        }

        internal static bool ShouldWatcherNotify(GOGSecondClassGameWatcherSettings settings, GogSecondClassGame gogSecondClassGame)
        {
            // General Issues
            if (settings.NotifyMissingUpdates && gogSecondClassGame.GeneralIssues.MissingUpdates.Any()) return true;
            if (settings.NotifyMissingLanguages && gogSecondClassGame.GeneralIssues.MissingLanguages.Any()) return true;
            if (settings.NotifyMissingFreeDlc && gogSecondClassGame.GeneralIssues.MissingFreeDlc.Any()) return true;
            if (settings.NotifyMissingPaidDlc && gogSecondClassGame.GeneralIssues.MissingPaidDlc.Any()) return true;
            if (settings.NotifyMissingFeatures && gogSecondClassGame.GeneralIssues.MissingFeatures.Any()) return true;
            if (settings.NotifyMissingSoundtrack && gogSecondClassGame.GeneralIssues.MissingSoundtrack.Any()) return true;
            if (settings.NotifyOtherIssues && gogSecondClassGame.GeneralIssues.OtherIssues.Any()) return true;
            if (settings.NotifyMissingBuilds && gogSecondClassGame.GeneralIssues.MissingBuilds.Any()) return true;
            if (settings.NotifyRegionLocking && gogSecondClassGame.GeneralIssues.RegionLocking.Any()) return true;

            // Achievements
            if (settings.NotifyMissingAllAchievements && !gogSecondClassGame.AchievementsIssues.MissingAllAchievements.IsNullOrEmpty()) return true;
            if (settings.NotifyMissingSomeAchievements && !gogSecondClassGame.AchievementsIssues.MissingSomeAchievements.IsNullOrEmpty()) return true;
            if (settings.NotifyBrokenAchievements && gogSecondClassGame.AchievementsIssues.BrokenAchievements.Any()) return true;

            return false;
        }
    }
}
