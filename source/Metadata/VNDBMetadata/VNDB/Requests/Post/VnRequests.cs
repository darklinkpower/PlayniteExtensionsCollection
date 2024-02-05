using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDB.ApiConstants;
using VNDBMetadata.VNDB.Models;

namespace VNDBMetadata.VNDB.Requests.Post
{
    public class VnRequests
    {
        public static StandardPredicateBuilder<int> Id = new StandardPredicateBuilder<int>(Vn.Filters.Id);

        // Method for "search" - requires a string
        public static StandardPredicateBuilder<string> Search = new StandardPredicateBuilder<string>(Vn.Filters.Search);

        /// <summary>
        /// Lang search.
        /// </summary>
        public static StandardPredicateBuilder<string> Lang = new StandardPredicateBuilder<string>(Vn.Filters.Language);

        // Method for "olang" - requires a string
        public static StandardPredicateBuilder<string> Olang = new StandardPredicateBuilder<string>(Vn.Filters.OriginalLanguage);

        // Method for "platform" - requires a string
        public static StandardPredicateBuilder<string> Platform() => new StandardPredicateBuilder<string>(Vn.Filters.Platform);

        // Method for "length" - requires a double between 1 and 5
        public static OrderingPredicateBuilder<double> Length = new OrderingPredicateBuilder<double>(Vn.Filters.Length);

        // Method for "released" - requires a DateTime or string
        public static OrderingPredicateBuilder<DateTime> Released = new OrderingPredicateBuilder<DateTime>(Vn.Filters.Released);

        // Method for "rating" - requires an int between 10 and 100
        public static OrderingPredicateBuilder<int> Rating = new OrderingPredicateBuilder<int>(Vn.Filters.Rating);

        // Method for "votecount" - requires an int
        public static OrderingPredicateBuilder<int> VoteCount = new OrderingPredicateBuilder<int>(Vn.Filters.VoteCount);

        // Method for "has_description" - requires an int
        public static StandardPredicateBuilder<int> HasDescription = new StandardPredicateBuilder<int>(Vn.Filters.VoteCount);

        // Method for "has_anime" - requires an int
        public static StandardPredicateBuilder<int> HasAnime = new StandardPredicateBuilder<int>(Vn.Filters.HasAnime);

        // Method for "has_screenshot" - requires an int
        public static StandardPredicateBuilder<int> HasScreenshot = new StandardPredicateBuilder<int>(Vn.Filters.HasScreenshot);

        // Method for "has_review" - requires an int
        public static StandardPredicateBuilder<int> HasReview = new StandardPredicateBuilder<int>(Vn.Filters.HasReview);

        // Method for "devstatus" - requires an int
        public static StandardPredicateBuilder<int> DevStatus = new StandardPredicateBuilder<int>(Vn.Filters.DevStatus);

        // Method for "anime_id" - requires an int
        public static StandardPredicateBuilder<int> AnimeId = new StandardPredicateBuilder<int>(Vn.Filters.AnimeId);

        // Method for "label" - requires a two-element array of ints
        public static StandardPredicateBuilder<int[]> Label = new StandardPredicateBuilder<int[]>(Vn.Filters.Label);

        // Method for "release" - requires a string
        public static StandardPredicateBuilder<string> Release = new StandardPredicateBuilder<string>(Vn.Filters.Release);

        // Method for "character" - requires a string
        public static StandardPredicateBuilder<string> Character = new StandardPredicateBuilder<string>(Vn.Filters.Character);

        // Method for "staff" - requires a string
        public static StandardPredicateBuilder<string> Staff = new StandardPredicateBuilder<string>(Vn.Filters.Staff);

        // Method for "developer" - requires a string
        public static StandardPredicateBuilder<string> Developer = new StandardPredicateBuilder<string>(Vn.Filters.Developer);

        // Method for "label" - requires a two-element array of ints
        public static StandardPredicateBuilder<TFirst, TSecond> Label1<TFirst, TSecond>(TFirst first, TSecond second)
        {
            return new StandardPredicateBuilder<TFirst, TSecond>("label");
        }
    }
}