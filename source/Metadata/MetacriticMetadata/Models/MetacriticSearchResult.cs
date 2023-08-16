using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetacriticMetadata.Models
{
    public class MetacriticSearchResult : IEquatable<MetacriticSearchResult>
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Platform { get; set; }
        public string ReleaseInfo { get; set; }
        public string Description { get; set; }
        public int? MetacriticScore { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is MetacriticSearchResult metacriticSearchResult)
            {
                return Equals(metacriticSearchResult);
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(MetacriticSearchResult obj1, MetacriticSearchResult obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(MetacriticSearchResult obj1, MetacriticSearchResult obj2)
        {
            return !obj1.Equals(obj2);
        }

        public bool Equals(MetacriticSearchResult other)
        {
            return Name == other.Name &&
                Url == other.Url &&
                Platform == other.Platform &&
                ReleaseInfo == other.ReleaseInfo &&
                Description == other.Description &&
                MetacriticScore == other.MetacriticScore;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                Url.GetHashCode() ^
                Platform.GetHashCode() ^
                ReleaseInfo.GetHashCode() ^
                Description.GetHashCode() ^
                (MetacriticScore ?? 0).GetHashCode();
        }
    }
}