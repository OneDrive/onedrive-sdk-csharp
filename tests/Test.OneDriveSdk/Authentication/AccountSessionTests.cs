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
    using System;
    using System.Collections.Generic;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AccountSessionTests
    {
        [TestMethod]
        public void VerifyClassInitialization()
        {
            var responseValues = new Dictionary<string, string>
            {
                { Constants.Authentication.AccessTokenKeyName, "token" },
                { Constants.Authentication.ExpiresInKeyName, "45" },
                { Constants.Authentication.ScopeKeyName, "scope1%20scope2" },
                { Constants.Authentication.UserIdKeyName, "1" },
                { Constants.Authentication.RefreshTokenKeyName, "refresh" },
            };

            var accountSession = new AccountSession(responseValues);

            // Verify the expiration time is after now and somewhere between now and 45 seconds from now.
            // This accounts for delay in initialization until now.
            var dateTimeNow = DateTimeOffset.UtcNow;
            var dateTimeDifference = accountSession.ExpiresOnUtc - DateTimeOffset.UtcNow;
            Assert.IsTrue(accountSession.ExpiresOnUtc > dateTimeNow, "Unexpected expiration returned.");
            Assert.IsTrue(dateTimeDifference.Seconds <= 45, "Unexpected expiration returned.");

            Assert.IsNull(accountSession.ClientId, "Unexpected client ID.");
            Assert.AreEqual(AccountType.None, accountSession.AccountType, "Unexpected account type.");
            Assert.AreEqual("token", accountSession.AccessToken, "Unexpected access token.");
            Assert.AreEqual("1", accountSession.UserId, "Unexpected user ID.");
            Assert.AreEqual("refresh", accountSession.RefreshToken, "Unexpected refresh token.");

            Assert.AreEqual(2, accountSession.Scopes.Length, "Unexpected number of scopes.");
            Assert.AreEqual("scope1", accountSession.Scopes[0], "Unexpected first scope.");
            Assert.AreEqual("scope2", accountSession.Scopes[1], "Unexpected second scope.");
        }

        [TestMethod]
        public void VerifyClassInitialization_SpecifyOptionalParameters()
        {
            var accountSession = new AccountSession(null, "1", AccountType.MicrosoftAccount);

            Assert.AreEqual("1", accountSession.ClientId, "Unexpected client ID.");
            Assert.AreEqual(AccountType.MicrosoftAccount, accountSession.AccountType, "Unexpected account type.");
        }
    }
}
