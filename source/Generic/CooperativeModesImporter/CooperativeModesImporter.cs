using CooperativeModesImporter.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly string databasePath;
        private readonly Dictionary<string, string> specIdToSystemDictionary;

        private CooperativeModesImporterSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("9767ac15-6e26-4e4c-9d69-f6838625dde3");

        public CooperativeModesImporter(IPlayniteAPI api) : base(api)
        {
            settings = new CooperativeModesImporterSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            databasePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "database.json");
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
                }
            };
        }

        private void AddMpFeaturesToGamesProgress(List<Game> games)
        {
            var updatedGames = 0;
            PlayniteApi.Dialogs.ActivateGlobalProgress((_) =>
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

            var featuresDictionary = new Dictionary<string, GameFeature>();
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

                    var featuresUpdated = false;
                    if (game.FeatureIds == null)
                    {
                        game.FeatureIds = new List<Guid> {};
                        featuresUpdated = true;
                    }

                    foreach (var mpMode in dbGame.Modes)
                    {
                        var mpModeFormat = mpMode;
                        if (settings.Settings.AddPrefix)
                        {
                            mpModeFormat = settings.Settings.FeaturesPrefix + mpMode;
                        }

                        // Should make it faster than trying to create the same
                        // features a lot of times
                        if (!featuresDictionary.ContainsKey(mpModeFormat))
                        {
                            featuresDictionary.Add(mpModeFormat, PlayniteApi.Database.Features.Add(mpModeFormat));
                        }

                        if (game.FeatureIds.AddMissing(featuresDictionary[mpModeFormat].Id))
                        {
                            featuresUpdated = true;
                        }
                    }

                    if (featuresUpdated)
                    {
                        PlayniteApi.Database.Games.Update(game);
                        updatedGames += 1;
                    }
                }
            }

            return updatedGames;
        }

        private static string SatinizeGameName(string str)
        {
            return Regex.Replace(str, @"[^\p{L}\p{Nd}]", "")
                .ToLower()
                .Replace("gameoftheyearedition", "")
                .Replace("gameoftheyear", "")
                .Replace("premiumedition", "");
        }
    }
}