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
        /// <param name="qcd">A QueryContainer instance</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="siteFilter">Ignored.</param>
        /// <returns></returns>
        protected override Query GetQueryImpl(
            QueryDescriptor qcd,
            string searchTerm,
            IEnumerable<string> siteFilter)
        {
            return qcd.Bool(b => b
                .Must(
                    m => m.Term(t => t.Field("metatag.content-language").Value("es")),
                    m => m.Bool(contentBool => contentBool
                        .Should(
                            s => s.Match(ma => ma.Field("content.es").Query(searchTerm).Operator(Operator.And).Boost(1)),
                            s => s.Match(ma => ma.Field("searchtitle.es").Query(searchTerm).Boost(1)),
                            s => s.Match(ma => ma.Field("searchurl.es").Query(searchTerm).Boost(1)),
                            s => s.Bool(descBool => descBool
                                .Should(
                                    ds => ds.Match(ma => ma.Field("metatag.description.es").Query(searchTerm).Boost(0.01f))
                                )
                            )
                        )
                    )
                )
                .Should(
                    s => s.Term(t => t.Field("type").Value("text/html").Boost(1)),
                    s => s.Match(ma => ma.Field("metatag.dcterms.type").Query("pdqcancerinfosummary").Boost(1.2f)),
                    s => s.Match(ma => ma.Field("metatag.dcterms.type").Query("cgovcancertypehome").Boost(1.2f)),
                    s => s.Bool(hostBool => hostBool
                        .Should(
                            hs => hs.Term(t => t.Field("host").Value("www.cancer.gov").Boost(1))
                        )
                    )
                )
            );
        }
    }
}
