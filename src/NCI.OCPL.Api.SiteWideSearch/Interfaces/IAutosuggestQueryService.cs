using System.Threading.Tasks;

namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Abstraction layer for a service to run autosuggest queries.
    /// </summary>
    public interface IAutosuggestQueryService
    {
        /// <summary>
        /// Get a list of <paramref name="size" /> suggestions.
        /// </summary>
        /// <param name="collection">The collection of suggestions to use.</param>
        /// <param name="language">The language to use.</param>
        /// <param name="term">The term suggestions should be based on.</param>
        /// <param name="size">The number of suggestions to return.</param>
        /// <returns>A <see cref="T:NCI.OCPL.Api.SiteWideSearch.Suggestions" /> object containing the suggestions
        /// and the total number of matching terms available.</returns>
        Task<Suggestions> Get(string collection, string language, string term, int size);

        /// <summary>
        /// Checks whether the underlying data service is in a healthy condition.
        /// </summary>
        /// <returns>True if the data store is operational, false otherwise.</returns>
        Task<bool> GetIsHealthy();
    }
}