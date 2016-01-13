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
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class HttpProviderTests
    {
        private HttpClient httpClient;
        private HttpProvider httpProvider;
        private MockSerializer serializer = new MockSerializer();
        private TestHttpMessageHandler testHttpMessageHandler;

        [TestInitialize]
        public void Setup()
        {
            this.testHttpMessageHandler = new TestHttpMessageHandler();
            this.httpClient = new HttpClient(this.testHttpMessageHandler, /* disposeHandler */ true);
            this.httpProvider = new HttpProvider(this.serializer.Object);
            this.httpProvider.httpClient.Dispose();
            this.httpProvider.httpClient = this.httpClient;
        }

        [TestCleanup]
        public void Teardown()
        {
            this.httpClient.Dispose();
            this.httpProvider.Dispose();
        }

        [TestMethod]
        public void HttpProvider_CustomCacheHeaderAndTimeout()
        {
            var timeout = TimeSpan.FromSeconds(200);
            var cacheHeader = new CacheControlHeaderValue();
            using (var defaultHttpProvider = new HttpProvider(null) { CacheControlHeader = cacheHeader, OverallTimeout = timeout })
            {
                Assert.IsFalse(defaultHttpProvider.httpClient.DefaultRequestHeaders.CacheControl.NoCache, "NoCache true.");
                Assert.IsFalse(defaultHttpProvider.httpClient.DefaultRequestHeaders.CacheControl.NoStore, "NoStore true.");

                Assert.AreEqual(timeout, defaultHttpProvider.httpClient.Timeout, "Unexpected default timeout set.");
                Assert.IsNotNull(defaultHttpProvider.Serializer, "Serializer not initialized.");
                Assert.IsInstanceOfType(defaultHttpProvider.Serializer, typeof(Serializer), "Unexpected serializer initialized.");
            }
        }

        [TestMethod]
        public void HttpProvider_DefaultConstructor()
        {
            using (var defaultHttpProvider = new HttpProvider())
            {
                Assert.IsTrue(defaultHttpProvider.httpClient.DefaultRequestHeaders.CacheControl.NoCache, "NoCache false.");
                Assert.IsTrue(defaultHttpProvider.httpClient.DefaultRequestHeaders.CacheControl.NoStore, "NoStore false.");

                Assert.AreEqual(TimeSpan.FromSeconds(100), defaultHttpProvider.httpClient.Timeout, "Unexpected default timeout set.");

                Assert.IsInstanceOfType(defaultHttpProvider.Serializer, typeof(Serializer), "Unexpected serializer initialized.");
            }
        }

        [TestMethod]
        public async Task SendAsync()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);

                Assert.AreEqual(httpResponseMessage, returnedResponseMessage, "Unexpected response returned.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_ClientGeneralException()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                this.httpClient.Dispose();

                var clientException = new Exception();
                this.httpClient = new HttpClient(new ExceptionHttpMessageHandler(clientException), /* disposeHandler */ true);
                this.httpProvider.httpClient = this.httpClient;

                try
                {
                    await this.httpProvider.SendRequestAsync(httpRequestMessage);
                }
                catch (OneDriveException exception)
                {
                    Assert.IsNotNull(exception.Error, "No error body returned.");
                    Assert.AreEqual(OneDriveErrorCode.GeneralException.ToString(), exception.Error.Code, "Incorrect error code returned.");
                    Assert.AreEqual("An error occurred sending the request.", exception.Error.Message, "Unexpected error message.");
                    Assert.AreEqual(clientException, exception.InnerException, "Inner exception not set.");

                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_ClientTimeout()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                this.httpClient.Dispose();

                var clientException = new TaskCanceledException();
                this.httpClient = new HttpClient(new ExceptionHttpMessageHandler(clientException), /* disposeHandler */ true);
                this.httpProvider.httpClient = this.httpClient;

                try
                {
                    await this.httpProvider.SendRequestAsync(httpRequestMessage);
                }
                catch (OneDriveException exception)
                {
                    Assert.IsNotNull(exception.Error, "No error body returned.");
                    Assert.AreEqual(OneDriveErrorCode.Timeout.ToString(), exception.Error.Code, "Incorrect error code returned.");
                    Assert.AreEqual("The request timed out.", exception.Error.Message, "Unexpected error message.");
                    Assert.AreEqual(clientException, exception.InnerException, "Inner exception not set.");

                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_InvalidRedirectResponse()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.StatusCode = HttpStatusCode.Redirect;
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                try
                {
                    var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);
                }
                catch (OneDriveException exception)
                {
                    Assert.IsNotNull(exception.Error, "Error not set in exception.");
                    Assert.AreEqual(OneDriveErrorCode.GeneralException.ToString(), exception.Error.Code, "Unexpected error code returned.");
                    Assert.AreEqual(
                        "Location header not present in redirection response.",
                        exception.Error.Message,
                        "Unexpected error message returned.");

                    throw;
                }
            }
        }

        [TestMethod]
        public async Task SendAsync_RedirectResponse_VerifyHeadersOnRedirect()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var redirectResponseMessage = new HttpResponseMessage())
            using (var finalResponseMessage = new HttpResponseMessage())
            {
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", "token");
                httpRequestMessage.Headers.Add("testHeader", "testValue");

                redirectResponseMessage.StatusCode = HttpStatusCode.Redirect;
                redirectResponseMessage.Headers.Location = new Uri("https://localhost/redirect");
                redirectResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), redirectResponseMessage);
                this.testHttpMessageHandler.AddResponseMapping(redirectResponseMessage.Headers.Location.ToString(), finalResponseMessage);

                var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);

                Assert.AreEqual(2, finalResponseMessage.RequestMessage.Headers.Count(), "Unexpected number of headers on redirect request message.");
                
                foreach (var header in httpRequestMessage.Headers)
                {
                    var expectedValues = header.Value.ToList();
                    var actualValues = finalResponseMessage.RequestMessage.Headers.GetValues(header.Key).ToList();

                    Assert.AreEqual(actualValues.Count, expectedValues.Count, "Unexpected header on redirect request message.");

                    for (var i = 0; i < expectedValues.Count; i++)
                    {
                        Assert.AreEqual(expectedValues[i], actualValues[i], "Unexpected header on redirect request message.");
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_MaxRedirects()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var redirectResponseMessage = new HttpResponseMessage())
            using (var tooManyRedirectsResponseMessage = new HttpResponseMessage())
            {
                redirectResponseMessage.StatusCode = HttpStatusCode.Redirect;
                redirectResponseMessage.Headers.Location = new Uri("https://localhost/redirect");
                tooManyRedirectsResponseMessage.StatusCode = HttpStatusCode.Redirect;

                redirectResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), redirectResponseMessage);
                this.testHttpMessageHandler.AddResponseMapping(redirectResponseMessage.Headers.Location.ToString(), tooManyRedirectsResponseMessage);

                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.Headers.Bearer, "ticket");

                try
                {
                    await this.httpProvider.HandleRedirect(redirectResponseMessage, 5);
                }
                catch (OneDriveException exception)
                {
                    Assert.IsNotNull(exception.Error, "Error not set in exception.");
                    Assert.AreEqual(OneDriveErrorCode.TooManyRedirects.ToString(), exception.Error.Code, "Unexpected error code returned.");
                    Assert.AreEqual(
                        "More than 5 redirects encountered while sending the request.",
                        exception.Error.Message,
                        "Unexpected error message returned.");

                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_NotFoundWithoutErrorBody()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<ErrorResponse>(
                        It.IsAny<Stream>()))
                    .Returns((ErrorResponse)null);

                try
                {
                    await this.httpProvider.SendAsync(httpRequestMessage);
                }
                catch (OneDriveException exception)
                {
                    Assert.IsNotNull(exception.Error, "No error body returned.");
                    Assert.AreEqual(OneDriveErrorCode.ItemNotFound.ToString(), exception.Error.Code, "Incorrect error code returned.");
                    Assert.IsTrue(string.IsNullOrEmpty(exception.Error.Message), "Unexpected error message returned.");

                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_NotFoundWithBody()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                var notFoundErrorString = OneDriveErrorCode.ItemNotFound.ToString();

                var expectedError = new ErrorResponse
                {
                    Error = new Error
                    {
                        Code = notFoundErrorString,
                        Message = "Error message"
                    }
                };

                this.serializer.Setup(serializer => serializer.DeserializeObject<ErrorResponse>(It.IsAny<Stream>())).Returns(expectedError);

                try
                {
                    await this.httpProvider.SendAsync(httpRequestMessage);
                }
                catch (OneDriveException exception)
                {
                    Assert.IsNotNull(exception.Error, "No error body returned.");
                    Assert.AreEqual(notFoundErrorString, exception.Error.Code, "Incorrect error code returned.");
                    Assert.AreEqual("Error message", exception.Error.Message, "Unexpected error message.");

                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_CopyThrowSiteHeader()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                const string throwSite = "throw site";

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.Headers.Add(Constants.Headers.ThrowSiteHeaderName, throwSite);
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<ErrorResponse>(
                        It.IsAny<Stream>()))
                    .Returns(new ErrorResponse { Error = new Error() });

                try
                {
                    var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);
                }
                catch (OneDriveException exception)
                {
                    Assert.IsNotNull(exception.Error, "Error not set in exception.");
                    Assert.AreEqual(
                        throwSite,
                        exception.Error.ThrowSite,
                        "Unexpected error throw site returned.");

                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_CopyThrowSiteHeader_ThrowSiteAlreadyInError()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;

                const string throwSiteBodyValue = "throw site in body";
                const string throwSiteHeaderValue = "throw site in header";

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.Headers.Add(Constants.Headers.ThrowSiteHeaderName, throwSiteHeaderValue);
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<ErrorResponse>(
                        It.IsAny<Stream>()))
                    .Returns(new ErrorResponse { Error = new Error { ThrowSite = throwSiteBodyValue } });

                try
                {
                    var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);
                }
                catch (OneDriveException exception)
                {
                    Assert.IsNotNull(exception.Error, "Error not set in exception.");
                    Assert.AreEqual(
                        throwSiteBodyValue,
                        exception.Error.ThrowSite,
                        "Unexpected error throw site returned.");

                    throw;
                }
            }
        }
    }
}
