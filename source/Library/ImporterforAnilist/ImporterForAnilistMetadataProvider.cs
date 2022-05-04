using Playnite.SDK.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImporterforAnilist.Models;
using System.Net.Http;
using System.Threading;
using Playnite.SDK.Data;

namespace ImporterforAnilist
{
    public class AnilistMetadataProvider : LibraryMetadataProvider
    {
        private ILogger logger = LogManager.GetLogger(); 
        private readonly IPlayniteAPI PlayniteApi;
        private readonly ImporterForAnilist library;
        private HttpClient client;
        public const string GraphQLEndpoint = @"https://graphql.AniList.co";
        private readonly string apiListQueryString = @"";
        public const string MalSyncAnilistEndpoint = @"https://api.malsync.moe/mal/{0}/anilist:{1}";
        public const string MalSyncMyanimelistEndpoint = @"https://api.malsync.moe/mal/{0}/{1}";
        private readonly string propertiesPrefix = @"";
        private readonly MalSyncRateLimiter malSyncRateLimiter;

        public override void Dispose()
        {
            client.Dispose();
        }

        public AnilistMetadataProvider(ImporterForAnilist library, IPlayniteAPI PlayniteApi, string propertiesPrefix, MalSyncRateLimiter malSyncRateLimiter)
        {
            this.PlayniteApi = PlayniteApi;
            this.library = library;
            this.propertiesPrefix = propertiesPrefix;
            this.malSyncRateLimiter = malSyncRateLimiter;
            this.client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            this.apiListQueryString = @"
                query ($id: Int) {
                    Media (id: $id) {
                        id
        	            idMal
        	            siteUrl	
                        type 
                        format
                        episodes
                        chapters
                        averageScore
                        title {
                            romaji
                            english
                            native
                        }
                        description(asHtml: true)
                        startDate {
                            year
                            month
                            day
                        }
                        genres
                        tags {
                            name
                            isGeneralSpoiler
                            isMediaSpoiler
                        }
                        season
                        status
                        studios(sort: [NAME]) {
                            nodes {
                                name
                                isAnimationStudio
                            }
                        }
                        staff {
                            nodes {
                                name {
                                    full
                                }
                            }
                        }
                        coverImage {
                            extraLarge
                        }
                        bannerImage
                    }
                }";
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var metadata = new GameMetadata() { };
            string type = string.Empty;
            string idMal = string.Empty;
            try
            {
                var variables = new Dictionary<string, string>
                {
                    { "id", game.GameId }
                };

                var variablesJson = Serialization.ToJson(variables);
                var postParams = new Dictionary<string, string>
                {
                    { "query", apiListQueryString },
                    { "variables", variablesJson }
                };

                var response = client.PostAsync(GraphQLEndpoint, new FormUrlEncodedContent(postParams));
                var contents = response.Result.Content.ReadAsStringAsync();

                var mediaEntryData = Serialization.FromJson<MediaEntryData>(contents.Result);
                metadata = AnilistResponseHelper.MediaToGameMetadata(mediaEntryData.Data.Media, true, propertiesPrefix);
                type = mediaEntryData.Data.Media.Type.ToString().ToLower() ?? string.Empty;
                idMal = mediaEntryData.Data.Media.IdMal.ToString().ToLower() ?? string.Empty;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to process AniList query");
            }

            GetMalSyncData(game, metadata, type, idMal);
            return metadata;
        }

        private void GetMalSyncData(Game game, GameMetadata metadata, string type, string idMal)
        {
            if (type.IsNullOrEmpty())
            {
                return;
            }
            
            var queryUri = string.Format(MalSyncAnilistEndpoint, type, game.GameId);
            if (!idMal.IsNullOrEmpty())
            {
                queryUri = string.Format(MalSyncMyanimelistEndpoint, type, idMal);
            }

            try
            {
                malSyncRateLimiter.WaitForSlot();
                var response = client.GetAsync(queryUri);
                var contents = response.Result.Content.ReadAsStringAsync();
                if (contents.Status != TaskStatus.RanToCompletion || contents.Result.IsNullOrEmpty())
                {
                    return;
                }

                if (contents.Result == "Not found in the fire" || contents.Result == "Request failed with status code 404")
                {
                    logger.Info($"MalSync query {queryUri} doesn't have data");
                    return;
                }

                var malSyncResponse = Serialization.FromJson<MalSyncResponse>(contents.Result);
                AddMalSyncLinksToMetadata(metadata, malSyncResponse);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to process MalSync query {queryUri}");
            }
        }

        private static void AddMalSyncLinksToMetadata(GameMetadata metadata, MalSyncResponse malSyncResponse)
        {
            if (malSyncResponse.Sites == null)
            {
                return;
            }

            foreach (var site in malSyncResponse.Sites)
            {
                var siteName = site.Key;
                foreach (var siteItem in site.Value)
                {
                    var malSyncItem = siteItem.Value;
                    metadata.Links.Add(new Link(string.Format("{0} - {1}", siteName, malSyncItem.Title), malSyncItem.Url));
                }
            }
        }
    }
}