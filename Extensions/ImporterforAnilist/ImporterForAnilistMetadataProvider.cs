using ImporterforAnilist.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
        public int malsyncRequestsCount = 0;

        public override void Dispose()
        {
            client.Dispose();
        }

        public AnilistMetadataProvider(ImporterForAnilist library, IPlayniteAPI PlayniteApi, string propertiesPrefix)
        {
            this.PlayniteApi = PlayniteApi;
            this.library = library;
            this.propertiesPrefix = propertiesPrefix;
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

        public GameInfo MediaToGameInfo(Media media, string propertiesPrefix)
        {
            var game = new GameInfo()
            {
                Source = "AniList",
                GameId = media.Id.ToString(),
                Name = media.Title.Romaji ?? media.Title.English ?? media.Title.Native ?? string.Empty,
                IsInstalled = true,
                Platform = string.Format("AniList {0}", media.Type.ToString()),
                CommunityScore = media.AverageScore ?? null,
                Description = media.Description ?? string.Empty,
                Links = new List<Link>()
            };

            //Links
            game.Links.Add(new Link("AniList", media.SiteUrl.ToString()));
            if (media.Genres != null)
            {
                game.Genres = media.Genres.Select(a => string.Format("{0}{1}", propertiesPrefix, a)).ToList();
            }
            if (media.IdMal != null)
            {
                game.Links.Add(new Link("MyAnimeList", string.Format("https://myanimelist.net/{0}/{1}/", media.Type.ToString().ToLower(), media.IdMal.ToString())));
            }

            //ReleaseDate
            if (media.StartDate.Year != null && media.StartDate.Month != null && media.StartDate.Day != null)
            {
                game.ReleaseDate = new DateTime((int)media.StartDate.Year, (int)media.StartDate.Month, (int)media.StartDate.Day);
            }

            //Developers and Publishers
            if (media.Type == TypeEnum.Manga)
            {
                game.Developers = media.Staff.Nodes.
                    Select(a => string.Format("{0}{1}", propertiesPrefix, a.Name.Full)).ToList();
            }
            else if (media.Type == TypeEnum.Anime)
            {
                game.Developers = media.Studios.Nodes.Where(s => s.IsAnimationStudio == true).
                    Select(a => string.Format("{0}{1}", propertiesPrefix, a.Name)).ToList();
                game.Publishers = media.Studios.Nodes.Where(s => s.IsAnimationStudio == false).
                    Select(a => string.Format("{0}{1}", propertiesPrefix, a.Name)).ToList();
            }

            //Tags
            var tags = media.Tags.
                Where(s => s.IsMediaSpoiler == false).
                Where(s => s.IsGeneralSpoiler == false).
                Select(a => string.Format("{0}{1}", propertiesPrefix, a.Name)).ToList();

            if (media.Season != null)
            {
                tags.Add(string.Format("{0}Season: {1}", propertiesPrefix, media.Season.ToString()));
            }
            tags.Add(string.Format("{0}Status: {1}", propertiesPrefix, media.Status.ToString()));
            tags.Add(string.Format("{0}Format: {1}", propertiesPrefix, media.Format.ToString()));

            game.Tags = tags;

            return game;
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
                metadata.GameInfo = MediaToGameInfo(mediaEntryData.Data.Media, propertiesPrefix);
                metadata.BackgroundImage = new MetadataFile(mediaEntryData.Data.Media.BannerImage ?? string.Empty);
                metadata.CoverImage = new MetadataFile(mediaEntryData.Data.Media.CoverImage.ExtraLarge ?? string.Empty);
                type = mediaEntryData.Data.Media.Type.ToString().ToLower() ?? string.Empty;
                idMal = mediaEntryData.Data.Media.IdMal.ToString().ToLower() ?? string.Empty;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to process AniList query");
            }

            if (!string.IsNullOrEmpty(type))
            {
                //malSync allows 20 requests without limits. After 20 it opens a new request slot each 5 seconds. Using 10 for safety.
                if (malsyncRequestsCount > 10)
                {
                    Thread.Sleep(5000);
                }
                malsyncRequestsCount++;

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
                                        metadata.GameInfo.Links.Add(new Link(string.Format("{0} - {1}", siteName, malSyncItem.Title), malSyncItem.Url));
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