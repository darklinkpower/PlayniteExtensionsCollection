using Playnite.SDK;
using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKUpdater.Application;
using SpecialKHelper.SpecialKUpdater.Domain;
using SpecialKHelper.SpecialKUpdater.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKUpdater.Infrastructure
{
    public class SpecialKUpdateService
    {
        private readonly ILogger _logger;
        private readonly SpecialKRepositoryClient _repository;

        public SpecialKUpdateService(
            ILogger logger,
            SpecialKRepositoryClient repository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<UpdateCheckResult> CheckForUpdatesAsync(
            SpecialKVersionInformation currentVersion,
            SpecialKUpdateChannel channel)
        {
            _logger.Info($"Checking Special K updates for channel '{channel}'.");

            var repo = await _repository.GetRepositoryAsync();
            var channelName = string.Empty;
            switch (channel)
            {
                case SpecialKUpdateChannel.Website:
                    channelName = "Website";
                    break;
                case SpecialKUpdateChannel.Discord:
                    channelName = "Discord";
                    break;
                case SpecialKUpdateChannel.Ancient:
                    channelName = "Ancient";
                    break;
            }

            var latest = repo.Main
                .Versions
                .FirstOrDefault(v =>
                    v.Branches
                        .Contains(channelName));

            if (latest is null)
            {
                _logger.Warn($"No matching branch '{channelName}' found.");
                return UpdateCheckResult.NoUpdate(currentVersion.ToString());
            }

            var latestVersion = latest.Name;
            var comparison = SpecialKVersionComparer.Compare(
                latestVersion,
                currentVersion.Service64BitsVersion);

            if (comparison <= 0)
            {
                _logger.Info($"No update available. Current version: {currentVersion.Service64BitsVersion}, Latest version: {latestVersion}");
                return UpdateCheckResult.NoUpdate(currentVersion.Service64BitsVersion);
            }

            _logger.Info($"Found update {latest.Name}");

            return new UpdateCheckResult(
                true,
                currentVersion.Service64BitsVersion,
                latest.Name,
                latest.ReleaseNotes,
                latest.Description,
                latest.Installer,
                latest.SHA256);
        }
    }
}
