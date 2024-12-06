using GOGSecondClassGameWatcher.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Infrastructure
{
    public class CsvFileDataSource : ICsvDataSource
    {
        private readonly string _filePath;
        private readonly string _achievementsFilePath;

        public CsvFileDataSource(string filePath, string achievementsFilePath)
        {
            _filePath = filePath;
            _achievementsFilePath = achievementsFilePath;
        }

        public string GetGeneralIssuesCsvData()
        {
            return File.ReadAllText(_filePath);
        }

        public string GetAchievementsIssuesCsvData()
        {
            return File.ReadAllText(_achievementsFilePath);
        }
    }
}