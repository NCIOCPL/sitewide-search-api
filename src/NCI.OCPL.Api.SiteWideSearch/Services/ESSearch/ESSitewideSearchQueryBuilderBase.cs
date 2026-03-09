using System.Collections.Generic;
using System.Linq;

using Elastic.Clients.Elasticsearch.QueryDsl;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Base class for building Sitewide Search queries.
    /// </summary>
    public abstract class ESDocSitewideSearchQueryBuilderBase : ISiteWideSearchQueryBuilder
    {
        /// <summary>
        /// Builds the sitewide search query.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">List of sites to include.</param>
        /// <returns></returns>
        public Query GetQuery(string searchTerm, IEnumerable<string> siteFilter)
        {
            return GetQueryImpl(searchTerm, siteFilter);
        }

        /// <summary>
        /// Implementation of GetQuery for a specific combination of collection and language.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">The site search results should be limited to.</param>
        /// <returns></returns>
        protected abstract Query GetQueryImpl(string searchTerm, IEnumerable<string> siteFilter);

        /// <summary>
        /// Creates a collection of queries for restricting the list of search results to a given list of sites.
        /// </summary>
        /// <remark>This does not return a standalone query.</remark>
        /// <param name="siteList">A collection of site URL prefixes. e.g. dceg.cancer.gov or www.cancer.gov/nano</param>
        /// <returns>An array of Query objects containing prefix queries matching the searchurl field against values from siteList.</returns>
        protected Query[] GetSiteFilterSubQueries(IEnumerable<string> siteList)
        {
            return siteList.Select(site =>
                (Query)new PrefixQuery("searchurl.raw", site)
            ).ToArray();
        }
    }
}