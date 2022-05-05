using System;
using System.Text;
using System.IO;

using Microsoft.Extensions.Logging.Testing;

using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json.Linq;
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
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<Suggestion>>((req, res) =>
            {
                res.StatusCode = statusCode;
            });
            // The URL doesn't matter, it won't be used.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var connectionSettings = new ConnectionSettings(pool, conn, sourceSerializer: JsonNetSerializer.Default);
            IElasticClient client = new ElasticClient(connectionSettings);

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
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<Suggestion>>((req, res) =>
            {
                byte[] byteArray = Encoding.UTF8.GetBytes("\"This is not the server you were looking for.\"");
                res.Stream = new MemoryStream(byteArray);
                res.StatusCode = 200;
            });
            // The URL doesn't matter, it won't be used.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var connectionSettings = new ConnectionSettings(pool, conn, sourceSerializer: JsonNetSerializer.Default);
            IElasticClient client = new ElasticClient(connectionSettings);

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
            JObject expectedBody = JObject.Parse(@"
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

            JToken requestBody = null;

            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<Suggestion>>((req, res) =>
            {
                // We don't really care about the response for this test.
                res.Stream = ElastcsearchTestingTools.MockEmptyResponse;
                res.StatusCode = 200;

                esURI = req.Uri;
                esContentType = req.RequestMimeType;
                esMethod = req.Method;
                requestBody = conn.GetRequestPost(req);
            });
            // The URI does not matter, an InMemoryConnection never requests from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn, sourceSerializer: JsonNetSerializer.Default);
            IElasticClient client = new ElasticClient(connectionSettings);


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
            Assert.Equal(expectedBody, requestBody, new JTokenEqualityComparer());
        }

    }
}