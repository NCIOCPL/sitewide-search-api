using System;
using System.Text;
using System.IO;

using Microsoft.Extensions.Logging.Testing;

using Elasticsearch.Net;
using Nest;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.Common.Testing;
using System.Collections.Generic;

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
            var connectionSettings = new ConnectionSettings(pool, conn);
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
            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            ESSearchQueryService searchClient = new ESSearchQueryService(client, MockSearchOptions, new NullLogger<ESSearchQueryService>());

            // The parameters and result doesn't matter, just verify that it throws APIInternalException.
            await Assert.ThrowsAsync<APIInternalException>(
                () => searchClient.Get("cgov", "en", "breast cancer", 10, 10, "all")
            );
        }

        /// <summary>
        /// Helper method to build a SearchTemplateRequest in a more compact manner
        /// </summary>
        /// <param name="index">The index to fetch from</param>
        /// <param name="fileName">The template fileName to use</param>
        /// <param name="term">The search term we are looking for</param>
        /// <param name="size">The result set size</param>
        /// <param name="from">Where to start the results from</param>
        /// <param name="fields">The fields we are requesting</param>
        /// <param name="site">The sites to filter the results by</param>
        /// <returns>A SearchTemplateRequest</returns>
        private SearchTemplateRequest<SiteWideSearchResult> GetSearchRequest(
            string index,
            string fileName,
            string term,
            int size,
            int from,
            string fields,
            string site
        ) {

            // ISearchTemplateRequest.File is obsolete.
            // Refactoring to remove this dependency is recorded as issue #28
            // https://github.com/NCIOCPL/sitewide-search-api/issues/28
#pragma warning disable CS0618
            SearchTemplateRequest<SiteWideSearchResult> expReq = new SearchTemplateRequest<SiteWideSearchResult>(index){
                File = fileName
            };
#pragma warning restore CS0618

            expReq.Params = new Dictionary<string, object>();
            expReq.Params.Add("my_value", term);
            expReq.Params.Add("my_size", size);
            expReq.Params.Add("my_from", from);
            expReq.Params.Add("my_fields", fields);
            expReq.Params.Add("my_site", site);

            return expReq;
        }

        // TODO: Add tests for varying the various parameters.

        [Fact]
        /// <summary>
        /// Verify that the request sent to ES for a single term is being set up correctly.
        /// </summary>
        public async void Check_For_Correct_Request_Data()
        {
            string term = "Breast Cancer";

            ISearchTemplateRequest actualReq = null;

            //Setup the client with the request handler callback to be executed later.
            IElasticClient client =
                NCI.OCPL.Utils.Testing.ElasticTools.GetMockedSearchTemplateClient<SiteWideSearchResult>(
                    req => actualReq = req,
                    resMock => {
                        //Make sure we say that the response is valid.
                        resMock.Setup(res => res.IsValid).Returns(true);
                    } // We don't care what the response looks like.
                );

            ISearchQueryService searchClient = new ESSearchQueryService(
                client,
                MockSearchOptions,
                NullLogger<ESSearchQueryService>.Instance
            );


            //NOTE: this is when actualReq will get set.
            await searchClient.Get(
                "cgov", // Search collection to use
                "en",   // language
                term,   // term
                0,      // from
                10,     // size
                "all"   // site parameter
            );

            SearchTemplateRequest<SiteWideSearchResult> expReq = GetSearchRequest(
                "cgov",                 // Search index to look in.
                "cgov_search_cgov_en",  // Template name, preceded by the name of the directory it's stored in.
                term,                   // Search term
                10,                     // Max number of records to retrieve.
                0,                      // Offset of first record to retrieve.
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
