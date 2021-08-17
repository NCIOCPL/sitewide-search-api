using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

using NCI.OCPL.Api.Common;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Sitewide search query service using Elasticsearch for the backend.
    /// </summary>
    public class ESSearchQueryService : ISearchQueryService
    {
        private readonly IElasticClient _elasticClient;

        private readonly SearchIndexOptions _indexConfig;

        private readonly ILogger<ESSearchQueryService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClient">An Elasticsearch client instance.</param>
        /// <param name="config">Configuration/settings for the query.</param>
        /// <param name="logger">The logger</param>
        public ESSearchQueryService(IElasticClient elasticClient,
            IOptions<SearchIndexOptions> config,
            ILogger<ESSearchQueryService> logger)
        {
            _elasticClient = elasticClient;
            _indexConfig = config.Value;
            _logger = logger;
        }

        /// <param name="collection">The search collection/strategy to use.  This defines the ES template to use.</param>
        /// <param name="language">The language to use. Only "en" and "es" are currently supported.</param>
        /// <param name="term">The search term to search for</param>
        /// <param name="from">The offset of results to retrieve</param>
        /// <param name="size">The number of items to retrieve</param>
        /// <param name="site">An optional parameter used to limit the number of items returned based on site.</param>
        public async Task<SiteWideSearchResults> Get(string collection, string language, string term, int from, int size, string site)
        {
            // Setup our template name based on the collection name.  Template name is the directory the
            // file is stored in, an underscore, the template name prefix (search), an underscore,
            // the name of the collection (only "cgov" or "doc" at this time), another underscore and then
            // the language code (either "en" or "es").
            string templateName = $"cgov_search_{collection}_{language}";

            //TODO: Make this a parameter that can take in a list of fields and turn them
            //into this string.
            // Setup the list of fields we want ES to return.
            string fields = "\"url\", \"title\", \"metatag.description\", \"metatag.dcterms.type\"";

            // ISearchTemplateRequest.File is obsolete.
            // Refactoring to remove this dependency is recorded as issue #28
            // https://github.com/NCIOCPL/sitewide-search-api/issues/28
#pragma warning disable CS0618

            ISearchResponse<SiteWideSearchResult> response;
            try
            {
                response = await _elasticClient.SearchTemplateAsync<SiteWideSearchResult>(sd => sd
                    .Index(_indexConfig.AliasName)
                    .File(templateName)
                    .Params(pd => pd
                        .Add("my_value", term)
                        .Add("my_size", size)
                        .Add("my_from", from)
                        .Add("my_fields", fields)
                        .Add("my_site", site)
                    )
                );
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error searching index '{this._indexConfig.AliasName}'");
                throw new APIInternalException("Errors occured.");
            }
#pragma warning restore CS0618

            if (response.IsValid)
            {
                return new SiteWideSearchResults(
                    response.Total,
                    response.Documents
                );

            }
            else
            {
                string message = $"Invalid response when searching for '{term}' with template '{templateName}'.";
                _logger.LogError(message);
                _logger.LogError(response.DebugInformation);
                throw new APIInternalException("Error occured.");
            }
        }

        /// <summary>
        /// Checks whether the underlying data service is in a healthy condition.
        /// </summary>
        /// <returns>True if the data store is operational, false otherwise.</returns>
        public async Task<bool> GetIsHealthy()
        {
            // Use the cluster health API to verify that the Best Bets index is functioning.
            // Maps to https://ncias-d1592-v.nci.nih.gov:9299/_cluster/health/bestbets?pretty (or other server)
            //
            // References:
            // https://www.elastic.co/guide/en/elasticsearch/reference/master/cluster-health.html
            // https://github.com/elastic/elasticsearch/blob/master/rest-api-spec/src/main/resources/rest-api-spec/api/cluster.health.json#L20
            IClusterHealthResponse response;

            try
            {
                response = await _elasticClient.ClusterHealthAsync(hd =>
                {
                    hd = hd
                        .Index(_indexConfig.AliasName);

                    return hd;
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error checking ElasticSearch health for index '{_indexConfig.AliasName}'.");
                return false;

            }

            if (!response.IsValid)
            {
                _logger.LogError("Error checking ElasticSearch health.");
                _logger.LogError($"Returned debug info: {response.DebugInformation}.");
                return false;
            }

            if (response.Status != "green"
                && response.Status != "yellow")
            {
                _logger.LogError($"Elasticsearch not healthy. Index status is '{response.Status}'.");
                return false;
            }

            return true;

        }

    }
}