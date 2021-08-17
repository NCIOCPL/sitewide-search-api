using Microsoft.Extensions.Logging.Testing;

using Moq;
using Xunit;

using NCI.OCPL.Api.SiteWideSearch.Controllers;

namespace NCI.OCPL.Api.SiteWideSearch.Tests.AutoSuggestControllerTests
{
    /// <summary>
    /// Defines Tests for the AutosuggestController class
    /// <remarks>
    /// The AutosuggestController class requires an IElasticClient, which is how
    /// the controller queries an ElasticSearch server.  As these are unit tests, we
    /// will not be connecting to a ES server.  So we are using the Moq framework for
    /// mocking up the methods in an IElasticClient.
    ///
    ///
    /// The primary method we use is the SearchTemplate method.  This calls an ElasticSearch
    /// template (which is like a stored procedure).  Most of the tests will be for validating
    /// the parameters passed into the SearchTemplate method.  In order for the Nest library to
    /// provide a fluent interface in defining queries and parameters for templates, most methods
    /// will take in an anonymous function for defining the parameters.  These functions usually
    /// return an object that defines the request the client should send to the server.
    ///
    /// I note all of this since the class names are quite long and the code may start to get
    /// funky looking.
    /// </remarks>
    /// </summary>


    /// <summary>
    /// Defines a class with all of the data mapping tests to ensure we are able to correctly
    /// map the responses from ES into the correct response from the AutosuggestController
    /// </summary>
    public class Get_DataMapTests
    {
        /// <summary>
        /// Simulated Autosuggest query service response to the keyword "breast".
        /// </summary>
        private Suggestions BreastCancerAutoSuggest_English_Return =>
            new Suggestions(
                222,
                new Suggestion[]
                {
                    new Suggestion(){Term = "breast cancer"},
                    new Suggestion(){Term = "breast"},
                    new Suggestion(){Term = "inflammatory breast cancer"},
                    new Suggestion(){Term = "metastatic breast cancer"},
                    new Suggestion(){Term = "male breast cancer"},
                    new Suggestion(){Term = "types of breast cancer"},
                    new Suggestion(){Term = "breast cancer risk"},
                    new Suggestion(){Term = "understanding breast changes"},
                    new Suggestion(){Term = "breast cancer statistics"},
                    new Suggestion(){Term = "breast cancer screening"},
                    new Suggestion(){Term = "breast cancer staging"},
                    new Suggestion(){Term = "breast changes"},
                    new Suggestion(){Term = "breast reconstruction"},
                    new Suggestion(){Term = "breast cancer prevention"},
                    new Suggestion(){Term = "breast cancer treatment"},
                    new Suggestion(){Term = "Breast Cancer Risk Assessment Tool"},
                    new Suggestion(){Term = "stages of breast cancer"},
                    new Suggestion(){Term = "breast cancer risk assessment"},
                    new Suggestion(){Term = "breast cancer risk tool"},
                    new Suggestion(){Term = "breast cancer symptoms"}
                });

        [Fact]
        /// <summary>
        /// Test that the list of results exists.
        /// </summary>
        public async void Check_Results_Exist()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())
            )
            .ReturnsAsync(BreastCancerAutoSuggest_English_Return);

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            //Parameters don't matter in this case...
            Suggestions results = await ctrl.Get(
                "cgov",
                "en",
                "breast cancer"
            );

            Assert.NotEmpty(results.Results);
        }

        [Fact]
        /// <summary>
        /// Test that the search results at arbitrary offsets
        /// in the collection are present
        /// </summary>
        public async void Check_Results_Present()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())
            )
            .ReturnsAsync(BreastCancerAutoSuggest_English_Return);

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            //Parameters don't matter in this case...
            Suggestions results = await ctrl.Get(
                "cgov",
                "en",
                "breast cancer"
            );

            Assert.All(results.Results, item => Assert.NotNull(item));
        }

        [Fact]
        /// <summary>
        /// Test that the list of returned results has the right number of items.
        /// </summary>
        public async void Check_Result_Count()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())
            )
            .ReturnsAsync(BreastCancerAutoSuggest_English_Return);

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            //Parameters don't matter in this case...
            Suggestions results = await ctrl.Get(
                "cgov",
                "en",
                "breast cancer"
            );

            Assert.Equal(20, results.Results.Length);
        }


        [Fact]
        /// <summary>
        /// Test that the first result contains the expected string.
        /// </summary>
        public async void Check_First_Result()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())
            )
            .ReturnsAsync(BreastCancerAutoSuggest_English_Return);

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            //Parameters don't matter in this case...
            Suggestions results = await ctrl.Get(
                "cgov",
                "en",
                "breast cancer"
            );

            Assert.Equal("breast cancer", results.Results[0].Term);
        }

        [Theory]
        [InlineData(0, "breast cancer")]
        [InlineData(3, "metastatic breast cancer")]
        [InlineData(17, "breast cancer risk assessment")]
        [InlineData(19, "breast cancer symptoms")]
        /// <summary>
        /// Test that the suggested search strings from arbitrary offsets
        /// in the collection have the correct values
        /// </summary>
        /// <param name="offset">Offset into the list of results of the item to check.</param>
        /// <param name="expectedTerm">The expected term text</param>
        public async void Check_Arbitrary_Result(int offset, string expectedTerm)
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())
            )
            .ReturnsAsync(BreastCancerAutoSuggest_English_Return);

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            //Parameters don't matter in this case...
            Suggestions results = await ctrl.Get(
                "cgov",
                "en",
                "breast cancer"
            );

            Assert.Equal(expectedTerm, results.Results[offset].Term);
        }

        [Fact]
        /// <summary>
        /// Test for Breast Cancer search string and ensures Total is mapped correctly.
        /// </summary>
        public async void Has_Correct_Total()
        {
            Mock<IAutosuggestQueryService> querySvc = new Mock<IAutosuggestQueryService>();
            querySvc.Setup(
                svc => svc.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())
            )
            .ReturnsAsync(BreastCancerAutoSuggest_English_Return);

            AutosuggestController ctrl = new AutosuggestController(
                NullLogger<AutosuggestController>.Instance,
                querySvc.Object
            );

            //Parameters don't matter in this case...
            Suggestions results = await ctrl.Get(
                "cgov",
                "en",
                "breast cancer"
            );

            Assert.Equal(222, results.Total);
        }

    }

}
