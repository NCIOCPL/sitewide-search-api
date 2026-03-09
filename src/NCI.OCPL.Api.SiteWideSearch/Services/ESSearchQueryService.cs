using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NCI.OCPL.Api.Common;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    /// <summary>
    /// Sitewide search query service using Elasticsearch for the backend.
    /// </summary>
    public class ESSearchQueryService : ISearchQueryService
    {
        private static Dictionary<Tuple<string, string>, ISiteWideSearchQueryBuilder> _queryBuilders = new Dictionary<Tuple<string, string>, ISiteWideSearchQueryBuilder>();

        private readonly ElasticsearchClient _elasticClient;

        private readonly SearchIndexOptions _indexConfig;

        private readonly ILogger<ESSearchQueryService> _logger;

        static ESSearchQueryService()
        {
            _queryBuilders.Add(new Tuple<string, string>("cgov", "en"), new ESCGovEnglishSitewideSearchQueryBuilder());
            _queryBuilders.Add(new Tuple<string, string>("cgov", "es"), new ESCGovSpanishSitewideSearchQueryBuilder());
            _queryBuilders.Add(new Tuple<string, string>("doc", "en"), new ESDocEnglishSitewideSearchQueryBuilder());
            _queryBuilders.Add(new Tuple<string, string>("doc", "es"), new ESDocSpanishSitewideSearchQueryBuilder());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClient">An Elasticsearch client instance.</param>
        /// <param name="config">Configuration/settings for the query.</param>
        /// <param name="logger">The logger</param>
        public ESSearchQueryService(ElasticsearchClient elasticClient,
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
        /// <param name="siteList">An optional parameter used to limit the items returned to a given list of sites.</param>
        public async Task<SiteWideSearchResults> Get(string collection, string language, string term, int from, int size, IEnumerable<string> siteList)
        {
            //TODO: Make this a parameter that can take in a list of fields.
            // Setup the list of fields we want ES to return.
            string[] fields = new string[] { "url", "title", "metatag.description", "metatag.dcterms.type" };

            SearchResponse<SiteWideSearchResult> response;

            try
            {
                ISiteWideSearchQueryBuilder builder = _queryBuilders[new Tuple<string, string>(collection, language)];

                SearchRequest request = new SearchRequest(this._indexConfig.AliasName)
                {
                    Query = builder.GetQuery(term, siteList),
                    Size = size,
                    From = from,
                    Source = new SourceConfig(new SourceFilter
                    {
                        Includes = fields
                    }),
                    Sort = new SortOptions[]
                    {
                        new FieldSort("_score"),
                        new FieldSort("url")
                    },
                    TrackTotalHits = true
                };

                response = await _elasticClient.SearchAsync<SiteWideSearchResult>(request);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error searching index '{this._indexConfig.AliasName}'");
                throw new APIInternalException("Errors occured.");
            }

            if (response.IsValidResponse)
            {
                return new SiteWideSearchResults(
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
                throw new APIInternalException("Error occured.");
            }
        }

        /// <summary>
        /// Checks whether the underlying data service is in a healthy condition.
        /// </summary>
        /// <returns>True if the data store is operational, false otherwise.</returns>
        public async Task<bool> GetIsHealthy()
        {
            // Use the cluster health API to verify that the index is functioning.
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
                _logger.LogError("Error checking ElasticSearch health.");
                _logger.LogError($"Returned debug info: {response.DebugInformation}.");
                return false;
            }

            if (response.Status != HealthStatus.Green
                && response.Status != HealthStatus.Yellow )
            {
                _logger.LogError($"Elasticsearch not healthy. Index status is '{response.Status}'.");
                return false;
            }

            return true;

        }

    }
}