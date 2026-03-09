using System.Text.Json.Serialization;

namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Represents a single Autosuggest suggestion.
    /// </summary>
    public class Suggestion
    {
        /// <summary>
        /// The Backend ID for this item
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("term")]
        public string Term { get; set; }

    }
}