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
                    new BoolQuery
                    {
                        Should = new Query[]
                        {
                            new TermQuery("metatag.content-language", "en"),
                            new BoolQuery
                            {
                                MustNot = new Query[]
                                {
                                    new ExistsQuery { Field = "metatag.content-language" }
                                }
                            }
                        }
                    }
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
                            new MatchQuery("content", searchTerm) { Operator = Operator.And, Boost = 2 },
                            new MatchQuery("searchtitle", searchTerm) { Boost = 2 },
                            new MatchQuery("searchurl", searchTerm) { Boost = 3 },
                            new MatchPhraseQuery("content", searchTerm) { Boost = 3 },
                            new BoolQuery
                            {
                                Should = new Query[]
                                {
                                    new MatchQuery("metatag.description", searchTerm) { Boost = 0.01f }
                                }
                            }
                        }
                    }
                },
                Should = new Query[]
                {
                    new TermQuery("type", "text/html") { Boost = 2 }
                }
            };
        }
    }
}
