using System;
using System.IO;
using System.Text;

using Microsoft.Extensions.Logging.Testing;

using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.Common.Testing;
using Newtonsoft.Json.Linq;

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
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<SiteWideSearchResults>>((req, res) =>
            {
                res.StatusCode = statusCode;
            });
            // The URL doesn't matter, it won't be used.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var connectionSettings = new ConnectionSettings(pool, conn, sourceSerializer: JsonNetSerializer.Default);
            IElasticClient client = new ElasticClient(connectionSettings);

            ESSearchQueryService searchClient = new ESSearchQueryService(client, MockSearchOptions, new NullLogger<ESSearchQueryService>());

            // The parameters and result doesn't matter, just verify that it throws APIInternalException.
            await Assert.ThrowsAsync<APIInternalException>(
                () => searchClient.Get("cgov", "en", "breast cancer", 10, 10, "all")
            );
        }

        /// <summary>
        /// Verify the query service throws the correct exception when
        /// elasticsearch returns an invalid result.
        /// </summary>
        [Fact]
        public async void Get_BadESReturn()
        {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<SiteWideSearchResults>>((req, res) =>
            {
                byte[] byteArray = Encoding.UTF8.GetBytes("\"This is not the server you were looking for.\"");
                res.Stream = new MemoryStream(byteArray);
                res.StatusCode = 200;
            });
            // The URL doesn't matter, it won't be used.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var connectionSettings = new ConnectionSettings(pool, conn, sourceSerializer: JsonNetSerializer.Default);
            IElasticClient client = new ElasticClient(connectionSettings);

            ESSearchQueryService searchClient = new ESSearchQueryService(client, MockSearchOptions, new NullLogger<ESSearchQueryService>());

            // The parameters and result doesn't matter, just verify that it throws APIInternalException.
            await Assert.ThrowsAsync<APIInternalException>(
                () => searchClient.Get("cgov", "en", "breast cancer", 10, 10, "all")
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

            Uri actualURI = null;
            string actualMimeType = String.Empty;
            HttpMethod actualMethod = HttpMethod.DELETE; // Something other than the expected value (default is GET).

            JToken requestBody = null;

            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<SiteWideSearchResult>>((req, res) =>
            {
                requestBody = conn.GetRequestPost(req);

                actualURI = req.Uri;
                actualMimeType = req.RequestMimeType;
                actualMethod = req.Method;

                res.StatusCode = 200;
                res.Stream = ElastcsearchTestingTools.MockEmptyResponse;
            });

            // The URL doesn't matter, it won't be used.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var connectionSettings = new ConnectionSettings(pool, conn, sourceSerializer: JsonNetSerializer.Default);
            IElasticClient client = new ElasticClient(connectionSettings);

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
                "all"   // site parameter
            );

            Assert.Equal(expectedMimeType, actualMimeType);
            Assert.Equal(expectedUrl, actualURI.AbsolutePath);
            Assert.Equal(expectedMethod, actualMethod);

            // The structure of the actual query is tested in the SitewideSearchQueryBuilder_Test class.
            Assert.Equal(requestedSize, ((int)requestBody["size"]));
            Assert.Equal(requestedFrom, (int)requestBody["from"]);
        }

    }
}
