using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist.Models
{
    public class UpdateMediaListEntriesResponse
    {
        [SerializationPropertyName("data")]
        public UpdateMediaListData Data { get; set; }
    }

    public class UpdateMediaListData
    {
        [SerializationPropertyName("UpdateMediaListEntries")]
        public UpdateMediaListEntry[] UpdateMediaListEntries { get; set; }
    }

    public class UpdateMediaListEntry
    {
        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("status")]
        public EntryStatus Status { get; set; }
    }
}