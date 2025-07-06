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
        public StatusUpdateResults UpdateResults { get; }
        public int TotalGamesSetAsInstalled => UpdateResults.Installed.Count;
        public int TotalGamesSetAsUninstalled => UpdateResults.Uninstalled.Count;

        public StatusUpdateResultsWindowViewModel(StatusUpdateResults updateResults)
        {
            UpdateResults = updateResults ?? throw new ArgumentNullException(nameof(updateResults));
        }
    }
}