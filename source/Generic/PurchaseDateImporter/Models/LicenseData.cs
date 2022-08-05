using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseDateImporter.Models
{
    public class LicenseData
    {
        public string Name;
        public DateTime PurchaseDate;

        public LicenseData(string name, DateTime purchaseDate)
        {
            Name = name;
            PurchaseDate = purchaseDate;
        }
    }
}