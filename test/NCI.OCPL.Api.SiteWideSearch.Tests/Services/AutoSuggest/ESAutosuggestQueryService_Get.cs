using System;
using System.Text;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Logging.Testing;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Xunit;

using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.Common;


namespace NCI.OCPL.Api.SiteWideSearch.Services.Tests
{
    /// <summary>
    /// Tests for <see cref="M:NCI.OCPL.Api.SiteWideSearch.Services.ESAutosuggestQueryService.Get" />
    /// </summary>
    public partial class ESAutosuggestQueryServiceTest
    {
        /// <summary>
        /// Test behavior when elasticsearch returns an error code indicating that a search couldn't be performed.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(500)]
        [InlineData(403)]
        public async void Get_ConnectionFailure(int statusCode)
        {
            var settings = TestingElasticsearchClientSettingsFactory.Create(
                ElastcsearchTestingTools.MockEmptyResponseString, // Don't care about the response for this test.
                statusCode
            );
            ElasticsearchClient client = new ElasticsearchClient(settings);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, MockAutoSuggestOptions, new NullLogger<ESAutosuggestQueryService>());

            // The parameters and result doesn't matter, just verify that it throws APIInternalException.
            await Assert.ThrowsAsync<APIInternalException>(
                () => autosuggestClient.Get("cgov", "en", "breast cancer", 10)
            );
        }

        /// <summary>
        /// Verify the query service throws the correct exception when
        /// elasticsearch returns an invalid result.
        /// </summary>
        [Fact]
        public async void Get_BadESReturn()
        {
            var settings = TestingElasticsearchClientSettingsFactory.Create(
                "\"This is not the server you were looking for.\"",
                200
            );
            ElasticsearchClient client = new ElasticsearchClient(settings);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, MockAutoSuggestOptions, new NullLogger<ESAutosuggestQueryService>());

            // The parameters and result doesn't matter, just verify that it throws APIInternalException.
            await Assert.ThrowsAsync<APIInternalException>(
                () => autosuggestClient.Get("cgov", "en", "breast cancer", 10)
            );
        }


        /// <summary>
        /// Verify that the request sent to ES for a single term is being set up correctly.
        /// </summary>
        [Theory]
        [InlineData("en", "Breast Cancer")]
        [InlineData("es", "Cáncer de seno")]
        [InlineData("en", " ")]
        [InlineData("es", "    \n ")]
        [InlineData("es", "    \t ")]
        [InlineData("", "")]
        [InlineData("\t", "")]
        public async void Check_For_Correct_Request_Data(string language, string term)
        {
            string expectedPath = "/autosg/_search";
            string expectedContentType = "application/json";
            HttpMethod expectedMethod = HttpMethod.POST;
            JsonNode expectedBody = JsonNode.Parse(@"
{
    ""query"": {
                ""bool"": {
                    ""filter"": [ { ""term"": { ""language"": { ""value"": """ + language + @""" } } } ],
            ""must"": [ { ""match"": { ""term"": { ""query"": """ + term + @""" } } } ]
        }
            },
    ""size"": 25,
    ""sort"": [ { ""weight"": { ""order"": ""desc"" } } ],
    ""_source"": { ""includes"": [ ""term"" ] }
}");

            Uri esURI = null;
            string esContentType = String.Empty;
            HttpMethod esMethod = HttpMethod.DELETE; // Basically, something other than the expected value.

            string requestBody = null;

            var settings = TestingElasticsearchClientSettingsFactory.Create(
                ElastcsearchTestingTools.MockEmptyResponseString,
                200,
                details =>
                {
                    esURI = details.Uri;
                    esContentType = details.ResponseContentType;
                    esMethod = details.HttpMethod;
                    requestBody = Encoding.UTF8.GetString(details.RequestBodyInBytes);
                }
            );
            ElasticsearchClient client = new ElasticsearchClient(settings);


            IAutosuggestQueryService autoSuggestClient = new ESAutosuggestQueryService(
                client,
                MockAutoSuggestOptions,
                NullLogger<ESAutosuggestQueryService>.Instance
            );


            //NOTE: this is when actualReq will get set.
            await autoSuggestClient.Get(
                "cgov",
                language,
                term,
                25
            );

            Assert.Equal(expectedPath, esURI.AbsolutePath);
            Assert.Equal(expectedContentType, esContentType);
            Assert.Equal(expectedMethod, esMethod);
            Assert.True(JsonNode.DeepEquals(expectedBody, JsonNode.Parse(requestBody)));
        }

    }
}