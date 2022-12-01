using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Models
{
    public class StatusUpdateResults
    {
        private List<UpdatedEntryData> setAsInstalled = new List<UpdatedEntryData>();
        public List<UpdatedEntryData> SetAsInstalled
        {
            get => setAsInstalled;
            set
            {
                setAsInstalled = value;
            }
        }

        private List<UpdatedEntryData> setAsUninstalled = new List<UpdatedEntryData>();
        public List<UpdatedEntryData> SetAsUninstalled
        {
            get => setAsUninstalled;
            set
            {
                setAsUninstalled = value;
            }
        }

        internal void AddSetAsInstalledGame(Game game)
        {
            SetAsInstalled.Add(new UpdatedEntryData(game));
        }

        internal void AddSetAsUninstalledGame(Game game)
        {
            SetAsUninstalled.Add(new UpdatedEntryData(game));
        }
    }
}
