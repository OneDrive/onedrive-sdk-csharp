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
    public class OAuthRequestStringBuilderTests
    {
        private ServiceInfo serviceInfo;
        private OAuthRequestStringBuilder oAuthRequestStringBuilder;

        [TestInitialize]
        public void Setup()
        {
            this.serviceInfo = new ServiceInfo
            {
                AppId = "12345",
                AuthenticationServiceUrl = "https://login.live.com/authenticate",
                ReturnUrl = "https://login.live.com/return",
                Scopes = new string[] { "scope1", "scope2" },
                SignOutUrl = "https://login.live.com/signout",
                TokenServiceUrl = "https://login.live.com/token",
            };

            this.oAuthRequestStringBuilder = new OAuthRequestStringBuilder(this.serviceInfo);
        }

        [TestMethod]
        public void GetCodeRedemptionRequestBody_ClientSecret()
        {
            this.serviceInfo.ClientSecret = "secret";
            var code = "code";
            var requestBodyString = this.oAuthRequestStringBuilder.GetCodeRedemptionRequestBody(code);
            Assert.IsTrue(requestBodyString.Contains("code=" + code), "Code not set correctly.");
            Assert.IsTrue(
                requestBodyString.Contains("client_secret=" + this.serviceInfo.ClientSecret),
                "Client secret not set correctly.");
        }

        [TestMethod]
        public void GetCodeRedemptionRequestBody_NoClientSecret()
        {
            var code = "code";
            var requestBodyString = this.oAuthRequestStringBuilder.GetCodeRedemptionRequestBody(code);
            Assert.IsTrue(requestBodyString.Contains("code=" + code), "Code not set correctly.");
            Assert.IsFalse(requestBodyString.Contains("client_secret"), "Client secret set.");
        }

        [TestMethod]
        public void GetRefreshTokenRequestBody_ClientSecret()
        {
            this.serviceInfo.ClientSecret = "secret";
            var token = "token";
            var requestBodyString = this.oAuthRequestStringBuilder.GetRefreshTokenRequestBody(token);
            Assert.IsTrue(requestBodyString.Contains("refresh_token=" + token), "Token not set correctly.");
            Assert.IsTrue(
                requestBodyString.Contains("client_secret=" + this.serviceInfo.ClientSecret),
                "Client secret not set correctly.");
        }

        [TestMethod]
        public void GetRefreshTokenRequestBody_NoClientSecret()
        {
            var token = "token";
            var requestBodyString = this.oAuthRequestStringBuilder.GetRefreshTokenRequestBody(token);
            Assert.IsTrue(requestBodyString.Contains("refresh_token=" + token), "Token not set correctly.");
            Assert.IsFalse(requestBodyString.Contains("client_secret"), "Client secret set.");
        }
    }
}
