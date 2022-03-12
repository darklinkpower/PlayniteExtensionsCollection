using CooperativeModesImporter.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CooperativeModesImporter
{
    public class CooperativeModesImporter : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string databaseZipPath;
        private readonly string databasePath;
        private readonly int currentDatabaseVersion = 2;
        private readonly Dictionary<string, string> specIdToSystemDictionary;
        private Dictionary<string, GameFeature> featuresDictionary;

        private CooperativeModesImporterSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("9767ac15-6e26-4e4c-9d69-f6838625dde3");

        public CooperativeModesImporter(IPlayniteAPI api) : base(api)
        {
            settings = new CooperativeModesImporterSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            databaseZipPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "database.zip");
            databasePath = Path.Combine(GetPluginUserDataPath(), "database.json");
            specIdToSystemDictionary = new Dictionary<string, string>
            {
                //{ "3do", string.Empty },
                //{ "adobe_flash", string.Empty },
                //{ "amstrad_cpc", string.Empty },
                //{ "apple_2", string.Empty },
                //{ "bandai_wonderswan", string.Empty },
                //{ "bandai_wonderswan_color", string.Empty },
                //{ "coleco_vision", string.Empty },
                //{ "commodore_64", string.Empty },
                //{ "commodore_amiga", string.Empty },
                //{ "commodore_amiga_cd32", string.Empty },
                //{ "commodore_pet", string.Empty },
                //{ "commodore_plus4", string.Empty },
                //{ "commodore_vci20", string.Empty },
                //{ "macintosh", string.Empty },
                //{ "nintendo_virtualboy", string.Empty },
                //{ "pc_linux", string.Empty },
                //{ "sega_gamegear", string.Empty },
                //{ "sega_mastersystem", string.Empty },
                //{ "sinclair_zx81", string.Empty },
                //{ "sinclair_zxspectrum", string.Empty },
                //{ "sinclair_zxspectrum3", string.Empty },
                //{ "snk_neogeo_cd", string.Empty },
                //{ "snk_neogeopocket", string.Empty },
                //{ "snk_neogeopocket_color", string.Empty },
                //{ "vectrex", string.Empty },
                { "atari_2600", "ATA" },
                { "atari_5200", "ATA" },
                { "atari_7800", "ATA" },
                { "atari_8bit", "ATA" },
                { "atari_jaguar", "JAG" },
                { "atari_lynx", "ATA" },
                { "atari_st", "ATA" },
                { "mattel_intellivision", "INT" },
                { "nec_pcfx", "TGFX" },
                { "nec_supergrafx", "TGFX" },
                { "nec_turbografx_16", "TGFX" },
                { "nec_turbografx_cd", "TGFX" },
                { "nintendo_3ds", "3DS" },
                { "nintendo_64", "N64" },
                { "nintendo_ds", "DS" },
                { "nintendo_famicom_disk", "NES" },
                { "nintendo_gameboy", "GB" },
                { "nintendo_gameboyadvance", "GBA" },
                { "nintendo_gameboycolor", "GB" },
                { "nintendo_gamecube", "GCN" },
                { "nintendo_nes", "NES" },
                { "nintendo_super_nes", "SNES" },
                { "nintendo_switch", "NSWI" },
                { "nintendo_wii", "Wii" },
                { "nintendo_wiiu", "WIIU" },
                { "pc_dos", "DOS" },
                { "pc_windows", "PC" },
                { "sega_32x", "GEN" },
                { "sega_cd", "SCD" },
                { "sega_dreamcast", "DCST" },
                { "sega_genesis", "GEN" },
                { "sega_saturn", "SAT" },
                { "sony_playstation", "PS1" },
                { "sony_playstation2", "PS2" },
                { "sony_playstation3", "PS3" },
                { "sony_playstation4", "PS4" },
                { "sony_playstation5", "PS5" },
                { "sony_psp", "PSP" },
                { "sony_vita", "VITA" },
                { "xbox", "XBOX" },
                { "xbox360", "360" },
                { "xbox_one", "XB1" },
                { "xbox_series", "XS" }
            };
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (settings.Settings.DatabaseVersion < currentDatabaseVersion || !File.Exists(databasePath))
            {

                PlayniteApi.Dialogs.ActivateGlobalProgress((_) =>
                {
                    logger.Info("Decompressing database zip file...");
                    ZipFile.ExtractToDirectory(databaseZipPath, GetPluginUserDataPath());
                    logger.Info("Decompressed database zip file");
                }, new GlobalProgressOptions(ResourceProvider.GetString("LOCCooperativeModesImporter_DialogProgressMessageUpdatingDatabase")));

                if (settings.Settings.DatabaseVersion != currentDatabaseVersion)
                {
                    logger.Info($"Database updated from {settings.Settings.DatabaseVersion} to {currentDatabaseVersion}");
                    settings.Settings.DatabaseVersion = currentDatabaseVersion;
                    SavePluginSettings(settings.Settings);
                }
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CooperativeModesImporterSettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCCooperativeModesImporter_MenuItemDescriptionAddMultiplayerFeaturesAllGames"),
                    MenuSection = "@Cooperative Modes Importer",
                    Action = a => {
                        AddMpFeaturesToGamesProgress(PlayniteApi.Database.Games.ToList());
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCCooperativeModesImporter_MenuItemDescriptionAddMultiplayerFeaturesSelectedGames"),
                    MenuSection = "@Cooperative Modes Importer",
                    Action = a => {
                        AddMpFeaturesToGamesProgress(PlayniteApi.MainView.SelectedGames.ToList());
                    }
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCCooperativeModesImporter_MenuItemDescriptionAddMultiplayerFeaturesSelectedGamesManual"),
                    MenuSection = "@Cooperative Modes Importer",
                    Action = a => {
                        AddModesManual();
                    }
                }
            };
        }

        private void AddMpFeaturesToGamesProgress(List<Game> games)
        {
            var updatedGames = 0;
            PlayniteApi.Dialogs.ActivateGlobalProgress(progArgs =>
            {
                updatedGames = AddMpFeaturesToGames(games);
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCCooperativeModesImporter_ProgressDialogMessageFeaturesUpdateInProgress")));

            PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCCooperativeModesImporter_UpdateFeaturesResult"), updatedGames));
        }

        private int AddMpFeaturesToGames(List<Game> games)
        {
            var database = Serialization.FromJson<CooperativeDatabase>(File.ReadAllText(databasePath));
            foreach (var databaseItem in database.Games)
            {
                databaseItem.Name = SatinizeGameName(databaseItem.Name);
            }

            featuresDictionary = new Dictionary<string, GameFeature>();
            var updatedGames = 0;
            foreach (var game in games)
            {
                if (game.Platforms == null || game.Platforms.Count < 0
                    || string.IsNullOrEmpty(game.Platforms[0].SpecificationId))
                {
                    continue;
                }
                
                if (specIdToSystemDictionary.TryGetValue(game.Platforms[0].SpecificationId, out var systemId))
                {
                    var satinizedName = SatinizeGameName(game.Name);
                    var dbGame = database.Games.FirstOrDefault(x => x.Name == satinizedName && x.System == systemId);
                    if (dbGame == null)
                    {
                        continue;
                    }

                    if (ApplyCooptimusData(game, dbGame))
                    {
                        updatedGames++;
                    }
                }
            }

            return updatedGames;
        }

        private void AddModesManual()
        {
            var database = Serialization.FromJson<CooperativeDatabase>(File.ReadAllText(databasePath));
            featuresDictionary = new Dictionary<string, GameFeature>();
            var updatedGames = 0;
            foreach (var game in PlayniteApi.MainView.SelectedGames)
            {
                if (game.Platforms == null || game.Platforms.Count < 0
                    || string.IsNullOrEmpty(game.Platforms[0].SpecificationId))
                {
                    continue;
                }

                if (specIdToSystemDictionary.TryGetValue(game.Platforms[0].SpecificationId, out var systemId))
                {
                    var selectedItem = PlayniteApi.Dialogs.ChooseItemWithSearch(
                        new List<GenericItemOption>(),
                        (a) => GetCooptimusItemOptions(a, systemId, database),
                        game.Name,
                        ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectGame"));
                    
                    if (selectedItem != null)
                    {
                        var selectedDb = database.Games.FirstOrDefault(x => x.Id.ToString() == selectedItem.Description);
                        if (selectedDb != null)
                        {
                            if (ApplyCooptimusData(game, selectedDb))
                            {
                                updatedGames++;
                            }
                        }
                    }
                }
            }

            PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCCooperativeModesImporter_UpdateFeaturesResult"), updatedGames));
        }

        private bool ApplyCooptimusData(Game game, CooperativeGame dbGame)
        {
            var gameUpdated = false;
            if (game.FeatureIds == null)
            {
                game.FeatureIds = new List<Guid> { };
                gameUpdated = true;
            }

            var modesBasic = GetBasicModes(dbGame);
            var modesDetailed = GetDetailedModes(dbGame);
            foreach (var coopName in modesBasic.Concat(modesDetailed))
            {
                // Should make it faster than trying to create the same
                // features a lot of times
                if (!featuresDictionary.ContainsKey(coopName))
                {
                    featuresDictionary.Add(coopName, PlayniteApi.Database.Features.Add(coopName));
                }

                if (game.FeatureIds.AddMissing(featuresDictionary[coopName].Id))
                {
                    gameUpdated = true;
                }
            }

            if (settings.Settings.AddLinkOnImport)
            {
                if (game.Links == null)
                {
                    game.Links = new ObservableCollection<Link> { new Link { Name = "Co-Optimus", Url = dbGame.Url } };
                    gameUpdated = true;
                }
                else if (!game.Links.Any(x => x.Url == dbGame.Url))
                {
                    var linksCopy = new ObservableCollection<Link>(game.Links)
                        {
                            new Link { Name = "Co-Optimus", Url = dbGame.Url }
                        };
                    game.Links = linksCopy;
                    gameUpdated = true;
                }
            }

            if (gameUpdated)
            {
                PlayniteApi.Database.Games.Update(game);
            }

            return gameUpdated;
        }

        private List<GenericItemOption> GetCooptimusItemOptions(string gameName, string systemId, CooperativeDatabase database)
        {
            var selectOptions = new List<Tuple<int, CooperativeGame>>();
            foreach (var dbGame in database.Games)
            {
                if (dbGame.System != systemId)
                {
                    continue;
                }

                var distance = gameName.GetLevenshteinDistance(dbGame.Name);
                if (distance <= 5)
                {
                    selectOptions.Add(Tuple.Create(distance, dbGame));
                }
            }

            if (selectOptions.Count == 0)
            {
                return new List<GenericItemOption>();
            }

            selectOptions.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            return selectOptions.Select(x => new GenericItemOption($"{x.Item2.Name} ({x.Item2.System})", x.Item2.Id.ToString())).Take(10).ToList();
        }

        private List<string> GetBasicModes(CooperativeGame dbGame)
        {
            var modesBasic = new List<string>();
            if (!settings.Settings.ImportBasicModes)
            {
                return modesBasic;
            }

            foreach (var mode in dbGame.Modes)
            {
                if (settings.Settings.AddPrefix)
                {
                    modesBasic.Add(settings.Settings.FeaturesPrefix + mode);
                }
                else
                {
                    modesBasic.Add(mode);
                }
            }

            return modesBasic;
        }

        private List<string> GetDetailedModes(CooperativeGame dbGame)
        {
            var modesDetailed = new List<string>();
            if (!settings.Settings.ImportDetailedModes)
            {
                return modesDetailed;
            }
            
            if (settings.Settings.ImportDetailedModeLocal && dbGame.ModesDetailed.LocalCoop != "Not Supported")
            {
                var modeName = GetDetailedModeFormatedStr($"Local Co-Op: {dbGame.ModesDetailed.LocalCoop}");
                modesDetailed.Add(modeName);
            }

            if (settings.Settings.ImportDetailedModeOnline && dbGame.ModesDetailed.OnlineCoop != "Not Supported")
            {
                var modeName = GetDetailedModeFormatedStr($"Online Co-Op: {dbGame.ModesDetailed.OnlineCoop}");
                modesDetailed.Add(modeName);
            }

            if (settings.Settings.ImportDetailedModeCombo && dbGame.ModesDetailed.ComboCoop != "Not Supported")
            {
                var modeName = GetDetailedModeFormatedStr($"Combo Co-Op: {dbGame.ModesDetailed.ComboCoop}");
                modesDetailed.Add(modeName);
            }

            if (settings.Settings.ImportDetailedModeLan && dbGame.ModesDetailed.LanPlay != "Not Supported")
            {
                var modeName = GetDetailedModeFormatedStr($"LAN Play: {dbGame.ModesDetailed.LanPlay}");
                modesDetailed.Add(modeName);
            }

            if (settings.Settings.ImportDetailedModeExtras && dbGame.ModesDetailed.Extras?.Count < 0)
            {
                foreach (var extraInfo in dbGame.ModesDetailed.Extras)
                {
                    var modeName = GetDetailedModeFormatedStr($"Co-Op Extras: {extraInfo}");
                    modesDetailed.Add(modeName);
                }
            }

            return modesDetailed;
        }

        private string GetDetailedModeFormatedStr(string modeName)
        {
            if (settings.Settings.AddDetailedPrefix)
            {
                return settings.Settings.FeaturesDetailedPrefix + modeName;
            }
            else
            {
                return modeName;
            }
        }

        private static string SatinizeGameName(string str)
        {
            return Regex.Replace(str.Replace(" & ", " And "), @"[^\p{L}\p{Nd}]", "")
                .ToLower()
                .Replace("gameoftheyearedition", "")
                .Replace("gameoftheyear", "")
                .Replace("premiumedition", "");
        }
    }
}