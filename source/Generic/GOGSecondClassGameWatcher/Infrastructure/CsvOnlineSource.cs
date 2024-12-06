using FlowHttp;
using GOGSecondClassGameWatcher.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Infrastructure
{
    public class CsvOnlineSource : ICsvDataSource
    {
        private const string GeneralCsvDownloadUrl = @"https://docs.google.com/spreadsheets/d/1zjwUN1mtJdCkgtTDRB2IoFp7PP41fraY-oFNY00fEkI/gviz/tq?tqx=out:csv&headers=0";
        private const string AchievementsCsvDownloadUrl = @"https://docs.google.com/spreadsheets/d/1pDO6WTHLHyrrtidQ1MAxW6u8j3BxUaGcFaJsVyWj2QY/gviz/tq?tqx=out:csv&headers=0";

        public CsvOnlineSource()
        {
        }

        public string GetGeneralIssuesCsvData()
        {
            return GetCsvDataFromUrl(GeneralCsvDownloadUrl);
        }

        public string GetAchievementsIssuesCsvData()
        {
            return GetCsvDataFromUrl(AchievementsCsvDownloadUrl);
        }

        private string GetCsvDataFromUrl(string url)
        {
            var csvDataResult = HttpRequestFactory.GetHttpRequest(url).DownloadString();
            if (!csvDataResult.IsSuccess)
            {
                return null;
            }

            return csvDataResult.Content;
        }
    }

}