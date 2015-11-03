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
    public class ThumbnailRequestTests : RequestTestBase
    {
        [TestMethod]
        public void ThumbnailContentRequest_BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id/thumbnails/0/id/content");
            var thumbnailContentRequestBuilder = this.oneDriveClient.Drive.Items["id"].Thumbnails["0"]["id"].Content as ThumbnailContentRequestBuilder;

            Assert.IsNotNull(thumbnailContentRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(thumbnailContentRequestBuilder.RequestUrl), "Unexpected request URL.");

            var thumbnailContentRequest = thumbnailContentRequestBuilder.Request() as ThumbnailContentRequest;
            Assert.IsNotNull(thumbnailContentRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(thumbnailContentRequest.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public async Task ThumbnailContentRequest_GetAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var stringContent = new StringContent("body"))
            {
                httpResponseMessage.Content = stringContent;

                var requestUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id/thumbnails/0/id/content";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl))))
                    .Returns(Task.FromResult(httpResponseMessage));

                using (var response = await this.oneDriveClient.Drive.Items["id"].Thumbnails["0"]["id"].Content.Request().GetAsync())
                {
                    Assert.IsNotNull(response, "Response stream not returned.");

                    using (var streamReader = new StreamReader(response))
                    {
                        var responseString = await streamReader.ReadToEndAsync();
                        Assert.AreEqual("body", responseString, "Unexpected response returned.");
                    }
                }
            }
        }

        [TestMethod]
        public async Task ThumbnailContentRequest_PutAsync()
        {
            using (var requestStream = new MemoryStream())
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0") + "/drive/items/id/thumbnails/0/id/content";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl))))
                    .Returns(Task.FromResult(httpResponseMessage));

                var expectedThumbnail = new Thumbnail { Url = "https://localhost" };

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<Thumbnail>(It.IsAny<string>()))
                    .Returns(expectedThumbnail);

                var responseThumbnail = await this.oneDriveClient.Drive.Items["id"].Thumbnails["0"]["id"].Content.Request().PutAsync<Thumbnail>(requestStream);

                Assert.IsNotNull(responseThumbnail, "Thumbnail not returned.");
                Assert.AreEqual(expectedThumbnail, responseThumbnail, "Unexpected thumbnail returned.");
            }
        }

        [TestMethod]
        public void ThumbnailSetExtensions_AdditionalDataNull()
        {
            var thumbnailSet = new ThumbnailSet();

            var thumbnail = thumbnailSet["custom"];

            Assert.IsNull(thumbnail, "Unexpected thumbnail returned.");
        }

        [TestMethod]
        public void ThumbnailSetExtensions_CustomThumbnail()
        {
            var expectedThumbnail = new Thumbnail { Url = "https://localhost" };
            var thumbnailSet = new ThumbnailSet
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "custom", expectedThumbnail }
                }
            };

            var thumbnail = thumbnailSet["custom"];

            Assert.IsNotNull(thumbnail, "Custom thumbnail not returned.");
            Assert.AreEqual(expectedThumbnail.Url, thumbnail.Url, "Unexpected thumbnail returned.");
        }

        [TestMethod]
        public void ThumbnailSetExtensions_CustomThumbnailNotFound()
        {
            var expectedThumbnail = new Thumbnail { Url = "https://localhost" };
            var thumbnailSet = new ThumbnailSet
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "custom", expectedThumbnail }
                }
            };

            var thumbnail = thumbnailSet["custom2"];

            Assert.IsNull(thumbnail, "Unexpected thumbnail returned.");
        }
    }
}
