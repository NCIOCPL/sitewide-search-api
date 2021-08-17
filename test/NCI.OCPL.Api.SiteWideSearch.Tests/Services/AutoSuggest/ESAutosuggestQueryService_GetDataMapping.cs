using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Nest;
using Xunit;

using NCI.OCPL.Api.Common.Testing;

namespace NCI.OCPL.Api.SiteWideSearch.Services.Tests
{
    /// <summary>
    /// All of the data mapping tests to ensure we are able to correctly map the responses
    /// from ES into the correct response from the Autosuggest service.
    /// </summary>
    public partial class ESAutosuggestQueryServiceTest
    {
        [Fact]
        /// <summary>
        /// Test that the list of results exists.
        /// </summary>
        public async void Check_Results_Exist()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = MockAutoSuggestOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, config, new NullLogger<ESAutosuggestQueryService>());

            //Parameters don't matter in this case...
            Suggestions results = await autosuggestClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10
            );

            Assert.NotEmpty(results.Results);
        }

        [Fact]
        /// <summary>
        /// Test that the search results at arbitrary offsets
        /// in the collection are present
        /// </summary>
        public async void Check_Results_Present()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = MockAutoSuggestOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, config, new NullLogger<ESAutosuggestQueryService>());

            //Parameters don't matter in this case...
            Suggestions results = await autosuggestClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10
            );

            Assert.All(results.Results, item => Assert.NotNull(item));
        }

        [Fact]
        /// <summary>
        /// Test that the list of returned results has the right number of items.
        /// </summary>
        public async void Check_Result_Count()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = MockAutoSuggestOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, config, new NullLogger<ESAutosuggestQueryService>());

            //Parameters don't matter in this case...
            Suggestions results = await autosuggestClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10
            );

            Assert.Equal(20, results.Results.Length);
        }

        [Fact]
        /// <summary>
        /// Test that the first result contains the expected string.
        /// </summary>
        public async void Check_First_Result()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = MockAutoSuggestOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, config, new NullLogger<ESAutosuggestQueryService>());

            //Parameters don't matter in this case...
            Suggestions results = await autosuggestClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10
            );

            Assert.Equal("breast cancer", results.Results[0].Term);
        }

        [Theory]
        [InlineData(0, "breast cancer")]
        [InlineData(3, "metastatic breast cancer")]
        [InlineData(17, "breast cancer risk assessment")]
        [InlineData(19, "breast cancer symptoms")]
        /// <summary>
        /// Test that the suggested search strings from arbitrary offsets
        /// in the collection have the correct values
        /// </summary>
        /// <param name="offset">Offset into the list of results of the item to check.</param>
        /// <param name="expectedTerm">The expected term text</param>
        public async void Check_Arbitrary_Result(int offset, string expectedTerm)
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = MockAutoSuggestOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, config, new NullLogger<ESAutosuggestQueryService>());

            //Parameters don't matter in this case...
            Suggestions results = await autosuggestClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10
            );

            Assert.Equal(expectedTerm, results.Results[offset].Term);
        }

        [Fact]
        /// <summary>
        /// Test for Breast Cancer search string and ensures Total is mapped correctly.
        /// </summary>
        public async void Has_Correct_Total()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = MockAutoSuggestOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, config, new NullLogger<ESAutosuggestQueryService>());

            //Parameters don't matter in this case...
            Suggestions results = await autosuggestClient.Get(
                "cgov",
                "en",
                "breast cancer",
                10
            );

            Assert.Equal(222, results.Total);
        }
    }
}