using InstallationStatusUpdater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.ViewModels
{
    public class StatusUpdateResultsWindowViewModel
    {
        private StatusUpdateResults updateResults;
        public StatusUpdateResults UpdateResults
        {
            get => updateResults;
            set
            {
                updateResults = value;
            }
        }

        public int TotalGamesSetAsInstalled
        {
            get => UpdateResults.SetAsInstalled.Count();
        }

        public int TotalGamesSetAsUninstalled
        {
            get => UpdateResults.SetAsUninstalled.Count();
        }

        public StatusUpdateResultsWindowViewModel(StatusUpdateResults updateResults)
        {
            this.updateResults = updateResults;
        }
    }
}