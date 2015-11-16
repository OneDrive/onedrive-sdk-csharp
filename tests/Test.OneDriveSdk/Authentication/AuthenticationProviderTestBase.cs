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

namespace Test.OneDriveSdk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Mocks;

    [TestClass]
    public class AuthenticationProviderTestBase
    {
        protected AuthenticationProvider authenticationProvider;
        protected MockCredentialCache credentialCache;
        protected MockHttpProvider httpProvider;
        protected HttpResponseMessage httpResponseMessage;
        protected MockSerializer serializer;
        protected ServiceInfo serviceInfo;
        protected MockWebAuthenticationUi webAuthenticationUi;

        [TestInitialize]
        public virtual void Setup()
        {
            this.httpResponseMessage = new HttpResponseMessage();
            this.credentialCache = new MockCredentialCache();
            this.serializer = new MockSerializer();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer.Object);
            this.webAuthenticationUi = new MockWebAuthenticationUi();

            this.serviceInfo = new ServiceInfo
            {
                AppId = "12345",
                AuthenticationServiceUrl = "https://login.live.com/authenticate",
                CredentialCache = this.credentialCache.Object,
                HttpProvider = this.httpProvider.Object,
                ReturnUrl = "https://login.live.com/return",
                Scopes = new string[] { "scope1", "scope2" },
                SignOutUrl = "https://login.live.com/signout",
                TokenServiceUrl = "https://login.live.com/token",
                WebAuthenticationUi = this.webAuthenticationUi.Object
            };
        }

        [TestCleanup]
        public virtual void Teardown()
        {
            this.httpResponseMessage.Dispose();
        }

        protected Task AuthenticateWithCodeFlow(AccountSession refreshedAccountSession)
        {
            var tokenResponseDictionary = new Dictionary<string, string> { { "code", "code" } };

            this.webAuthenticationUi.Setup(webUi => webUi.AuthenticateAsync(
                It.Is<Uri>(uri => uri.ToString().Contains("response_type=code")),
                It.Is<Uri>(uri => uri.ToString().Equals(this.serviceInfo.ReturnUrl))))
                .Returns(
                    Task.FromResult<IDictionary<string, string>>(tokenResponseDictionary));

            return this.AuthenticateWithRefreshToken(refreshedAccountSession);
        }

        protected async Task AuthenticateWithRefreshToken(AccountSession refreshedAccountSession)
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().Equals(this.serviceInfo.TokenServiceUrl))))
                    .Returns(Task.FromResult<HttpResponseMessage>(httpResponseMessage));

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<IDictionary<string, string>>(It.IsAny<Stream>()))
                    .Returns(new Dictionary<string, string>
                        {
                            { Constants.Authentication.AccessTokenKeyName, refreshedAccountSession.AccessToken },
                            { Constants.Authentication.RefreshTokenKeyName, refreshedAccountSession.RefreshToken },
                        });

                var accountSession = await this.authenticationProvider.AuthenticateAsync();

                Assert.IsNotNull(accountSession, "No account session returned.");
                Assert.AreEqual(
                    refreshedAccountSession.AccessToken,
                    accountSession.AccessToken,
                    "Unexpected access token returned.");
                Assert.AreEqual(
                    refreshedAccountSession.RefreshToken,
                    accountSession.RefreshToken,
                    "Unexpected refresh token returned.");
                Assert.AreEqual(
                    refreshedAccountSession.AccessToken,
                    this.authenticationProvider.CurrentAccountSession.AccessToken,
                    "Unexpected cached access token.");
                Assert.AreEqual(
                    refreshedAccountSession.RefreshToken,
                    this.authenticationProvider.CurrentAccountSession.RefreshToken,
                    "Unexpected cached refresh token.");
            }
        }
    }
}
