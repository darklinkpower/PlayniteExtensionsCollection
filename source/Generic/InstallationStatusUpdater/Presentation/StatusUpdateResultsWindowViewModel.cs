using InstallationStatusUpdater.Domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Presentation
{
    public class StatusUpdateResultsWindowViewModel
    {
        public List<UpdatedEntryData> GamesSetAsInstalled { get; }
        public List<UpdatedEntryData> GamesSetAsUninstalled { get; }

        public int TotalGamesSetAsInstalled => GamesSetAsInstalled.Count;
        public int TotalGamesSetAsUninstalled => GamesSetAsUninstalled.Count;

        public StatusUpdateResultsWindowViewModel(StatusUpdateResults updateResults)
        {
            if (updateResults is null)
            {
                throw new ArgumentNullException(nameof(updateResults));
            }

            GamesSetAsInstalled = updateResults.Installed.OrderBy(x => x.Name).ToList();
            GamesSetAsUninstalled = updateResults.Uninstalled.OrderBy(x => x.Name).ToList();
        }
    }
}