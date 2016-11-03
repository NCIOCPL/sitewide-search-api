using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging.Testing;

using Elasticsearch.Net;
using Nest;

using Newtonsoft.Json.Linq;

using Moq;
using Xunit;

using NCI.OCPL.Utils.Testing;

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

using NCI.OCPL.Services.SiteWideSearch.Controllers;

namespace NCI.OCPL.Services.SiteWideSearch.Tests.SearchControllerTests
{
    /// <summary>
    /// Defines a class with all of the query tests for the get method to ensure that the 
    /// parameters passed into the SearchController are translated correctly into ES 
    /// requests.
    /// </summary>
    public class Get_QueryTests {

        /// <summary>
        /// Helper method to build a SearchTemplateRequest in a more compact manner
        /// </summary>
        /// <param name="index">The index to fetch from</param>
        /// <param name="file">The template file to use</param>
        /// <param name="term">The search term we are looking for</param>
        /// <param name="size">The result set size</param>
        /// <param name="from">Where to start the results from</param>
        /// <param name="fields">The fields we are requesting</param>
        /// <param name="site">The sites to filter the results by</param>
        /// <returns>A SearchTemplateRequest</returns>
        private SearchTemplateRequest<SiteWideSearchResult> GetSearchRequest(
            string index,
            string file,
            string term,
            int size,
            int from,
            string fields,
            string site
        ) {

            SearchTemplateRequest<SiteWideSearchResult> expReq = new SearchTemplateRequest<SiteWideSearchResult>(index){
                File = file
            };

            expReq.Params = new Dictionary<string, object>();
            expReq.Params.Add("my_value", term);
            expReq.Params.Add("my_size", size);
            expReq.Params.Add("my_from", from);
            expReq.Params.Add("my_fields", fields);
            expReq.Params.Add("my_site", site);

            return expReq;
        }

        [Fact]
        /// <summary>
        /// Test for Get with a single term.
        /// </summary>
        public void Using_DefaultParams()
        {
            string term = "Breast Cancer";

            ISearchTemplateRequest actualReq = null;

            //Setup the client with the request handler callback to be executed later.
            IElasticClient client = 
                ElasticTools.GetMockedSearchTemplateClient<SiteWideSearchResult>(
                    req => actualReq = req,
                    resMock => {
                        //Make sure we say that the response is valid.
                        resMock.Setup(res => res.IsValid).Returns(true);
                    } // We don't care what the response looks like.
                );

            SearchController controller = new SearchController(
                client,
                NullLogger<SearchController>.Instance
            );

            //NOTE: this is when actualReq will get set.
            controller.Get(
                "cgov_en",
                term
            ); 

            SearchTemplateRequest<SiteWideSearchResult> expReq = GetSearchRequest(
                "cgov",
                "cgov_cgov_en",
                term,
                10,
                0,
                "\"id\", \"url\", \"metatag-description\", \"metatag-dcterms-type\"",
                "all"
            );

            Assert.Equal(
                expReq, 
                actualReq,
                new ElasticTools.SearchTemplateRequestComparer()
            );
        }
    }

    /// <summary>
    /// Defines a class with all of the data mapping tests to ensure we are able to correctly 
    /// map the responses from ES into the correct response from the SearchController
    /// </summary>
    public class Get_DataMapTests {

        [Fact]
        /// <summary>
        /// Test for Breast Cancer term and ensures TotalResults is mapped correctly.
        /// </summary>
        public void Has_Correct_Total()
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.Equal(13858, results.TotalResults);
        }

        [Fact]
        /// <summary>
        /// Test that search mapping returns correct number of results for an empty result set.
        /// (And also that it doesn't explode!)
        /// </summary>
        public void No_Results_Has_Correct_Total()
        {
            string testFile = "Search.CGov.En.NoResults.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.Empty(results.Results);
        }

        [Fact]
        /// <summary>
        /// Test that the search results at arbitrary offsets
        /// in the collection are present
        /// </summary>
        public void Check_Results_Present()
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.All(results.Results, item => Assert.NotNull(item));
        }

        [Fact]
        /// <summary>
        /// Test that the search results at arbitrary offsets
        /// in the collection are present
        /// </summary>
        public void Check_RequiredField_Present()
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.All(results.Results, item => Assert.NotNull(item.URL));
        }


    }


    /// <summary>
    /// Class to encapsulate all support code for testing optional fields  
    /// </summary>
    public class OptionalFieldTests
    {
        [Theory, MemberData(nameof(FieldData))]
        /// <summary>
        /// Test that the search result mapping returns null when an optional field is not present.
        /// Inputs for each call are obtained by iterating over the FieldData property.
        /// </summary>
        /// <param name="offset">Offset into testFile's set of search results.</param>
        /// <param name="nullTest">A test function of tupe Func&lt;SiteWideSearchResult, Boolean&gt; which checks
        /// wheter a specific field in the selected result is null.</param>
        /// <param name="fieldName">Name of the field being tested, used for display purposes.</param>
        public void Optional_Field_Is_Null(int offset, Object nullTest, string fieldName)
        {
            string testFile = "Search.CGov.En.AbsentFields.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
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
                    new  object[]{1, (Func<SiteWideSearchResult, Boolean>)(x => x.Description == null ), "metatag-description" },
                    new  object[]{2, (Func<SiteWideSearchResult, Boolean>)(x => x.ContentType == null ), "metatag-dcterms-type" }
                };

    }
    

    /// <summary>
    /// Defines tests of SearchController error behavior.
    /// <remarks>
    /// </remarks>
    /// </summary>
    public class ErrorTests
    {

        [Theory]
        [InlineData(403)] // Forbidden
        [InlineData(404)] // Not Found
        [InlineData(500)] // Server error
        /// <summary>
        /// Verify that controller throws the correct exception when the
        /// ES client reports an error.
        /// </summary>
        /// <param name="offset">Offset into the list of results of the item to check.</param>
        /// <param name="expectedTerm">The expected term text</param>
        public void Handle_Failed_Query(int errorCode)
        {
            SearchController ctrl = new SearchController(
                ElasticTools.GetErrorElasticClient(errorCode),
                NullLogger<SearchController>.Instance
            );

            Exception ex = Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get (
                        "cgov_en",
                        "breast cancer"
                    )
                );

            // Failed search request should always report 500. 
            Assert.Equal(500, ((APIErrorException)ex).HttpStatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("        ")] // Spaces
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        /// <summary>
        /// Verify that controller throws the correct exception when no collection is specified.
        /// </summary>
        /// <param name="collectionValue">A string specifying the collection to search.</param>
        public void Get_EmptyCollection_ReturnsError(String collectionValue)
        {
            // The file needs to exist so it can be deserialized, but we don't make
            // use of the actual content. 
            string testFile = "Search.CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            Exception ex = Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get (
                        collectionValue,
                        "some term"
                    )
                );

            // Search without a collection should report bad request (400) 
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")] // Empty string
        [InlineData("        ")] // Spaces
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        /// <summary>
        /// Verify that controller throws the correct exception when no search text is specified.
        /// </summary>
        /// <param name="termValue">A string the text to search for.</param>
        public void Get_EmptyTerm_ReturnsError(String termValue)
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            Exception ex = Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get (
                        "some collection",
                        termValue
                    )
                );

            // Search without something to search for should report bad request (400) 
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }

    }
}
