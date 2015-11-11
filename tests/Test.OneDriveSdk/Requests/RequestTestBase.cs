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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class RequestTestBase
    {
        protected AppConfig appConfig;
        protected MockAuthenticationProvider authenticationProvider;
        protected MockCredentialCache credentialCache;
        protected MockHttpProvider httpProvider;
        protected HttpResponseMessage httpResponseMessage;
        protected IOneDriveClient oneDriveClient;
        protected MockSerializer serializer;
        protected ServiceInfo serviceInfo;
        protected MockServiceInfoProvider serviceInfoProvider;
        protected MockWebAuthenticationUi webUi;

        [TestInitialize]
        public void Setup()
        {
            this.appConfig = new AppConfig();
            this.authenticationProvider = new MockAuthenticationProvider();
            this.authenticationProvider.Setup(provider => provider.AppendAuthHeaderAsync(It.IsAny<HttpRequestMessage>())).Returns(Task.FromResult(0));
            this.credentialCache = new MockCredentialCache();
            this.serializer = new MockSerializer();
            this.httpResponseMessage = new HttpResponseMessage();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer.Object);
            this.serviceInfo = new ServiceInfo
            {
                AuthenticationProvider = this.authenticationProvider.Object,
            };

            this.serviceInfoProvider = new MockServiceInfoProvider(this.serviceInfo);
            this.webUi = new MockWebAuthenticationUi();
            this.oneDriveClient = new OneDriveClient(
                this.appConfig,
                this.credentialCache.Object,
                this.httpProvider.Object,
                this.serviceInfoProvider.Object)
            {
                BaseUrl = string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0"),
                ServiceInfo = this.serviceInfo,
            };
        }

        [TestCleanup]
        public void Teardown()
        {
            this.httpResponseMessage.Dispose();
        }
    }
}
