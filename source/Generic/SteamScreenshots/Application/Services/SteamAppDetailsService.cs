using FlowHttp;
using Playnite.SDK.Data;
using PluginsCommon;
using SteamCommon.Models;
using SteamScreenshots.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamScreenshots.Application.Services
{
    public class SteamAppDetailsService : ISteamAppDetailsService
    {
        private readonly ISteamAppDetailsRepository _appDetailsRepository;

        public SteamAppDetailsService(ISteamAppDetailsRepository appDetailsRepository)
        {
            _appDetailsRepository = appDetailsRepository;
        }

        public SteamAppDetails GetAppDetails(string id, bool downloadFromNetwork, CancellationToken cancellationToken = default)
        {
            if (!downloadFromNetwork)
            {
                return _appDetailsRepository.GetAppDetails(id);
            }

            var existingDataCreationDate = _appDetailsRepository.GetAppDetailsCreationDate(id);
            if (!existingDataCreationDate.HasValue || existingDataCreationDate.Value < DateTime.Now.AddDays(-12))
            {
                var url = string.Format(@"https://store.steampowered.com/api/appdetails?appids={0}", id);
                var result = HttpRequestFactory.GetHttpRequest()
                    .WithUrl(url)
                    .DownloadString(cancellationToken);

                if (result.IsSuccess &&
                    Serialization.TryFromJson<Dictionary<string, SteamAppDetails>>(result.Content, out var parsedData)
                    && parsedData.Keys?.Any() == true)
                {
                    var appDetails = parsedData[parsedData.Keys.First()];
                    SaveAppDetails(id, appDetails);
                    return appDetails;
                }
            }

            return _appDetailsRepository.GetAppDetails(id);
        }

        public void SaveAppDetails(string id, SteamAppDetails details)
        {
            _appDetailsRepository.SaveAppDetails(id, details);
        }

        public void DeleteAppDetails(string id)
        {
            _appDetailsRepository.DeleteAppDetails(id);
        }
    }

}