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
    /// A Controller for handling requests for SiteWideSearch Suggestions
    /// </summary>
    [Route("[controller]")]
    public class AutosuggestController : Controller
    {
        // Static to limit to a single instance (can't do const for non-scalar types)
        static readonly string[] validLanguages = {"en", "es"};

        private readonly ILogger<AutosuggestController> _logger;
        private readonly IAutosuggestQueryService _autoSuggestQueryService;

        /// <summary>
        /// Message to return for a "healthy" status.
        /// </summary>
        public const string HEALTHY_STATUS = "alive!";

        /// <summary>
        /// Message to return for an "unhealthy" status.
        /// </summary>
        public const string UNHEALTHY_STATUS = "Service not healthy.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="service">Instance of the query service.</param>
        public AutosuggestController(
            ILogger<AutosuggestController> logger,
            IAutosuggestQueryService service)
        {
            _logger = logger;
            _autoSuggestQueryService = service;
        }

        // GET autosuggset/cgov_en/lung+cancer
        /// <summary>
        /// Retrieves a collection of potential search terms based on the value passed as term.
        /// </summary>
        /// <param name="collection">The search collection/strategy to use. This defines the ES template to use.</param>
        /// <param name="language">The language to use. Only "en" and "es" are currently supported.</param>
        /// <param name="term">The search term to use as a basis for search terms</param>
        /// <param name="size">The maximum number of results to return.</param>
        /// <returns>A Suggestions collection of Suggestion objects.</returns>
        /// <remarks>
        /// Collection is of the form {sitename}_{lang_code}.  Currently, {sitename} is always "cgov" and {lang_code} may
        /// be either "en" (English) or "es" (Español).
        /// </remarks>
        [HttpGet("{collection}/{language}/{*term}")]

        public async Task<Suggestions> Get(
            string collection,
            string language,
            string term,
            [FromQuery] int size = 10
            )
        {
            if (string.IsNullOrWhiteSpace(collection))
                throw new APIErrorException(400, "You must supply a language and term");

            if(!validLanguages.Contains(language))
                throw new APIErrorException(400, "Not a valid language code.");

            if (string.IsNullOrWhiteSpace(term))
                throw new APIErrorException(400, "You must supply a search term");

            // Term comes from from a catch-all parameter, so make sure it's been decoded.
            term = WebUtility.UrlDecode(term);

            try
            {
                return await _autoSuggestQueryService.Get(collection, language,term, size);
            }
            catch (Exception)
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
                bool isHealthy = await _autoSuggestQueryService.GetIsHealthy();
                if (isHealthy)
                    return HEALTHY_STATUS;
                else
                    throw new APIErrorException(500, UNHEALTHY_STATUS);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error checking health.");
                throw new APIErrorException(500, UNHEALTHY_STATUS);
            }
        }
    }
}
