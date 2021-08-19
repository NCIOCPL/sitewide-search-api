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
            // A future version will want to use one ore more sites, but for now, we only use the first one.
            IEnumerator<string> enumerator = siteFilter.GetEnumerator();
            string site = enumerator.MoveNext() ? enumerator.Current : String.Empty;

            QueryContainerDescriptor<SiteWideSearchResult> qcd = new QueryContainerDescriptor<SiteWideSearchResult>();

            return GetQueryImpl(qcd, searchTerm, site);
        }

        /// <summary>
        /// Implementation of GetQuery for a specific combination of collection and language.
        /// </summary>
        /// <param name="qcd">A QueryContainer instance</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="site">The site search results should be limited to.</param>
        /// <returns></returns>
        protected abstract QueryContainer GetQueryImpl(QueryContainerDescriptor<SiteWideSearchResult> qcd, string searchTerm, string site);
    }
}