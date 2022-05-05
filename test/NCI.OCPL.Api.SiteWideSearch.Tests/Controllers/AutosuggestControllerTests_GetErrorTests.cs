using System;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.SiteWideSearch.Controllers;

namespace NCI.OCPL.Api.SiteWideSearch.Tests.AutoSuggestControllerTests
{
    /// <summary>
    /// Tests for the <see cref="M:NCI.OCPL.Api.SiteWideSearch.Controllers.AutosuggestController.Get" />
    /// error behavior.
    /// </summary>
    public class AutosuggestControllerTests_GetErrorTests
    {

        [Fact]
        /// <summary>
        /// Verify that controller throws the correct exception when the
        /// ES client encounters an error.
        /// </summary>
        /// <param name="offset">Offset into the list of results of the item to check.</param>
        /// <param name="expectedTerm">The expected term text</param>
        public async void Handle_Failed_Query()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())
            )
            .Throws(new APIInternalException("Internal error"));

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            // Parameters don't matter, and for this test we don't care about saving the results.
            // Just verify we're responding with APIErrorException and status 500.
            Exception ex = await Assert.ThrowsAsync<APIErrorException>(
                () =>
                    ctrl.Get(
                        "cgov",
                        "en",
                        "breast cancer"
                    )
                );
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
        public async void Get_EmptyCollection_ReturnsError(String collectionValue)
        {
            // No setup, because we don't expect anything to actually be invoked.
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            Exception ex = await Assert.ThrowsAsync<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get(
                        collectionValue,
                        "en",
                        "some term"
                    )
                );

            // Search without a collection should report bad request (400)
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }


        [Theory]
        [InlineData("english")] // Language that "sounds" right but isn't.
        [InlineData("spanish")]
        [InlineData(null)]
        [InlineData("")] // Empty string
        [InlineData("        ")] // Spaces
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        /// <summary>
        /// Verify that controller throws the correct exception when an invalid language   is specified.
        /// </summary>
        /// <param name="termValue">A string the text to search for.</param>
        public async void Get_InvalidLanguage_ReturnsError(string language)
        {
            // No setup, because we don't expect anything to actually be invoked.
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
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

            // Search without something to search for should report bad request (400)
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
        public async void Get_EmptyTerm_ReturnsError(String termValue)
        {
            // No setup, because we don't expect anything to actually be invoked.
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            Exception ex = await Assert.ThrowsAsync<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get(
                        "some collection",
                        "en",
                        termValue
                    )
                );

            // Search without something to search for should report bad request (400)
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }

    }
}
