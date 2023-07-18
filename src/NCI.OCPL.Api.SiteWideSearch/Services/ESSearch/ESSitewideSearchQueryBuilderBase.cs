using System;
using System.Collections.Generic;

using Nest;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Base class for building Sitewide Search queries.
    /// </summary>
    public abstract class ESDocSitewideSearchQueryBuilderBase : ISiteWideSearchQueryBuilder
    {
        /// <summary>
        /// Builds the sitewide search query for English CancerGov.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">Ignored.</param>
        /// <returns></returns>

        public QueryContainer GetQuery(string searchTerm, IEnumerable<string> siteFilter)
        {
            QueryContainerDescriptor<SiteWideSearchResult> qcd = new QueryContainerDescriptor<SiteWideSearchResult>();

            return GetQueryImpl(qcd, searchTerm, siteFilter);
        }

        /// <summary>
        /// Implementation of GetQuery for a specific combination of collection and language.
        /// </summary>
        /// <param name="qcd">A QueryContainer instance</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">The site search results should be limited to.</param>
        /// <returns></returns>
        protected abstract QueryContainer GetQueryImpl(QueryContainerDescriptor<SiteWideSearchResult> qcd, string searchTerm, IEnumerable<string> siteFilter);

        /// <summary>
        /// Creates a collection of queries for restricting the list of search results to a given list of sites.
        /// </summary>
        /// <remark>This does not return a standalone query.</remark>
        /// <param name="siteList">A collection of site URL prefixes. e.g. dceg.cancer.gov or www.cancer.gov/nano</param>
        /// <returns>An array of QueryContainer objects containing prefix queries matching the searchurl field against values from siteList.</returns>
        protected QueryContainer[] GetSiteFilterSubQueries(IEnumerable<string> siteList)
        {
            List<QueryContainer> siteFilterQueries = new List<QueryContainer>();
            //QueryContainerDescriptor<SiteWideSearchResult> siteFilterQueries = new QueryContainerDescriptor<SiteWideSearchResult>();
            foreach (string site in siteList)
            {
                QueryContainerDescriptor<SiteWideSearchResult> sq = new QueryContainerDescriptor<SiteWideSearchResult>();
                sq.Prefix(p => p.Field("searchurl.raw").Value(site).Verbatim());
                siteFilterQueries.Add(sq);
            }

            return siteFilterQueries.ToArray();
        }
    }
}