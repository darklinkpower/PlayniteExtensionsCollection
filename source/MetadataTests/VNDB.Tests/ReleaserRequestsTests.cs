using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiInfrastructure.ReleaseAggregate;
using VndbApiInfrastructure.Services;
using Xunit;

namespace VNDB.Tests
{
    public class ReleaserRequestsTests
    {
        [Fact]
        public async void Request_GetsAnyResults()
        {
            var releaseFilter = ReleaseFilterFactory.Voiced.EqualTo(null);
            var releaseQuery = new ReleaseRequestQuery(releaseFilter)
            {
                Results = 10
            };

            var releaseQueryResult = await VndbService.ExecutePostRequestAsync(releaseQuery);
            Assert.True(releaseQueryResult.Results.Any());
        }

        [Fact]
        public async void Request_GetsById()
        {
            var releaseFilter = ReleaseFilterFactory.Id.EqualTo("r47588");
            var releaseQuery = new ReleaseRequestQuery(releaseFilter);

            releaseQuery.Fields.Subfields.Images.EnableAllFlags();
            releaseQuery.Fields.Flags |= ReleaseRequestFieldsFlags.ImagesLanguages;

            var releaseQueryResult = await VndbService.ExecutePostRequestAsync(releaseQuery);
            Assert.True(releaseQueryResult.Results.Any());
        }
    }
}