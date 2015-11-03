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

namespace Test.OneDriveSdk.Authentication
{
    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MicrosoftAccountServiceInfoTests
    {
        [TestMethod]
        public void VerifyClassInitialization()
        {
            var serviceInfo = new MicrosoftAccountServiceInfo();

            Assert.AreEqual(AccountType.MicrosoftAccount, serviceInfo.AccountType, "Unexpected account type.");
            Assert.AreEqual(Constants.Authentication.MicrosoftAccountAuthenticationServiceUrl, serviceInfo.AuthenticationServiceUrl, "Unexpected authentication service URL.");
            Assert.AreEqual(string.Format(Constants.Authentication.OneDriveConsumerBaseUrlFormatString, "v1.0"), serviceInfo.BaseUrl, "Unexpected base URL.");
            Assert.AreEqual(Constants.Authentication.MicrosoftAccountSignOutUrl, serviceInfo.SignOutUrl, "Unexpected sign out URL.");
            Assert.AreEqual(Constants.Authentication.MicrosoftAccountTokenServiceUrl, serviceInfo.TokenServiceUrl, "Unexpected token service URL.");
        }
    }
}
