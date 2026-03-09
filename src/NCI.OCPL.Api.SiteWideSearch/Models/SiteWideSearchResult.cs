using System.Text.Json.Serialization;

namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Represents a Single Site-Wide Search Result
    /// </summary>
    public class SiteWideSearchResult
    {

        /// <summary>
        /// The title of this item
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// The URL for this result
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("url")]
        public string URL { get; set; }

        /// <summary>
        /// Gets the content type of this result if there is one
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("metatag.dcterms.type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets the description of this result
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("metatag.description")]
        [JsonConverter(typeof(MetadataDescriptionConverter))]
        public string Description { get; set; }

    }
}