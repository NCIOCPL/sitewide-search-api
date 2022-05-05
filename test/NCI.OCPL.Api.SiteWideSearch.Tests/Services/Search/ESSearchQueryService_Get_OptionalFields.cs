using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Elasticsearch.Net;
using Nest;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.Common.Testing;

/*
 The SearchController class requires an IElasticClient, which is how
 the controller queries an ElasticSearch server.  As these are unit tests, we
 will not be connecting to a ES server.  So we are using the Moq framework for
 mocking up the methods in an IElasticClient.


 The primary method we use is the SearchTemplate method.  This calls an ElasticSearch
 template (which is like a stored procedure).  Most of the tests will be for validating
 the parameters passed into the SearchTemplate method.  In order for the Nest library to
 provide a fluent interface in defining queries and parameters for templates, most methods
 will take in an anonymous function for defining the parameters.  These functions usually
 return an object that defines the request the client should send to the server.

 I note all of this since the class names are quite long and the code may start to get
 funky looking.
*/

using NCI.OCPL.Api.SiteWideSearch.Controllers;

namespace NCI.OCPL.Api.SiteWideSearch.Services.Tests
{

    /// <summary>
    /// Class to encapsulate all support code for testing optional fields
    /// </summary>
    public class OptionalFieldTests : ESSearchQueryService_Base
    {
        [Theory, MemberData(nameof(FieldData))]
        /// <summary>
        /// Test that the search result mapping returns null when an optional field is missing from
        /// the results returned by Elasticsearch.
        /// Inputs for each call are obtained by iterating over the FieldData property.
        /// </summary>
        /// <param name="offset">Offset into testFile's set of search results where we know
        /// <see cref="fieldName" /> to be missing..</param>
        /// <param name="nullTest">A test function of tupe Func&lt;SiteWideSearchResult, Boolean&gt; which checks
        /// wheter a specific field in the selected result is null.</param>
        /// <param name="fieldName">Name of the field being tested, used for display purposes.</param>
        public async void Optional_Field_Is_Null(int offset, Object nullTest, string fieldName)
        {
            string testFile = "Search.CGov.En.AbsentFields.json";

            IOptions<SearchIndexOptions> config = MockSearchOptions;
            IElasticClient client = ElasticTools.GetInMemoryElasticClient(testFile);

            ESSearchQueryService searchClient =
                new ESSearchQueryService(client, config, NullLogger<ESSearchQueryService>.Instance);

            //Parameters don't matter in this case...
            SiteWideSearchResults results = await searchClient.Get(
                "cgov",
                "en",
                "breast cancer",
                20,
                10,
                "all"
            );

            SiteWideSearchResult item = results.Results[offset];
            Assert.True(((Func<SiteWideSearchResult, Boolean>)nullTest)(item), fieldName);

        }

        /// <summary>
        /// Provides an IEnumerable containing the inputs for successive calls to the Optional_Field_Is_Null test.
        /// Each entry contains three items, mapping to the corresponding method parameters.
        /// </summary>
        public static IEnumerable<object[]> FieldData => new[]
                {
                    new  object[]{0, (Func<SiteWideSearchResult, Boolean>)(x => x.Title == null ), "title" },
                    new  object[]{1, (Func<SiteWideSearchResult, Boolean>)(x => x.Description == null ), "metatag.description" },
                    new  object[]{2, (Func<SiteWideSearchResult, Boolean>)(x => x.ContentType == null ), "metatag.dcterms.type" }
                };

    }

}
