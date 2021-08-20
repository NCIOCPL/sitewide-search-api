using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nest;
using Elasticsearch.Net;
using System.Text;
using NSwag.AspNetCore;
using System.Reflection;
using NJsonSchema;

using NCI.OCPL.Api.Common;


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
