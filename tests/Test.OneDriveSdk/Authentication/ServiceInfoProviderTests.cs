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

namespace Test.OneDriveSdk.Authentication
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class ServiceInfoProviderTests
    {
        private AppConfig appConfig;
        private MockCredentialCache credentialCache;
        private MockHttpProvider httpProvider;
        private HttpResponseMessage httpResponseMessage;
        private ServiceInfoProvider serviceInfoProvider;
        private MockWebAuthenticationUi webAuthenticationUi;

        [TestInitialize]
        public void Setup()
        {
            this.appConfig = new AppConfig
            {
                MicrosoftAccountAppId = "12345",
                MicrosoftAccountClientSecret = "secret",
                MicrosoftAccountReturnUrl = "https://localhost/return",
                MicrosoftAccountScopes = new string[] { "scope" }
            };

            this.credentialCache = new MockCredentialCache();
            this.httpResponseMessage = new HttpResponseMessage();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage);
            this.webAuthenticationUi = new MockWebAuthenticationUi();
            this.serviceInfoProvider = new ServiceInfoProvider(this.webAuthenticationUi.Object);
        }

        [TestCleanup]
        public virtual void Teardown()
        {
            this.httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetServiceInfo()
        {
            var serviceInfo = await this.serviceInfoProvider.GetServiceInfo(
                this.appConfig,
                this.credentialCache.Object,
                this.httpProvider.Object,
                ClientType.Consumer);

            Assert.IsTrue(serviceInfo is MicrosoftAccountServiceInfo, "Unexpected service info type.");
            Assert.IsTrue(serviceInfo.AuthenticationProvider is MicrosoftAccountAuthenticationProvider, "Unexpected authentication provider type.");

            Assert.AreEqual(this.appConfig.MicrosoftAccountAppId, serviceInfo.AppId, "Unexpected app ID set.");
            Assert.AreEqual(this.credentialCache.Object, serviceInfo.CredentialCache, "Unexpected credential cache set.");
            Assert.AreEqual(this.httpProvider.Object, serviceInfo.HttpProvider, "Unexpected HTTP provider set.");
            Assert.AreEqual(this.appConfig.MicrosoftAccountClientSecret, serviceInfo.ClientSecret, "Unexpected client secret set.");
            Assert.AreEqual(this.appConfig.MicrosoftAccountReturnUrl, serviceInfo.ReturnUrl, "Unexpected return URL set.");
            Assert.AreEqual(this.appConfig.MicrosoftAccountScopes, serviceInfo.Scopes, "Unexpected scopes set.");
            Assert.AreEqual(this.credentialCache.Object, serviceInfo.CredentialCache, "Unexpected credential cache set.");
            Assert.AreEqual(this.webAuthenticationUi.Object, serviceInfo.WebAuthenticationUi, "Unexpected web UI set.");
        }

        [TestMethod]
        public async Task GetServiceInfo_AuthenticationProviderAlreadySet()
        {
            var mockAuthenticationProvider = new Mock<IAuthenticationProvider>().Object;
            this.serviceInfoProvider = new ServiceInfoProvider(mockAuthenticationProvider, this.webAuthenticationUi.Object);
            var serviceInfo = await this.serviceInfoProvider.GetServiceInfo(this.appConfig, this.credentialCache.Object, this.httpProvider.Object, ClientType.Consumer);
            
            Assert.IsFalse(serviceInfo.AuthenticationProvider is MicrosoftAccountAuthenticationProvider, "Unexpected authentication provider type.");
            Assert.AreEqual(mockAuthenticationProvider, serviceInfo.AuthenticationProvider, "Unexpected authentication provider set.");
        }
    }
}
