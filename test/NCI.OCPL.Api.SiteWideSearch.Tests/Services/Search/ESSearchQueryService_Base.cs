using System.IO;
using System.Text;

using Microsoft.Extensions.Options;
using Moq;

namespace NCI.OCPL.Api.SiteWideSearch.Services.Tests
{
    /// <summary>
    /// Common resources for the various test methods
    /// </summary>
    public class ESSearchQueryService_Base
    {
        ///<summary>
        /// A private getter to enrich IOptions
        ///</summary>
        protected IOptions<SearchIndexOptions> MockSearchOptions
        {
            get
            {
                Mock<IOptions<SearchIndexOptions>> apiClientOptions = new Mock<IOptions<SearchIndexOptions>>();
                apiClientOptions
                    .SetupGet(opt => opt.Value)
                    .Returns(new SearchIndexOptions()
                    {
                        AliasName = "cgov"
                    }
                );

                return apiClientOptions.Object;
            }
        }

        protected Stream MockHealthCheckResponse
        {
            get
            {
                string response = @"
{
  ""cluster_name"": ""es232_dev"",
  ""status"": ""green"",
  ""timed_out"": false,
  ""number_of_nodes"": 1,
  ""number_of_data_nodes"": 1,
  ""active_primary_shards"": 1,
  ""active_shards"": 1,
  ""relocating_shards"": 0,
  ""initializing_shards"": 0,
  ""unassigned_shards"": 0,
  ""delayed_unassigned_shards"": 0,
  ""number_of_pending_tasks"": 0,
  ""number_of_in_flight_fetch"": 0,
  ""task_max_waiting_in_queue_millis"": 0,
  ""active_shards_percent_as_number"": 100.0
}
";
                byte[] byteArray = Encoding.UTF8.GetBytes(response);
                return new MemoryStream(byteArray);
            }
        }
    }
}