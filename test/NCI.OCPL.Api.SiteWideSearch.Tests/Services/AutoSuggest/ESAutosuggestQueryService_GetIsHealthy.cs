using System;
using System.IO;
using System.Text;

using Microsoft.Extensions.Logging.Testing;

using Elasticsearch.Net;
using Nest;
using Xunit;

using NCI.OCPL.Api.Common.Testing;
using Newtonsoft.Json.Linq;

namespace NCI.OCPL.Api.SiteWideSearch.Services.Tests
{
    /// <summary>
    /// Tests for <see cref="M:NCI.OCPL.Api.SiteWideSearch.Services.ESAutosuggestQueryService.Get" />
    /// </summary>
    public partial class ESAutosuggestQueryServiceTest
    {
        /// <summary>
        /// Test GetIsHealthy behavior when elasticsearch returns an error code
        /// indicating that a search couldn't be performed.
        /// </summary>
        [Theory]
        [InlineData(500)]
        [InlineData(403)]
        public async void GetIsHealthy_ConnectionFailure(int statusCode)
        {
            IElasticClient client = ElasticTools.GetErrorElasticClient(statusCode);
            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, MockAutoSuggestOptions, new NullLogger<ESAutosuggestQueryService>());

            bool result = await autosuggestClient.GetIsHealthy();
            Assert.False(result);
        }

        /// <summary>
        /// Test GetIsHealthy behavior when the remote server returns an invalid/unexpected result.
        /// </summary>
        [Fact]
        public async void GetIsHealthy_BadESReturn()
        {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.ClusterHealthResponse>((req, res) =>
            {
                byte[] byteArray = Encoding.UTF8.GetBytes("\"This is not the server you were looking for.\"");
                res.Stream = new MemoryStream(byteArray);
                res.StatusCode = 200;
            });
            // The URL doesn't matter, it won't be used.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, MockAutoSuggestOptions, new NullLogger<ESAutosuggestQueryService>());

            bool result = await autosuggestClient.GetIsHealthy();
            Assert.False(result);
        }

        /// <summary>
        /// Verify healthcheck requests to ES have the expected structure.
        /// </summary>
        [Fact]
        public async void GetIsHealthy_RequestStructure()
        {
            string expectedMimeType = "application/json";
            string expectedUrl = "http://localhost:9200/_cluster/health/autosg";
            HttpMethod expectedMethod = HttpMethod.GET;

            Uri actualURI = null;
            string actualMimeType = String.Empty;
            HttpMethod actualMethod = HttpMethod.DELETE; // Something other than the expected value (default is GET).

            JToken actualRequestBody = null;

            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.ClusterHealthResponse>((req, res) =>
            {
                res.Stream = MockHealthCheckResponse;
                res.StatusCode = 200;

                actualURI = req.Uri;
                actualMimeType = req.ContentType; // req.RequestMimeType; -- Property name will change for ES7.
                actualMethod = req.Method;
                actualRequestBody = conn.GetRequestPost(req);
            });
            // The URL doesn't matter, it won't be used.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, MockAutoSuggestOptions, new NullLogger<ESAutosuggestQueryService>());

            // We don't care about the call's result, only the request.
            await autosuggestClient.GetIsHealthy();
            Assert.Equal(expectedMimeType, actualMimeType);
            Assert.Equal(expectedUrl, actualURI.AbsoluteUri);
            Assert.Equal(expectedMethod, actualMethod);
            Assert.Null(actualRequestBody);
        }

        /// <summary>
        /// Verify the service will return true when Elasticsearch reports itself healthy.
        /// </summary>
        /// <param name="datafile">Path to a JSON file containing a simulated Elasticsearch
        /// health check response. The path is relative to the test project's TestData
        /// directory.</param>
        [Theory]
        [InlineData("ESHealthData/green.json")]
        [InlineData("ESHealthData/yellow.json")]
        public async void GetStatus_Healthy(string datafile)
        {
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(datafile);
            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, MockAutoSuggestOptions, new NullLogger<ESAutosuggestQueryService>());

            bool status = await autosuggestClient.GetIsHealthy();
            Assert.True(status);
        }

        /// <summary>
        /// Verify the service will return false when Elasticsearch reports itself unhealthy.
        /// </summary>
        /// <param name="datafile">Path to a JSON file containing a simulated Elasticsearch response.
        /// The path is relative to the test project's TestData directory.</param>
        [Theory]
        [InlineData("ESHealthData/red.json")]
        //[InlineData("ESHealthData/unexpected.json")]   // i.e. "Unexpected color" - it appears as if 5.6.x does not have unexpected
        public async void GetStatus_Unhealthy(string datafile)
        {
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(datafile);
            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, MockAutoSuggestOptions, new NullLogger<ESAutosuggestQueryService>());

            bool status = await autosuggestClient.GetIsHealthy();
            Assert.False(status);
        }



    }
}