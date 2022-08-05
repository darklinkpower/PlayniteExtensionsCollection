using Playnite.SDK;
using PurchaseDateImporter.Models;
using PurchaseDateImporter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseDateImporter.ViewModels
{
    public class PurchaseDateImporterWindowViewModel : ObservableObject
    {
        private IPlayniteAPI playniteApi;

        private string selectedLibrary;
        public string SelectedLibrary { get => selectedLibrary; set => SetValue(ref selectedLibrary, value); }

        private Dictionary<string, string> librariesSource;
        public Dictionary<string, string> LibrariesSource { get => librariesSource; set => SetValue(ref librariesSource, value); }

        public PurchaseDateImporterWindowViewModel(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;

            LibrariesSource = new Dictionary<string, string>
            {
                [EaLicenseService.LibraryName] = EaLicenseService.LibraryName,
                [EpicLicenseService.LibraryName] = EpicLicenseService.LibraryName,
                [GogLicenseService.LibraryName] = GogLicenseService.LibraryName,
                [SteamLicenseService.LibraryName] = SteamLicenseService.LibraryName
            };

            SelectedLibrary = SteamLicenseService.LibraryName;
        }

        public RelayCommand ImportPurchaseDatesCommand
        {
            get => new RelayCommand(() =>
            {
                ImportPurchasedDates();
            });
        }

        private void ImportPurchasedDates()
        {
            switch (selectedLibrary)
            {
                case EaLicenseService.LibraryName:
                    ApplyDatesUsingNames(EaLicenseService.LibraryName,
                        EaLicenseService.PluginId, EaLicenseService.GetLicensesDict(), false);
                    break;
                case EpicLicenseService.LibraryName:
                    ApplyDatesUsingNames(EpicLicenseService.LibraryName,
                        EpicLicenseService.PluginId, EpicLicenseService.GetLicensesDict(), true);
                    break;
                case GogLicenseService.LibraryName:
                    ApplyDatesUsingNames(GogLicenseService.LibraryName,
                        GogLicenseService.PluginId, GogLicenseService.GetLicensesDict(), false);
                    break;
                case SteamLicenseService.LibraryName:
                    ApplyDatesUsingNames(SteamLicenseService.LibraryName,
                        SteamLicenseService.PluginId, SteamLicenseService.GetLicensesDict(), true, true);
                    break;
                default:
                    break;
            }
        }

        private void ApplyDatesUsingNames(string libraryName, Guid pluginId, Dictionary<string, LicenseData> licensesDictionary, bool useNameToCompare, bool compareOnlyDay = false)
        {
            if (!licensesDictionary.HasItems())
            {
                playniteApi.Dialogs.ShowErrorMessage("Not logged in library name, verify", "Purchase Date Importer");
                return;
            }

            var updated = 0;
            playniteApi.Database.BufferedUpdate();
            foreach (var game in playniteApi.Database.Games)
            {
                if (game.PluginId != pluginId)
                {
                    continue;
                }

                if (useNameToCompare)
                {
                    var matchingName = game.Name.GetMatchModifiedName();
                    if (licensesDictionary.TryGetValue(matchingName, out var licenseData))
                    {
                        if (!IsDateDifferent(game.Added, licenseData.PurchaseDate, compareOnlyDay))
                        {
                            continue;
                        }

                        game.Added = licenseData.PurchaseDate;
                        playniteApi.Database.Games.Update(game);
                        updated++;
                    }
                }
                else if (licensesDictionary.TryGetValue(game.GameId.ToString(), out var licenseData))
                {
                    if (!IsDateDifferent(game.Added, licenseData.PurchaseDate, compareOnlyDay))
                    {
                        continue;
                    }

                    game.Added = licenseData.PurchaseDate;
                    playniteApi.Database.Games.Update(game);
                    updated++;
                }

            }

            playniteApi.Dialogs.ShowMessage($"Changed {updated}", "Purchase Date Importer");
        }

        private bool IsDateDifferent(DateTime? dateAdded, DateTime purchaseDate, bool compareOnlyDay)
        {
            if (dateAdded == null)
            {
                return true;
            }

            if (!compareOnlyDay)
            {
                return dateAdded != purchaseDate;
            }

            // On Steam we only change the Added date if the day is different,
            // because we only obtain the DateTime without specific time
            if (dateAdded.Value.Day != purchaseDate.Day)
            {
                return true;
            }
            else if (dateAdded.Value.Month != purchaseDate.Month)
            {
                return true;
            }
            else if (dateAdded.Value.Year != purchaseDate.Year)
            {
                return true;
            }


            return false;
        }
    }
}