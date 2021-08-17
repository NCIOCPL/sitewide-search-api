using System.Threading.Tasks;

namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Abstraction layer for a service to run sitewide search queries.
    /// </summary>
    public interface ISearchQueryService
    {
        /// <param name="collection">The search collection/strategy to use.  This defines the ES template to use.</param>
        /// <param name="language">The language to use. Only "en" and "es" are currently supported.</param>
        /// <param name="term">The search term to search for</param>
        /// <param name="from">The offset of results to retrieve</param>
        /// <param name="size">The number of items to retrieve</param>
        /// <param name="site">An optional parameter used to limit the number of items returned based on site.</param>
        Task<SiteWideSearchResults> Get(string collection, string language, string term, int from, int size, string site);

        /// <summary>
        /// Checks whether the underlying data service is in a healthy condition.
        /// </summary>
        /// <returns>True if the data store is operational, false otherwise.</returns>
        Task<bool> GetIsHealthy();
    }
}