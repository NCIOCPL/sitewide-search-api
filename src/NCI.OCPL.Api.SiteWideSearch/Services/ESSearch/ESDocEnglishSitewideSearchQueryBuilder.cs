using System;
using System.Collections.Generic;

using Nest;

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
        protected override QueryContainer GetQueryImpl(
            QueryContainerDescriptor<SiteWideSearchResult> qcd,
            string searchTerm,
            string siteFilter)
        {
            // Q: Why didn't you use the overloaded operators instead of a Bool query?
            // A: Because the overloaded operators promote sub-queries to the level of
            //    their parents. This syntax is more verbose, but gets the correct structure.
            qcd
            .Bool( b => b
                .Filter( bf =>
                    (
                        bf.Term(t => t.Field("metatag.content-language").Value("en")) ||
                        !bf.Exists(e => e.Field("metatag.content-language"))
                    )
                )
                .Must( bm => (
                        bm.Exists(e => e.Field("searchtitle")) &&
                        bm.Prefix(p => p.Field("searchurl.raw").Value(siteFilter).Verbatim())
                    ),
                    bm => bm
                        .Bool( b => b
                            .Should(
                            bs => bm.Match(m => m.Field("content").Query(searchTerm).Operator(Operator.And).Boost(2).Verbatim() ),
                            bs => bm.Match(m => m.Field("searchtitle").Query(searchTerm).Boost(2).Verbatim()),
                            bs => bm.Match(m => m.Field("searchurl").Query(searchTerm).Boost(3).Verbatim()),
                            bs => bm.MatchPhrase(mp => mp.Field("content").Query(searchTerm).Boost(3).Verbatim()),
                            bs =>  bs.Bool( bb =>
                                bb.Should( bbs =>
                                    bbs.Match(m => m.Field("metatag.description").Query(searchTerm).Boost(0.01).Verbatim())
                                )
                            )
                        )
                    )
                )
                .Should(
                    bs => bs.Term(t => t.Field("type").Value("text/html").Boost(2))
                )
            );

            return qcd;
        }
    }
}
