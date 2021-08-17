using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCI.OCPL.Api.Common;
using Nest;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Autosuggest query service using Elasticsearch for the backend.
    /// </summary>
    public class ESAutosuggestQueryService : IAutosuggestQueryService
    {
        private readonly IElasticClient _elasticClient;

        private readonly AutosuggestIndexOptions _indexConfig;

        private readonly ILogger<ESAutosuggestQueryService> _logger;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClient">An Elasticsearch client instance.</param>
        /// <param name="config">Configuration/settings for the query.</param>
        /// <param name="logger">The logger</param>
        public ESAutosuggestQueryService(IElasticClient elasticClient,
            IOptions<AutosuggestIndexOptions> config,
            ILogger<ESAutosuggestQueryService> logger)
        {
            _elasticClient = elasticClient;
            _indexConfig = config.Value;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of <paramref name="size" /> suggestions.
        /// </summary>
        /// <param name="collection">The collection of suggestions to use.</param>
        /// <param name="language">The language to use.</param>
        /// <param name="term">The term suggestions should be based on.</param>
        /// <param name="size">The number of suggestions to return.</param>
        /// <returns>A <see cref="T:NCI.OCPL.Api.SiteWideSearch.Suggestions" /> object containing the suggestions
        /// and the total number of matching terms available.</returns>
        public async Task<Suggestions> Get(string collection, string language, string term, int size)
        {
            // Setup our template name based on the collection name.  Template name is the directory the
            // file is stored in, an underscore, the template name prefix (search), an underscore,
            // the name of the collection (only "cgov" at this time), another underscore and then
            // the language code (either "en" or "es").
            string templateName = $"autosg_suggest_{collection}_{language}";

            // ISearchTemplateRequest.File is obsolete.
            // Refactoring to remove this dependency is recorded as issue #28
            // https://github.com/NCIOCPL/sitewide-search-api/issues/28
#pragma warning disable CS0618

            ISearchResponse<Suggestion> response;
            try
            {
                response = await _elasticClient.SearchTemplateAsync<Suggestion>(sd => sd
                    .Index(_indexConfig.AliasName)
                    .File(templateName)
                    .Params(pd => pd
                        .Add("searchstring", term)
                        .Add("my_size", 10)
                    )
                );
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error searching index '{this._indexConfig.AliasName}'");
                throw new APIInternalException("Errors occured");
            }
#pragma warning restore CS0618

            if (response.IsValid)
            {
                return new Suggestions(
                    response.Total,
                    response.Documents
                );
            }
            else
            {
                string message = $"Invalid response when searching for '{term}' with template '{templateName}'.";
                _logger.LogError(message);
                _logger.LogError(response.DebugInformation);
                throw new APIInternalException("errors occured.");
            }
        }

        /// <summary>
        /// Checks whether the underlying data service is in a healthy condition.
        /// </summary>
        /// <returns>True if the data store is operational, false otherwise.</returns>
        public async Task<bool> GetIsHealthy()
        {
            // Use the cluster health API to verify that the index is functioning.
            // Maps to https://<SERVER_NAME>/_cluster/health/<INDEX_NAME>?pretty (or other server)
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
                _logger.LogError($"Error checking ElasticSearch health for index '{_indexConfig.AliasName}'.");
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