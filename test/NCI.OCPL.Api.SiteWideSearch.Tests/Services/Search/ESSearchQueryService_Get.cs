using System;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Logging.Testing;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.Common.Testing;
using System.Text;

namespace NCI.OCPL.Api.SiteWideSearch.Services.Tests
{
    /// <summary>
    /// Tests for <see cref="M:NCI.OCPL.Api.SiteWideSearch.Services.ESSearchQueryService.Get" />
    /// </summary>
    public partial class ESSearchQueryServiceTest : ESSearchQueryService_Base
    {
        /// <summary>
        /// Verify the query service throws the correct exception when elasticsearch
        /// returns an error code indicating that a search couldn't be performed.
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

            ESSearchQueryService searchClient = new ESSearchQueryService(client, MockSearchOptions, new NullLogger<ESSearchQueryService>());

            // The parameters and result doesn't matter, just verify that it throws APIInternalException.
            await Assert.ThrowsAsync<APIInternalException>(
                () => searchClient.Get("cgov", "en", "breast cancer", 10, 10, new string[] {"all"})
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

            ESSearchQueryService searchClient = new ESSearchQueryService(client, MockSearchOptions, new NullLogger<ESSearchQueryService>());

            // The parameters and result doesn't matter, just verify that it throws APIInternalException.
            await Assert.ThrowsAsync<APIInternalException>(
                () => searchClient.Get("cgov", "en", "breast cancer", 10, 10, new string[] { "all" })
            );
        }


        [Theory]
        [InlineData("cgov", "en", 0, 10)]
        [InlineData("cgov", "es", 500, 20)]
        [InlineData("doc", "en", 200, 30)]
        [InlineData("doc", "es", 500, 100)]
        /// <summary>
        /// Verify that the request sent to ES for a single term is being set up correctly.
        /// </summary>
        public async void Check_For_Correct_Request_Data(string requestedCollection, string requestedLanguage, int requestedFrom, int requestedSize)
        {
            string expectedMimeType = "application/json";
            string expectedUrl = "/cgov/_search";
            HttpMethod expectedMethod = HttpMethod.POST;
            JsonNode expectedSort = JsonNode.Parse(@"
                [
                    {""_score"": {}},
                    {""url"": {}}
                ]
            ");


            Uri actualURI = null;
            string actualMimeType = String.Empty;
            HttpMethod actualMethod = HttpMethod.DELETE; // Something other than the expected value (default is GET).

            string requestBody = null;

            var settings = TestingElasticsearchClientSettingsFactory.Create(
                ElastcsearchTestingTools.MockEmptyResponseString,
                200,
                details =>
                {
                    actualURI = details.Uri;
                    actualMimeType = details.ResponseContentType;
                    actualMethod = details.HttpMethod;
                    if(details.RequestBodyInBytes != null)
                    {
                        requestBody = Encoding.UTF8.GetString(details.RequestBodyInBytes);
                    }
                }
            );
            ElasticsearchClient client = new ElasticsearchClient(settings);

            ISearchQueryService searchClient = new ESSearchQueryService(
                client,
                MockSearchOptions,
                NullLogger<ESSearchQueryService>.Instance
            );


            // NOTE: this is when actualReq will get set.
            await searchClient.Get(
                requestedCollection, // Search collection to use
                requestedLanguage,   // language
                "Breast Cancer",   // term
                requestedFrom,      // from
                requestedSize,     // size
                new string[] { "all" }   // site parameter
            );

            Assert.Equal(expectedMimeType, actualMimeType);
            Assert.Equal(expectedUrl, actualURI.AbsolutePath);
            Assert.Equal(expectedMethod, actualMethod);

            // The structure of the actual query is tested in the SitewideSearchQueryBuilder_Test class.
            JsonNode body = JsonNode.Parse(requestBody);
            Assert.Equal(requestedSize, (int)body["size"]);
            Assert.Equal(requestedFrom, (int)body["from"]);
            Assert.True(JsonNode.DeepEquals(expectedSort, body["sort"]));
            Assert.True((bool)body["track_total_hits"]);
        }

    }
}
