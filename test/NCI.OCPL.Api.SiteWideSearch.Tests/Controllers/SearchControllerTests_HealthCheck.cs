using System;

using Microsoft.Extensions.Logging.Testing;

using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.SiteWideSearch.Controllers;

namespace NCI.OCPL.Api.SiteWideSearch.Tests.SearchControllerTests
{
    /// <summary>
    /// Tests for the Search Controller's healthcheck endpoint
    /// </summary>
    public class SearchControllerTests_HealthCheck
    {
        /// <summary>
        /// Verify the GetStatus method responds correctly when the service outright fails.
        /// </summary>
        [Fact]
        public async void GetStatus_ServiceFail()
        {
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();
            querySvc.Setup(
                svc => svc.GetIsHealthy()
            )
            .ThrowsAsync(new Exception("Any exception at all."));

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            Exception ex = await Assert.ThrowsAsync<APIErrorException>(
                () => ctrl.GetStatus()
            );

            Assert.Equal(500, ((APIErrorException)ex).HttpStatusCode);
        }

        /// <summary>
        /// Verify the GetStatus method responds correctly when the service reports the service is healthy.
        /// </summary>
        [Fact]
        public async void GetStatus_Healthy()
        {
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();
            querySvc.Setup(
                svc => svc.GetIsHealthy()
            )
            .ReturnsAsync(true);

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            string status = await ctrl.GetStatus();
            Assert.Equal(SearchController.HEALTHY_STATUS, status);
        }

        /// <summary>
        /// Verify the GetStatus method responds correctly when the service reports as not healthy.
        /// </summary>
        [Fact]
        public async void GetStatus_Unhealthy()
        {
            Mock<ISearchQueryService> querySvc = new Mock<ISearchQueryService>();
            querySvc.Setup(
                svc => svc.GetIsHealthy()
            )
            .ReturnsAsync(false);

            SearchController ctrl = new SearchController(
                NullLogger<SearchController>.Instance,
                querySvc.Object
            );

            Exception ex = await Assert.ThrowsAsync<APIErrorException>(
                () => ctrl.GetStatus()
            );

            Assert.Equal(500, ((APIErrorException)ex).HttpStatusCode);
            Assert.Equal(SearchController.UNHEALTHY_STATUS, ex.Message);
        }

    }
}