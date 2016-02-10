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

namespace Test.OneDriveSdk.WindowsForms.Authentication
{
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.OneDrive.Sdk.WindowsForms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;
    using OneDriveSdk.Mocks;

    [TestClass]
    public class BusinessClientExtensionsTests
    {
        protected const string serviceEndpointUri = "https://localhost";
        protected const string serviceResourceId = "https://localhost/resource/";

        protected MockAuthenticationProvider authenticationProvider;
        protected MockAdalCredentialCache credentialCache;
        protected MockHttpProvider httpProvider;
        protected HttpResponseMessage httpResponseMessage;
        protected MockSerializer serializer;

        [TestInitialize]
        public void Setup()
        {
            this.credentialCache = new MockAdalCredentialCache();
            this.httpResponseMessage = new HttpResponseMessage();
            this.serializer = new MockSerializer();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer.Object);

            this.authenticationProvider = new MockAuthenticationProvider();
            this.authenticationProvider.Setup(provider => provider.AuthenticateAsync()).Returns(Task.FromResult(new AccountSession()));
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetAuthenticatedClientUsingCustomAuthenticationAsync()
        {
            var baseEndpointUrl = "https://resource/";

            var client = await BusinessClientExtensions.GetAuthenticatedClientUsingCustomAuthenticationAsync(
                baseEndpointUrl,
                this.authenticationProvider.Object,
                this.httpProvider.Object) as OneDriveClient;

            var clientServiceInfoProvider = client.serviceInfoProvider as ServiceInfoProvider;

            Assert.IsNotNull(clientServiceInfoProvider, "Unexpected service info provider initialized for client.");
            Assert.AreEqual(this.authenticationProvider.Object, clientServiceInfoProvider.AuthenticationProvider, "Unexpected authentication provider set.");
            Assert.AreEqual(this.httpProvider.Object, client.HttpProvider, "Unexpected HTTP provider set.");
            Assert.IsNull(client.credentialCache, "Unexpected credential cache set.");

            Assert.AreEqual(
                string.Format(
                    Constants.Authentication.OneDriveBusinessBaseUrlFormatString,
                    baseEndpointUrl,
                    "v2.0"),
                client.BaseUrl,
                "Unexpected base service URL initialized.");

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public void GetClient()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";
            var userId = "userId";
            
            var client = BusinessClientExtensions.GetClient(
                new BusinessAppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryReturnUrl = returnUrl,
                    ActiveDirectoryServiceResource = serviceResourceId,
                },
                userId,
                this.credentialCache.Object,
                this.httpProvider.Object) as OneDriveClient;

            var clientServiceInfoProvider = client.serviceInfoProvider as ServiceInfoProvider;

            Assert.IsNotNull(clientServiceInfoProvider, "Unexpected service info provider initialized for client.");
            Assert.AreEqual(userId, clientServiceInfoProvider.UserSignInName, "Unexpected user sign-in name set.");
            Assert.AreEqual(this.httpProvider.Object, client.HttpProvider, "Unexpected HTTP provider set.");
            Assert.AreEqual(this.credentialCache.Object, client.credentialCache, "Unexpected credential cache set.");
        }

        [TestMethod]
        public void GetWebClientUsingAppOnlyAuthentication()
        {
            var appId = "appId";
            var siteId = "site_id";
            var tenant = "tenant";

            var clientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");

            var client = BusinessClientExtensions.GetWebClientUsingAppOnlyAuthentication(
                new BusinessAppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryClientCertificate = clientCertificate,
                    ActiveDirectoryServiceResource = serviceResourceId,
                },
                siteId,
                tenant,
                this.credentialCache.Object,
                this.httpProvider.Object);
        }

        [TestMethod]
        public void GetClientUsingAppOnlyAuthentication_InitializeDefaults()
        {
            var appId = "appId";
            var siteId = "site_id";
            var tenant = "tenant";

            var clientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");

            var client = BusinessClientExtensions.GetWebClientUsingAppOnlyAuthentication(
                new BusinessAppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryClientCertificate = clientCertificate,
                    ActiveDirectoryServiceResource = serviceResourceId,
                },
                siteId,
                tenant,
                credentialCache: null,
                httpProvider: null) as OneDriveClient;

            var adalAppConfig = client.appConfig as BusinessAppConfig;

            Assert.IsNotNull(adalAppConfig, "Unexpected app configuration initialized.");
            Assert.AreEqual(appId, adalAppConfig.ActiveDirectoryAppId, "Unexpected app ID initialized.");
            Assert.AreEqual(clientCertificate, adalAppConfig.ActiveDirectoryClientCertificate, "Unexpected client certificate initialized.");
            Assert.AreEqual(siteId, adalAppConfig.ActiveDirectorySiteId, "Unexpected site ID initialized.");
            Assert.AreEqual(
                string.Format(
                    Constants.Authentication.ActiveDirectoryAuthenticationServiceUrlFormatString,
                    tenant),
                adalAppConfig.ActiveDirectoryAuthenticationServiceUrl,
                "Unexpected authentication service URL initialized.");

            Assert.IsNotNull(client.credentialCache, "Credential cache not initialized.");
            Assert.IsInstanceOfType(client.credentialCache, typeof(AdalCredentialCache), "Unexpected credential cache initialized.");

            Assert.IsNotNull(client.HttpProvider, "HTTP provider not initialized.");
            Assert.IsInstanceOfType(client.HttpProvider, typeof(HttpProvider), "Unexpected HTTP provider initialized.");

            Assert.IsNotNull(client.serviceInfoProvider, "Service info provider not initialized.");
            Assert.IsInstanceOfType(client.serviceInfoProvider, typeof(AdalAppOnlyServiceInfoProvider), "Unexpected service info provider initialized.");
        }
    }
}
