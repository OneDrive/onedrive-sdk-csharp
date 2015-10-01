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
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;
    
    [TestClass]
    public class AsyncMonitorTests
    {
        private const string itemUrl = "https://localhost/item";
        private const string monitorUrl = "https://localhost/monitor";

        private ItemCopyAsyncMonitor asyncMonitor;
        private MockAuthenticationProvider authenticationProvider;
        private MockHttpProvider httpProvider;
        private HttpResponseMessage httpResponseMessage;
        private Mock<IOneDriveClient> oneDriveClient;
        private MockProgress progress;
        private MockSerializer serializer;

        [TestInitialize]
        public void Setup()
        {
            this.authenticationProvider = new MockAuthenticationProvider();
            this.serializer = new MockSerializer();

            this.httpResponseMessage = new HttpResponseMessage();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer.Object);

            this.oneDriveClient = new Mock<IOneDriveClient>(MockBehavior.Strict);
            this.oneDriveClient.SetupAllProperties();
            this.oneDriveClient.SetupGet(client => client.AuthenticationProvider).Returns(this.authenticationProvider.Object);
            this.oneDriveClient.Setup(client => client.AuthenticateAsync()).Returns(Task.FromResult(new AccountSession()));
            this.oneDriveClient.SetupGet(client => client.HttpProvider).Returns(this.httpProvider.Object);

            this.progress = new MockProgress();
            
            this.asyncMonitor = new ItemCopyAsyncMonitor(this.oneDriveClient.Object, AsyncMonitorTests.monitorUrl);
        }

        [TestCleanup]
        public void Teardown()
        {
            this.httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task PollForOperationCompletionAsync_IsCancelled()
        {
            var item = await this.asyncMonitor.CompleteOperationAsync(this.progress.Object, new CancellationToken(true));
            Assert.IsNull(item, "Operation not cancelled.");
        }

        [TestMethod]
        public async Task PollForOperationCompletionAsync_OperationCompleted()
        {
            bool called = false;
            this.progress.Setup(
                mockProgress => mockProgress.Report(
                    It.IsAny<AsyncOperationStatus>()))
                .Callback<AsyncOperationStatus>(status => this.ProgressCallback(status, out called));

            this.serializer.Setup(serializer => serializer.DeserializeObject<AsyncOperationStatus>(It.IsAny<Stream>())).Returns(new AsyncOperationStatus());
            this.serializer.Setup(serializer => serializer.DeserializeObject<Item>(It.IsAny<Stream>())).Returns(new Item { Id = "id" });
            this.oneDriveClient.SetupGet(client => client.IsAuthenticated).Returns(false);

            using (var redirectedResponseMessage = new HttpResponseMessage())
            using (var stringContent = new StringContent("content"))
            using (var redirectedStringContent = new StringContent("content"))
            {
                this.httpResponseMessage.Content = stringContent;
                this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;
                redirectedResponseMessage.Content = redirectedStringContent;

                this.httpProvider.Setup(provider =>
                    provider.SendAsync(
                        It.Is<HttpRequestMessage>(requestMessage => requestMessage.RequestUri.ToString().Equals(AsyncMonitorTests.itemUrl))))
                    .Returns(Task.FromResult(redirectedResponseMessage));

                var item = await this.asyncMonitor.CompleteOperationAsync(this.progress.Object, CancellationToken.None);

                Assert.IsTrue(called, "Progress not called");
                Assert.IsNotNull(item, "No item returned.");
                Assert.AreEqual("id", item.Id, "Unexpected item returned.");

                this.oneDriveClient.Verify(client => client.AuthenticateAsync(), Times.Exactly(2));
                this.authenticationProvider.Verify(
                    provider => provider.AppendAuthHeaderAsync(
                        It.Is<HttpRequestMessage>(message => message.RequestUri.ToString().Equals(AsyncMonitorTests.monitorUrl))),
                    Times.Once);

                this.authenticationProvider.Verify(
                    provider => provider.AppendAuthHeaderAsync(
                        It.Is<HttpRequestMessage>(message => message.RequestUri.ToString().Equals(AsyncMonitorTests.itemUrl))),
                    Times.Once);
            }
        }

        [TestMethod]
        public async Task PollForOperationCompletionAsync_OperationCancelled()
        {
            this.serializer.Setup(
                serializer => serializer.DeserializeObject<AsyncOperationStatus>(
                    It.IsAny<Stream>()))
                .Returns(new AsyncOperationStatus { Status = "cancelled" });
            this.oneDriveClient.SetupGet(client => client.IsAuthenticated).Returns(true);

            using (var stringContent = new StringContent("content"))
            {
                this.httpResponseMessage.Content = stringContent;
                this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;
                
                var item = await this.asyncMonitor.CompleteOperationAsync(this.progress.Object, CancellationToken.None);
                Assert.IsNull(item, "Unexpected item returned.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task PollForOperationCompletionAsync_OperationDeleteFailed()
        {
            this.serializer.Setup(
                serializer => serializer.DeserializeObject<AsyncOperationStatus>(
                    It.IsAny<Stream>()))
                .Returns(new AsyncOperationStatus { Status = "deleteFailed" });
            this.oneDriveClient.SetupGet(client => client.IsAuthenticated).Returns(true);

            using (var stringContent = new StringContent("content"))
            {
                this.httpResponseMessage.Content = stringContent;
                this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

                try
                {
                    await this.asyncMonitor.CompleteOperationAsync(this.progress.Object, CancellationToken.None);
                }
                catch (OneDriveException exception)
                {
                    Assert.AreEqual(OneDriveErrorCode.GeneralException.ToString(), exception.Error.Code, "Unexpected error code.");
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task PollForOperationCompletionAsync_OperationFailed()
        {
            this.serializer.Setup(
                serializer => serializer.DeserializeObject<AsyncOperationStatus>(
                    It.IsAny<Stream>()))
                .Returns(new AsyncOperationStatus
                    {
                        AdditionalData = new Dictionary<string, object> { { "message", "message" } },
                        Status = "failed"
                    });

            this.oneDriveClient.SetupGet(client => client.IsAuthenticated).Returns(true);
            
            using (var stringContent = new StringContent("content"))
            {
                this.httpResponseMessage.Content = stringContent;
                this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

                try
                {
                    await this.asyncMonitor.CompleteOperationAsync(this.progress.Object, CancellationToken.None);
                }
                catch (OneDriveException exception)
                {
                    Assert.AreEqual(OneDriveErrorCode.GeneralException.ToString(), exception.Error.Code, "Unexpected error code.");
                    Assert.AreEqual("message", exception.Error.Message, "Unexpected error message.");
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task PollForOperationCompletionAsync_OperationNull()
        {
            this.serializer.Setup(
                serializer => serializer.DeserializeObject<AsyncOperationStatus>(
                    It.IsAny<Stream>()))
                .Returns((AsyncOperationStatus)null);
            this.oneDriveClient.SetupGet(client => client.IsAuthenticated).Returns(true);

            using (var stringContent = new StringContent("content"))
            {
                this.httpResponseMessage.Content = stringContent;
                this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

                try
                {
                    await this.asyncMonitor.CompleteOperationAsync(this.progress.Object, CancellationToken.None);
                }
                catch (OneDriveException exception)
                {
                    Assert.AreEqual(OneDriveErrorCode.GeneralException.ToString(), exception.Error.Code, "Unexpected error code.");
                    throw;
                }
            }
        }

        public void ProgressCallback(AsyncOperationStatus asyncOperationStatus, out bool called)
        {
            this.httpResponseMessage.StatusCode = HttpStatusCode.OK;
            this.asyncMonitor.monitorUrl = AsyncMonitorTests.itemUrl;

            called = true;
        }
    }
}
