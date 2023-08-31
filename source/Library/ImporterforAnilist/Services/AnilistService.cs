using ImporterforAnilist.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Playnite.SDK.Data;
using WebCommon;

namespace ImporterforAnilist.Services
{
    public class AnilistService
    {
        private ILogger logger = LogManager.GetLogger();
        public string anilistUsername = string.Empty;
        private const string graphQLEndpoint = @"https://graphql.AniList.co";
        private ImporterForAnilistSettingsViewModel settings;
        private const string getUsernameQuery = @"
        query GetUsername{
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

        private const string updateIdsStatusQuery = @"
        mutation ($ids: [Int], $status: MediaListStatus) {
            UpdateMediaListEntries (ids: $ids, status: $status) {
                id
                status
            }
        }";

        private const string getUserListQuery = @"
        query GetUserListQuery($userName: String!, $type: MediaType!) {
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
                            startDate {
                                year
                                month
                                day
                            }
                            status
                        }
                    }
                }
            }
        }";

        private const string getEntryDataByIdQuery = @"
        query GetEntryDataById($id: Int) {
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

        public AnilistService(ImporterForAnilistSettingsViewModel settings)
        {
            this.settings = settings;
        }

        public bool GetIsLoggedIn()
        {
            anilistUsername = GetUsername();
            if (!anilistUsername.IsNullOrEmpty())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetApiPostResponse(Dictionary<string, string> postParams)
        {
            var headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["Authorization"] = $"Bearer {settings.Settings.AccountAccessCode}"
            };

            var jsonPostContent = Serialization.ToJson(postParams);
            var downloadStringResult = HttpDownloader.DownloadStringFromPostContent(graphQLEndpoint, jsonPostContent, headers);
            if (downloadStringResult.Success)
            {
                return downloadStringResult.Result;
            }
            else
            {
                logger.Error(downloadStringResult.HttpRequestException, "Failed to process post request.");
                return string.Empty;
            }
        }

        public string GetUsername()
        {
            var postParams = new Dictionary<string, string>
            {
                { "query", getUsernameQuery },
                { "variables", "" }
            };
            
            var response = GetApiPostResponse(postParams);
            if (response.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var anilistUser = Serialization.FromJson<AnilistUser>(response);
            return anilistUser.Data.Viewer?.Name ?? string.Empty;
        }

        public UpdateMediaListEntriesResponse UpdateEntriesStatuses(List<int> Ids, EntryStatus newStatus)
        {
            var vars = @"
            {{
              ""ids"": [{0}],
              ""status"": ""{1}""
            }}";

            var varsEnc = string.Format(vars, string.Join(", ", Ids), newStatus.ToString().ToUpperInvariant());
            var postParams = new Dictionary<string, string>
                {
                    { "query", updateIdsStatusQuery },
                    { "variables", varsEnc }
                };

            var response = GetApiPostResponse(postParams);
            if (response.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                return Serialization.FromJson<UpdateMediaListEntriesResponse>(response);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error during UpdateEntriesStatuses");
                return null;
            }
        }

        public List<Entry> GetEntries(string listType)
        {
            var entriesList = new List<Entry>();
            string type = listType.ToUpper();
            var variables = new Dictionary<string, string>
            {
                { "userName", anilistUsername },
                { "type", type }
            };

            var variablesJson = Serialization.ToJson(variables);
            var postParams = new Dictionary<string, string>
            {
                { "query", getUserListQuery },
                { "variables", variablesJson }
            };

            var response = GetApiPostResponse(postParams);
            if (response.IsNullOrEmpty())
            {
                return entriesList;
            }

            var mediaList = Serialization.FromJson<MediaList>(response);
            entriesList.AddRange(mediaList.Data.List.Lists
                .SelectMany(listElement => listElement.Entries));

            return entriesList;
        }

        public MediaEntryData GetMediaDataById(string anilistId)
        {
            var headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json"
            };

            var variables = new Dictionary<string, string>
            {
                { "id", anilistId }
            };

            var variablesJson = Serialization.ToJson(variables);
            var postParams = new Dictionary<string, string>
            {
                { "query", getEntryDataByIdQuery },
                { "variables", variablesJson }
            };

            var jsonPostContent = Serialization.ToJson(postParams);
            var downloadStringResult = HttpDownloader.DownloadStringFromPostContent(graphQLEndpoint, jsonPostContent, headers);
            if (downloadStringResult.Success)
            {
                return Serialization.FromJson<MediaEntryData>(downloadStringResult.Result);
            }

            return null;
        }

    }
}