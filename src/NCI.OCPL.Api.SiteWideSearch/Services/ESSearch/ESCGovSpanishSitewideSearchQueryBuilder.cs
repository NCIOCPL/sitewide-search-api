using System;
using System.Collections.Generic;

using Nest;

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
        /// <param name="qcd">A QueryContainer instance</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">Ignored.</param>
        /// <returns></returns>
        protected override QueryContainer GetQueryImpl(
            QueryContainerDescriptor<SiteWideSearchResult> qcd,
            string searchTerm,
            string siteFilter)
        {
            // Q: Why didn't you use the overloaded operators instead of a Bool query?
            // A: Because the overloaded operators promote sub-queries to the level of
            //    their parents. This syntax is more verbose, but gets the correct structure.
            qcd.Bool( b => b
                .Must(
                    bm => bm.Term(t => t.Field("metatag.content-language").Value("es")),
                    bm =>
                        bm.Bool(bmb => bmb
                            .Should
                            (
                                bmbs => bmbs.Match(m => m.Field("content.es").Query(searchTerm).Operator(Operator.And).Boost(1).Verbatim()),
                                bmbs => bmbs.Match(m => m.Field("searchtitle.es").Query(searchTerm).Boost(1).Verbatim()),
                                bmbs => bmbs.Match(m => m.Field("searchurl.es").Query(searchTerm).Boost(1).Verbatim()),
                                bmbs => bmbs.Bool(
                                    bmbsb => bmbsb.Should(bbs =>
                                        bbs.Match(m => m.Field("metatag.description.es").Query(searchTerm).Boost(0.01).Verbatim())
                                    )
                                )
                        )
                    )
                )
                .Should(
                    bs => bs.Term(t => t.Field("type").Value("text/html").Boost(1)),
                    bs => bs.Match(m => m.Field("metatag.dcterms.type").Query("pdqcancerinfosummary").Boost(1.2)),
                    bs => bs.Match(m => m.Field("metatag.dcterms.type").Query("cgovcancertypehome").Boost(1.2)),
                    bs => bs.Bool(
                        bsb => bsb.Should(bsbs => bsbs.Term(t => t.Field("host").Value("www.cancer.gov").Boost(1)))
                    )
                )
            );

            return qcd;
        }
    }
}
