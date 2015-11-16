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
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MicrosoftAccountAuthenticationProviderTests : AuthenticationProviderTestBase
    {
        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            this.authenticationProvider = new MicrosoftAccountAuthenticationProvider(this.serviceInfo);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ExpiredResultNoRefreshToken()
        {
            var cachedAccountSession = new AccountSession
            {
                AccountType = this.serviceInfo.AccountType,
                AccessToken = "token",
                ClientId = this.serviceInfo.AppId,
                ExpiresOnUtc = DateTimeOffset.UtcNow.AddMinutes(4),
                UserId = this.serviceInfo.UserId,
            };

            var refreshedAccountSession = new AccountSession
            {
                AccountType = AccountType.MicrosoftAccount,
                ClientId = "1",
                AccessToken = "token2",
            };

            this.serviceInfo.CredentialCache.AddToCache(cachedAccountSession);

            this.authenticationProvider.CurrentAccountSession = cachedAccountSession;

            await this.AuthenticateWithCodeFlow(refreshedAccountSession);

            this.credentialCache.Verify(cache => cache.OnGetResultFromCache(), Times.Once);
            this.credentialCache.Verify(cache => cache.OnDeleteFromCache(), Times.Once);
        }

        [TestMethod]
        public void GetCodeRedemptionRequestBody_ClientSecret()
        {
            this.serviceInfo.ClientSecret = "secret";
            var code = "code";
            var requestBodyString = ((MicrosoftAccountAuthenticationProvider)this.authenticationProvider).GetCodeRedemptionRequestBody(code);
            Assert.IsTrue(requestBodyString.Contains("code=" + code), "Code not set correctly.");
            Assert.IsTrue(
                requestBodyString.Contains("client_secret=" + this.serviceInfo.ClientSecret),
                "Client seceret not set correctly.");
        }

        [TestMethod]
        public void GetCodeRedemptionRequestBody_NoClientSecret()
        {
            var code = "code";
            var requestBodyString = ((MicrosoftAccountAuthenticationProvider)this.authenticationProvider).GetCodeRedemptionRequestBody(code);
            Assert.IsTrue(requestBodyString.Contains("code=" + code), "Code not set correctly.");
            Assert.IsFalse(requestBodyString.Contains("client_secret"), "Client seceret set.");
        }

        [TestMethod]
        public async Task SignOutAsync()
        {
            var expectedSignOutUrl = string.Format(
                "{0}?client_id={1}&redirect_uri={2}",
                this.serviceInfo.SignOutUrl,
                this.serviceInfo.AppId,
                this.serviceInfo.ReturnUrl);

            var accountSession = new AccountSession
            {
                AccessToken = "accessToken",
                CanSignOut = true,
                ClientId = "12345",
            };

            this.authenticationProvider.CurrentAccountSession = accountSession;

            await this.authenticationProvider.SignOutAsync();

            this.webAuthenticationUi.Verify(
                webAuthenticationUi => webAuthenticationUi.AuthenticateAsync(
                    It.Is<Uri>(uri => uri.ToString().Equals(expectedSignOutUrl)),
                    It.Is<Uri>(uri => uri.ToString().Equals(this.serviceInfo.ReturnUrl))),
                Times.Once);

            Assert.IsNull(this.authenticationProvider.CurrentAccountSession, "Current account session not cleared.");
            
            this.credentialCache.Verify(cache => cache.OnDeleteFromCache(), Times.Once);
        }
    }
}
