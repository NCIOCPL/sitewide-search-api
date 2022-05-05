using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
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
            Indices index = Indices.Index(new string[]{this._indexConfig.AliasName});
            SearchRequest request = new SearchRequest(index)
            {
                // Note the '+' operator for a Boolean's Filter clause.
                Query = +new TermQuery { Field = "language", Value = language, IsVerbatim = true } &&
                        new MatchQuery { Field = "term", Query = term, IsVerbatim = true }
                ,
                Sort = new List<ISort>
                {
                    new FieldSort { Field = "weight", Order = SortOrder.Descending }
                },
                Source = new SourceFilter
                {
                    Includes = new Field[] { new Field("term")}
                },
                Size = size
            };

            ISearchResponse<Suggestion> response;
            try
            {
                response = await _elasticClient.SearchAsync<Suggestion>(request);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error searching index '{this._indexConfig.AliasName}'");
                throw new APIInternalException("Errors occured");
            }

            if (response.IsValid)
            {
                return new Suggestions(
                    response.Total,
                    response.Documents
                );
            }
            else
            {
                string message = $"Invalid response when searching for '{term}'.";
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

            ClusterHealthResponse response;
            try
            {
                Indices index = Indices.Index(new string[] { _indexConfig.AliasName });
                response = await _elasticClient.Cluster.HealthAsync(index);
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

            if (response.Status != Health.Green
                && response.Status != Health.Yellow)
            {
                _logger.LogError($"Elasticsearch not healthy. Index status is '{response.Status}'.");
                return false;
            }

            return true;
        }

    }
}