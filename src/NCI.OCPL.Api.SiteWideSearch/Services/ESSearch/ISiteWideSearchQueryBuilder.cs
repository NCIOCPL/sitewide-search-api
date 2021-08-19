using System.Collections.Generic;

using Nest;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Abstraction layer for a service to run sitewide search queries.
    /// </summary>
    public interface ISiteWideSearchQueryBuilder
    {
        /// <summary>
        /// Build the sitewide search query.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilters">List of sites to include. If the list is empty, results from all available sites will be considered.</param>
        /// <returns></returns>
        QueryContainer GetQuery(string searchTerm, IEnumerable<string> siteFilters);
    }
}
