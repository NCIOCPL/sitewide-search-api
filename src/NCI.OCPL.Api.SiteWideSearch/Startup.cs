using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.SiteWideSearch.Services;

namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Defines the configuration for the Sitewide Search API.
    /// </summary>
    public class Startup : NciStartupBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NCI.OCPL.Api.SiteWideSearch.Startup"/> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public Startup(IConfiguration configuration)
            : base(configuration) { }



        /*****************************
         * ConfigureServices methods *
         *****************************/

        /// <summary>
        /// Adds the configuration mappings.
        /// </summary>
        /// <param name="services">Services.</param>
        protected override void AddAdditionalConfigurationMappings(IServiceCollection services)
        {
        }

        /// <summary>
        /// Adds in the application specific services
        /// </summary>
        /// <param name="services">Services.</param>
        protected override void AddAppServices(IServiceCollection services)
        {
            services.Configure<SearchIndexOptions>(Configuration.GetSection("SearchIndexOptions"));
            services.Configure<AutosuggestIndexOptions>(Configuration.GetSection("AutosuggestIndexOptions"));

            services.AddTransient<IAutosuggestQueryService, ESAutosuggestQueryService>();
            services.AddTransient<ISearchQueryService, ESSearchQueryService>();
        }


        /*****************************
         *     Configure methods     *
         *****************************/

        /// <summary>
        /// Configure the specified app and env.
        /// </summary>
        /// <returns>The configure.</returns>
        /// <param name="app">App.</param>
        /// <param name="env">Env.</param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        protected override void ConfigureAppSpecific(IApplicationBuilder app, IWebHostEnvironment env)
        {
            return;
        }

    }
}
