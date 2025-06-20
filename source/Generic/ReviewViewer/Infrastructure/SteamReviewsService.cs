using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlowHttp;
using Playnite.SDK.Data;
using ReviewViewer.Application;
using ReviewViewer.Domain;

namespace ReviewViewer.Infrastructure
{
    public class SteamReviewsService : ISteamReviewsProvider
    {
        private const string BaseUrl = "https://store.steampowered.com/appreviews";
        private static readonly Dictionary<SteamLanguage, string> _steamQueryMap = new Dictionary<SteamLanguage, string>
        {
            { SteamLanguage.Bulgarian, "bulgarian" },
            { SteamLanguage.Czech, "czech" },
            { SteamLanguage.Danish, "danish" },
            { SteamLanguage.Dutch, "dutch" },
            { SteamLanguage.English, "english" },
            { SteamLanguage.Finnish, "finnish" },
            { SteamLanguage.French, "french" },
            { SteamLanguage.German, "german" },
            { SteamLanguage.Greek, "greek" },
            { SteamLanguage.Hungarian, "hungarian" },
            { SteamLanguage.Indonesian, "indonesian" },
            { SteamLanguage.Italian, "italian" },
            { SteamLanguage.Japanese, "japanese" },
            { SteamLanguage.Korean, "koreana" },
            { SteamLanguage.Norwegian, "norwegian" },
            { SteamLanguage.Polish, "polish" },
            { SteamLanguage.PortugueseBrazil, "brazilian" },
            { SteamLanguage.PortuguesePortugal, "portuguese" },
            { SteamLanguage.Romanian, "romanian" },
            { SteamLanguage.Russian, "russian" },
            { SteamLanguage.SimplifiedChinese, "schinese" },
            { SteamLanguage.TraditionalChinese, "tchinese" },
            { SteamLanguage.SpanishSpain, "spanish" },
            { SteamLanguage.SpanishLatinAmerica, "latam" },
            { SteamLanguage.Swedish, "swedish" },
            { SteamLanguage.Thai, "thai" },
            { SteamLanguage.Turkish, "turkish" },
            { SteamLanguage.Ukrainian, "ukrainian" },
            { SteamLanguage.Vietnamese, "vietnamese" },
        };

        public async Task<ReviewsResponseDto> GetReviewsAsync(
            int appId, QueryOptions options, CancellationToken cancellationToken = default, string cursor = "*")
        {
            var url = BuildUrl(appId, options, cursor);
            var request = HttpRequestFactory.GetHttpRequest().WithUrl(url);
            var response = await request.DownloadStringAsync(cancellationToken);
            if (Serialization.TryFromJson<ReviewsResponseDto>(response.Content, out var data))
            {
                return data;
            }

            return null;
        }

        private string BuildUrl(int appId, QueryOptions opt, string cursor)
        {
            var sb = new StringBuilder($"{BaseUrl}/{appId}?json=1&cursor={Uri.EscapeDataString(cursor)}");
            if (opt.FilterOfftopicActivity)
            {
                sb.Append("&filter_offtopic_activity=1");
            }
            else
            {
                sb.Append("&filter_offtopic_activity=0");
            }

            if (opt.LanguageSelectionMode == LanguageSelectionMode.Custom && opt.SelectedLanguages?.Count > 0)
            {
                var languageParam = string.Join(",", opt.SelectedLanguages
                    .Distinct()
                    .Select(lang => _steamQueryMap[lang]));
                sb.Append($"&language={languageParam}");
            }
            else
            {
                sb.Append("&language=all");
            }

            sb.Append("&l=english");
            switch (opt.ReviewType)
            {
                case ReviewType.All:
                    sb.Append("&review_type=all");
                    break;
                case ReviewType.Positive:
                    sb.Append("&review_type=positive");
                    break;
                case ReviewType.Negative:
                    sb.Append("&review_type=negative");
                    break;
                default:
                    break;
            }

            switch (opt.PurchaseType)
            {
                case PurchaseType.All:
                    sb.Append("&purchase_type=all");
                    break;
                case PurchaseType.Steam:
                    sb.Append("&purchase_type=steam");
                    break;
                case PurchaseType.Other:
                    sb.Append("&purchase_type=non_steam_purchase");
                    break;
                default:
                    break;
            }

            switch (opt.DateRangeMode)
            {
                case DateRangeMode.Lifetime:
                    sb.Append($"&date_range_type=all");
                    break;
                case DateRangeMode.Specific:
                    sb.Append($"&date_range_type=include");
                    break;
                case DateRangeMode.Exclude:
                    sb.Append($"&date_range_type=exclude");
                    break;
                default:
                    break;
            }

            if (opt.DateRangeMode == DateRangeMode.Lifetime)
            {
                sb.Append($"&start_date=-1&end_date=-1");
            }
            else if (opt.StartDateUtc.HasValue && opt.EndDateUtc.HasValue)
            {
                var unixStart = new DateTimeOffset(opt.StartDateUtc.Value).ToUnixTimeSeconds();
                sb.Append($"&start_date={unixStart}");
                var unixEnd = new DateTimeOffset(opt.EndDateUtc.Value).ToUnixTimeSeconds();
                sb.Append($"&end_date={unixEnd}");
            }

            if (opt.PlaytimePreset == PlaytimePreset.None)
            {
                sb.Append($"&playtime_filter_min=0");
                sb.Append($"&playtime_filter_max=0");
            }
            else if (opt.PlaytimePreset == PlaytimePreset.Over1Hour)
            {
                sb.Append("&playtime_filter_min=1");
            }
            else if (opt.PlaytimePreset == PlaytimePreset.Over10Hours)
            {
                sb.Append("&playtime_filter_min=10");
            }
            else if (opt.PlaytimePreset == PlaytimePreset.Custom)
            {
                sb.Append($"&playtime_filter_min={opt.CustomPlaytimeMinHours}");
                sb.Append($"&playtime_filter_max={opt.CustomPlaytimeMaxHours}");
            }

            switch (opt.PlaytimeDevice)
            {
                case PlaytimeDevice.All:
                    sb.Append("&playtime_type=all");
                    break;
                case PlaytimeDevice.SteamDeck:
                    sb.Append("&playtime_type=deck");
                    break;
                default:
                    break;
            }
            
            switch (opt.Display)
            {
                case DisplayType.Summary:
                    sb.Append("&filter=summary");
                    break;
                case DisplayType.MostHelpful:
                    sb.Append("&filter=all");
                    break;
                case DisplayType.Recent:
                    sb.Append("&filter=recent");
                    break;
                case DisplayType.Funny:
                    sb.Append("&filter=funny");
                    break;
                default:
                    break;
            }

            // Only applies to MostHelpful and Summary but is included in all display options requests anyways
            if (opt.UseHelpfulSystem)
            {
                sb.Append("&use_review_quality=1");
            }
            else
            {
                sb.Append("&use_review_quality=0");
            }

            return sb.ToString();
        }
    }
}