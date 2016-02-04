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
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;

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
        protected ServiceInfo serviceInfo;
        protected MockServiceInfoProvider serviceInfoProvider;

        [TestInitialize]
        public void Setup()
        {
            this.credentialCache = new MockAdalCredentialCache();
            this.httpResponseMessage = new HttpResponseMessage();
            this.serializer = new MockSerializer();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer.Object);

            this.authenticationProvider = new MockAuthenticationProvider();
            this.authenticationProvider.Setup(provider => provider.AuthenticateAsync()).Returns(Task.FromResult(new AccountSession()));

            this.serviceInfoProvider = new MockServiceInfoProvider();
            this.serviceInfo = new ActiveDirectoryServiceInfo
            {
                AppId = "12345",
                AuthenticationProvider = this.authenticationProvider.Object,
                AuthenticationServiceUrl = "https://login.live.com/authenticate",
                BaseUrl = serviceEndpointUri,
                CredentialCache = this.credentialCache.Object,
                HttpProvider = this.httpProvider.Object,
                ReturnUrl = "https://login.live.com/return",
                SignOutUrl = "https://login.live.com/signout",
                TokenServiceUrl = "https://login.live.com/token"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetAuthenticatedClient_NoSecretOrCertificate()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";

            this.SetupServiceInfoProvider(appId, null, null, returnUrl, serviceResourceId);

            var client = await BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                serviceResourceId,
                /* userId */ null,
                this.credentialCache.Object,
                this.httpProvider.Object,
                this.serviceInfoProvider.Object);


            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAuthenticatedClient_WithSecret()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";
            var clientSecret = "secret";

            this.SetupServiceInfoProvider(appId, null, clientSecret, returnUrl, serviceResourceId);

            var client = await BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                clientSecret,
                serviceResourceId,
                /* userId */ null,
                this.credentialCache.Object,
                this.httpProvider.Object,
                this.serviceInfoProvider.Object);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAuthenticatedClient_WithCertificate()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";
            var clientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");

            this.SetupServiceInfoProvider(appId, clientCertificate, null, returnUrl, serviceResourceId);

            var client = await BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                clientCertificate,
                /* clientSecret */ null,
                serviceResourceId,
                /* userId */ null,
                this.credentialCache.Object,
                this.httpProvider.Object,
                this.serviceInfoProvider.Object);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAuthenticatedClientUsingAppOnlyAuthentication()
        {
            var appId = "appId";
            var siteId = "site_id";
            var tenant = "tenant";

            var clientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");

            this.serviceInfoProvider.Setup(provider => provider.GetServiceInfo(It.Is<AdalAppConfig>(
                        config => config.ActiveDirectoryAppId.Equals(appId)
                            && config.ActiveDirectoryClientCertificate == clientCertificate
                            && string.Equals(config.ActiveDirectoryServiceResource, serviceResourceId)
                            && string.Equals(config.ActiveDirectorySiteId, siteId)
                            && string.Equals(
                                string.Format(
                                    Constants.Authentication.ActiveDirectoryAuthenticationServiceUrlFormatString,
                                    tenant),
                                config.ActiveDirectoryAuthenticationServiceUrl)),
                    this.credentialCache.Object,
                    this.httpProvider.Object,
                    ClientType.Business))
                .Returns(Task.FromResult<ServiceInfo>(
                    this.serviceInfo));

            var client = await BusinessClientExtensions.GetAuthenticatedClientUsingAppOnlyAuthenticationAsync(
                appId,
                clientCertificate,
                serviceResourceId,
                siteId,
                tenant,
                this.credentialCache.Object,
                this.httpProvider.Object,
                this.serviceInfoProvider.Object);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAuthenticatedClientUsingDiscoveryService_NoSecret()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";

            this.SetupServiceInfoProvider(appId, null, null, returnUrl, null);

            var client = await BusinessClientExtensions.GetAuthenticatedClientUsingDiscoveryServiceAsync(
                appId,
                returnUrl,
                this.serviceInfoProvider.Object,
                /* userId */ null,
                this.credentialCache.Object,
                this.httpProvider.Object);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAuthenticatedClientUsingDiscoveryService_WithSecret()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";
            var clientSecret = "secret";

            this.SetupServiceInfoProvider(appId, null, clientSecret, returnUrl, null);

            var client = await BusinessClientExtensions.GetAuthenticatedWebClientUsingDiscoveryServiceAsync(
                appId,
                returnUrl,
                clientSecret,
                this.serviceInfoProvider.Object,
                /* userId */ null,
                this.credentialCache.Object,
                this.httpProvider.Object);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public void GetClientUsingAppOnlyAuthentication_InitializeDefaults()
        {
            var appId = "appId";
            var siteId = "site_id";
            var tenant = "tenant";

            var clientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");

            var client = BusinessClientExtensions.GetClientUsingAppOnlyAuthentication(
                appId,
                clientCertificate,
                serviceResourceId,
                siteId,
                tenant,
                credentialCache: null,
                httpProvider: null,
                serviceInfoProvider: null) as OneDriveClient;

            var adalAppConfig = client.appConfig as AdalAppConfig;

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

        private void SetupServiceInfoProvider(
            string appId,
            X509Certificate2 clientCertificate,
            string clientSecret,
            string returnUrl,
            string serviceResource)
        {
            this.serviceInfoProvider.Setup(provider => provider.GetServiceInfo(It.Is<AdalAppConfig>(
                        config => config.ActiveDirectoryAppId.Equals(appId)
                            && config.ActiveDirectoryReturnUrl.Equals(returnUrl)
                            && config.ActiveDirectoryClientCertificate == clientCertificate
                            && string.Equals(config.ActiveDirectoryClientSecret, clientSecret)
                            && string.Equals(config.ActiveDirectoryServiceResource, serviceResource)
                            && (serviceResource == null || string.Equals(config.ActiveDirectoryServiceEndpointUrl, string.Format("{0}_api/v2.0", serviceResource)))),
                    this.credentialCache.Object,
                    this.httpProvider.Object,
                    ClientType.Business))
                .Returns(Task.FromResult<ServiceInfo>(
                    this.serviceInfo));
        }
    }
}
