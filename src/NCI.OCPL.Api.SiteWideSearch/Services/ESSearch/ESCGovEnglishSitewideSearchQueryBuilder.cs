using System.Collections.Generic;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Builds Sitewide Search queries for the CGov collection and English language.
    /// </summary>
    public class ESCGovEnglishSitewideSearchQueryBuilder : ESDocSitewideSearchQueryBuilderBase
    {

        /// <summary>
        /// Builds the sitewide search query for English CancerGov.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">Ignored.</param>
        /// <returns></returns>
        protected override Query GetQueryImpl(
            string searchTerm,
            IEnumerable<string> siteFilter)
        {
            return new BoostingQuery
            {
                Positive = new BoolQuery
                {
                    Must = new Query[]
                    {
                        // (Term(metatag.content-language, "en") || !Exists(metatag.content-language))
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
                        },
                        // Content/title matching
                        new BoolQuery
                        {
                            Should = new Query[]
                            {
                                new MatchQuery("content", searchTerm) { Operator = Operator.And, Boost = 1 },
                                new MatchPhraseQuery("content", searchTerm) { Boost = 1 },
                                new MatchPhraseQuery("searchtitle", searchTerm) { Boost = 1 },
                                new MatchQuery("searchtitle", searchTerm) { Boost = 1 },
                                new BoolQuery
                                {
                                    Should = new Query[]
                                    {
                                        new MatchQuery("metatag.description", searchTerm) { Boost = 0.01f },
                                        new MatchPhraseQuery("metatag.description", searchTerm) { Boost = 0.01f }
                                    }
                                }
                            }
                        }
                    },
                    Should = new Query[]
                    {
                        new TermQuery("type", "text/html") { Boost = 4 },
                        new MatchQuery("metatag.dcterms.type", "pdqcancerinfosummary") { Boost = 1.2f },
                        new MatchQuery("metatag.dcterms.type", "cgovcancertypehome") { Boost = 1.2f },
                        new BoolQuery
                        {
                            Should = new Query[]
                            {
                                new TermQuery("host", "www.cancer.gov") { Boost = 10 }
                            }
                        }
                    }
                },
                Negative = new BoolQuery
                {
                    Should = new Query[]
                    {
                        new BoolQuery
                        {
                            Must = new Query[]
                            {
                                new TermsQuery
                                {
                                    Field = "searchurl",
                                    Terms = new TermsQueryField(new FieldValue[] { "2012", "2011", "2010", "2013" })
                                },
                                new PrefixQuery("searchurl.raw", "www.cancer.gov/news-events/press-releases/")
                            }
                        },
                        new PrefixQuery("searchurl.raw", "www.cancer.gov/news-events/media-resources/multicultural/lifelines/"),
                        new MatchQuery("searchurl", "video")
                    }
                },
                NegativeBoost = 0.5
            };
        }
    }
}
