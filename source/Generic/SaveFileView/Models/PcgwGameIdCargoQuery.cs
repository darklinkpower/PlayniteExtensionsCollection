using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveFileView.Models
{
    public partial class PcgwGameIdCargoQuery
    {
        [SerializationPropertyName("cargoquery")]
        public CargoQuery[] CargoQuery { get; set; }
    }

    public partial class CargoQuery
    {
        [SerializationPropertyName("title")]
        public Title Title { get; set; }
    }

    public partial class Title
    {
        [SerializationPropertyName("PageID")]
        public string PageId { get; set; }
    }
}