namespace SteamShortcuts.Domain.ValueObjects
{
    public class PointsShopCount
    {
        public class Response
        {
            public int total_count { get; set; }
            public int count { get; set; }
        }

        public Response response;
    }
}
