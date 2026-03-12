using System;
using System.Text.Json;

using Xunit;


namespace NCI.OCPL.Api.SiteWideSearch.Models.Tests
{
    /// <summary>
    /// Tests for the various formats Elasticsearch may use when presenting the
    /// metatag.description property for deserialization.
    ///
    /// Tests are for <see cref="SiteWideSearch.SiteWideSearchResultConverter"/> and
    /// a) The behavior when the description is an array vs a single string, and
    /// b) The different JSON structures used when deserializing from Elasticsearch
    ///    versus serializing to JSON for the end user.
    /// </summary>
    public class SiteWideSearchResultConverterTest
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

            // NOTE: This is straight-up JSON deserialization without Elasticsearch handling any of the mapping.
            string data = @"
                {
                    ""title"":                  ""test title"",
                    ""url"":                    ""http://nowhere"",
                    ""metatag.description"":    ""simple description"",
                    ""metatag.dcterms.type"":    ""test content type""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

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

            // NOTE: This is straight-up JSON deserialization without Elasticsearch handling any of the mapping.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhere"",
                    ""metatag.description"":    [
                        ""description line 1"",
                        ""description line 2""
                    ],
                    ""metatag.dcterms.type"":    ""test content type""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

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

            // NOTE: This is straight-up JSON deserialization without Elasticsearch handling any of the mapping.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhere"",
                    ""metatag.description"":    [
                        [
                            ""description line 1"",
                            ""description line 2""
                        ]
                    ],
                    ""metatag.dcterms.type"":    ""test content type""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

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

            // NOTE: This is straight-up JSON deserialization without Elasticsearch handling any of the mapping.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhere"",
                    ""metatag.description"":    [
                        [
                            """",
                            ""description line 2""
                        ]
                    ],
                    ""metatag.dcterms.type"":   ""test content type""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

            Assert.Equal(expected, actual.Description);
            Assert.Equal(expectedUrl, actual.URL);
            Assert.Equal(expectedTitle, actual.Title);
            Assert.Equal(expectedContentType, actual.ContentType);
        }

        /// <summary>
        /// Verify that Description is empty when the metatag.description key is absent.
        /// </summary>
        [Fact]
        public void Description_WhenAbsent_IsNull()
        {
            string data = @"
                {
                    ""title"":                  ""test title"",
                    ""url"":                    ""http://nowhere"",
                    ""metatag.dcterms.type"":   ""test content type""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

            Assert.Null(actual.Description);
        }

        /// <summary>
        /// Verify an exception is thrown when the description is an unexpected type.
        /// </summary>
        [Fact]
        public void IncorrectValueType()
        {
            const string expectedMessage = "Don't know how to work with tokens of type 'Number'.";

            // NOTE: This is straight-up JSON deserialization without Elasticsearch handling any of the mapping.
            string data = @"
                {
                    ""title"":          ""test title"",
                    ""url"":            ""http://nowhwere"",
                    ""metatag.description"":    1234
                }";

            JsonException ex = Assert.Throws<JsonException>(
                () => JsonSerializer.Deserialize<SiteWideSearchResult>(data)
            );

            Assert.Equal(expectedMessage, ex.Message);
        }

        /// <summary>
        /// Verify that ContentType is deserialized correctly when metatag.dcterms.type is present.
        /// </summary>
        [Fact]
        public void ContentType_WhenPresent_IsDeserialized()
        {
            string data = @"
                {
                    ""title"":                  ""test title"",
                    ""url"":                    ""http://nowhere"",
                    ""metatag.description"":    ""simple description"",
                    ""metatag.dcterms.type"":   ""test content type""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

            Assert.Equal("test content type", actual.ContentType);
        }

        /// <summary>
        /// Verify that ContentType is null when metatag.dcterms.type is absent.
        /// </summary>
        [Fact]
        public void ContentType_WhenAbsent_IsNull()
        {
            string data = @"
                {
                    ""title"":                  ""test title"",
                    ""url"":                    ""http://nowhere"",
                    ""metatag.description"":    ""simple description""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

            Assert.Null(actual.ContentType);
        }

        /// <summary>
        /// Verify that Title is null when the title key is absent.
        /// </summary>
        [Fact]
        public void Title_WhenAbsent_IsNull()
        {
            string data = @"
                {
                    ""url"":                    ""http://nowhere"",
                    ""metatag.description"":    ""simple description""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

            Assert.Null(actual.Title);
        }

        /// <summary>
        /// Verify that URL is null when the url key is absent.
        /// </summary>
        [Fact]
        public void URL_WhenAbsent_IsNull()
        {
            string data = @"
                {
                    ""title"":                  ""test title"",
                    ""metatag.description"":    ""simple description""
                }";

            SiteWideSearchResult actual = JsonSerializer.Deserialize<SiteWideSearchResult>(data);

            Assert.Null(actual.URL);
        }

        /// <summary>
        /// Verify that serialization uses the C# property names when no naming policy is set.
        /// </summary>
        [Fact]
        public void Serialize_NoPolicyUsesPropertyNames()
        {
            var value = new SiteWideSearchResult
            {
                Title = "test title",
                URL = "http://nowhere",
                ContentType = "test content type",
                Description = "simple description"
            };

            string json = JsonSerializer.Serialize(value);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            Assert.Equal("test title", root.GetProperty("Title").GetString());
            Assert.Equal("http://nowhere", root.GetProperty("URL").GetString());
            Assert.Equal("test content type", root.GetProperty("ContentType").GetString());
            Assert.Equal("simple description", root.GetProperty("Description").GetString());
        }

        /// <summary>
        /// Verify that serialization applies the naming policy the caller specifies.
        /// </summary>
        [Fact]
        public void Serialize_NamingPolicyIsApplied()
        {
            var value = new SiteWideSearchResult
            {
                Title = "test title",
                URL = "http://nowhere",
                ContentType = "test content type",
                Description = "simple description"
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            string json = JsonSerializer.Serialize(value, options);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            Assert.Equal("test title", root.GetProperty("title").GetString());
            Assert.Equal("http://nowhere", root.GetProperty("url").GetString());
            Assert.Equal("test content type", root.GetProperty("contentType").GetString());
            Assert.Equal("simple description", root.GetProperty("description").GetString());
        }

    }
}