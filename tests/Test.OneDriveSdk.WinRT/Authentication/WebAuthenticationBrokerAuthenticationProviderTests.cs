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

namespace Test.OneDriveSdk.WinRT
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Mocks;
    using System.Collections.Generic;

    [TestClass]
    public class WebAuthenticationBrokerAuthenticationProviderTests
    {
        protected WebAuthenticationBrokerAuthenticationProvider authenticationProvider;
        protected MockCredentialCache credentialCache;
        protected ServiceInfo serviceInfo;
        protected MockWebAuthenticationUi webAuthenticationUi;

        private bool signOut;

        [TestInitialize]
        public void Setup()
        {
            this.credentialCache = new MockCredentialCache();
            this.webAuthenticationUi = new MockWebAuthenticationUi();
            this.webAuthenticationUi.OnAuthenticateAsync = this.OnAuthenticateAsync;

            this.serviceInfo = new ServiceInfo
            {
                AppId = "12345",
                AuthenticationServiceUrl = "https://login.live.com/authenticate",
                CredentialCache = this.credentialCache,
                Scopes = new string[] { "scope1", "scope2" },
                SignOutUrl = "https://login.live.com/signout",
                TokenServiceUrl = "https://login.live.com/token",
                WebAuthenticationUi = this.webAuthenticationUi
            };

            this.authenticationProvider = new WebAuthenticationBrokerAuthenticationProvider(this.serviceInfo);
        }

        [TestMethod]
        public async Task GetAccountSessionAsync_ReturnUri()
        {
            const string token = "token";

            this.serviceInfo.ReturnUrl = "https://login.live.com/returnUrl";
            this.signOut = false;
            this.webAuthenticationUi.responseValues = new Dictionary<string, string> { { Constants.Authentication.AccessTokenKeyName, token } };

            var accountSession = await this.authenticationProvider.GetAccountSessionAsync();

            Assert.IsNotNull(accountSession, "No account session returned.");
            Assert.AreEqual(token, accountSession.AccessToken, "Unexpected token returned.");
        }

        [TestMethod]
        public async Task GetAccountSessionAsync_SingleSignOn()
        {
            const string token = "token";

            this.signOut = false;
            this.webAuthenticationUi.responseValues = new Dictionary<string, string> { { Constants.Authentication.AccessTokenKeyName, token } };

            var accountSession = await this.authenticationProvider.GetAccountSessionAsync();

            Assert.IsNotNull(accountSession, "No account session returned.");
            Assert.AreEqual(token, accountSession.AccessToken, "Unexpected token returned.");
        }

        [TestMethod]
        public async Task SignOutAsync_ReturnUri()
        {
            this.serviceInfo.ReturnUrl = "https://login.live.com/returnUrl";
            this.signOut = true;
            var expectedSignOutUrl = string.Format(
                "{0}?client_id={1}&redirect_uri={2}",
                this.serviceInfo.SignOutUrl,
                this.serviceInfo.AppId,
                this.serviceInfo.ReturnUrl);

            var accountSession = new AccountSession
            {
                AccessToken = "accessToken",
                ClientId = "12345",
            };

            this.authenticationProvider.CurrentAccountSession = accountSession;

            await this.authenticationProvider.SignOutAsync();

            Assert.IsNull(this.authenticationProvider.CurrentAccountSession, "Current account session not cleared.");
            Assert.IsTrue(this.credentialCache.DeleteFromCacheCalled, "DeleteFromCache not called.");
        }

        [TestMethod]
        public async Task SignOutAsync_SingleSignOn()
        {
            this.signOut = true;
            var expectedSignOutUrl = string.Format(
                "{0}?client_id={1}&redirect_uri={2}",
                this.serviceInfo.SignOutUrl,
                this.serviceInfo.AppId,
                this.serviceInfo.ReturnUrl);

            var accountSession = new AccountSession
            {
                AccessToken = "accessToken",
                ClientId = "12345",
            };

            this.authenticationProvider.CurrentAccountSession = accountSession;

            await this.authenticationProvider.SignOutAsync();

            Assert.IsNull(this.authenticationProvider.CurrentAccountSession, "Current account session not cleared.");
            Assert.IsTrue(this.credentialCache.DeleteFromCacheCalled, "DeleteFromCache not called.");
        }

        private void OnAuthenticateAsync(Uri requestUri, Uri callbackUri)
        {
            if (string.IsNullOrEmpty(this.serviceInfo.ReturnUrl))
            {
                Assert.IsNull(callbackUri, "Unexpected callbackUri set.");
            }
            else
            {
                Assert.AreEqual(this.serviceInfo.ReturnUrl, callbackUri.ToString(), "Unexpected callbackUri set.");
            }

            if (this.signOut)
            {
                Assert.IsTrue(requestUri.ToString().StartsWith(this.serviceInfo.SignOutUrl), "Unexpected request URI.");
            }
            else
            {
                Assert.IsTrue(requestUri.ToString().StartsWith(this.serviceInfo.AuthenticationServiceUrl), "Unexpected authentication URI.");
            }
        }
    }
}
