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
            var menuSection = "Game directories";
            
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
                        /*throw NotImplementedException*/;
                    }
                }
            };
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
                    GetGameDirectories(game, pathsStorePath);
                    if (!File.Exists(pathsStorePath))
                    {
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCSaveFileView_CouldntGetGameDirectoriesMessage"), game.Name));
                        continue;
                    }
                }

                var data = Serialization.FromJsonFile<GameDirectoriesData>(pathsStorePath);
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

                var openedDirs = 0;
                foreach (var path in pathDefinitions.Distinct())
                {
                    if (Directory.Exists(path))
                    {
                        ProcessStarter.StartProcess(path);
                        openedDirs++;
                    }
                    else if (File.Exists(path))
                    {
                        ProcessStarter.StartProcess(Path.GetDirectoryName(path));
                        openedDirs++;
                    }
                }

                if (openedDirs == 0)
                {
                    PlayniteApi.Dialogs.ShowMessage(
                        string.Format(ResourceProvider.GetString("LOCSaveFileView_DirectoriesNotDetectedMessage"), game.Name),
                        "Save File View"
                    );
                    continue;
                }
            }
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

        private bool GetGameDirectories(Game game, string pathsStorePath)
        {
            var askArgsUri = GetPcgwAskArgsUri(game);
            if (askArgsUri.IsNullOrEmpty())
            {
                return false;
            }

            var downloadedString = HttpDownloader.DownloadString(askArgsUri);
            if (downloadedString.IsNullOrEmpty())
            {
                return false;
            }

            var query = Serialization.FromJson<PcgwGameIdCargoQuery>(downloadedString);
            var pageId = query.CargoQuery.First().Title.PageId;
            var apiUri = string.Format(@"https://www.pcgamingwiki.com/w/api.php?action=parse&format=json&pageid={0}&prop=wikitext", pageId);
            var apiDownloadedString = HttpDownloader.DownloadString(apiUri);
            if (apiDownloadedString.IsNullOrEmpty())
            {
                return false;
            }

            var wikiTextQuery = Serialization.FromJson<PcgwWikiTextQuery>(apiDownloadedString);
            var gameLibraryPluginString = BuiltinExtensions.GetExtensionFromId(game.PluginId).ToString();

            var configDirectories = GetPathsFromContent(wikiTextQuery.Parse.Wikitext.TextDump, PathType.Config, gameLibraryPluginString);
            var saveDirectories = GetPathsFromContent(wikiTextQuery.Parse.Wikitext.TextDump, PathType.Save, gameLibraryPluginString);
            var gameDirsData = new GameDirectoriesData
            {
                PcgwPageId = pageId,
                PathsData = configDirectories.Concat(saveDirectories).ToList()
            };

            FileSystem.WriteStringToFile(pathsStorePath, Serialization.ToJson(gameDirsData));
            return true;
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

        private string GetPcgwAskArgsUri(Game game)
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

            // TODO Search on PCGW with game name
            return string.Empty;
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
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