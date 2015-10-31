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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Mocks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    [TestClass]
    public class AdalAuthenticationProviderTests
    {
        protected AdalAuthenticationProvider authenticationProvider;
        protected MockCredentialCache credentialCache;
        protected ServiceInfo serviceInfo;
        protected MockWebAuthenticationUi webAuthenticationUi;

        private bool signOut;

        [TestInitialize]
        public void Setup()
        {
            this.credentialCache = new MockCredentialCache();
            this.webAuthenticationUi = new MockWebAuthenticationUi();

            this.serviceInfo = new ServiceInfo
            {
                AppId = "12345",
                AuthenticationServiceUrl = "https://login.live.com/authenticate",
                CredentialCache = this.credentialCache,
                ReturnUrl = "https://login.live.com/return",
                SignOutUrl = "https://login.live.com/signout",
                TokenServiceUrl = "https://login.live.com/token",
                WebAuthenticationUi = this.webAuthenticationUi
            };

            this.authenticationProvider = new AdalAuthenticationProvider(this.serviceInfo);
        }

        [TestMethod]
        public void AuthenticateResourceAsync()
        {
            //var authenticationResult = new AuthenticationResult();
           // this.authenticationProvider.authenticationContext = new MockAuthenticationContext();
        }
    }
}
