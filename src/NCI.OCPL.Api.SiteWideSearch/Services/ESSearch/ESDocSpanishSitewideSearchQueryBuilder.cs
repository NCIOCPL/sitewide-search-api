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
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">The site search results should be limited to.</param>
        /// <returns></returns>
        protected override Query GetQueryImpl(
            string searchTerm,
            IEnumerable<string> siteFilter)
        {
            // Get the collection of subqueries for restricting the results to specific sites.
            Query[] siteFilterSubqueries = GetSiteFilterSubQueries(siteFilter);

            return new BoolQuery
            {
                Filter = new Query[]
                {
                    new TermQuery("metatag.content-language", "es")
                },
                Must = new Query[]
                {
                    new BoolQuery
                    {
                        Must = new Query[]
                        {
                            new ExistsQuery { Field = "searchtitle" }
                        },
                        Should = siteFilterSubqueries,
                        MinimumShouldMatch = 1
                    },
                    new BoolQuery
                    {
                        Should = new Query[]
                        {
                            new MatchQuery("content.es", searchTerm) { Operator = Operator.And, Boost = 1 },
                            new MatchQuery("searchtitle.es", searchTerm) { Boost = 1 },
                            new MatchQuery("searchurl.es", searchTerm) { Boost = 1 },
                            new MatchPhraseQuery("content.es", searchTerm) { Boost = 1 },
                            new BoolQuery
                            {
                                Should = new Query[]
                                {
                                    new MatchQuery("metatag.description.es", searchTerm) { Boost = 0.01f }
                                }
                            }
                        }
                    }
                },
                Should = new Query[]
                {
                    new TermQuery("type", "text/html") { Boost = 1 }
                }
            };
        }
    }
}
