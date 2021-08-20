using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using NCI.OCPL.Api.Common;

namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Defines the start up program
    /// </summary>
    public class Program : NciApiProgramBase
    {
        /// <summary>
        /// Main program
        /// </summary>
        public static void Main(string[] args)
        {
            CreateHostBuilder<Startup>(args).Build().Run();
        }
    }
}
