using Csv;
using Playnite.SDK;
using PluginsCommon;
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
        private const string webViewUserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 Vivaldi/4.3";
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

        private void ExportLicenses()
        {
            switch (selectedLibrary)
            {
                case EaLicenseService.LibraryName:
                    ExportLicenses(EaLicenseService.LibraryName, EaLicenseService.GetLicenses());
                    break;
                case EpicLicenseService.LibraryName:
                    ExportLicensesIdMatch(EpicLicenseService.LibraryName, EpicLicenseService.PluginId, EpicLicenseService.GetLicensesDict());
                    break;
                case GogLicenseService.LibraryName:
                    ExportLicenses(GogLicenseService.LibraryName, GogLicenseService.GetLicenses());
                    break;
                case SteamLicenseService.LibraryName:
                    ExportLicensesIdMatch(SteamLicenseService.LibraryName, SteamLicenseService.PluginId, SteamLicenseService.GetLicensesDict());
                    break;
                default:
                    break;
            }
        }

        private void Login()
        {
            switch (selectedLibrary)
            {
                case EaLicenseService.LibraryName:
                    OpenWebViewForLogin(EaLicenseService.LoginUrl);
                    break;
                case EpicLicenseService.LibraryName:
                    OpenWebViewForLogin(EpicLicenseService.LoginUrl);
                    break;
                case GogLicenseService.LibraryName:
                    OpenWebViewForLogin(GogLicenseService.LoginUrl);
                    break;
                case SteamLicenseService.LibraryName:
                    OpenWebViewForLogin(SteamLicenseService.LoginUrl);
                    break;
                default:
                    break;
            }
        }

        private void OpenWebViewForLogin(string loginUrl)
        {
            using (var webView = playniteApi.WebViews.CreateView(
                new WebViewSettings
                {
                    WindowHeight = 600,
                    WindowWidth = 1024,
                    UserAgent = webViewUserAgent
                }))
            {
                webView.Navigate(loginUrl);
                webView.OpenDialog();
            }
        }

        private void ExportLicensesIdMatch(string libraryName, Guid pluginId, Dictionary<string, LicenseData> licensesDictionary)
        {
            if (!licensesDictionary.HasItems())
            {
                return;
            }

            // For libraries that we don't obtain the license Id, we use the library
            // in Playnite to match and add it when possible
            foreach (var game in playniteApi.Database.Games)
            {
                if (game.PluginId != pluginId)
                {
                    continue;
                }

                var matchingName = game.Name.GetMatchModifiedName();
                if (licensesDictionary.TryGetValue(matchingName, out var licenseData))
                {
                    licenseData.Id = game.GameId;
                }
            }

            ExportLicenses(libraryName, licensesDictionary.Select(x => x.Value).ToList());
        }

        private void ExportLicenses(string libraryName, List<LicenseData> licenses)
        {
            if (!licenses.HasItems())
            {
                return;
            }

            var savePath = playniteApi.Dialogs.SaveFile("CSV|*.csv");
            if (savePath.IsNullOrEmpty())
            {
                return;
            }

            var columnNames = new string[] { "Name", "Date", "Id" };
            var rows = licenses.Select(x => new string[] { x.Name, x.PurchaseDate.ToString("u"), x.Id ?? string.Empty });
            var csv = CsvWriter.WriteToText(columnNames, rows, ',');

            FileSystem.WriteStringToFile(savePath, csv, true);
            playniteApi.Dialogs.ShowMessage("Licenses for LIBRARYNAME exported successfully to SAVEPATH", "Purchase Date Importer");
        }

        public RelayCommand ImportPurchaseDatesCommand
        {
            get => new RelayCommand(() =>
            {
                ImportPurchasedDates();
            });
        }

        public RelayCommand ExportLicensesCommand
        {
            get => new RelayCommand(() =>
            {
                ExportLicenses();
            });
        }

        public RelayCommand LoginCommand
        {
            get => new RelayCommand(() =>
            {
                Login();
            });
        }

    }
}