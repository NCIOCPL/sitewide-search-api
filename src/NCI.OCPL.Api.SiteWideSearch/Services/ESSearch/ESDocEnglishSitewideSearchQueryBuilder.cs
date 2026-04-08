using System.Collections.Generic;

using Elastic.Clients.Elasticsearch.QueryDsl;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Builds Sitewide Search queries for the DOC collection and English language.
    /// </summary>
    public class ESDocEnglishSitewideSearchQueryBuilder : ESDocSitewideSearchQueryBuilderBase
    {

        /// <summary>
        /// Builds the sitewide search query for English DOC queries.
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
                    f => f.Bool(langBool => langBool
                        .Should(
                            s => s.Term(t => t.Field("metatag.content-language").Value("en")),
                            s => s.Bool(nb => nb
                                .MustNot(
                                    mn => mn.Exists(e => e.Field("metatag.content-language"))
                                )
                            )
                        )
                    )
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
                            s => s.Match(ma => ma.Field("content").Query(searchTerm).Operator(Operator.And).Boost(2)),
                            s => s.Match(ma => ma.Field("searchtitle").Query(searchTerm).Boost(2)),
                            s => s.Match(ma => ma.Field("searchurl").Query(searchTerm).Boost(3)),
                            s => s.MatchPhrase(mp => mp.Field("content").Query(searchTerm).Boost(3)),
                            s => s.Bool(descBool => descBool
                                .Should(
                                    ds => ds.Match(ma => ma.Field("metatag.description").Query(searchTerm).Boost(0.01f))
                                )
                            )
                        )
                    )
                )
                .Should(
                    s => s.Term(t => t.Field("type").Value("text/html").Boost(2))
                )
            );
        }
    }
}
