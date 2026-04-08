using System.Text.Json.Serialization;

namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Represents a Single Site-Wide Search Result
    /// </summary>
    [JsonConverter(typeof(SiteWideSearchResultConverter))]
    public class SiteWideSearchResult
    {

        /// <summary>
        /// The title of this item
        /// </summary>
        /// <returns></returns>
        public string Title { get; set; }

        /// <summary>
        /// The URL for this result
        /// </summary>
        /// <returns></returns>
        public string URL { get; set; }

        /// <summary>
        /// Gets the content type of this result if there is one
        /// </summary>
        /// <returns></returns>
        /// <remarks>Stored in Elasticsearch as `metatag.dcterms.type`.</remarks>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets the description of this result
        /// </summary>
        /// <returns>The result page's metatag.description value.</returns>
        /// <remarks>
        /// The description is stored in Elasticsearch as `metatag.description`.
        /// It may also be either a single string, an array of strings, or a nested
        /// array of arrays of strings.
        /// All of this is handled by the <see cref="SiteWideSearchResultConverter"/>.
        /// </remarks>
        public string Description { get; set; }

    }
}