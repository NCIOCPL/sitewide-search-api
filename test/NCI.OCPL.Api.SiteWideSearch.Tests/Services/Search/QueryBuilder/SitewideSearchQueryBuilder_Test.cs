using System;
using System.IO;
using System.Text.Json.Nodes;
using Xunit;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport.Extensions;

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
        [InlineData(typeof(ESDocEnglishSitewideSearchQueryBuilder), "chicken", new string[]{"dceg.cancer.gov","www.cancer.gov/connect-prevention-study"}, "doc-en-multisite-structure.json")]
        [InlineData(typeof(ESDocSpanishSitewideSearchQueryBuilder), "pollo", new string[]{"www.cancer.gov/pediatric-adult-rare-tumor/espanol","www.cancer.gov/rare-brain-spine-tumor/espanol"}, "doc-es-multisite-structure.json")]
        public void Structure_Is_Correct(Type builderType, string searchTerm, string[] siteFilter, string expectedFile)
        {
            JsonNode expected = TestingTools.GetDataFileAsJson($"Search/QueryBuilder/{expectedFile}");

            // Instantiate the builder.
            ISiteWideSearchQueryBuilder builder =
                (ISiteWideSearchQueryBuilder)Activator.CreateInstance(builderType);

            var qcd = builder.GetQuery(searchTerm, siteFilter);

            ElasticsearchClient client = new ElasticsearchClient();

            string json = client.RequestResponseSerializer.SerializeToString((Query)qcd);

            SaveActualOutput(json, Path.Join(nameof(SitewideSearchQueryBuilder_Test), nameof(Structure_Is_Correct)), expectedFile);

            JsonNode actual = JsonNode.Parse(json);

            Assert.True(JsonNode.DeepEquals(expected, actual));
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