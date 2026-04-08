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
        /// <param name="qcd">A QueryContainer instance</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">Ignored.</param>
        /// <returns></returns>
        protected override Query GetQueryImpl(
            QueryDescriptor qcd,
            string searchTerm,
            IEnumerable<string> siteFilter)
        {
            return qcd.Boosting(b => b
                .Positive(p => p
                    .Bool(bo => bo
                        .Must(
                            // (Term(metatag.content-language, "en") || !Exists(metatag.content-language))
                            m => m.Bool(langBool => langBool
                                .Should(
                                    s => s.Term(t => t.Field("metatag.content-language").Value("en")),
                                    s => s.Bool(nb => nb
                                        .MustNot(
                                            mn => mn.Exists(e => e.Field("metatag.content-language"))
                                        )
                                    )
                                )
                            ),
                            // Content/title matching
                            m => m.Bool(contentBool => contentBool
                                .Should(
                                    s => s.Match(ma => ma.Field("content").Query(searchTerm).Operator(Operator.And).Boost(1)),
                                    s => s.MatchPhrase(mp => mp.Field("content").Query(searchTerm).Boost(1)),
                                    s => s.MatchPhrase(mp => mp.Field("searchtitle").Query(searchTerm).Boost(1)),
                                    s => s.Match(ma => ma.Field("searchtitle").Query(searchTerm).Boost(1)),
                                    s => s.Bool(descBool => descBool
                                        .Should(
                                            ds => ds.Match(ma => ma.Field("metatag.description").Query(searchTerm).Boost(0.01f)),
                                            ds => ds.MatchPhrase(mp => mp.Field("metatag.description").Query(searchTerm).Boost(0.01f))
                                        )
                                    )
                                )
                            )
                        )
                        .Should(
                            s => s.Term(t => t.Field("type").Value("text/html").Boost(4)),
                            s => s.Match(ma => ma.Field("metatag.dcterms.type").Query("pdqcancerinfosummary").Boost(1.2f)),
                            s => s.Match(ma => ma.Field("metatag.dcterms.type").Query("cgovcancertypehome").Boost(1.2f)),
                            s => s.Bool(hostBool => hostBool
                                .Should(
                                    hs => hs.Term(t => t.Field("host").Value("www.cancer.gov").Boost(10))
                                )
                            )
                        )
                    )
                )
                .Negative(n => n
                    .Bool(bo => bo
                        .Should(
                            s => s.Bool(pressBool => pressBool
                                .Must(
                                    pm => pm.Terms(ts => ts
                                        .Field("searchurl")
                                        .Terms(new TermsQueryField(new FieldValue[] { "2012", "2011", "2010", "2013" }))
                                    ),
                                    pm => pm.Prefix(pfx => pfx.Field("searchurl.raw").Value("www.cancer.gov/news-events/press-releases/"))
                                )
                            ),
                            s => s.Prefix(pfx => pfx.Field("searchurl.raw").Value("www.cancer.gov/news-events/media-resources/multicultural/lifelines/")),
                            s => s.Match(ma => ma.Field("searchurl").Query("video"))
                        )
                    )
                )
                .NegativeBoost(0.5)
            );
        }
    }
}
