using ReviewViewer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewViewer.Application
{
    public static class CacheKeyBuilder
    {
        public static string BuildCacheKey(int appId, QueryOptions options, string cursor = "*")
        {
            var sb = new StringBuilder();
            sb.Append("app=").Append(appId);

            sb.Append("_rt=").Append((int)options.ReviewType);
            sb.Append("_pt=").Append((int)options.PurchaseType);
            sb.Append("_lang=").Append(options.Language ?? "null");

            sb.Append("_dr=").Append((int)options.DateRangeMode);
            if (options.DateRangeMode == DateRangeMode.Specific || options.DateRangeMode == DateRangeMode.Exclude)
            {
                sb.Append("_start=").Append(options.StartDateUtc?.ToString("yyyyMMdd") ?? "null");
                sb.Append("_end=").Append(options.EndDateUtc?.ToString("yyyyMMdd") ?? "null");
            }

            sb.Append("_ppt=").Append((int)options.PlaytimePreset);
            if (options.PlaytimePreset == PlaytimePreset.Custom)
            {
                sb.Append("_ptmin=").Append(options.CustomPlaytimeMinHours);
                sb.Append("_ptmax=").Append(options.CustomPlaytimeMaxHours);
            }

            sb.Append("_dev=").Append((int)options.PlaytimeDevice);
            sb.Append("_disp=").Append((int)options.Display);
            sb.Append("_lsm=").Append((int)options.LanguageSelectionMode);

            var langs = options.SelectedLanguages?.OrderBy(l => (int)l)
                          .Select(l => ((int)l).ToString()) ?? Enumerable.Empty<string>();
            sb.Append("_langs=").Append(string.Join(",", langs.DefaultIfEmpty("none")));

            sb.Append("_help=").Append(options.UseHelpfulSystem ? "1" : "0");
            sb.Append("_off=").Append(options.FilterOfftopicActivity ? "1" : "0");

            sb.Append("_cur=").Append(cursor);

            return sb.ToString();
        }
    }
}
