using Nest;
using Newtonsoft.Json;

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
        [Text(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// The URL for this result
        /// </summary>
        /// <returns></returns>
        [Text(Name = "url")]
        public string URL { get; set; }

        /// <summary>
        /// Gets the content type of this result if there is one
        /// </summary>
        /// <returns></returns>
        [Text(Name = "metatag.dcterms.type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets the description of this result
        /// </summary>
        /// <returns></returns>
        [Text(Name = "metatag.description")]
        [JsonConverter(typeof(MetadataDescriptionConverter))]
        public string Description { get; set; }

    }
}