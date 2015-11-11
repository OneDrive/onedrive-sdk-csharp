// ------------------------------------------------------------------------------
//  Copyright (c) 2015 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

namespace Test.OneDriveSdk.Requests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BaseRequestTests : RequestTestBase
    {
        [TestMethod]
        public void BaseRequest_InitializeWithQueryStringAndOptions()
        {
            var baseUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id";
            var requestUrl = baseUrl + "?key=value";

            var options = new List<Option>
            {
                new QueryOption("key2", "value2"),
                new HeaderOption("header", "value"),
            };

            var baseRequest = new BaseRequest(requestUrl, this.oneDriveClient, options);

            Assert.AreEqual(new Uri(baseUrl), new Uri(baseRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(2, baseRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.IsTrue(baseRequest.QueryOptions[0].Name.Equals("key") && baseRequest.QueryOptions[0].Value.Equals("value"), "Unexpected first query option.");
            Assert.IsTrue(baseRequest.QueryOptions[1].Name.Equals("key2") && baseRequest.QueryOptions[1].Value.Equals("value2"), "Unexpected second query option.");
            Assert.AreEqual(1, baseRequest.Headers.Count, "Unexpected number of header options.");
            Assert.IsTrue(baseRequest.Headers[0].Name.Equals("header") && baseRequest.Headers[0].Value.Equals("value"), "Unexpected header option.");
        }

        [TestMethod]
        public void BaseRequest_GetWebRequestWithHeadersAndQueryOptions()
        {
            var requestUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id";

            var options = new List<Option>
            {
                new HeaderOption("header1", "value1"),
                new HeaderOption("header2", "value2"),
                new QueryOption("query1", "value1"),
                new QueryOption("query2", "value2"),
            };

            var baseRequest = new BaseRequest(requestUrl, this.oneDriveClient, options) { Method = "PUT" };

            var httpRequestMessage = baseRequest.GetHttpRequestMessage();
            Assert.AreEqual(HttpMethod.Put, httpRequestMessage.Method, "Unexpected HTTP method in request.");
            Assert.AreEqual(requestUrl + "?query1=value1&query2=value2",
                httpRequestMessage.RequestUri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.Unescaped),
                "Unexpected base URL in request.");
            Assert.AreEqual("value1", httpRequestMessage.Headers.GetValues("header1").First(), "Unexpected first header in request.");
            Assert.AreEqual("value2", httpRequestMessage.Headers.GetValues("header2").First(), "Unexpected second header in request.");

            var expectedVersionNumber = typeof(BaseRequest).GetTypeInfo().Assembly.GetName().Version;
            Assert.AreEqual(
                string.Format(Constants.Headers.SdkVersionHeaderValue, expectedVersionNumber),
                httpRequestMessage.Headers.GetValues(Constants.Headers.ConsumerSdkVersionHeaderName).First(), "Unexpected request stats header.");
        }

        [TestMethod]
        public void BaseRequest_GetWebRequestNoOptions()
        {
            var requestUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id";

            var baseRequest = new BaseRequest(requestUrl, this.oneDriveClient) { Method = "DELETE" };

            var httpRequestMessage = baseRequest.GetHttpRequestMessage();
            Assert.AreEqual(HttpMethod.Delete, httpRequestMessage.Method, "Unexpected HTTP method in request.");
            Assert.AreEqual(requestUrl,
                httpRequestMessage.RequestUri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.Unescaped),
                "Unexpected base URL in request.");
            Assert.AreEqual(1, httpRequestMessage.Headers.Count(), "Unexpected headers in request.");

            var expectedVersionNumber = typeof(BaseRequest).GetTypeInfo().Assembly.GetName().Version;
            Assert.AreEqual(
                string.Format(Constants.Headers.SdkVersionHeaderValue, expectedVersionNumber),
                httpRequestMessage.Headers.GetValues(Constants.Headers.ConsumerSdkVersionHeaderName).First(), "Unexpected request stats header.");
        }

        [TestMethod]
        public async Task BaseRequest_SendAsync()
        {
            var requestUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id";

            var baseRequest = new BaseRequest(requestUrl, this.oneDriveClient) { ContentType = "application/json" };

            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request =>
                                string.Equals(request.Content.Headers.ContentType.ToString(), "application/json")
                               && request.RequestUri.ToString().Equals(requestUrl))))
                        .Returns(Task.FromResult(httpResponseMessage));

                var expectedResponseItem = new Item { Id = "id" };
                this.serializer.Setup(
                    serializer => serializer.SerializeObject(It.IsAny<string>()))
                    .Returns(string.Empty);
                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<Item>(It.IsAny<string>()))
                    .Returns(expectedResponseItem);

                var responseItem = await baseRequest.SendAsync<Item>("string");

                Assert.IsNotNull(responseItem, "Item not returned.");
                Assert.AreEqual(expectedResponseItem.Id, responseItem.Id, "Unexpected item ID.");

                this.authenticationProvider.Verify(provider => provider.AppendAuthHeaderAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task BaseRequest_SendAsync_ClientNotAuthenticated()
        {
            var client = new OneDriveClient(new AppConfig());

            var baseRequest = new BaseRequest("https://localhost", client);

            try
            {
                await baseRequest.SendAsync<Item>("string");
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.InvalidRequest.ToString(), exception.Error.Code, "Unexpected error code.");
                Assert.AreEqual("The client must be authenticated before sending a request.", exception.Error.Message, "Unexpected error message.");
                throw;
            }
        }
    }
}
