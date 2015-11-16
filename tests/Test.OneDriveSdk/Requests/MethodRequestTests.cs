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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class MethodRequestTests : RequestTestBase
    {
        [TestMethod]
        public void ItemCreateLinkRequest_BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id/action.createLink");
            var createLinkRequestBuilder = this.oneDriveClient.Drive.Items["id"].CreateLink("view") as ItemCreateLinkRequestBuilder;

            Assert.IsNotNull(createLinkRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(createLinkRequestBuilder.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual("view", createLinkRequestBuilder.Type, "Unexpected token.");

            var createLinkRequest = createLinkRequestBuilder.Request() as ItemCreateLinkRequest;
            Assert.IsNotNull(createLinkRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(createLinkRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual("POST", createLinkRequest.Method, "Unexpected method.");
            Assert.IsNotNull(createLinkRequest.RequestBody, "Request body not set.");
            Assert.AreEqual("view", createLinkRequest.RequestBody.Type, "Unexpected type in body.");
        }

        [TestMethod]
        public async Task ItemCreateLinkRequest_PostAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id/action.createLink";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl))))
                    .Returns(Task.FromResult(httpResponseMessage));

                var expectedPermission = new Permission { Id = "id", Link = new SharingLink { Type = "edit" } };

                this.serializer.Setup(
                    serializer => serializer.SerializeObject(It.IsAny<ItemCreateLinkRequestBody>()))
                    .Returns("request body value");

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<Permission>(It.IsAny<string>()))
                    .Returns(expectedPermission);

                var permission = await this.oneDriveClient.Drive.Items["id"].CreateLink("edit").Request().PostAsync();

                Assert.IsNotNull(permission, "Permission not returned.");
                Assert.AreEqual(expectedPermission, permission, "Unexpected permission returned.");
            }
        }

        [TestMethod]
        public void ItemDeltaRequest_BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id/view.delta");
            var deltaRequestBuilder = this.oneDriveClient.Drive.Items["id"].Delta("token") as ItemDeltaRequestBuilder;

            Assert.IsNotNull(deltaRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(deltaRequestBuilder.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual("token", deltaRequestBuilder.Token, "Unexpected token.");

            var deltaRequest = deltaRequestBuilder.Request() as ItemDeltaRequest;
            Assert.IsNotNull(deltaRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(deltaRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual("GET", deltaRequest.Method, "Unexpected method.");
            Assert.AreEqual(1, deltaRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("token", deltaRequest.QueryOptions[0].Name, "Unexpected token option name.");
            Assert.AreEqual("token", deltaRequest.QueryOptions[0].Value, "Unexpected token option value.");
        }

        [TestMethod]
        public async Task ItemDeltaRequest_GetAsyncWithNextLink()
        {
            await this.ItemDeltaRequest_GetAsync(true);
        }

        [TestMethod]
        public async Task ItemDeltaRequest_GetAsyncWithoutNextLink()
        {
            await this.ItemDeltaRequest_GetAsync(false);
        }

        private async Task ItemDeltaRequest_GetAsync(bool includeNextLink)
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;
                
                var requestUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id/view.delta";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl))))
                    .Returns(Task.FromResult(httpResponseMessage));

                var expectedDeltaCollectionPage = new ItemDeltaCollectionPage
                {
                    new Item { Id = "id" }
                };

                var expectedDeltaResponse = new ItemDeltaCollectionResponse
                {
                    DeltaLink = "deltaLink",
                    Token = "nextToken",
                    Value = expectedDeltaCollectionPage
                };

                if (includeNextLink)
                {
                    expectedDeltaResponse.AdditionalData = new Dictionary<string, object>
                    {
                        { "@odata.nextLink", requestUrl + "/next" }
                    };
                }

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<ItemDeltaCollectionResponse>(It.IsAny<string>()))
                    .Returns(expectedDeltaResponse);

                var deltaCollectionPage = await this.oneDriveClient.Drive.Items["id"].Delta("token").Request().GetAsync();

                Assert.IsNotNull(deltaCollectionPage, "Collection page not returned.");
                Assert.AreEqual(1, deltaCollectionPage.CurrentPage.Count, "Unexpected number of items in page.");
                Assert.AreEqual("id", deltaCollectionPage.CurrentPage[0].Id, "Unexpected item ID in page.");
                Assert.AreEqual(expectedDeltaResponse.DeltaLink, deltaCollectionPage.DeltaLink, "Unexpected delta link in page.");
                Assert.AreEqual(expectedDeltaResponse.Token, deltaCollectionPage.Token, "Unexpected token in page.");

                if (includeNextLink)
                {
                    var nextPageRequest = deltaCollectionPage.NextPageRequest as ItemDeltaRequest;
                    Assert.IsNotNull(nextPageRequest, "Next page request not initialized correctly.");
                    Assert.AreEqual(new Uri(requestUrl + "/next"), new Uri(nextPageRequest.RequestUrl), "Unexpected request URL for next page request.");
                    Assert.AreEqual(expectedDeltaResponse.AdditionalData, deltaCollectionPage.AdditionalData, "Additional data not initialized correctly.");
                }
            }
        }
    }
}
