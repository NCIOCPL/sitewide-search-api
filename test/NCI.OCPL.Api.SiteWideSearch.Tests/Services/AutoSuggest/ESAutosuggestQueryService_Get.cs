using System;

using Elasticsearch.Net;
using Nest;
using Xunit;

using NCI.OCPL.Api.Common.Testing;
using Microsoft.Extensions.Logging.Testing;
using NCI.OCPL.Api.Common;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

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
            var connectionSettings = new ConnectionSettings(pool, conn);
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
            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            ESAutosuggestQueryService autosuggestClient = new ESAutosuggestQueryService(client, MockAutoSuggestOptions, new NullLogger<ESAutosuggestQueryService>());

            // The parameters and result doesn't matter, just verify that it throws APIInternalException.
            await Assert.ThrowsAsync<APIInternalException>(
                () => autosuggestClient.Get("cgov", "en", "breast cancer", 10)
            );
        }

        // TODO: Add tests for varying the various parameters.
        // TODO: Rewrite Check_For_Correct_Request_Data() and variants
        //       to not do templates.

        /// <summary>
        /// Helper method to build a SearchTemplateRequest for testing purposes.
        /// </summary>
        /// <param name="index">The index to fetch from</param>
        /// <param name="fileName">The template fileName to use</param>
        /// <param name="term">The search term we are looking for</param>
        /// <param name="size">The result set size</param>
        /// <param name="fields">The fields we are requesting</param>
        /// <param name="site">The sites to filter the results by</param>
        /// <returns>A SearchTemplateRequest</returns>
        private SearchTemplateRequest<T> GetSearchRequest<T>(
            string index,
            string fileName,
            string term,
            int size,
            string fields,
            string site
            ) where T : class {

            // ISearchTemplateRequest.File is obsolete.
            // Refactoring to remove this dependency is recorded as issue #28
            // https://github.com/NCIOCPL/sitewide-search-api/issues/28
#pragma warning disable CS0618
            SearchTemplateRequest<T> expReq = new SearchTemplateRequest<T>(index){
                File = fileName
            };
#pragma warning restore CS0618

            expReq.Params = new Dictionary<string, object>();
            expReq.Params.Add("searchstring", term);
            expReq.Params.Add("my_size", size);

            return expReq;
        }

        /// <summary>
        /// Verify that the request sent to ES for a single term is being set up correctly.
        /// </summary>
        [Fact]
        public async void Check_For_Correct_Request_Data()
        {
            string term = "Breast Cancer";

            ISearchTemplateRequest actualReq = null;

            //Setup the client with the request handler callback to be executed later.
            IElasticClient client =
                Utils.Testing.ElasticTools.GetMockedSearchTemplateClient<Suggestion>(
                    req => actualReq = req,
                    resMock => {
                        //Make sure we say that the response is valid.
                        resMock.Setup(res => res.IsValid).Returns(true);
                    } // We don't care what the response looks like.
                );

            IAutosuggestQueryService autoSuggestClient = new ESAutosuggestQueryService(
                client,
                MockAutoSuggestOptions,
                NullLogger<ESAutosuggestQueryService>.Instance
            );


            //NOTE: this is when actualReq will get set.
            await autoSuggestClient.Get(
                "cgov",
                "en",
                term,
                25
            );

            SearchTemplateRequest<Suggestion> expReq = GetSearchRequest<Suggestion>(
                "cgov",                 // Search index to look in.
                "autosg_suggest_cgov_en",  // Template name, preceded by the name of the directory it's stored in.
                term,                   // Search term
                10,                     // Max number of records to retrieve.
                "\"url\", \"title\", \"metatag.description\", \"metatag.dcterms.type\"",
                "all"
            );

            Assert.Equal(
                expReq,
                actualReq,
                new Utils.Testing.ElasticTools.SearchTemplateRequestComparer()
            );
        }

    }
}