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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Mocks;

    [TestClass]
    public class AuthenticationProviderTests : AuthenticationProviderTestBase
    {
        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            this.authenticationProvider = new MicrosoftAccountAuthenticationProvider(this.serviceInfo);
        }

        [TestMethod]
        public async Task AppendAuthenticationHeader()
        {
            var cachedAccountSession = new AccountSession
            {
                AccessToken = "token",
            };

            this.authenticationProvider.CurrentAccountSession = cachedAccountSession;

            using (var httpRequestMessage = new HttpRequestMessage())
            {
                await this.authenticationProvider.AppendAuthHeaderAsync(httpRequestMessage);
                Assert.AreEqual(
                    string.Format("{0} {1}", Constants.Headers.Bearer, cachedAccountSession.AccessToken),
                    httpRequestMessage.Headers.Authorization.ToString(),
                    "Unexpected authorization header set.");
            }
        }

        [TestMethod]
        public async Task AuthenticateAsync_CachedCurrentAccountSession()
        {
            var cachedAccountSession = new AccountSession
            {
                AccessToken = "token",
                ExpiresOnUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            };

            this.authenticationProvider.CurrentAccountSession = cachedAccountSession;

            var accountSession = await this.authenticationProvider.AuthenticateAsync();

            Assert.IsNotNull(accountSession, "No account session returned.");
            Assert.AreEqual(cachedAccountSession.AccessToken, accountSession.AccessToken, "Unexpected access token returned.");
            Assert.AreEqual(cachedAccountSession.ExpiresOnUtc, accountSession.ExpiresOnUtc, "Unexpected expiration returned.");
        }

        [TestMethod]
        public async Task AuthenticateAsync_RefreshToken()
        {
            var cachedAccountSession = new AccountSession
            {
                AccountType = AccountType.MicrosoftAccount,
                ClientId = "1",
                AccessToken = "token",
                ExpiresOnUtc = DateTimeOffset.UtcNow.AddMinutes(4),
                RefreshToken = "refresh",
            };

            var refreshedAccountSession = new AccountSession
            {
                AccountType = AccountType.MicrosoftAccount,
                ClientId = "1",
                AccessToken = "token2",
                RefreshToken = "refresh2",
            };

            this.authenticationProvider.CurrentAccountSession = cachedAccountSession;

            await this.AuthenticateWithRefreshToken(refreshedAccountSession);
        }

        [TestMethod]
        public void GetRefreshTokenRequestBody_ClientSecret()
        {
            this.serviceInfo.ClientSecret = "secret";
            var token = "token";
            var requestBodyString = this.authenticationProvider.GetRefreshTokenRequestBody(token);
            Assert.IsTrue(requestBodyString.Contains("refresh_token=" + token), "Token not set correctly.");
            Assert.IsTrue(
                requestBodyString.Contains("client_secret=" + this.serviceInfo.ClientSecret),
                "Client secret not set correctly.");
        }

        [TestMethod]
        public void GetRefreshTokenRequestBody_NoClientSecret()
        {
            var token = "token";
            var requestBodyString = this.authenticationProvider.GetRefreshTokenRequestBody(token);
            Assert.IsTrue(requestBodyString.Contains("refresh_token=" + token), "Token not set correctly.");
            Assert.IsFalse(requestBodyString.Contains("client_secret"), "Client secret set.");
        }
    }
}
