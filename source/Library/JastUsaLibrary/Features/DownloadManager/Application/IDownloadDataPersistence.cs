using JastUsaLibrary.DownloadManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.DownloadManager.Application
{
    public interface IDownloadDataPersistence
    {
        void PersistDownloadData(IEnumerable<DownloadData> downloadsData);
        void PersistDownloadData(DownloadData downloadData);
        List<DownloadData> LoadPersistedDownloads();
        bool ClearPersistedDownloads();
    }
}