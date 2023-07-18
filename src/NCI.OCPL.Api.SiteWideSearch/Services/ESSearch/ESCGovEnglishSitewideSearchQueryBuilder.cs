using System;
using System.Collections.Generic;

using Nest;

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
        protected override QueryContainer GetQueryImpl(
            QueryContainerDescriptor<SiteWideSearchResult> qcd,
            string searchTerm,
            IEnumerable<string> siteFilter)
        {
            qcd.Boosting(boost => boost
                .Positive(pos => pos
                    // Q: Why didn't you use the overloaded operators instead of a Bool query?
                    // A: Because the overloaded operators promote sub-queries to the level of
                    //    their parents. This syntax is more verbose, but gets the correct structure.
                    .Bool( b => b
                        .Must( bm =>
                            (
                                bm.Term(t => t.Field("metatag.content-language").Value("en")) ||
                                !bm.Exists(e => e.Field("metatag.content-language"))
                            )
                            ,
                            bm =>
                            (
                                bm.Match(m => m.Field("content").Query(searchTerm).Operator(Operator.And).Boost(1).Verbatim() ) ||
                                bm.MatchPhrase(mp => mp.Field("content").Query(searchTerm).Boost(1).Verbatim()) ||
                                bm.MatchPhrase(mp => mp.Field("searchtitle").Query(searchTerm).Boost(1).Verbatim()) ||
                                bm.Match(mp => mp.Field("searchtitle").Query(searchTerm).Boost(1).Verbatim()) ||
                                bm.Bool( bb =>
                                    bb.Should( bbs =>
                                        bbs.Match(m => m.Field("metatag.description").Query(searchTerm).Boost(0.01).Verbatim()) ||
                                        bbs.MatchPhrase(mp => mp.Field("metatag.description").Query(searchTerm).Boost(0.01).Verbatim())
                                    )
                                )
                            )
                        )
                        .Should(
                            bs => bs.Term(t => t.Field("type").Value("text/html").Boost(4)),
                            bs => bs.Match(m => m.Field("metatag.dcterms.type").Query("pdqcancerinfosummary").Boost(1.2)),
                            bs => bs.Match(m => m.Field("metatag.dcterms.type").Query("cgovcancertypehome").Boost(1.2)),
                            bs => bs.Bool(
                                bsb => bsb.Should( bsbs => bsbs.Term(t => t.Field("host").Value("www.cancer.gov").Boost(10)))
                            )
                        )
                    )
                )
                .Negative(neg =>
                    neg.Terms(t => t.Field("searchurl").Terms("2012", "2011", "2010", "2013")) &&
                    neg.Prefix(p => p.Field("searchurl.raw").Value("www.cancer.gov/news-events/press-releases/"))
                    ||
                    neg.Prefix(p => p.Field("searchurl.raw").Value("www.cancer.gov/news-events/media-resources/multicultural/lifelines/")) ||
                    neg.Match(m => m.Field("searchurl").Query("video"))
                )
                .NegativeBoost(0.5)
            );

            return qcd;
        }
    }
}
