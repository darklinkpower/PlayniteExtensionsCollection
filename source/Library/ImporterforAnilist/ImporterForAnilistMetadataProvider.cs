using Playnite.SDK.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImporterforAnilist.Models;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading;

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
        private MalSyncRateLimiter malSyncRateLimiter;

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
                string variablesJson = JsonConvert.SerializeObject(variables);
                var postParams = new Dictionary<string, string>
                {
                    { "query", apiListQueryString },
                    { "variables", variablesJson }
                };
                var response = client.PostAsync(GraphQLEndpoint, new FormUrlEncodedContent(postParams));
                var contents = response.Result.Content.ReadAsStringAsync();

                var mediaEntryData = JsonConvert.DeserializeObject<MediaEntryData>(contents.Result);
                metadata = AnilistResponseHelper.MediaToGameMetadata(mediaEntryData.Data.Media, true, propertiesPrefix);
                type = mediaEntryData.Data.Media.Type.ToString().ToLower() ?? string.Empty;
                idMal = mediaEntryData.Data.Media.IdMal.ToString().ToLower() ?? string.Empty;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to process AniList query");
            }

            if (!string.IsNullOrEmpty(type))
            {
                string queryUri = string.Empty;
                if (!string.IsNullOrEmpty(idMal))
                {
                    queryUri = string.Format(MalSyncMyanimelistEndpoint, type, idMal);
                }
                else
                {
                    queryUri = string.Format(MalSyncAnilistEndpoint, type, game.GameId);
                }

                try
                {
                    if (malSyncRateLimiter.ApiReqRemaining == 0)
                    {
                        Thread.Sleep(5000);
                    }
                    malSyncRateLimiter.Decrease();
                    var response = client.GetAsync(string.Format(queryUri));
                    var contents = response.Result.Content.ReadAsStringAsync();
                    if (contents.Result != "Not found in the fire")
                    {
                        JObject jsonObject = JObject.Parse(contents.Result);
                        if (jsonObject["Sites"] != null)
                        {
                            foreach (var site in jsonObject["Sites"])
                            {
                                string siteName = site.Path.Replace("Sites.", "");

                                foreach (var siteItem in site)
                                {
                                    foreach (var item in siteItem)
                                    {
                                        string str = item.First().ToString();
                                        var malSyncItem = JsonConvert.DeserializeObject<MalSyncSiteItem>(str);
                                        metadata.Links.Add(new Link(string.Format("{0} - {1}", siteName, malSyncItem.Title), malSyncItem.Url));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Info($"MalSync query {queryUri} doesn't have data");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to process MalSync query {queryUri}");
                }
            }
            return metadata;
        }
    }
}