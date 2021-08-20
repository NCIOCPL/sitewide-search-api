using System;
using System.IO;
using System.Reflection;

using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json.Linq;

using Moq;
using System.Collections.Generic;

namespace NCI.OCPL.Utils.Testing
{

    /// <summary>
    /// Tools for mocking elasticsearch clients
    /// </summary>
    public static class ElasticTools {


        /// <summary>
        /// This function mocks the IElasticClient.SearchTemplate method and can be used to capture
        /// the requests being made to the ElasticSearch servers.
        /// </summary>
        /// <param name="requestInspectorCallback">An Action to be called once the IElasticClient.SearchTemplate
        /// method has been called.  This should be used to store off the ISearchTemplateRequest for later
        /// comparison.
        /// </param>
        /// <param name="dataFiller">This is a callback function that is used to fill in the mocked
        /// response from a searchTemplate call.  This is generic as the caller of
        /// </param>
        /// <returns></returns>
        public static IElasticClient GetMockedSearchTemplateClient<T>(
            Action<ISearchTemplateRequest> requestInspectorCallback,
            Action<Mock<ISearchResponse<T>>> dataFiller
        ) where T : class
        {

            // Setup Mocked Response
            Mock<ISearchResponse<T>> mockResponse = new Mock<ISearchResponse<T>>();

            // Call our dataFiller to setup the results to be whatever the caller needs it
            // to be.
            if (dataFiller != null)
            {
                dataFiller(mockResponse);
            }

            // Setup the client mock.
            Mock<IElasticClient> elasticClientMock = new Mock<IElasticClient>();

            /// Mock up the Search Template Function
            elasticClientMock
                // Handle the condition where this code should run
                .Setup(
                    ec => ec.SearchTemplate(
                        It.IsAny<Func<SearchTemplateDescriptor<T>, ISearchTemplateRequest>>()
                    )
                )
                // Give a callback for the mocked signature.  This will store off the request.
                // This is a little inside baseball, but the invoking of the anon function below is taken from
                // how the Nest code will execute the search based on the above mocked call.
                // https://github.com/elastic/elasticsearch-net/blob/master/src/Nest/Search/SearchTemplate/ElasticClient-SearchTemplate.cs
                .Callback<Func<SearchTemplateDescriptor<T>,ISearchTemplateRequest>>(
                    sd => {
                        ISearchTemplateRequest savedTemplateRequest;
                        savedTemplateRequest = sd?.Invoke(new SearchTemplateDescriptor<T>());
//throw new Exception(JObject.FromObject(savedTemplateRequest).ToString());
                        //Call the callback so that the calling function can save the searchrequest
                        //for comparing once the IElasticClient.SearchTemplate function has executed.
                        if (requestInspectorCallback != null) {
                            requestInspectorCallback((ISearchTemplateRequest)savedTemplateRequest);
                        }
                    }
                )
                // Return something from our method.
                .Returns(mockResponse.Object);

            return elasticClientMock.Object;
        }


        /// <summary>
        /// Comparer for comparing SearchTemplate Requests
        /// </summary>
        public class SearchTemplateRequestComparer : IEqualityComparer<ISearchTemplateRequest>
        {
            public bool Equals(ISearchTemplateRequest x, ISearchTemplateRequest y)
            {
                // If the items are both null, or if one or the other is null, return
                // the correct response right away.
                if (x == null && y== null)
                {
                    return true;
                }
                else if (x == null || y == null)
                {
                    return false;
                }

                // ISearchTemplateRequest.File is obsolete.
                // Refactoring to remove this dependency is recorded as issue #28
                // https://github.com/NCIOCPL/sitewide-search-api/issues/28
#pragma warning disable CS0618
                //Initial test is that both objects are not null.
                bool isEqual =
                    x.Id == y.Id &&
                    x.File == y.File &&
                    x.Inline == y.Inline;
#pragma warning restore CS0618

                if (isEqual)
                {
                    foreach (KeyValuePair<string, object> pair in x.Params)
                    {
                        //If a pair in x does not exist, or is not equal to y, then we must return false.
                        bool doesContain = y.Params.ContainsKey(pair.Key) && y.Params[pair.Key].Equals(pair.Value);
                        if (!doesContain)
                        {
                            isEqual = false;
                            break;
                        }
                    }
                }

                return isEqual;
            }

            public int GetHashCode(ISearchTemplateRequest obj)
            {
                throw new NotImplementedException();
            }
        }

    }
}