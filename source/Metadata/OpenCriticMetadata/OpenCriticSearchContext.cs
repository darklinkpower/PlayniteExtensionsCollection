using OpenCriticMetadata.Services;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCriticMetadata
{
    public class OpenCriticSearchContext : SearchContext
    {
        private readonly OpenCriticService openCriticService;
        private const string openCriticGameUrlTemplate = @"https://opencritic.com/game/{0}/{1}";

        public OpenCriticSearchContext(OpenCriticService openCriticService)
        {
            Description = "Enter search term";
            Label = "OpenCritic";
            Hint = "Searches games on OpenCritic";
            Delay = 600;
            this.openCriticService = openCriticService;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            var searchItems = new List<SearchItem>();
            var searchTerm = args.SearchTerm;
            if (searchTerm.IsNullOrEmpty())
            {
                return searchItems;
            }

            var searchResults = openCriticService.GetGameSearchResults(searchTerm);
            if (args.CancelToken.IsCancellationRequested)
            {
                return searchItems;
            }

            foreach (var searchResult in searchResults)
            {
                var cleanName = GetGameUrlName(searchResult.Name);
                var url = string.Format(openCriticGameUrlTemplate, searchResult.Id, cleanName);
                var searchItem = new SearchItem(
                    searchResult.Name,
                    new SearchItemAction("Open on browser",
                    () => { ProcessStarter.StartUrl(url); })
                )
                {
                    SecondaryAction = new SearchItemAction("Open on browser",
                    () => { PlayniteUtilities.OpenUrlOnWebView(url); })
                };

                searchItems.Add(searchItem);
            }

            return searchItems;
        }

        private static string GetGameUrlName(string input)
        {
            var words = input.Split(new char[] { ' ', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var resultBuilder = new StringBuilder();

            for (int i = 0; i < words.Length; i++)
            {
                var addedCharacters = 0;
                foreach (var c in words[i])
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        resultBuilder.Append(char.ToLowerInvariant(c));
                        addedCharacters++;
                    }
                }

                if (addedCharacters > 0)
                {
                    resultBuilder.Append("-");
                }
            }

            if (resultBuilder.Length > 0)
            {
                resultBuilder.Length--; //To remove the last appended - char
            }

            return resultBuilder.ToString();
        }
    }
}