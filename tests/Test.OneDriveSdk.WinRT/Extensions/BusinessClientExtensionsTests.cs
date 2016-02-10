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

namespace Test.OneDriveSdk.WinRT.Authentication
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Mocks;
    

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
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer);

            this.authenticationProvider = new MockAuthenticationProvider(new AccountSession());
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetAuthenticatedClientAsync_AppIdRequired()
        {
            bool exceptionThrown = false;

            try
            {
                var client = await BusinessClientExtensions.GetAuthenticatedClientAsync(
                    new AppConfig
                    {
                        ActiveDirectoryReturnUrl = "https://return"
                    },
                    /* userId */ null,
                    this.credentialCache,
                    this.httpProvider);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("ActiveDirectoryAppId is required for authentication.", exception.Error.Message, "Unexpected error thrown.");

                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Expected exception not thrown.");
        }

        [TestMethod]
        public async Task GetAuthenticatedClientAsync_ReturnUrlRequired()
        {
            bool exceptionThrown = false;

            try
            {
                var client = await BusinessClientExtensions.GetAuthenticatedClientAsync(
                    new AppConfig(),
                    /* userId */ null,
                    this.credentialCache,
                    this.httpProvider);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("ActiveDirectoryReturnUrl is required for authenticating a business client.", exception.Error.Message, "Unexpected error thrown.");

                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Expected exception not thrown.");
        }

        [TestMethod]
        public async Task GetAuthenticatedClientUsingCustomAuthenticationAsync()
        {
            var baseEndpointUrl = "https://resource/";

            var client = await BusinessClientExtensions.GetAuthenticatedClientUsingCustomAuthenticationAsync(
                baseEndpointUrl,
                this.authenticationProvider,
                this.httpProvider) as OneDriveClient;

            var clientServiceInfoProvider = client.serviceInfoProvider as ServiceInfoProvider;

            Assert.IsNotNull(clientServiceInfoProvider, "Unexpected service info provider initialized for client.");
            Assert.AreEqual(this.authenticationProvider, clientServiceInfoProvider.AuthenticationProvider, "Unexpected authentication provider set.");
            Assert.AreEqual(this.httpProvider, client.HttpProvider, "Unexpected HTTP provider set.");
            Assert.IsNull(client.credentialCache, "Unexpected credential cache set.");

            Assert.AreEqual(
                string.Format(
                    Constants.Authentication.OneDriveBusinessBaseUrlFormatString,
                    baseEndpointUrl,
                    "v2.0"),
                client.BaseUrl,
                "Unexpected base service URL initialized.");
        }

        [TestMethod]
        public async Task GetAuthenticatedClientUsingCustomAuthenticationAsync_AuthenticationProviderRequired()
        {
            var baseEndpointUrl = "https://resource/";

            bool exceptionThrown = false;

            try
            {
                var client = await BusinessClientExtensions.GetAuthenticatedClientUsingCustomAuthenticationAsync(
                    baseEndpointUrl,
                    /* authenticationProvider */ null,
                    this.httpProvider);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("An authentication provider is required for a client using custom authentication.", exception.Error.Message, "Unexpected error thrown.");

                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Expected exception not thrown.");
        }

        [TestMethod]
        public async Task GetAuthenticatedClientUsingCustomAuthenticationAsync_ServiceEndpointBaseUrlRequired()
        {
            bool exceptionThrown = false;

            try
            {
                var client = await BusinessClientExtensions.GetAuthenticatedClientUsingCustomAuthenticationAsync(
                    /* serviceEndpointBaseUrl */ null,
                    this.authenticationProvider,
                    this.httpProvider);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("Service endpoint base URL is required when using custom authentication.", exception.Error.Message, "Unexpected error thrown.");

                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Expected exception not thrown.");
        }

        [TestMethod]
        public void GetClient()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";
            var userId = "userId";
            
            var client = BusinessClientExtensions.GetClient(
                new AppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryReturnUrl = returnUrl,
                    ActiveDirectoryServiceResource = serviceResourceId,
                },
                userId,
                this.credentialCache,
                this.httpProvider) as OneDriveClient;

            var clientServiceInfoProvider = client.serviceInfoProvider as ServiceInfoProvider;

            Assert.IsNotNull(clientServiceInfoProvider, "Unexpected service info provider initialized for client.");
            Assert.AreEqual(userId, clientServiceInfoProvider.UserSignInName, "Unexpected user sign-in name set.");
            Assert.AreEqual(this.httpProvider, client.HttpProvider, "Unexpected HTTP provider set.");
            Assert.AreEqual(this.credentialCache, client.credentialCache, "Unexpected credential cache set.");
        }

        [TestMethod]
        public void GetClient_InitializeDefaults()
        {
            var appId = "appId";

            var client = BusinessClientExtensions.GetClientInternal(
                new AppConfig
                {
                    ActiveDirectoryAppId = appId,
                },
                serviceInfoProvider: null,
                credentialCache: null,
                httpProvider: null) as OneDriveClient;

            Assert.IsNotNull(client.credentialCache, "Credential cache not initialized.");
            Assert.IsInstanceOfType(client.credentialCache, typeof(AdalCredentialCache), "Unexpected credential cache initialized.");

            Assert.IsNotNull(client.HttpProvider, "HTTP provider not initialized.");
            Assert.IsInstanceOfType(client.HttpProvider, typeof(HttpProvider), "Unexpected HTTP provider initialized.");

            Assert.IsNotNull(client.serviceInfoProvider, "Service info provider not initialized.");
            Assert.IsInstanceOfType(client.serviceInfoProvider, typeof(AdalServiceInfoProvider), "Unexpected service info provider initialized.");

            Assert.AreEqual(ClientType.Business, client.ClientType, "Unexpected client type set.");
        }

        [TestMethod]
        public async Task GetClientUsingCustomAuthentication_InitializeDefaults()
        {
            var baseEndpointUrl = "https://resource/";

            var client = await BusinessClientExtensions.GetAuthenticatedClientUsingCustomAuthenticationAsync(
                baseEndpointUrl,
                this.authenticationProvider) as OneDriveClient;

            var clientServiceInfoProvider = client.serviceInfoProvider as ServiceInfoProvider;

            Assert.IsNotNull(clientServiceInfoProvider, "Unexpected service info provider initialized for client.");
            Assert.AreEqual(this.authenticationProvider, clientServiceInfoProvider.AuthenticationProvider, "Unexpected authentication provider set.");
            Assert.IsInstanceOfType(client.HttpProvider, typeof(HttpProvider), "Unexpected HTTP provider set.");
            Assert.IsNull(client.credentialCache, "Unexpected credential cache set.");
            Assert.AreEqual(ClientType.Business, client.ClientType, "Unexpected client type set.");
        }
    }
}
