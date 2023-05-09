using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Nest;

using NCI.OCPL.Api.Common;

namespace NCI.OCPL.Api.SiteWideSearch.Controllers
{
    /// <summary>
    /// This controller handles requests for performaning site wide searches.
    /// </summary>
    [Route("[controller]")]
    public class SearchController : Controller
    {
        // Static to limit to a single instance (can't do const for non-scalar types)
        static readonly string[] validLanguages = { "en", "es" };
        static readonly string[] validCollections = { "cgov", "doc" };

        /// <summary>
        /// Default starting offset into the list of search results.
        /// </summary>
        public const int DEFAULT_FROM_LOCATION  = 0;

        /// <summary>
        /// Default number of search results to return.
        /// </summary>
        public const int DEFAULT_QUERY_SIZE = 10;

        /// <summary>
        /// Default site filter.
        /// </summary>
        public const string DEFAULT_SITE = "all";

        private readonly ILogger<SearchController> _logger;
        private readonly ISearchQueryService _searchQueryService;

        /// <summary>
        /// Message to return for a "healthy" status.
        /// </summary>
        public const string HEALTHY_STATUS = "alive!";

        /// <summary>
        /// Message to return for an "unhealthy" status.
        /// </summary>
        public const string UNHEALTHY_STATUS = "Service not healthy.";

        /// <summary>
        /// Creates a new instance of a Search Controller
        /// </summary>
        /// <param name="logger">An instance of a ILogger to use for logging messages.</param>
        /// <param name="service">Instance of the query service.</param>
        public SearchController(
            ILogger<SearchController> logger,
            ISearchQueryService service)
        {
            _logger = logger;
            _searchQueryService = service;
        }

        // GET search/cgov/en/lung+cancer
        /// <summary>
        /// Gets the results of a search
        /// </summary>
        /// <param name="collection">The search collection/strategy to use.  This defines the ES template to use.</param>
        /// <param name="language">The language to use. Only "en" and "es" are currently supported.</param>
        /// <param name="term">The search term to search for</param>
        /// <param name="from">The offset of results to retrieve</param>
        /// <param name="size">The number of items to retrieve</param>
        /// <param name="site">An optional parameter used to limit the number of items returned based on site.</param>
        /// <returns>A SiteWideSearchResults collection object</returns>
        [HttpGet("{collection}/{language}/{*term}")]
        public async Task<SiteWideSearchResults> Get(
            string collection,
            string language,
            string term = null,
            [FromQuery] int from = DEFAULT_FROM_LOCATION,
            [FromQuery] int size = DEFAULT_QUERY_SIZE,
            [FromQuery] string site = DEFAULT_SITE
            )
        {

            if (string.IsNullOrWhiteSpace(collection))
                throw new APIErrorException(400, "You must supply a collection name");

            if(!validCollections.Contains(collection))
                throw new APIErrorException(400, "Not a valid collection.");

            if (!validLanguages.Contains(language))
                throw new APIErrorException(400, "Not a valid language code.");

            // The catch-all parameter can match the empty string, effectively giving us
            // the {collection}/{language} route as well. We want to handle searches
            // for "nothing" by returning nothing, no need to invoke Elasticsearch.
            if(string.IsNullOrWhiteSpace(term))
            {
                return new SiteWideSearchResults(
                    0,
                    new SiteWideSearchResult[0]
                );
            }

            if(from < 0)
                from = DEFAULT_FROM_LOCATION;

            if(size <= 0)
                size = DEFAULT_QUERY_SIZE;

            //TODO: Access Logging with params
            //_logger.LogInformation("Search Request -- Term: {0}, Page{1} ", term, pagenum);

            // Term comes from from a catch-all parameter, so make sure it's been decoded.
            term = WebUtility.UrlDecode(term);

            try
            {
                return await _searchQueryService.Get(collection,language,term, from,size,site);
            }
            catch(Exception)
            {
                throw new APIErrorException(500, "errors occured.");
            }
        }


        /// <summary>
        /// Provides an endpoint for checking that the various services which make up the API
        /// (and thus the API itself) are all in a state where they can return information.
        /// </summary>
        /// <returns>The contents of SearchController.HEALTHY_STATUS ('alive!') if
        /// all services are running. If unhealthy services are found, APIErrorException is thrown
        /// with HTTPStatusCode set to 500.</returns>
        [HttpGet("status")]
        public async Task<string> GetStatus()
        {
            try
            {
                bool isHealthy = await _searchQueryService.GetIsHealthy();
                if (isHealthy)
                    return HEALTHY_STATUS;
                else
                    throw new APIErrorException(500, UNHEALTHY_STATUS);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health.");
                throw new APIErrorException(500, UNHEALTHY_STATUS);
            }
        }
    }
}
