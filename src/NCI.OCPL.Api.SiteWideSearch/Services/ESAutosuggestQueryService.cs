using System;
using System.Threading.Tasks;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NCI.OCPL.Api.Common;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Autosuggest query service using Elasticsearch for the backend.
    /// </summary>
    public class ESAutosuggestQueryService : IAutosuggestQueryService
    {
        private readonly ElasticsearchClient _elasticClient;

        private readonly AutosuggestIndexOptions _indexConfig;

        private readonly ILogger<ESAutosuggestQueryService> _logger;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClient">An Elasticsearch client instance.</param>
        /// <param name="config">Configuration/settings for the query.</param>
        /// <param name="logger">The logger</param>
        public ESAutosuggestQueryService(ElasticsearchClient elasticClient,
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
            SearchRequest request = new SearchRequest(this._indexConfig.AliasName)
            {
                Query = new BoolQuery
                {
                    Filter = new Query[]
                    {
                        new TermQuery("language", language)
                    },
                    Must = new Query[]
                    {
                        new MatchQuery("term", term)
                    }
                },
                Sort = new SortOptions[]
                {
                    new FieldSort("weight") { Order = SortOrder.Desc }
                },
                Source = new SourceConfig(new SourceFilter
                {
                    Includes = new string[] { "term" }
                }),
                Size = size
            };

            SearchResponse<Suggestion> response;
            try
            {
                response = await _elasticClient.SearchAsync<Suggestion>(request);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error searching index '{this._indexConfig.AliasName}'");
                throw new APIInternalException("Errors occured");
            }

            if (response.IsValidResponse)
            {
                return new Suggestions(
                    response.Total,
                    response.Documents
                );
            }
            else
            {
                string message = $"Invalid response when searching for '{term}'."
                    .Replace(Environment.NewLine, String.Empty);
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

            HealthResponse response;
            try
            {
                response = await _elasticClient.Cluster.HealthAsync(new HealthRequest(_indexConfig.AliasName));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error checking ElasticSearch health for index '{_indexConfig.AliasName}'.");
                return false;
            }

            if (!response.IsValidResponse)
            {
                _logger.LogError($"Error checking ElasticSearch health for index '{_indexConfig.AliasName}'.");
                _logger.LogError($"Returned debug info: {response.DebugInformation}.");
                return false;
            }

            if (response.Status != HealthStatus.Green
                && response.Status != HealthStatus.Yellow)
            {
                _logger.LogError($"Elasticsearch not healthy. Index status is '{response.Status}'.");
                return false;
            }

            return true;
        }

    }
}