using Csv;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExporter.ViewModels
{
    class LibraryExporterViewModel
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();

        public LibraryExporterSettingsViewModel Settings { get; }

        public LibraryExporterViewModel(IPlayniteAPI playniteApi, LibraryExporterSettingsViewModel settings)
        {
            this.playniteApi = playniteApi;
            Settings = settings;
        }

        public RelayCommand ExportAllGamesCommand
        {
            get => new RelayCommand(() =>
            {
                ExportGamesToCsv(playniteApi.Database.Games);
            });
        }

        public RelayCommand ExportSelectedGamesCommand
        {
            get => new RelayCommand(() =>
            {
                ExportGamesToCsv(playniteApi.MainView.SelectedGames.Distinct());
            });
        }

        public void ExportGamesToCsv(IEnumerable<Game> games)
        {
            if (!games.HasItems())
            {
                return;
            }

            var selectedPath = playniteApi.Dialogs.SaveFile(@"Csv|*.csv", true);
            if (selectedPath.IsNullOrEmpty())
            {
                return;
            }

            List<string> columnNames = GetColumnsList();

            var columnCount = columnNames.Count();
            var rows = new List<string[]>(games.Count());

            rows.AddRange(games.Select(x => GenerateGameColumns(columnCount, x)));
            var csv = CsvWriter.WriteToText(columnNames.ToArray(), rows, ',');

            try
            {
                FileSystem.WriteStringToFile(selectedPath, csv, true);
                playniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LibraryExporterAdvanced_ExportSuccessMessage").Format(selectedPath));
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to export library to {selectedPath}");
                playniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LibraryExporterAdvanced_ExportFailedMessage").Format(selectedPath) + "\n\n" + e.Message);
            }
            
        }

        private List<string> GetColumnsList()
        {
            var columnNames = new List<string>
            {
                GetLocalizationString("LOCNameLabel")
            };

            if (Settings.Settings.ExportSettings.AgeRatings) { columnNames.Add(GetLocalizationString("LOCAgeRatingLabel")); }
            if (Settings.Settings.ExportSettings.Categories) { columnNames.Add(GetLocalizationString("LOCCategoriesLabel")); }
            if (Settings.Settings.ExportSettings.Description) { columnNames.Add(GetLocalizationString("LOCGameDescriptionTitle")); }
            if (Settings.Settings.ExportSettings.Developers) { columnNames.Add(GetLocalizationString("LOCDevelopersLabel")); }
            if (Settings.Settings.ExportSettings.Publishers) { columnNames.Add(GetLocalizationString("LOCPublishersLabel")); }
            if (Settings.Settings.ExportSettings.Favorite) { columnNames.Add(GetLocalizationString("LOCGameFavoriteTitle")); }
            if (Settings.Settings.ExportSettings.Hidden) { columnNames.Add(GetLocalizationString("LOCGameHiddenTitle")); }
            if (Settings.Settings.ExportSettings.Features) { columnNames.Add(GetLocalizationString("LOCFeaturesLabel")); }
            if (Settings.Settings.ExportSettings.GameId) { columnNames.Add(GetLocalizationString("LOCGameId")); }
            if (Settings.Settings.ExportSettings.Genres) { columnNames.Add(GetLocalizationString("LOCGenresLabel")); }
            if (Settings.Settings.ExportSettings.InstallDirectory) { columnNames.Add(GetLocalizationString("LOCGameInstallDirTitle")); }
            if (Settings.Settings.ExportSettings.InstallSize) { columnNames.Add(GetLocalizationString("LOCInstallSizeMenuLabel")); }
            if (Settings.Settings.ExportSettings.IsInstalled) { columnNames.Add(GetLocalizationString("LOCGameIsGameInstalledTitle")); }
            if (Settings.Settings.ExportSettings.ReleaseDate) { columnNames.Add(GetLocalizationString("LOCGameReleaseDateTitle")); }
            if (Settings.Settings.ExportSettings.Added) { columnNames.Add(GetLocalizationString("LOCAddedLabel")); }
            if (Settings.Settings.ExportSettings.LastActivity) { columnNames.Add(GetLocalizationString("LOCGameLastActivityTitle")); }
            if (Settings.Settings.ExportSettings.Modified) { columnNames.Add(GetLocalizationString("LOCDateModifiedLabel")); }
            if (Settings.Settings.ExportSettings.RecentActivity) { columnNames.Add(GetLocalizationString("LOCRecentActivityLabel")); }
            if (Settings.Settings.ExportSettings.Roms) { columnNames.Add("Roms"); }
            if (Settings.Settings.ExportSettings.Links) { columnNames.Add(GetLocalizationString("LOCLinksLabel")); }
            if (Settings.Settings.ExportSettings.Manual) { columnNames.Add(GetLocalizationString("LOCGameManualTitle")); }
            if (Settings.Settings.ExportSettings.Notes) { columnNames.Add(GetLocalizationString("LOCNotesLabel")); }
            if (Settings.Settings.ExportSettings.Platforms) { columnNames.Add(GetLocalizationString("LOCPlatformsTitle")); }
            if (Settings.Settings.ExportSettings.PlayCount) { columnNames.Add(GetLocalizationString("LOCPlayCountLabel")); }
            if (Settings.Settings.ExportSettings.Playtime) { columnNames.Add(GetLocalizationString("LOCTimePlayed")); }
            if (Settings.Settings.ExportSettings.PluginId) { columnNames.Add(GetLocalizationString("PluginId")); }
            if (Settings.Settings.ExportSettings.Source) { columnNames.Add(GetLocalizationString("LOCSourcesLabel")); }
            if (Settings.Settings.ExportSettings.Regions) { columnNames.Add(GetLocalizationString("LOCRegionsLabel")); }
            if (Settings.Settings.ExportSettings.Series) { columnNames.Add(GetLocalizationString("LOCSeriesLabel")); }
            if (Settings.Settings.ExportSettings.Tags) { columnNames.Add(GetLocalizationString("LOCTagsLabel")); }
            if (Settings.Settings.ExportSettings.CommunityScore) { columnNames.Add(GetLocalizationString("LOCCommunityScore")); }
            if (Settings.Settings.ExportSettings.CriticScore) { columnNames.Add(GetLocalizationString("LOCCriticScore")); }
            if (Settings.Settings.ExportSettings.UserScore) { columnNames.Add(GetLocalizationString("LOCUserScore")); }
            if (Settings.Settings.ExportSettings.Version) { columnNames.Add(GetLocalizationString("LOCVersionLabel")); }
            if (Settings.Settings.ExportSettings.Id) { columnNames.Add("Id"); }

            return columnNames;
        }

        private string GetLocalizationString(string locKey)
        {
            return ResourceProvider.GetString(locKey);
        }

        private string[] GenerateGameColumns(int columnCount, Game game)
        {
            var properties = new string[columnCount];
            properties[0] = game.Name;

            var currentInsertColumn = 1;
            if (Settings.Settings.ExportSettings.AgeRatings)
            {
                properties[currentInsertColumn] = game.AgeRatings.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.AgeRatings.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Categories)
            {
                properties[currentInsertColumn] = game.Categories.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Categories.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Description)
            {
                properties[currentInsertColumn] = string.IsNullOrEmpty(game.Description) ? string.Empty : game.Description;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Developers)
            {
                properties[currentInsertColumn] = game.Developers.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Developers.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Publishers)
            {
                properties[currentInsertColumn] = game.Publishers.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Publishers.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Favorite)
            {
                properties[currentInsertColumn] = game.Favorite.ToString();
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Hidden)
            {
                properties[currentInsertColumn] = game.Hidden.ToString();
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Features)
            {
                properties[currentInsertColumn] = game.Features.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Features.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.GameId)
            {
                properties[currentInsertColumn] = game.GameId;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Genres)
            {
                properties[currentInsertColumn] = game.Genres.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Genres.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.InstallDirectory)
            {
                properties[currentInsertColumn] = !string.IsNullOrEmpty(game.InstallDirectory) ? game.InstallDirectory : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.InstallSize)
            {
                properties[currentInsertColumn] = game.InstallSize.HasValue ? game.InstallSize.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.IsInstalled)
            {
                properties[currentInsertColumn] = game.IsInstalled.ToString();
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.ReleaseDate)
            {
                properties[currentInsertColumn] = game.ReleaseDate.HasValue ? game.ReleaseDate.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Added)
            {
                properties[currentInsertColumn] = game.Added.HasValue ? game.Added.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.LastActivity)
            {
                properties[currentInsertColumn] = game.LastActivity.HasValue ? game.LastActivity.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Modified)
            {
                properties[currentInsertColumn] = game.Modified.HasValue ? game.Modified.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.RecentActivity)
            {
                properties[currentInsertColumn] = game.RecentActivity.HasValue ? game.RecentActivity.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Roms)
            {
                properties[currentInsertColumn] = game.Roms?.HasItems() == true ? string.Join(Settings.Settings.ListsSeparator, game.Roms.Select(x => $"{x.Name}:{x.Path}")) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Links)
            {
                properties[currentInsertColumn] = game.Links?.HasItems() == true ? string.Join(Settings.Settings.ListsSeparator, game.Links.Select(x => $"{x.Name}:{x.Url}")) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Manual)
            {
                properties[currentInsertColumn] = !string.IsNullOrEmpty(game.Manual) ? game.Manual : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Notes)
            {
                properties[currentInsertColumn] = !string.IsNullOrEmpty(game.Notes) ? game.Notes : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Platforms)
            {
                properties[currentInsertColumn] = game.Platforms.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Platforms.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.PlayCount)
            {
                properties[currentInsertColumn] = game.PlayCount.ToString();
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Playtime)
            {
                properties[currentInsertColumn] = game.Playtime.ToString();
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.PluginId)
            {
                properties[currentInsertColumn] = game.PluginId.ToString();
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Source)
            {
                properties[currentInsertColumn] = game.Source != null ? game.Source.Name : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Regions)
            {
                properties[currentInsertColumn] = game.Regions.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Regions.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Series)
            {
                properties[currentInsertColumn] = game.Series.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Series.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Tags)
            {
                properties[currentInsertColumn] = game.Tags.HasItems() ? string.Join(Settings.Settings.ListsSeparator, game.Tags.Select(x => x.Name)) : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.CommunityScore)
            {
                properties[currentInsertColumn] = game.CommunityScore.HasValue ? game.CommunityScore.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.CriticScore)
            {
                properties[currentInsertColumn] = game.CriticScore.HasValue ? game.CriticScore.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.UserScore)
            {
                properties[currentInsertColumn] = game.UserScore.HasValue ? game.UserScore.ToString() : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Version)
            {
                properties[currentInsertColumn] = !string.IsNullOrEmpty(game.Version) ? game.Version : string.Empty;
                currentInsertColumn++;
            }

            if (Settings.Settings.ExportSettings.Id)
            {
                properties[currentInsertColumn] = game.Id.ToString();
                currentInsertColumn++;
            }
            
            return properties;
        }

    }
}