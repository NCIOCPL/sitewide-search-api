using System;
using System.IO;
using Xunit;

using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json.Linq;

using NCI.OCPL.Api.Common.Testing;

namespace NCI.OCPL.Api.SiteWideSearch.Services
{
    public class SitewideSearchQueryBuilder_Test
    {

        [Theory]
        [InlineData(typeof(ESCGovEnglishSitewideSearchQueryBuilder), "chicken", new string[]{"all"}, "cgov-en-structure.json")]
        [InlineData(typeof(ESCGovSpanishSitewideSearchQueryBuilder), "pollo", new string[]{"all"}, "cgov-es-structure.json")]
        [InlineData(typeof(ESDocEnglishSitewideSearchQueryBuilder), "chicken", new string[]{"physics.cancer.gov"}, "doc-en-structure.json")]
        [InlineData(typeof(ESDocSpanishSitewideSearchQueryBuilder), "pollo", new string[]{"www-test-acsf.cancer.gov/rare-brain-spine-tumor/espanol"}, "doc-es-structure.json")]
        public void Structure_Is_Correct(Type builderType, string searchTerm, string[] siteFilter, string expectedFile)
        {
            JToken expected = TestingTools.GetDataFileAsJObject($"Search/QueryBuilder/{expectedFile}");

            // Instantiate the builder.
            ISiteWideSearchQueryBuilder builder =
                (ISiteWideSearchQueryBuilder)Activator.CreateInstance(builderType);

            var qcd = builder.GetQuery(searchTerm, siteFilter);

            IElasticClient client = new ElasticClient();

            string json = client.RequestResponseSerializer.SerializeToString((QueryContainer)qcd);

            SaveActualOutput(json, Path.Join(nameof(SitewideSearchQueryBuilder_Test), nameof(Structure_Is_Correct)), expectedFile);

            JToken actual = JToken.Parse(json);

            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }

        private void SaveActualOutput(string data, string testName, string fileName)
        {
            Directory.CreateDirectory(Path.Join(Directory.GetCurrentDirectory(), testName));
            string outputFile = Path.Join(Directory.GetCurrentDirectory(), testName, $"actual-{fileName}");
            using(StreamWriter sw = File.CreateText(outputFile))
            {
                sw.Write(data);
            }
        }

    }
}