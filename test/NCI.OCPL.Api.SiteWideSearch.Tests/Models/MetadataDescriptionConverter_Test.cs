using System;
using System.IO;

using Newtonsoft.Json;
using Xunit;


namespace NCI.OCPL.Api.SiteWideSearch.Models.Tests
{
    /// <summary>
    /// Tests for the various formats Elasticsearch may use when presenting the
    /// metatag.description property for deserialization.
    /// </summary>
    public class MetadataDescriptionConverterTest
    {
        /// <summary>
        /// Verify deserialization of simple string descriptions.
        /// </summary>
        [Fact]
        public void SingleStringValue()
        {
            const string expectedDescription = "simple description";
            const string expectedUrl = "http://nowhere";
            const string expectedTitle = "test title";
            const string expectedContentType = "test content type";

            // NOTE: This is straight-up Json.Net deserialization without NEST mapping fields to properties.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhere"",
                    ""description"":    ""simple description"",
                    ""contenttype"":    ""test content type""
                }";

            var serializer = new JsonSerializer();
            var reader = new JsonTextReader(new StringReader(data));

            SiteWideSearchResult actual = serializer.Deserialize<SiteWideSearchResult>(reader);

            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedUrl, actual.URL);
            Assert.Equal(expectedTitle, actual.Title);
            Assert.Equal(expectedContentType, actual.ContentType);
        }

        /// <summary>
        /// Verify that when the description shows up as an array, only the first value is saved.
        /// (Elasticsearch 5 behavior.)
        /// </summary>
        [Fact]
        public void SimpleArray()
        {
            const string expected = "description line 1";
            const string expectedUrl = "http://nowhere";
            const string expectedTitle = "test title";
            const string expectedContentType = "test content type";

            // NOTE: This is straight-up Json.Net deserialization without NEST mapping fields to properties.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhere"",
                    ""description"":    [
                        ""description line 1"",
                        ""description line 2""
                    ],
                    ""contenttype"":    ""test content type""
                }";

            var serializer = new JsonSerializer();
            var reader = new JsonTextReader(new StringReader(data));

            SiteWideSearchResult actual = serializer.Deserialize<SiteWideSearchResult>(reader);

            Assert.Equal(expected, actual.Description);
            Assert.Equal(expectedUrl, actual.URL);
            Assert.Equal(expectedTitle, actual.Title);
            Assert.Equal(expectedContentType, actual.ContentType);
        }

        /// <summary>
        /// Verify that when the description shows up as an array of arrays, only the first value is saved.
        /// (Elasticsearch 7 behavior.)
        /// </summary>
        [Fact]
        public void NestedArray()
        {
            const string expected = "description line 1";
            const string expectedUrl = "http://nowhere";
            const string expectedTitle = "test title";
            const string expectedContentType = "test content type";

            // NOTE: This is straight-up Json.Net deserialization without NEST mapping fields to properties.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhere"",
                    ""description"":    [
                        [
                            ""description line 1"",
                            ""description line 2""
                        ]
                    ],
                    ""contenttype"":    ""test content type""
                }";

            var serializer = new JsonSerializer();
            var reader = new JsonTextReader(new StringReader(data));

            SiteWideSearchResult actual = serializer.Deserialize<SiteWideSearchResult>(reader);

            Assert.Equal(expected, actual.Description);
            Assert.Equal(expectedUrl, actual.URL);
            Assert.Equal(expectedTitle, actual.Title);
            Assert.Equal(expectedContentType, actual.ContentType);
        }

        /// <summary>
        /// Verify that things don't blow up if the description is empty.
        /// </summary>
        [Fact]
        public void NoDescription()
        {
            string expected = String.Empty;
            const string expectedUrl = "http://nowhere";
            const string expectedTitle = "test title";
            const string expectedContentType = "test content type";

            // NOTE: This is straight-up Json.Net deserialization without NEST mapping fields to properties.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhere"",
                    ""description"":    [
                        [
                            """",
                            ""description line 2""
                        ]
                    ],
                    ""contenttype"":    ""test content type""
                }";

            var serializer = new JsonSerializer();
            var reader = new JsonTextReader(new StringReader(data));

            SiteWideSearchResult actual = serializer.Deserialize<SiteWideSearchResult>(reader);

            Assert.Equal(expected, actual.Description);
            Assert.Equal(expectedUrl, actual.URL);
            Assert.Equal(expectedTitle, actual.Title);
            Assert.Equal(expectedContentType, actual.ContentType);

        }

        /// <summary>
        /// Verify an exception is thrown when the description is an unexptected type.
        /// </summary>
        [Fact]
        public void IncorrectValueType()
        {
            const string expectedMessage = "Don't know how to work with tokens of type 'Integer'.";

            // NOTE: This is straight-up Json.Net deserialization without NEST mapping fields to properties.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhwere"",
                    ""description"":    1234
                }";

            var serializer = new JsonSerializer();
            var reader = new JsonTextReader(new StringReader(data));

            Exception ex = Assert.Throws<InvalidOperationException>(
                () => serializer.Deserialize<SiteWideSearchResult>(reader)
            );

            Assert.Equal(expectedMessage, ex.Message);
        }

    }
}