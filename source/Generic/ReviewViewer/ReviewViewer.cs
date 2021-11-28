using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using ReviewViewer.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ReviewViewer
{
    public class ReviewViewer : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly Regex steamLinkRegex = new Regex(@"^https?:\/\/store\.steampowered\.com\/app\/(\d+)", RegexOptions.Compiled);
        private string steamApiLanguage;

        private ReviewViewerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("ca24e37a-76d9-49bf-89ab-d3cba4a54bd1");

        public ReviewViewer(IPlayniteAPI api) : base(api)
        {
            settings = new ReviewViewerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = "ReviewViewer",
                ElementList = new List<string> { "ReviewsControl" }
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "ReviewViewer",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            steamApiLanguage = "english";
            if (settings.Settings.UseMatchingSteamApiLang)
            {
                steamApiLanguage = GetSteamApiMatchingLanguage(PlayniteApi.ApplicationSettings.Language);
            }
        }

        public string GetSteamApiMatchingLanguage(string playniteLanguage)
        {
            // From https://partner.steamgames.com/doc/store/localization

            switch (playniteLanguage)
            {
                case "en_US":
                    return "english";
                case "es_ES":
                    return "spanish";
                case "ar_SA":
                    return "ar";
                case "ca_ES":
                    return "spanish";
                case "cs_CZ":
                    return "cs";
                case "de_DE":
                    return "de";
                case "el_GR":
                    return "el";
                case "fa_IR":
                    return "english";
                case "fi_FI":
                    return "fi";
                case "fr_FR":
                    return "fr";
                case "he_IL":
                    return "english";
                case "hr_HR":
                    return "english";
                case "hu_HU":
                    return "hu";
                case "id_ID":
                    return "english";
                case "it_IT":
                    return "it";
                case "ja_JP":
                    return "ja";
                case "ko_KR":
                    return "ko";
                case "lt_LT":
                    return "english";
                case "nl_NL":
                    return "nl";
                case "no_NO":
                    return "no";
                case "pl_PL":
                    return "pl";
                case "pt_BR":
                    return "pt-BR";
                case "pt_PT":
                    return "pt";
                case "ro_RO":
                    return "ro";
                case "ru_RU":
                    return "ru";
                case "sk_SK":
                    return "english";
                case "sr_SP":
                    return "english";
                case "sv_SE":
                    return "sv";
                case "tr_TR":
                    return "tr";
                case "uk_UA":
                    return "english";
                case "zh_CN":
                    return "zh-CN";
                case "zh_TW":
                    return "zh-TW";
                default:
                    return "english";
            }
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "ReviewsControl")
            {
                return new ReviewsControl(GetPluginUserDataPath(), steamApiLanguage, settings, PlayniteApi);
            }

            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ReviewViewerSettingsView();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCReview_Viewer_MenuItemUpdateDataDescription"),
                    MenuSection = "Review Viewer",
                    Action = a => {
                       RefreshGameData(args.Games);
                    }
                }
            };
        }

        public void RefreshGameData(List<Game> games)
        {
            var userOverwriteChoice = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCReview_Viewer_DialogOverwriteChoiceMessage"), "Review Viewer", MessageBoxButton.YesNo);
            var reviewSearchTypes = new string[] { "all", "positive", "negative" };
            var pluginDataPath = GetPluginUserDataPath();
            var steamPluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
            var reviewsApiMask = @"https://store.steampowered.com/appreviews/{0}?json=1&purchase_type=all&language={1}&review_type={2}&playtime_filter_min=0&filter=summary";

            PlayniteApi.Dialogs.ActivateGlobalProgress((a) => {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.Timeout = TimeSpan.FromMilliseconds(2000);
                    foreach (Game game in games)
                    {
                        string steamId;
                        if (game.PluginId == steamPluginId)
                        {
                            steamId = game.GameId;
                        }
                        else
                        {
                            steamId = GetSteamIdFromLinks(game);
                            if (steamId == null)
                            {
                                continue;
                            }
                        }

                        foreach (string reviewSearchType in reviewSearchTypes)
                        {
                            var gameDataPath = Path.Combine(pluginDataPath, $"{game.Id}_{reviewSearchType}.json");
                            if (File.Exists(gameDataPath))
                            {
                                if (userOverwriteChoice != MessageBoxResult.Yes)
                                {
                                    continue;
                                }
                            }
                            var uri = string.Format(reviewsApiMask, steamId, steamApiLanguage, reviewSearchType);

                            // To prevent being rate limited
                            Thread.Sleep(200);
                            DownloadFile(httpClient, uri, gameDataPath).GetAwaiter().GetResult();
                        }
                    }
                }
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCReview_Viewer_DialogDataUpdateProgressMessage")));

            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCReview_Viewer_DialogResultsDataRefreshFinishedMessage"), "Review Viewer");
        }

        public async Task DownloadFile(HttpClient client, string requestUri, string fileToWriteTo)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during file download, url {requestUri}. Error: {e.Message}.");
            }
        }

        private string GetSteamIdFromLinks(Game game)
        {
            if (game.Links == null)
            {
                return null;
            }

            foreach (Link gameLink in game.Links)
            {
                var linkMatch = steamLinkRegex.Match(gameLink.Url);
                if (linkMatch.Success)
                {
                    return linkMatch.Groups[1].Value;
                }
            }
            return null;
        }

    }
}