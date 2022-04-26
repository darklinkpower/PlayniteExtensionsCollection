using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayState.Enums;
using SteamCommon;
using PlayniteUtilitiesCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using PluginsCommon.Web;
using SaveFileView.Models;
using Playnite.SDK.Data;
using PluginsCommon;

namespace SaveFileView
{
    public class SaveFileView : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static Regex pcgwContentRegex = new Regex(@"{{Game data\/(config|saves)\|[^\|]+\|.*?(?=}}\n)", RegexOptions.None);
        private readonly List<string> pcgwRegistryVariables;

        private SaveFileViewSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("f68f302b-9799-4b77-a982-4bfca97130e2");

        public SaveFileView(IPlayniteAPI api) : base(api)
        {
            settings = new SaveFileViewSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            pcgwRegistryVariables = GetSkipList();
        }

        private List<string> GetSkipList()
        {
            return new List<string>
            {
                @"{{p|hkcu}}",
                @"{{p|hklm}}",
                @"{{p|wow64}}"
            };
        }

        private const string ubisoftInstallationDir = @"C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher";
        public static string GetUbisoftInstallationDirectory()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Ubisoft\Launcher"))
            {
                return key?.GetValue("InstallDir")?.ToString()
                    .Replace('/', '\\')
                    .Replace("Ubisoft Game Launcher\\", "Ubisoft Game Launcher") ?? ubisoftInstallationDir;
            }
        }

        private Dictionary<string, string> GetReplacementDict()
        {
            return new Dictionary<string, string>
            {
                [@"{{p|uid}}"] = @"*",
                [@"{{p|steam}}"] = SteamClient.GetSteamInstallationDirectory(),
                [@"{{p|uplay}}"] = GetUbisoftInstallationDirectory(),
                [@"{{p|username}}"] = Environment.UserName,
                [@"{{p|userprofile}}"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                [@"{{p|userprofile\documents}}"] = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                [@"{{p|userprofile\appdata\locallow}}"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"),
                [@"{{p|appdata}}"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                [@"{{p|localappdata}}"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                [@"{{p|public}}"] = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)),
                [@"{{p|allusersprofile}}"] = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                [@"p{{p|programdata}}"] = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                [@"{{p|windir}}"] = Environment.GetFolderPath(Environment.SpecialFolder.System),
                [@"{{p|syswow64}}"] = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                [@"<code>"] = @"*"
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCSaveFileView_MenuSection");
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSaveFileView_MenuItemOpenSaveDirectoriesDescription"),
                    MenuSection = menuSection,
                    Action = a => {
                        OpenGamesDirectories(PathType.Save);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSaveFileView_MenuItemOpenSaveConfigurationDescription"),
                    MenuSection = menuSection,
                    Action = a => {
                        OpenGamesDirectories(PathType.Config);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSaveFileView_MenuItemRefreshDirectoriesDescription"),
                    MenuSection = menuSection,
                    Action = a => {
                        RefreshGamesDirectories(args.Games.Distinct());
                    }
                }
            };
        }

        private void RefreshGamesDirectories(IEnumerable<Game> games)
        {
            var getDataSuccess = 0;
            var getDataFailed = 0;
            var progressTitle = ResourceProvider.GetString("LOCSaveFileView_DialogProgressDownloadingData");
            var progressOptions = new GlobalProgressOptions(progressTitle, true);
            progressOptions.IsIndeterminate = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                a.ProgressMaxValue = games.Count();
                foreach (var game in games)
                {
                    if (a.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    a.CurrentProgressValue++;
                    a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";

                    var pathsStorePath = Path.Combine(GetPluginUserDataPath(), $"{game.Id}.json");
                    if (GetGameDirectories(game, pathsStorePath, true))
                    {
                        getDataSuccess++;
                    }
                    else
                    {
                        getDataFailed++;
                    }
                }
            }, progressOptions);

            PlayniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCSaveFileView_RefreshResultsMessage"),
                getDataSuccess, getDataFailed), "Save File View");
        }

        private void OpenGamesDirectories(PathType pathType)
        {
            var replacementDict = GetReplacementDict();
            foreach (var game in PlayniteApi.MainView.SelectedGames.Distinct())
            {
                if (!PlayniteUtilities.IsGamePcGame(game))
                {
                    continue;
                }

                var pathsStorePath = Path.Combine(GetPluginUserDataPath(), $"{game.Id}.json");
                if (!File.Exists(pathsStorePath))
                {
                    GetGameDirectories(game, pathsStorePath, true);
                    if (!File.Exists(pathsStorePath))
                    {
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCSaveFileView_CouldntGetGameDirectoriesMessage"), game.Name));
                        continue;
                    }
                }

                var data = Serialization.FromJsonFile<GameDirectoriesData>(pathsStorePath);
                var dirDefinitions = GetAvailableDirsFromData(game, replacementDict, data, pathType);
                if (dirDefinitions.Count > 0)
                {
                    foreach (var dir in dirDefinitions)
                    {
                        ProcessStarter.StartProcess(dir);
                    }
                }
                else
                {
                    PlayniteApi.Dialogs.ShowMessage(
                        string.Format(ResourceProvider.GetString("LOCSaveFileView_DirectoriesNotDetectedMessage"), game.Name),
                        "Save File View"
                    );
                }
            }
        }

        private List<string> GetAvailableDirsFromData(Game game, Dictionary<string, string> replacementDict, GameDirectoriesData data, PathType pathType)
        {
            var dirDefinitions = new List<string>();
            var pathDefinitions = new List<string>();
            foreach (var pathData in data.PathsData)
            {
                if (pathData.Type != pathType)
                {
                    continue;
                }

                var path = pathData.Path;
                // Skip registry paths
                if (pcgwRegistryVariables.Any(x => path.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var pathDef = path;
                if (!game.InstallDirectory.IsNullOrEmpty())
                {
                    pathDef = path.Replace(@"{{p|game}}", game.InstallDirectory);
                }

                foreach (var kv in replacementDict)
                {
                    pathDef = ReplaceCaseInsensitive(pathDef, kv.Key, kv.Value);
                }

                if (!pathDef.Contains("*"))
                {
                    pathDefinitions.Add(pathDef);
                }
                else
                {
                    foreach (var matchingPath in GetAllMatchingPaths(pathDef))
                    {
                        if (matchingPath.IsNullOrEmpty())
                        {
                            continue;
                        }

                        pathDefinitions.Add(matchingPath);
                    }
                }
            }

            foreach (var path in pathDefinitions.Distinct())
            {
                if (Directory.Exists(path))
                {
                    dirDefinitions.Add(path);
                }
                else if (File.Exists(path))
                {
                    dirDefinitions.Add(Path.GetDirectoryName(path));
                }
            }

            return dirDefinitions;
        }

        private static string ReplaceCaseInsensitive(string input, string search, string replacement)
        {
            return Regex.Replace(
                input,
                Regex.Escape(search),
                replacement,
                RegexOptions.IgnoreCase
            );
        }

        // Based on https://stackoverflow.com/a/36754982
        public static IEnumerable<string> GetAllMatchingPaths(string pattern)
        {
            char separator = Path.DirectorySeparatorChar;
            string[] parts = pattern.Split(separator);

            if (parts[0].Contains('*') || parts[0].Contains('?'))
            {
                throw new ArgumentException("path root must not have a wildcard", nameof(parts));
            }

            var startPattern = string.Join(separator.ToString(), parts.Skip(1));
            return GetAllMatchingPathsInternal(startPattern, parts[0]);
        }

        private static IEnumerable<string> GetAllMatchingPathsInternal(string pattern, string root)
        {
            char separator = Path.DirectorySeparatorChar;
            string[] parts = pattern.Split(separator);

            for (int i = 0; i < parts.Length; i++)
            {
                // if this part of the path is a wildcard that needs expanding
                if (parts[i].Contains('*') || parts[i].Contains('?'))
                {
                    // create an absolute path up to the current wildcard and check if it exists
                    var combined = root + separator + string.Join(separator.ToString(), parts.Take(i));
                    if (!Directory.Exists(combined))
                    {
                        return new string[0];
                    }
                    
                    if (i == parts.Length - 1) // if this is the end of the path (a file name)
                    {
                        return Directory.EnumerateFiles(combined, parts[i], SearchOption.TopDirectoryOnly).Concat(
                               Directory.EnumerateDirectories(combined, parts[i], SearchOption.TopDirectoryOnly));
                    }
                    else // if this is in the middle of the path (a directory name)
                    {
                        var directories = Directory.EnumerateDirectories(combined, parts[i], SearchOption.TopDirectoryOnly);
                        return directories.SelectMany(dir =>
                            GetAllMatchingPathsInternal(string.Join(separator.ToString(), parts.Skip(i + 1)), dir));
                    }
                }
            }

            // if pattern ends in an absolute path with no wildcards in the filename
            var absolute = root + separator + string.Join(separator.ToString(), parts);
            if (File.Exists(absolute) || Directory.Exists(absolute))
            {
                return new[] { absolute };
            }
            
            return new string[0];
        }

        private string GetPcgwPageId(Game game, string pathsStorePath, bool useCache, bool isBackgroundDownload = false)
        {
            if (useCache && FileSystem.FileExists(pathsStorePath))
            {
                return Serialization.FromJsonFile<GameDirectoriesData>(pathsStorePath).PcgwPageId;
            }
            
            var uri = GetPcgwGameIdSearchUri(game);
            if (!uri.IsNullOrEmpty())
            {
                var downloadedString = HttpDownloader.DownloadString(uri);
                if (!downloadedString.IsNullOrEmpty())
                {
                    var query = Serialization.FromJson<PcgwGameIdCargoQuery>(downloadedString);
                    return query.CargoQuery.First().Title.PageId;
                }
            }

            return GetPcgwPageIdBySearch(game, isBackgroundDownload);
        }

        private bool GetGameDirectories(Game game, string pathsStorePath, bool useCacheId, bool isBackgroundDownload = false)
        {
            var pageId = GetPcgwPageId(game, pathsStorePath, useCacheId, isBackgroundDownload);
            if (pageId.IsNullOrEmpty())
            {
                return false;
            }

            var apiUri = string.Format(@"https://www.pcgamingwiki.com/w/api.php?action=parse&format=json&pageid={0}&prop=wikitext", pageId);
            var apiDownloadedString = HttpDownloader.DownloadString(apiUri);
            if (apiDownloadedString.IsNullOrEmpty())
            {
                return false;
            }

            var wikiTextQuery = Serialization.FromJson<PcgwWikiTextQuery>(apiDownloadedString);
            if (wikiTextQuery.Parse.Wikitext.TextDump.StartsWith("#REDIRECT [["))
            {
                // Pages that redirect don't have any data so we have to obtain them
                // in another request. Example of this is "Horizon Zero Down Complete Edition"
                var titleMatch = Regex.Match(wikiTextQuery.Parse.Wikitext.TextDump, @"#REDIRECT \[\[([^\]]+)\]\]");
                if (!titleMatch.Success)
                {
                    return false;
                }

                var apiUri2 = string.Format(@"https://www.pcgamingwiki.com/w/api.php?action=cargoquery&tables=Infobox_game&fields=Infobox_game._pageID%3DPageID&where=Infobox_game._pageName%3D%22{0}%22&format=json", titleMatch.Groups[1].Value.UrlEncode());
                var apiDownloadedString2 = HttpDownloader.DownloadString(apiUri2);
                if (apiDownloadedString2.IsNullOrEmpty())
                {
                    return false;
                }
                var query = Serialization.FromJson<PcgwGameIdCargoQuery>(apiDownloadedString2);
                pageId = query.CargoQuery.First().Title.PageId;
                var apiUri3 = string.Format(@"https://www.pcgamingwiki.com/w/api.php?action=parse&format=json&pageid={0}&prop=wikitext", pageId);
                var apiDownloadedString3 = HttpDownloader.DownloadString(apiUri3);
                if (apiDownloadedString3.IsNullOrEmpty())
                {
                    return false;
                }

                wikiTextQuery = Serialization.FromJson<PcgwWikiTextQuery>(apiDownloadedString3);
                if (wikiTextQuery.Parse.Wikitext.TextDump.StartsWith("#REDIRECT [["))
                {
                    return false;
                }
            }

            var gameLibraryPluginString = BuiltinExtensions.GetExtensionFromId(game.PluginId).ToString();
            var configDirectories = GetPathsFromContent(wikiTextQuery.Parse.Wikitext.TextDump, PathType.Config, gameLibraryPluginString);
            var saveDirectories = GetPathsFromContent(wikiTextQuery.Parse.Wikitext.TextDump, PathType.Save, gameLibraryPluginString);
            var gameDirsData = new GameDirectoriesData
            {
                PcgwPageId = pageId,
                PathsData = configDirectories.Concat(saveDirectories).ToList()
            };

            FileSystem.WriteStringToFile(pathsStorePath, Serialization.ToJson(gameDirsData));
            AddPathsLinksToGame(game, gameDirsData);

            return true;
        }

        private const string linkSaveTemplate = @"[Save] ";
        private const string linkConfigTemplate = @"[Config] ";
        private void AddPathsLinksToGame(Game game, GameDirectoriesData gameDirsData)
        {
            var replacementDict = GetReplacementDict();
            var linksToAdd = new Dictionary<string, string>();
            var dirDefinitions = GetAvailableDirsFromData(game, replacementDict, gameDirsData, PathType.Config);

            if (settings.Settings.AddSaveDirsAsLinks)
            {
                foreach (var path in GetAvailableDirsFromData(game, replacementDict, gameDirsData, PathType.Save))
                {
                    linksToAdd[linkSaveTemplate + path] = @"file:///" + path;
                }
            }
            
            if (settings.Settings.AddConfigDirsAsLinks)
            {
                foreach (var path in GetAvailableDirsFromData(game, replacementDict, gameDirsData, PathType.Config))
                {
                    linksToAdd[linkConfigTemplate + path] = @"file:///" + path;
                }
            }

            var gameUpdated = false;
            if (game.Links == null)
            {
                game.Links = new System.Collections.ObjectModel.ObservableCollection<Link> { };
            }

            var linksCopy = new System.Collections.ObjectModel.ObservableCollection<Link>(game.Links);
            foreach (var link in linksCopy.ToList())
            {
                if (!link.Name.StartsWith(linkSaveTemplate) || !link.Name.StartsWith(linkConfigTemplate))
                {
                    continue;
                }

                if (!linksToAdd.Any(x => x.Value == link.Url))
                {
                    game.Links.Remove(link);
                    gameUpdated = true;
                }
            }

            foreach (var linkKv in linksToAdd)
            {
                if (!linksCopy.Any(x => x.Url == linkKv.Value))
                {
                    linksCopy.Add(new Link { Name = linkKv.Key, Url = linkKv.Value });
                    gameUpdated = true;
                }
            }

            if (gameUpdated)
            {
                game.Links = linksCopy;
                PlayniteApi.Database.Games.Update(game);
            }
        }

        private const char bracketOpen = '{';
        private const char bracketClose = '}';
        private const char pathSeparator = '|';
        private List<PathData> GetPathsFromContent(string apiDownloadedString, PathType pathType, string gameLibraryPluginString)
        {
            var pathMatches = pcgwContentRegex.Matches(apiDownloadedString); ;
            var paths = new List<PathData>();
            if (pathMatches.Count == 0)
            {
                return paths;         
            }

            foreach (Match match in pathMatches)
            {
                if (pathType == PathType.Config && !match.Value.StartsWith(@"{{Game data/config"))
                {
                    continue;
                }
                else if (pathType == PathType.Save && !match.Value.StartsWith(@"{{Game data/saves"))
                {
                    continue;
                }

                var sectionMatch = match.Value;
                var sectionVersion = Regex.Match(sectionMatch, @"^{{Game data\/[^\|]+\|([^\|]+)").Groups[1].Value.Trim(); ;
                // Paths that don't apply to the specific game version are skipped
                if (sectionVersion == "macOS" || sectionVersion == "OS X" || sectionVersion == "Linux")
                {
                    continue;
                }
                else if (gameLibraryPluginString != "GogLibrary" && sectionVersion == "GOG.com")
                {
                    continue;
                }
                else if (gameLibraryPluginString != "SteamLibrary" && sectionVersion == "Steam")
                {
                    continue;
                }
                else if ((gameLibraryPluginString != "XboxLibrary") && sectionVersion == "Microsoft Store")
                {
                    continue;
                }
                else if (gameLibraryPluginString != "UplayLibrary" && sectionVersion == "Uplay")
                {
                    continue;
                }
                else if (gameLibraryPluginString != "EpicLibrary" && sectionVersion == "Epic Games Store")
                {
                    continue;
                }

                // Remove section start e.g. "{{Game data/config|Windows|"
                sectionMatch = Regex.Replace(sectionMatch.Replace(@"\\", @"\"), @"^{{Game data\/[^\|]+\|[^\|]+\|", "");
                
                // Make all code blocks uniform
                sectionMatch = Regex.Replace(sectionMatch, @"{{code\|[^}]+}}", "{{code}}");

                // Remove all comments
                sectionMatch = Regex.Replace(sectionMatch, @"<!--[^-]+-->", "");
                if (sectionMatch.IsNullOrEmpty())
                {
                    continue;
                }

                // Path sections separate different paths by the "|" character in a single
                // string so we need to separate them
                StringBuilder sb = new StringBuilder();
                int bracketOpenCount = 0;
                int bracketClosedCount = 0;
                int index = 0;
                int stringLenght = sectionMatch.Length;
                foreach (char c in sectionMatch)
                {
                    index++;
                    // We verify if all variables are closed by comparing the number
                    // of open and close bracket chars. If the amount is the same,
                    // the '|' character is a separator for the next path
                    if (c == pathSeparator && bracketOpenCount == bracketClosedCount)
                    {
                        bracketOpenCount = 0;
                        bracketClosedCount = 0;
                        paths.Add(new PathData { Path = sb.ToString().Trim(), Type = pathType } );
                        sb.Clear();
                        continue;
                    }
                    else if (c == bracketOpen)
                    {
                        bracketOpenCount++;
                    }
                    else if (c == bracketClose)
                    {
                        bracketClosedCount++;
                    }
                    
                    if (index == stringLenght)
                    {
                        // We add the last character and create the string
                        sb.Append(c);
                        paths.Add(new PathData { Path = sb.ToString().Trim(), Type = pathType });
                        break;
                    }

                    sb.Append(c);
                }
            }
            
            return paths;
        }

        private string GetPcgwGameIdSearchUri(Game game, bool isBackgroundDownload = false)
        {
            var gameLibraryPlugin = BuiltinExtensions.GetExtensionFromId(game.PluginId);
            if (gameLibraryPlugin == BuiltinExtension.SteamLibrary)
            {
                return string.Format(@"https://www.pcgamingwiki.com/w/api.php?action=cargoquery&tables=Infobox_game&fields=Infobox_game._pageID%3DPageID&where=Infobox_game.Steam_AppID%20HOLDS%20%22{0}%22&format=json", game.GameId.UrlEncode());
            }
            else if (gameLibraryPlugin == BuiltinExtension.GogLibrary)
            {
                return string.Format(@"https://www.pcgamingwiki.com/w/api.php?action=cargoquery&tables=Infobox_game&fields=Infobox_game._pageID%3DPageID&where=Infobox_game.GogCom_ID%20HOLDS%20%22{0}%22&format=json", game.GameId.UrlEncode());
            }

            return null;
        }

        private const string pcgwTitleSearchQuery = @"https://www.pcgamingwiki.com/w/api.php?action=query&list=search&srlimit=10&srwhat=title&srsearch={0}&format=json";
        private string GetPcgwPageIdBySearch(Game game, bool isBackgroundDownload)
        {
            var query = GetPcgwSearchQuery(game.Name);
            if (query == null)
            {
                return null;
            }

            var matchName = game.Name.GetMatchModifiedName();
            foreach (var item in query.Query.Search)
            {
                if (item.Title.GetMatchModifiedName() == matchName)
                {
                    return item.PageId.ToString();
                }
            }

            if (isBackgroundDownload)
            {
                return null;
            }

            var selectedItem = PlayniteApi.Dialogs.ChooseItemWithSearch(
                new List<GenericItemOption>(),
                (a) => GetPcgwSearchOptions(a),
                game.Name,
                ResourceProvider.GetString("LOCSaveFileView_DialogTitleSelectPcgwGame"));

            if (selectedItem != null)
            {
                return selectedItem.Description;
            }

            return string.Empty;
        }

        private PcgwTitleSearch GetPcgwSearchQuery(string gameName)
        {
            var searchUri = string.Format(pcgwTitleSearchQuery, gameName.UrlEncode());
            var downloadedString = HttpDownloader.DownloadString(searchUri);
            if (downloadedString.IsNullOrEmpty())
            {
                return null;
            }

            return Serialization.FromJson<PcgwTitleSearch>(downloadedString);
        }

        private List<GenericItemOption> GetPcgwSearchOptions(string gameName)
        {
            var itemOptions = new List<GenericItemOption>();
            var query = GetPcgwSearchQuery(gameName);
            if (query == null)
            {
                return null;
            }

            foreach (var item in query.Query.Search)
            {
                itemOptions.Add(new GenericItemOption { Name = item.Title, Description = item.PageId.ToString() });
            }

            return itemOptions;
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            var game = args.Game;
            if (!PlayniteUtilities.IsGamePcGame(game))
            {
                return;
            }

            var pathsStorePath = Path.Combine(GetPluginUserDataPath(), $"{game.Id}.json");
            var refreshSuccess = GetGameDirectories(game, pathsStorePath, true, true);
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SaveFileViewSettingsView();
        }
    }
}