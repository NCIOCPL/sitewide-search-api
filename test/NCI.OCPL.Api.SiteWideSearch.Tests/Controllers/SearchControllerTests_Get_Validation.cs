using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging.Testing;

using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.SiteWideSearch.Controllers;

namespace NCI.OCPL.Api.SiteWideSearch.Tests.SearchControllerTests
{

    /// <summary>
    /// Tests for the <see cref="M:NCI.OCPL.Api.SiteWideSearch.Controllers.SearchController.Get" />
    /// input validation.
    /// </summary>
    public class SearchControllerTests_Get_Validation
    {
        /// <summary>
        /// Verify that controller changes negative "from" and "size" values to non-negative.
        /// </summary>
        [Fact]
        public async void Handle_Negative_Inputs()
        {
            const int inputFrom = -10;
            const int inputSize = -70;

            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>())
            )
            // We don't care about the actual object being returned, we just need one.
            .ReturnsAsync(new SiteWideSearchResults(0, new SiteWideSearchResult[0]));

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            await ctrl.Get("cgov", "en", "breast cancer", inputFrom, inputSize);

            querySvc.Verify(
                svc => svc.Get("cgov", "en", "breast cancer", SearchController.DEFAULT_FROM_LOCATION, SearchController.DEFAULT_QUERY_SIZE, new string[] {SearchController.DEFAULT_SITE}),
                Times.Once
            );
        }

        /// <summary>
        /// Verify the controller leaves valid "from" and "size" values unchanged.
        /// </summary>
        [Theory]
        [InlineData(0, 10)]
        [InlineData(200, 50)]
        public async void Does_Not_Alter_Valid_Inputs(int inputFrom, int inputSize)
        {
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>())
            )
            // We don't care about the actual object being returned, we just need one.
            .ReturnsAsync(new SiteWideSearchResults(0, new SiteWideSearchResult[0]));

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            await ctrl.Get("cgov", "en", "breast cancer", inputFrom, inputSize);

            querySvc.Verify(
                svc => svc.Get("cgov", "en", "breast cancer", inputFrom, inputSize, new string[] {SearchController.DEFAULT_SITE}),
                Times.Once
            );
        }

        public static IEnumerable<object[]> EmptySiteList => new[]
        {
            new object[]{null,                      new string[] { SearchController.DEFAULT_SITE } },
            new object[]{new string[0],             new string[] { SearchController.DEFAULT_SITE } },
            new object[]{new string[] {"", null},   new string[] { SearchController.DEFAULT_SITE } },
            new object[]{new string[] {"all"},      new string[] { SearchController.DEFAULT_SITE } },

            new object[]{new string[] {"physics.cancer.gov"},   new string[] { "physics.cancer.gov" } },
            new object[]{new string[] {"dceg.cancer.gov", "www.cancer.gov/connect-prevention-study" },   new string[] { "dceg.cancer.gov", "www.cancer.gov/connect-prevention-study" } },
        };

        /// <summary>
        /// Verify the controller works correctly with varying site filters.
        /// </summary>
        [Theory, MemberData(nameof(EmptySiteList))]
        public async void Default_For_Empty_Site_list(string[] inputSiteList, string[] expectedSiteList)
        {
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>())
            )
            // We don't care about the actual object being returned, we just need one.
            .ReturnsAsync(new SiteWideSearchResults(0, new SiteWideSearchResult[0]));

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            // The controller should pass the site list regardless of collection value.
            await ctrl.Get("cgov", "en", "breast cancer", 10, 20, inputSiteList);

            querySvc.Verify(
                svc => svc.Get("cgov", "en", "breast cancer", 10, 20, expectedSiteList),
                Times.Once
            );
        }
    }
}