using System;
using System.Collections.Generic;

using Nest;

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
                        bf.Term(t => t.Field("metatag.content-language").Value("es"))
                    )
                )
                .Must( bm => (
                        bm.Exists(e => e.Field("searchtitle")) &&
                        bm.Prefix(p => p.Field("searchurl.raw").Value(siteFilter).Verbatim())
                    ),
                    bm => bm
                        .Bool( b => b
                            .Should(
                            bs => bm.Match(m => m.Field("content.es").Query(searchTerm).Operator(Operator.And).Boost(1).Verbatim() ),
                            bs => bm.Match(m => m.Field("searchtitle.es").Query(searchTerm).Boost(1).Verbatim()),
                            bs => bm.Match(m => m.Field("searchurl.es").Query(searchTerm).Boost(1).Verbatim()),
                            bs => bm.MatchPhrase(mp => mp.Field("content.es").Query(searchTerm).Boost(1).Verbatim()),
                            bs =>  bs.Bool( bb =>
                                bb.Should( bbs =>
                                    bbs.Match(m => m.Field("metatag.description.es").Query(searchTerm).Boost(0.01).Verbatim())
                                )
                            )
                        )
                    )
                )
                .Should(
                    bs => bs.Term(t => t.Field("type").Value("text/html").Boost(1))
                )
            );

            return qcd;
        }
    }
}
