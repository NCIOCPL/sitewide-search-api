using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Testing;

using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.SiteWideSearch.Controllers;

namespace NCI.OCPL.Api.SiteWideSearch.Tests.AutoSuggestControllerTests
{
    /// <summary>
    /// Tests for the Autosuggest Controller's healthcheck endpoint
    /// </summary>
    public class AutosuggestControllerTests_HealthCheck
    {
        /// <summary>
        /// Verify the GetStatus method responds correctly when the service outright fails.
        /// </summary>
        [Fact]
        public async Task GetStatus_ServiceFail()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.GetIsHealthy()
            )
            .ThrowsAsync(new Exception("Any exception at all."));

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            APIErrorException ex = await Assert.ThrowsAsync<APIErrorException>(
                () => ctrl.GetStatus()
            );

            Assert.Equal(500, ex.HttpStatusCode);
        }

        /// <summary>
        /// Verify the GetStatus method responds correctly when the service reports the service is healthy.
        /// </summary>
        [Fact]
        public async Task GetStatus_Healthy()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.GetIsHealthy()
            )
            .ReturnsAsync(true);

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            string status = await ctrl.GetStatus();
            Assert.Equal(AutosuggestController.HEALTHY_STATUS, status);
        }

        /// <summary>
        /// Verify the GetStatus method responds correctly when the service reports as not healthy.
        /// </summary>
        [Fact]
        public async Task GetStatus_Unhealthy()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.GetIsHealthy()
            )
            .ReturnsAsync(false);

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            APIErrorException ex = await Assert.ThrowsAsync<APIErrorException>(
                () => ctrl.GetStatus()
            );

            Assert.Equal(500, ex.HttpStatusCode);
            Assert.Equal(AutosuggestController.UNHEALTHY_STATUS, ex.Message);
        }

    }
}
