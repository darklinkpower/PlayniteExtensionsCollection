using ImporterforAnilist.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Playnite.SDK.Data;

namespace ImporterforAnilist.Services
{
    class AnilistAccountClient
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI api;
        private readonly string accountAccessCode = string.Empty;
        private readonly string apiListQueryString = @"";
        public readonly string anilistUsername = string.Empty;
        public const string GraphQLEndpoint = @"https://graphql.AniList.co";
        private readonly string getUsernameQueryString = @"query {
            Viewer {
                name
                id
                options {
	                displayAdultContent
	            }
	            mediaListOptions {
	              scoreFormat
	            }
              }
            }";

        public AnilistAccountClient(IPlayniteAPI api, string accountAccessCode)
        {
            this.api = api;
            this.accountAccessCode = accountAccessCode;
            this.anilistUsername = GetUsername();
            this.apiListQueryString = @"
                query GetListByUsername($userName: String!, $type: MediaType!) {
                    list: MediaListCollection(userName: $userName, type: $type, forceSingleCompletedList: true) {
                        lists {
                            status
                            entries {
                                id
                                progress
                                score(format: POINT_100)
                                status
                                updatedAt
                                media {
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
                            }
                        }
                    }
                }";
        }

        public string GetApiPostResponse(Dictionary<string, string> postParams)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accountAccessCode}");
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    string postParamsString = Serialization.ToJson(postParams);
                    client.DefaultRequestHeaders.Add("Body", postParamsString);

                    var response = client.PostAsync(GraphQLEndpoint, new FormUrlEncodedContent(postParams));
                    var contents = response.Result.Content.ReadAsStringAsync();
                    return contents.Result;
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to process post request.");
                    return string.Empty;
                }
            }
        }

        public string GetUsername()
        {
            var postParams = new Dictionary<string, string>
            {
                { "query", getUsernameQueryString },
                { "variables", "" }
            };

            string response = GetApiPostResponse(postParams);
            if (string.IsNullOrEmpty(response))
            {
                return string.Empty;
            }
            var anilistUser = Serialization.FromJson<AnilistUser>(response);
            return anilistUser.Data.Viewer?.Name ?? string.Empty;
        }

        public List<Entry> GetEntries(string listType)
        {
            var entriesList = new List<Entry> { }; 
            
            string type = listType.ToUpper();
            var variables = new Dictionary<string, string>
            {
                { "userName", anilistUsername },
                { "type", type }
            };
            string variablesJson = Serialization.ToJson(variables);
            var postParams = new Dictionary<string, string>
            {
                { "query", apiListQueryString },
                { "variables", variablesJson }
            };

            string response = GetApiPostResponse(postParams);
            if (string.IsNullOrEmpty(response))
            {
                return entriesList;
            }

            var mediaList = Serialization.FromJson<MediaList>(response);

            foreach (ListElement listElement in mediaList.Data.List.Lists)
            {
                foreach (Entry entry in listElement.Entries)
                {
                    entriesList.Add(entry);
                }
            }
            return entriesList;
        }
    }
}