using System.Collections.Generic;

using Elastic.Clients.Elasticsearch.QueryDsl;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Builds Sitewide Search queries for the CGov collection and Spanish language.
    /// </summary>
    public class ESCGovSpanishSitewideSearchQueryBuilder : ESDocSitewideSearchQueryBuilderBase
    {

        /// <summary>
        /// Builds the sitewide search query for Spanish CancerGov.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">Ignored.</param>
        /// <returns></returns>
        protected override Query GetQueryImpl(
            string searchTerm,
            IEnumerable<string> siteFilter)
        {
            return new BoolQuery
            {
                Must = new Query[]
                {
                    new TermQuery("metatag.content-language", "es"),
                    new BoolQuery
                    {
                        Should = new Query[]
                        {
                            new MatchQuery("content.es", searchTerm) { Operator = Operator.And, Boost = 1 },
                            new MatchQuery("searchtitle.es", searchTerm) { Boost = 1 },
                            new MatchQuery("searchurl.es", searchTerm) { Boost = 1 },
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
                    new TermQuery("type", "text/html") { Boost = 1 },
                    new MatchQuery("metatag.dcterms.type", "pdqcancerinfosummary") { Boost = 1.2f },
                    new MatchQuery("metatag.dcterms.type", "cgovcancertypehome") { Boost = 1.2f },
                    new BoolQuery
                    {
                        Should = new Query[]
                        {
                            new TermQuery("host", "www.cancer.gov") { Boost = 1 }
                        }
                    }
                }
            };
        }
    }
}
