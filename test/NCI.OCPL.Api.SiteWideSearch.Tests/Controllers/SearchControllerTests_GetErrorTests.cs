using System;

using Microsoft.Extensions.Logging.Testing;

using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.SiteWideSearch.Controllers;

namespace NCI.OCPL.Api.SiteWideSearch.Tests.SearchControllerTests
{

    /// <summary>
    /// Tests for the <see cref="M:NCI.OCPL.Api.SiteWideSearch.Controllers.SearchController.Get" />
    /// error behavior.
    /// </summary>
    public class SearchControllerTests_GetErrorTests
    {
        /// <summary>
        /// Verify that controller throws the correct exception when the
        /// ES client reports an error.
        /// </summary>
        /// <param name="offset">Offset into the list of results of the item to check.</param>
        /// <param name="expectedTerm">The expected term text</param>
        [Fact]
        public async void Handle_Failed_Query()
        {
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())
            )
            .Throws(new APIInternalException("Internal error"));

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            // Parameters don't matter, and for this test we don't care about saving the results.
            // Just verify we're responding with APIErrorException and status 500.
            Exception ex =  await Assert.ThrowsAsync<APIErrorException>(
                () =>
                    ctrl.Get (
                        "cgov",
                        "en",
                        "breast cancer"
                    )
                );

            // Failed search request should always report 500.
            Assert.Equal(500, ((APIErrorException)ex).HttpStatusCode);
        }

        /// <summary>
        /// Verify that controller throws the correct exception when no collection is specified.
        /// </summary>
        /// <param name="collectionValue">A string specifying the collection to search.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("        ")] // Spaces
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        public async void Get_EmptyCollection_ReturnsError(String collectionValue)
        {
            // No setup, because we don't expect anything to actually be invoked.
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            Exception ex = await Assert.ThrowsAsync<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get (
                        collectionValue,
                        "en",
                        "some term"
                    )
                );

            // Search without a collection should report bad request (400)
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }

        /// <summary>
        /// Verify the controller throws the correct exception when an unsupported
        /// or blank language parameter is specified.
        /// </summary>
        /// <param name="collectionValue">A string specifying the collection to search.</param>
        [Theory]
        [InlineData("EN")] // We're case-sensitive
        [InlineData("Es")]
        [InlineData("es-us")]
        [InlineData("pt")]
        [InlineData("zh")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("        ")] // Spaces
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        public async void Get_UnsupportedLanguage_ReturnsError(string language)
        {
            // No setup, because we don't expect anything to actually be invoked.
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            Exception ex = await Assert.ThrowsAsync<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get(
                        "cgov",
                        language,
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
        /// Verify that controller returns no results when no search text is specified.
        /// </summary>
        /// <param name="termValue">A string the text to search for.</param>
        public async void Get_EmptyTerm_ReturnsNull(String termValue)
        {
            // No setup, because we don't expect anything to actually be invoked.
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            SiteWideSearchResults expectedRes = new SiteWideSearchResults(0, new SiteWideSearchResult[0]);

            SiteWideSearchResults actualRes = await ctrl.Get (
                        "some collection",
                        "en",
                        termValue
                    );

            // Search without something to search for should report bad request (400)
            Assert.Equal(expectedRes.TotalResults, actualRes.TotalResults);
        }

    }
}