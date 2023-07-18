using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Nest;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Xunit;

using NCI.OCPL.Api.Common.Testing;

namespace NCI.OCPL.Api.SiteWideSearch.Services.Tests
{
    /// <summary>
    /// All of the data mapping tests to ensure we are able to correctly map the responses
    /// from ES into the correct response from the Search service.
    /// </summary>
    public partial class ESSearchQueryServiceTest : ESSearchQueryService_Base
    {
        [Fact]
        /// <summary>
        /// Test for Breast Cancer term and ensures TotalResults is mapped correctly.
        /// </summary>
        public async void Has_Correct_Total()
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            IOptions<SearchIndexOptions> config =  MockSearchOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESSearchQueryService searchClient = new ESSearchQueryService(client, config, NullLogger<ESSearchQueryService>.Instance);

            //Parameters don't matter in this case...
            SiteWideSearchResults results = await searchClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10,
                10,
                new string[] { "all" }
            );

            Assert.Equal(12915, results.TotalResults);
        }

        [Fact]
        /// <summary>
        /// Test that search mapping returns correct number of results for an empty result set.
        /// (And also that it doesn't explode!)
        /// </summary>
        public async void No_Results_Has_Correct_Total()
        {
            string testFile = "Search.CGov.En.NoResults.json";

            IOptions<SearchIndexOptions> config = MockSearchOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESSearchQueryService searchClient = new ESSearchQueryService(client, config, new NullLogger<ESSearchQueryService>());

            //Parameters don't matter in this case...
            SiteWideSearchResults results = await searchClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10,
                10,
                new string[] { "all" }
            );

            Assert.Empty(results.Results);
        }

        [Fact]
        /// <summary>
        /// Test that the search results at arbitrary offsets
        /// in the collection are present
        /// </summary>
        public async void Check_Results_Present()
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            IOptions<SearchIndexOptions> config = MockSearchOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESSearchQueryService searchClient = new ESSearchQueryService(client, config, new NullLogger<ESSearchQueryService>());

            //Parameters don't matter in this case...
            SiteWideSearchResults results = await searchClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10,
                10,
                new string[] { "all" }
            );
            Assert.All(results.Results, item => Assert.NotNull(item));
        }

        [Theory]
        [InlineData("Search.CGov.En.MetadataNestedArray.json", "Search.CGov.En.MetadataNestedArray-Expected.json")]
        [InlineData("Search.CGov.En.MetadataArray.json", "Search.CGov.En.MetadataArray-Expected.json")]
        [InlineData("Search.CGov.En.MetadataSingle.json", "Search.CGov.En.MetadataSingle-Expected.json")]
        /// <summary>
        /// Test that the metadata description field is handled correctly when
        /// it contains an array of values instead of a single string.
        /// </summary>
        public async void Check_Metadata_Description_Handling(string testFile, string expectedFile)
        {
            JObject expected = TestingTools.GetDataFileAsJObject(expectedFile);

            IOptions<SearchIndexOptions> config = MockSearchOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESSearchQueryService searchClient = new ESSearchQueryService(client, config, new NullLogger<ESSearchQueryService>());

            //Parameters don't matter in this case...
            SiteWideSearchResults results = await searchClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10,
                10,
                new string[] { "all" }
            );


            JToken actual = JToken.Parse(JsonConvert.SerializeObject(results));
            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }

        [Fact]
        /// <summary>
        /// Test that the search results at arbitrary offsets
        /// in the collection are present
        /// </summary>
        public async void Check_RequiredField_Present()
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            IOptions<SearchIndexOptions> config = MockSearchOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESSearchQueryService searchClient = new ESSearchQueryService(client, config, new NullLogger<ESSearchQueryService>());

            //Parameters don't matter in this case...
            SiteWideSearchResults results = await searchClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10,
                10,
                new string[] { "all" }
            );

            Assert.All(results.Results, item => Assert.NotNull(item.URL));
        }

    }
}