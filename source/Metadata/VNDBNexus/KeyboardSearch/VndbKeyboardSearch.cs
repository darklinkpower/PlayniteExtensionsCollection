using EventsCommon;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.VisualNovelAggregate;
using VndbApiInfrastructure.ProducerAggregate;
using VndbApiInfrastructure.Services;
using VndbApiInfrastructure.VisualNovelAggregate;
using VNDBNexus.VndbVisualNovelViewControlAggregate;

namespace VNDBNexus.KeyboardSearch
{
    public class VndbKeyboardSearch : SearchContext
    {
        private readonly VNDBNexusSettingsViewModel _settings;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPlayniteAPI _playniteAPI = API.Instance;

        public VndbKeyboardSearch(VNDBNexusSettingsViewModel settings, IEventAggregator eventAggregator)
        {
            Description = ResourceProvider.GetString("LOC_VndbNexus_EnterSearchTermLabel");
            Label = "VNDB";
            Hint = ResourceProvider.GetString("LOC_VndbNexus_EnterSearchHint");
            Delay = 700;
            _settings = settings;
            _eventAggregator = eventAggregator;
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            var searchItems = new List<SearchItem>();
            if (args.SearchTerm.IsNullOrEmpty())
            {
                return searchItems;
            }

            var searchResults = Task.Run(async () => await GetVndbResultsAsync(args.SearchTerm, args.CancelToken)).Result;
            if (!searchResults.Any())
            {
                return searchItems;
            }

            searchItems.AddRange(searchResults.Select(searchResult => GetSearchItemFromSearchResult(searchResult)));
            return searchItems;
        }

        private SearchItem GetSearchItemFromSearchResult(VisualNovel visualNovel)
        {
            var searchItem = new SearchItem
            (
                visualNovel.Title,
                new SearchItemAction
                (
                    ResourceProvider.GetString("LOC_VndbNexus_ViewVnInPlayniteLabel"),
                    () => SearchAndSelectVisualNovel(visualNovel)
                )
            )
            {
                Description = GetSearchItemDescription(visualNovel),
                SecondaryAction = new SearchItemAction
                (
                    ResourceProvider.GetString("LOC_VndbNexus_OpenOnWebLabel"),
                    () => { ProcessStarter.StartUrl($"https://vndb.org/{visualNovel.Id}"); }
                )
            };

            if (visualNovel.Image != null)
            {
                searchItem.Icon = visualNovel.Image.ThumbnailUrl.ToString();
            }
            

            return searchItem;
        }

        private static string GetSearchItemDescription(VisualNovel visualNovel)
        {
            var descriptionLines = new List<string>
            {
                string.Format(ResourceProvider.GetString("LOC_VndbNexus_IdFormat"), visualNovel.Id)
            };

            if (visualNovel.ReleaseDate != null)
            {
                descriptionLines.Add(visualNovel.ReleaseDate.ToString());
            }

            if (visualNovel.Developers.HasItems())
            {
                var newLine = string.Format
                (
                    ResourceProvider.GetString("LOC_VndbNexus_DevelopersFormat"),
                    string.Join(", ", visualNovel.Developers)
                );

                descriptionLines.Add(newLine);
            }
            
            var finalString = string.Join(Environment.NewLine, descriptionLines);
            return finalString;
        }

        private async Task<IEnumerable<VisualNovel>> GetVndbResultsAsync(string searchTerm, CancellationToken cancelToken)
        {
            var isSearchVndbId = Regex.IsMatch(searchTerm, @"^v\d+$");
            var vndbRequestFilter = isSearchVndbId
                ? VisualNovelFilterFactory.Id.EqualTo(searchTerm)
                : VisualNovelFilterFactory.Search.EqualTo(searchTerm);

            var query = new VisualNovelRequestQuery(vndbRequestFilter);
            query.Fields.DisableAllFlags(true);

            query.Fields.Flags = VnRequestFieldsFlags.Title | VnRequestFieldsFlags.Id;
            query.Fields.Subfields.Developers.Flags = ProducerRequestFieldsFlags.Name;
            query.Fields.Subfields.Image.Flags = ImageRequestFieldsFlags.ThumbnailUrl;
            query.Results = 6;
            var queryResult = await VndbService.ExecutePostRequestAsync(query, cancelToken);
            if (queryResult?.Results?.Count > 0)
            {
                return queryResult.Results;
            }

            return Enumerable.Empty<VisualNovel>();
        }

        public void SearchAndSelectVisualNovel(VisualNovel visualNovel)
        {
            if (_playniteAPI.MainView.ActiveDesktopView == DesktopView.List)
            {
                _playniteAPI.MainView.ActiveDesktopView = DesktopView.Details;
            }

            _playniteAPI.MainView.SwitchToLibraryView();
            _eventAggregator.Publish(new InvokeVisualNovelDisplayEvent(visualNovel));
        }

    }
}
