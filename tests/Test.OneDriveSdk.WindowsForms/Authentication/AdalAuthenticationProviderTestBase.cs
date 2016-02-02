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
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Test.OneDriveSdk.Mocks;

    public class AdalAuthenticationProviderTestBase
    {
        protected const string serviceEndpointUri = "https://localhost";
        protected const string serviceResourceId = "https://localhost/resource/";

        protected AdalAuthenticationProvider authenticationProvider;
        protected MockAdalCredentialCache credentialCache;
        protected MockHttpProvider httpProvider;
        protected HttpResponseMessage httpResponseMessage;
        protected ISerializer serializer;
        protected ServiceInfo serviceInfo;

        [TestInitialize]
        public void Setup()
        {
            this.credentialCache = new MockAdalCredentialCache();
            this.httpResponseMessage = new HttpResponseMessage();
            this.serializer = new Serializer();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer);

            this.serviceInfo = new ActiveDirectoryServiceInfo
            {
                AppId = "12345",
                AuthenticationServiceUrl = "https://login.live.com/authenticate",
                CredentialCache = this.credentialCache.Object,
                HttpProvider = this.httpProvider.Object,
                ReturnUrl = "https://login.live.com/return",
                SignOutUrl = "https://login.live.com/signout",
                TokenServiceUrl = "https://login.live.com/token"
            };

            this.authenticationProvider = new AdalAuthenticationProvider(this.serviceInfo);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.httpResponseMessage.Dispose();
        }

        public async Task AuthenticateAsync_AuthenticateWithoutDiscoveryService(
            IAuthenticationContextWrapper authenticationContextWrapper,
            IAuthenticationResult authenticationResult)
        {
            this.serviceInfo.BaseUrl = "https://localhost";
            this.serviceInfo.ServiceResource = serviceResourceId;

            this.authenticationProvider.authenticationContextWrapper = authenticationContextWrapper;

            var accountSession = await this.authenticationProvider.AuthenticateAsync();

            Assert.AreEqual(accountSession, this.authenticationProvider.CurrentAccountSession, "Account session not cached correctly.");
            Assert.AreEqual(authenticationResult.AccessToken, accountSession.AccessToken, "Unexpected access token set.");
            Assert.AreEqual(authenticationResult.AccessTokenType, accountSession.AccessTokenType, "Unexpected access token type set.");
            Assert.AreEqual(AccountType.ActiveDirectory, accountSession.AccountType, "Unexpected account type set.");
            Assert.IsTrue(accountSession.CanSignOut, "CanSignOut set to false.");
            Assert.AreEqual(this.serviceInfo.AppId, accountSession.ClientId, "Unexpected client ID set.");
            Assert.AreEqual(authenticationResult.ExpiresOn, accountSession.ExpiresOnUtc, "Unexpected expiration set.");
            Assert.IsNull(accountSession.UserId, "Unexpected user ID set.");
        }

        public async Task AuthenticateAsync_AuthenticateWithDiscoveryService(
            MockAuthenticationContextWrapper mockAuthenticationContextWrapper,
            IAuthenticationResult authenticationResult)
        {
            var accountSession = await this.AuthenticateWithDiscoveryService(mockAuthenticationContextWrapper);

            Assert.AreEqual(accountSession, this.authenticationProvider.CurrentAccountSession, "Account session not cached correctly.");
            Assert.AreEqual(serviceEndpointUri, this.serviceInfo.BaseUrl, "Base URL not set.");
            Assert.AreEqual(serviceResourceId, this.serviceInfo.ServiceResource, "Service resource not set.");
            Assert.AreEqual(authenticationResult.AccessToken, accountSession.AccessToken, "Unexpected access token set.");
            Assert.AreEqual(authenticationResult.AccessTokenType, accountSession.AccessTokenType, "Unexpected access token type set.");
            Assert.AreEqual(AccountType.ActiveDirectory, accountSession.AccountType, "Unexpected account type set.");
            Assert.IsTrue(accountSession.CanSignOut, "CanSignOut set to false.");
            Assert.AreEqual(this.serviceInfo.AppId, accountSession.ClientId, "Unexpected client ID set.");
            Assert.AreEqual(authenticationResult.ExpiresOn, accountSession.ExpiresOnUtc, "Unexpected expiration set.");
            Assert.IsNull(accountSession.UserId, "Unexpected user ID set.");
        }

        public async Task<AccountSession> AuthenticateWithDiscoveryService(
            MockAuthenticationContextWrapper mockAuthenticationContextWrapper,
            DiscoveryServiceResponse discoveryServiceResponse = null)
        {
            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("discoveryResource");

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(Constants.Authentication.ActiveDirectoryDiscoveryResource)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(Constants.Authentication.ActiveDirectoryDiscoveryResource)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.serviceInfo.ReturnUrl)),
                PromptBehavior.Auto,
                UserIdentifier.AnyUser)).Returns(mockAuthenticationResult.Object);

            if (discoveryServiceResponse == null)
            {
                discoveryServiceResponse = new DiscoveryServiceResponse
                {
                    Value = new List<DiscoveryService>
                    {
                        new DiscoveryService
                        {
                            Capability = Constants.Authentication.MyFilesCapability,
                            ServiceApiVersion = this.serviceInfo.OneDriveServiceEndpointVersion,
                            ServiceEndpointUri = serviceEndpointUri,
                            ServiceResourceId = serviceResourceId,
                        }
                    }
                };
            }

            var requestBodyString = this.serializer.SerializeObject(discoveryServiceResponse);

            AccountSession accountSession;

            using (var stringContent = new StringContent(requestBodyString))
            {
                this.httpResponseMessage.Content = stringContent;
                this.authenticationProvider.authenticationContextWrapper = mockAuthenticationContextWrapper.Object;

                accountSession = await this.authenticationProvider.AuthenticateAsync();
            }

            return accountSession;
        }
    }
}
