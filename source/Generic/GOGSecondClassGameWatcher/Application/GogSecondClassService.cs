using Csv;
using GOGSecondClassGameWatcher.Domain.Interfaces;
using GOGSecondClassGameWatcher.Domain.ValueObjects;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Application
{
    public class GogSecondClassService
    {
        private readonly ILogger _logger;
        private readonly ICsvDataSource _csvDataSource;
        private readonly IGogSecondClassPersistence _persistence;
        private TimeSpan _backgroundServiceDelay;
        private bool _isBackgroundServiceRunning = false;
        private bool _isBackgroundServiceEnabled = false;
        private int _backgroundChecksWithoutSuccess = 0;

        public GogSecondClassService(ILogger logger, ICsvDataSource csvDataSource, IGogSecondClassPersistence persistence, TimeSpan backgroundDelayTime)
        {
            _logger = logger;
            _csvDataSource = csvDataSource;
            _persistence = persistence;
            _backgroundServiceDelay = backgroundDelayTime;
            if (!_persistence.GetLastCheckTime().HasValue || !_persistence.GetAllItems().Any())
            {
                UpdateData();
            }
        }

        public void EnableBackgroundServiceTracker()
        {
            _isBackgroundServiceEnabled = true;
            StartBackgroundServiceStatusCheckAsync();
        }

        public void DisableBackgroundServiceTracker()
        {
            _isBackgroundServiceEnabled = false;
        }

        private async void StartBackgroundServiceStatusCheckAsync()
        {
            if (_isBackgroundServiceRunning)
            {
                return;
            }

            _isBackgroundServiceRunning = true;
            await Task.Run(async () =>
            {
                while (true)
                {
                    await WaitUntilNextCheckTimeAsync();
                    if (!_isBackgroundServiceEnabled)
                    {
                        break;
                    }

                    var backgroundCheckSuccess = UpdateData();
                    if (backgroundCheckSuccess)
                    {
                        _backgroundChecksWithoutSuccess = 0;
                        _logger.Info("Background update successful.");
                    }
                    else
                    {
                        _backgroundChecksWithoutSuccess++;
                        _logger.Warn($"Background update failed, attempt {_backgroundChecksWithoutSuccess}.");
                    }
                }
            });

            _isBackgroundServiceRunning = false;
        }

        private async Task WaitUntilNextCheckTimeAsync()
        {
            var checkInterval = TimeSpan.FromMinutes(20);
            while (true)
            {
                var lastCheckTime = _persistence.GetLastCheckTime();
                if (!lastCheckTime.HasValue)
                {
                    break;
                }

                var timeSinceLastCheck = DateTime.UtcNow - lastCheckTime.Value;
                var delayUntilNextCheck = _backgroundServiceDelay - timeSinceLastCheck;
                if (delayUntilNextCheck > TimeSpan.Zero)
                {
                    await Task.Delay(checkInterval);
                }
                else
                {
                    break;
                }
            }

            if (_backgroundChecksWithoutSuccess >= 5)
            {
                var backoffDelay = TimeSpan.FromMinutes(Math.Min(Math.Pow(2, _backgroundChecksWithoutSuccess - 5), 40));
                _logger.Warn($"Background check failed {_backgroundChecksWithoutSuccess} times consecutively. Applying exponential backoff: waiting {backoffDelay.TotalMinutes} minute(s) before next check.");
                await Task.Delay(backoffDelay);
            }
        }

        public bool UpdateData()
        {
            try
            {
                var generalCsvData = _csvDataSource.GetGeneralIssuesCsvData();
                if (generalCsvData.IsNullOrEmpty())
                {
                    return false;
                }

                var achievementsCsvData = _csvDataSource.GetAchievementsIssuesCsvData();
                if (achievementsCsvData.IsNullOrEmpty())
                {
                    return false;
                }

                var generalIssues = ParseCsvToGeneralIssues(generalCsvData);
                var achievementsIssues = ParseCsvToAchievementsIssues(achievementsCsvData);
                var gogSecondClassGamesList = GetGamesListFromCsvs(achievementsIssues, generalIssues);

                _persistence.ExecuteInTransaction(() =>
                {
                    _persistence.ClearItems();
                    _persistence.SaveItems(gogSecondClassGamesList);
                    _persistence.SetLastCheckTime(DateTime.UtcNow);
                });

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while updating data");
                return false;
            }
        }

        public GogSecondClassGame GetDataForGame(Game game)
        {
            if (!GogLibraryUtilities.IsGogGame(game))
            {
                return null;
            }

            var dataById = _persistence.GetItemById(game.GameId);
            if (dataById != null)
            {
                return dataById;
            }

            return _persistence.GetItemByTitle(game.Name);
        }

        private List<GeneralIssues> ParseCsvToGeneralIssues(string csvData)
        {
            var titleLineNumber = FindLineNumberStartingWithTitle(csvData);
            var options = GetDefaultCsvOptions(titleLineNumber);

            // The Csv library doesn't support duplicate headers. Empty headers, which are represented by double quotes (""), 
            // are mistakenly identified as duplicates, leading to a crash. As a workaround, we replace these empty headers 
            // with random strings to avoid the issue and ensure the data is processed correctly.
            var supportedCsvData = ReplaceDoubleQuotesInLine(csvData, titleLineNumber);
            var csvLinesList = CsvReader.ReadFromText(supportedCsvData, options).ToList();

            return csvLinesList
                .Select(csvLine => new GeneralIssues(
                    csvLine["Title"].Trim(),
                    csvLine["Developer"].Trim(),
                    csvLine["Publisher"].Trim(),
                    SplitAndHandleEmpty(csvLine["Missing Updates"]),
                    SplitAndHandleEmpty(csvLine["Missing Languages"]),
                    SplitAndHandleEmpty(csvLine["Missing Free DLC"]),
                    SplitAndHandleEmpty(csvLine["Missing Paid DLC"]),
                    SplitAndHandleEmpty(csvLine["Missing Features"]),
                    SplitAndHandleEmpty(csvLine["Missing Soundtrack"]),
                    SplitAndHandleEmpty(csvLine["Other"]),
                    SplitAndHandleEmpty(csvLine["Missing Builds"]),
                    SplitAndHandleEmpty(csvLine["Region Locking"]),
                    SplitAndHandleEmpty(csvLine["Source 1"]),
                    SplitAndHandleEmpty(csvLine["Source 2"])
                )).ToList();
        }

        private List<AchievementsIssues> ParseCsvToAchievementsIssues(string csvData)
        {
            var titleLineNumber = FindLineNumberStartingWithTitle(csvData);
            var options = GetDefaultCsvOptions(titleLineNumber);

            var supportedCsvData = ReplaceDoubleQuotesInLine(csvData, titleLineNumber);
            var csvLinesList = CsvReader.ReadFromText(supportedCsvData, options).ToList();

            // No idea why but these two headers get downloaded with empty ("") headers, not the same as when viewed on Google Docs
            var idHeader = csvLinesList.FirstOrDefault()?.Headers[1];
            var releaseYearHeader = csvLinesList.FirstOrDefault()?.Headers[4];

            return csvLinesList
                .Select(csvLine => new AchievementsIssues(
                    csvLine["Title"].Trim(),
                    csvLine[idHeader].Trim(),
                    csvLine["Developer"].Trim(),
                    csvLine["Publisher"].Trim(),
                    csvLine[releaseYearHeader].Trim(),
                    csvLine["Missing All Achievements"].Trim(),
                    csvLine["Missing Some Achievements"].Trim(),
                    SplitAndHandleEmpty(csvLine["Broken Achievements"]),
                    csvLine["Achievements?"].Trim(),
                    csvLine["Source"]
                )).ToList();
        }

        public List<GogSecondClassGame> GetGamesListFromCsvs(List<AchievementsIssues> achievementsIssuesList, List<GeneralIssues> generalIssuesList)
        {
            // Title names can be repeated, such as in cases where a game has multiple entries with diferent ids
            // so we create groups by Title
            var generalIssuesDict = generalIssuesList
                .GroupBy(gi => gi.Title.Satinize(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList());
            var achievementsIssuesDict = achievementsIssuesList
                .GroupBy(ai => ai.Title.Satinize(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(a => a.Key, a => a.ToList());

            var gamesList = new List<GogSecondClassGame>();

            foreach (var achievementsIssuesGroup in achievementsIssuesDict)
            {
                var titleToMatch = achievementsIssuesGroup.Key;
                generalIssuesDict.TryGetValue(titleToMatch, out var matchingGeneralIssues);

                foreach (var achievementsIssues in achievementsIssuesGroup.Value)
                {
                    var developer = achievementsIssues.Developer ?? matchingGeneralIssues?.FirstOrDefault()?.Developer;
                    var publisher = achievementsIssues.Publisher ?? matchingGeneralIssues?.FirstOrDefault()?.Publisher;

                    var game = new GogSecondClassGame(
                        title: achievementsIssues.Title,
                        developer: developer ?? string.Empty,
                        publisher: publisher ?? string.Empty,
                        id: achievementsIssues.Id,
                        generalIssues: matchingGeneralIssues?.FirstOrDefault() ?? GeneralIssues.Empty,
                        achievementsIssues: achievementsIssues
                    );

                    gamesList.Add(game);
                }
            }

            // Add any remaining GeneralIssues that didn't have a match in AchievementsIssues
            var remainingGeneralIssues = generalIssuesList
                .Where(gi => !gamesList.Any(g => g.Title.Satinize().Equals(gi.Title.Satinize(), StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var generalIssues in remainingGeneralIssues)
            {
                var game = new GogSecondClassGame(
                    title: generalIssues.Title,
                    developer: generalIssues.Developer ?? string.Empty,
                    publisher: generalIssues.Publisher ?? string.Empty,
                    id: null,
                    generalIssues: generalIssues,
                    achievementsIssues: AchievementsIssues.Empty
                );

                gamesList.Add(game);
            }

            return gamesList;
        }



        private CsvOptions GetDefaultCsvOptions(int titleLineNumber)
        {
            return new CsvOptions
            {
                RowsToSkip = titleLineNumber - 1,
                SkipRow = (row, idx) => string.IsNullOrEmpty(row) || row[0] == '#',
                TrimData = true,
                HeaderMode = HeaderMode.HeaderPresent,
                AllowNewLineInEnclosedFieldValues = true,
                NewLine = Environment.NewLine
            };
        }

        private static List<string> SplitAndHandleEmpty(string input)
        {
            var splitValues = input.Split('\n').ToList();
            if (splitValues.Count == 1 && string.IsNullOrEmpty(splitValues[0]))
            {
                return null;
            }

            return splitValues;
        }

        private static int FindLineNumberStartingWithTitle(string input)
        {
            string[] lines = input.Split(new[] { '\n' }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("\"Title\""))
                {
                    return i + 1;
                }
            }

            return -1;
        }

        private static string ReplaceDoubleQuotesInLine(string input, int lineNumber)
        {
            string[] lines = input.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lineNumber > 0 && lineNumber <= lines.Length)
            {
                string linetToModify = lines[lineNumber - 1];
                var modifiedLine = new StringBuilder();
                for (int i = 0; i < linetToModify.Length; i++)
                {
                    // Check for double quote pair
                    if (i + 1 < linetToModify.Length && linetToModify[i] == '"' && linetToModify[i + 1] == '"')
                    {
                        modifiedLine.Append("\"" + Guid.NewGuid().ToString("N") + "\"");
                        i++;
                    }
                    else
                    {
                        modifiedLine.Append(linetToModify[i]);
                    }
                }

                lines[lineNumber - 1] = modifiedLine.ToString();
            }

            return string.Join("\n", lines);
        }


    }
}
