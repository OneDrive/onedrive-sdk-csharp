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
        private HttpResponseMessage httpResponseMessage;
        private MockSerializer serializer = new MockSerializer();

        [TestInitialize]
        public void Setup()
        {
            this.httpResponseMessage = new HttpResponseMessage();
            this.httpClient = new HttpClient(new TestHttpMessageHandler(this.httpResponseMessage), /* disposeHandler */ true);
            this.httpProvider = new HttpProvider(this.serializer.Object);
            this.httpProvider.httpClient.Dispose();
            this.httpProvider.httpClient = this.httpClient;
        }

        [TestCleanup]
        public void Teardown()
        {
            this.httpClient.Dispose();
            this.httpResponseMessage.Dispose();
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
                Assert.IsInstanceOfType(defaultHttpProvider.Serializer, typeof(Serializer), "Unexpected serializer initilaized.");
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

                Assert.IsInstanceOfType(defaultHttpProvider.Serializer, typeof(Serializer), "Unexpected serializer initilaized.");
            }
        }

        [TestMethod]
        public async Task SendAsync()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);

                Assert.AreEqual(this.httpResponseMessage, returnedResponseMessage, "Unexpected response returned.");
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
            this.httpResponseMessage.StatusCode = HttpStatusCode.Redirect;
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                this.httpResponseMessage.RequestMessage = httpRequestMessage;

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
        [ExpectedException(typeof(OneDriveException))]
        public async Task SendAsync_MaxRedirects()
        {
            int redirectCount = 0;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Redirect;
            this.httpResponseMessage.Headers.Location = new Uri("https://localhost/redirect");
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                this.httpResponseMessage.RequestMessage = httpRequestMessage;
                
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.Headers.Bearer, "ticket");

                try
                {
                    await this.httpProvider.HandleRedirect(this.httpResponseMessage, redirectCount);
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
            {
                this.httpResponseMessage.Content = stringContent;
                this.httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

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
            {
                this.httpResponseMessage.Content = stringContent;
                this.httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;

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
    }
}
