using Nest;

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
        [Text(Name = "term")]
        public string Term { get; set; }

    }
}