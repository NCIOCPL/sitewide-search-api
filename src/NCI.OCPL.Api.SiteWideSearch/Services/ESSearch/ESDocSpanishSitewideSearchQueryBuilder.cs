using System.Collections.Generic;

using Elastic.Clients.Elasticsearch.QueryDsl;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Builds Sitewide Search queries for the DOC collection and Spanish language.
    /// </summary>
    public class ESDocSpanishSitewideSearchQueryBuilder : ESDocSitewideSearchQueryBuilderBase
    {

        /// <summary>
        /// Builds the sitewide search query for Spanish DOC queries.
        /// </summary>
        /// <param name="qcd">A QueryContainer instance</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">The site search results should be limited to.</param>
        /// <returns></returns>
        protected override Query GetQueryImpl(
            QueryDescriptor qcd,
            string searchTerm,
            IEnumerable<string> siteFilter)
        {
            // Get the collection of subqueries for restricting the results to specific sites.
            Query[] siteFilterSubqueries = GetSiteFilterSubQueries(siteFilter);

            return qcd.Bool(b => b
                .Filter(
                    f => f.Term(t => t.Field("metatag.content-language").Value("es"))
                )
                .Must(
                    m => m.Bool(siteBool => siteBool
                        .Must(
                            sm => sm.Exists(e => e.Field("searchtitle"))
                        )
                        .Should(siteFilterSubqueries)
                        .MinimumShouldMatch(1)
                    ),
                    m => m.Bool(contentBool => contentBool
                        .Should(
                            s => s.Match(ma => ma.Field("content.es").Query(searchTerm).Operator(Operator.And).Boost(1)),
                            s => s.Match(ma => ma.Field("searchtitle.es").Query(searchTerm).Boost(1)),
                            s => s.Match(ma => ma.Field("searchurl.es").Query(searchTerm).Boost(1)),
                            s => s.MatchPhrase(mp => mp.Field("content.es").Query(searchTerm).Boost(1)),
                            s => s.Bool(descBool => descBool
                                .Should(
                                    ds => ds.Match(ma => ma.Field("metatag.description.es").Query(searchTerm).Boost(0.01f))
                                )
                            )
                        )
                    )
                )
                .Should(
                    s => s.Term(t => t.Field("type").Value("text/html").Boost(1))
                )
            );
        }
    }
}
