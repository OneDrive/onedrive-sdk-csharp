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
    using System.Linq;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CredentialCacheTests
    {
        private bool afterAccessCalled;
        private bool beforeAccessCalled;
        private bool beforeWriteCalled;

        private CredentialCache credentialCache;
        
        [TestInitialize]
        public void Setup()
        {
            this.credentialCache = new CredentialCache();
        }

        [TestMethod]
        public void AddToCache_CacheChangeNotifications()
        {
            var accountSession = new AccountSession
            {
                AccessToken = "token",
                UserId = "1",
            };

            this.credentialCache.AfterAccess = this.AfterAccess;
            this.credentialCache.BeforeAccess = this.BeforeAccess;
            this.credentialCache.BeforeWrite = this.BeforeWrite;

            Assert.IsNotNull(this.credentialCache.AfterAccess, "AfterAccess delegate not set.");
            Assert.IsNotNull(this.credentialCache.BeforeAccess, "BeforeAccess delegate not set.");
            Assert.IsNotNull(this.credentialCache.BeforeWrite, "BeforeWrite delegate not set.");

            this.credentialCache.AddToCache(accountSession);

            Assert.IsTrue(this.afterAccessCalled, "AfterAccess not called.");
            Assert.IsTrue(this.beforeAccessCalled, "BeforeAccess not called.");
            Assert.IsTrue(this.beforeWriteCalled, "BeforeWrite not called.");

            Assert.IsTrue(this.credentialCache.HasStateChanged, "State changed flag not set.");
        }

        [TestMethod]
        public void ClearCache()
        {
            this.credentialCache.AddToCache(new AccountSession { ClientId = "1" });
            this.credentialCache.AddToCache(new AccountSession { ClientId = "2" });

            // AddToCache will set the state changed flag to true. We need to reset it
            // for validations below.
            this.credentialCache.HasStateChanged = false;

            this.credentialCache.AfterAccess = this.AfterAccess;
            this.credentialCache.BeforeAccess = this.BeforeAccess;
            this.credentialCache.BeforeWrite = this.BeforeWrite;

            this.credentialCache.Clear();

            Assert.IsTrue(this.afterAccessCalled, "AfterAccess not called.");
            Assert.IsTrue(this.beforeAccessCalled, "BeforeAccess not called.");
            Assert.IsTrue(this.beforeWriteCalled, "BeforeWrite not called.");

            Assert.IsTrue(this.credentialCache.HasStateChanged, "State changed flag not set.");

            Assert.AreEqual(0, this.credentialCache.cacheDictionary.Count, "Cache not cleared.");
        }

        [TestMethod]
        public void DeleteFromCache()
        {
            var accountSession = new AccountSession
            {
                ClientId = "1",
            };

            this.credentialCache.AddToCache(accountSession);
            this.credentialCache.AddToCache(new AccountSession { ClientId = "2" });

            // AddToCache will set the state changed flag to true. We need to reset it
            // for validations below.
            this.credentialCache.HasStateChanged = false;

            this.credentialCache.AfterAccess = this.AfterAccess;
            this.credentialCache.BeforeAccess = this.BeforeAccess;
            this.credentialCache.BeforeWrite = this.BeforeWrite;

            this.credentialCache.DeleteFromCache(accountSession);

            Assert.IsTrue(this.afterAccessCalled, "AfterAccess not called.");
            Assert.IsTrue(this.beforeAccessCalled, "BeforeAccess not called.");
            Assert.IsTrue(this.beforeWriteCalled, "BeforeWrite not called.");

            Assert.IsTrue(this.credentialCache.HasStateChanged, "State changed flag not set.");

            Assert.AreEqual(1, this.credentialCache.cacheDictionary.Count, "Wrong number of account sessions in cache.");

            Assert.AreEqual(
                "2",
                this.credentialCache.cacheDictionary.First().Value.ClientId,
                "Incorrect account session deleted.");
        }

        [TestMethod]
        public void GetResultFromCache()
        {
            var accountSession = new AccountSession
            {
                AccessToken = "token",
                AccountType = AccountType.MicrosoftAccount,
                UserId = "1",
            };

            this.credentialCache.AddToCache(accountSession);

            // AddToCache will set the state changed flag to true. We need to reset it
            // for validations below.
            this.credentialCache.HasStateChanged = false;

            this.credentialCache.AfterAccess = this.AfterAccess;
            this.credentialCache.BeforeAccess = this.BeforeAccess;
            this.credentialCache.BeforeWrite = this.BeforeWrite;

            var cacheResult = this.credentialCache.GetResultFromCache(
                AccountType.None,
                /* clientId */ null,
                accountSession.UserId);

            Assert.IsNull(cacheResult, "Unexpected account session returned.");

            Assert.IsTrue(this.afterAccessCalled, "AfterAccess not called.");
            Assert.IsTrue(this.beforeAccessCalled, "BeforeAccess not called.");
            Assert.IsFalse(this.beforeWriteCalled, "BeforeWrite called.");
            Assert.IsFalse(this.credentialCache.HasStateChanged, "State changed flag set.");

            cacheResult = this.credentialCache.GetResultFromCache(
                AccountType.MicrosoftAccount,
                "1",
                accountSession.UserId);

            Assert.IsNull(cacheResult, "Unexpected account session returned.");

            cacheResult = this.credentialCache.GetResultFromCache(
                AccountType.MicrosoftAccount,
                /* clientId */ null,
                accountSession.UserId);

            Assert.IsNotNull(cacheResult, "Account session not returned.");
            Assert.AreEqual(accountSession.AccessToken, cacheResult.AccessToken, "Unexpected access token returned.");
        }

        [TestMethod]
        public void InitializeCache_InvalidBlobSchemeVersion()
        {
            this.credentialCache.AddToCache(new AccountSession());

            using (var stream = new MemoryStream())
            using (var binaryReader = new BinaryReader(stream))
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write(-1);

                var length = (int)stream.Position;
                stream.Position = 0;

                var byteArray = binaryReader.ReadBytes(length);

                this.credentialCache.InitializeCacheFromBlob(byteArray);

                Assert.AreEqual(
                    0,
                    this.credentialCache.cacheDictionary.Count,
                    "Cache not initialized correctly for invalid blob scheme version.");
            }
        }

        [TestMethod]
        public void InitializeCache_NullBlob()
        {
            this.credentialCache.AddToCache(new AccountSession());
            this.credentialCache.InitializeCacheFromBlob(null);

            Assert.AreEqual(
                0,
                this.credentialCache.cacheDictionary.Count,
                "Cache not initialized correctly for invalid blob scheme version.");
        }

        [TestMethod]
        public void VerifyBlobSerialization()
        {
            var accountSessions = new List<AccountSession>
            {
                new AccountSession
                {
                    AccessToken = "token",
                    ClientId = "1",
                    ExpiresOnUtc = DateTimeOffset.Now,
                    RefreshToken = "refresh",
                    Scopes = new string[] { "scope1", "scope2" },
                    UserId = "1",
                },
                new AccountSession
                {
                    AccessToken = "token2",
                    ClientId = "2",
                    ExpiresOnUtc = DateTimeOffset.Now,
                    RefreshToken = "refresh2",
                    Scopes = new string[] { "scope" },
                    UserId = "2",
                    AccountType = AccountType.MicrosoftAccount,
                }
            };

            foreach (var accountSession in accountSessions)
            {
                this.credentialCache.AddToCache(accountSession);
            }

            var cacheBlob = this.credentialCache.GetCacheBlob();
            var newCredentialCache = new CredentialCache(cacheBlob);

            Assert.AreEqual(2, newCredentialCache.cacheDictionary.Count, "Unexpected number of cache entries.");

            foreach (var accountSession in accountSessions)
            {
                var accountSessionKey = new CredentialCacheKey
                {
                    AccountType = accountSession.AccountType,
                    ClientId = accountSession.ClientId,
                    UserId = accountSession.UserId,
                };

                var sessionFromCacheDictionary = newCredentialCache.cacheDictionary[accountSessionKey];

                Assert.IsNotNull(sessionFromCacheDictionary, "Unexpected account session returned.");
                Assert.AreEqual(accountSession.AccessToken, sessionFromCacheDictionary.AccessToken, "Unexpected access token returned.");
                Assert.AreEqual(accountSession.AccountType, sessionFromCacheDictionary.AccountType, "Unexpected account type returned.");
                Assert.AreEqual(accountSession.ClientId, sessionFromCacheDictionary.ClientId, "Unexpected client ID returned.");
                Assert.AreEqual(accountSession.ExpiresOnUtc, sessionFromCacheDictionary.ExpiresOnUtc, "Unexpected expiration returned.");
                Assert.AreEqual(accountSession.RefreshToken, sessionFromCacheDictionary.RefreshToken, "Unexpected refresh token returned.");
                Assert.AreEqual(accountSession.UserId, sessionFromCacheDictionary.UserId, "Unexpected access token returned.");
                Assert.AreEqual(accountSession.Scopes.Length, sessionFromCacheDictionary.Scopes.Length, "Unexpected scopes returned.");

                for (int i = 0; i < accountSession.Scopes.Length; i++)
                {
                    Assert.AreEqual(accountSession.Scopes[i], sessionFromCacheDictionary.Scopes[i], "Unexpected scope returned.");
                }
            }
        }

        [TestMethod]
        public void VerifyBlobSerialization_EmptyCache()
        {
            var cacheBlob = this.credentialCache.GetCacheBlob();

            this.credentialCache.AddToCache(new AccountSession());
            this.credentialCache.InitializeCacheFromBlob(cacheBlob);

            Assert.AreEqual(0, this.credentialCache.cacheDictionary.Count, "Unexpected number of cache entries.");
        }

        private void AfterAccess(CredentialCacheNotificationArgs args)
        {
            Assert.AreEqual(this.credentialCache, args.CredentialCache, "Unexpected cache present in args.");
            this.afterAccessCalled = true;
        }

        private void BeforeAccess(CredentialCacheNotificationArgs args)
        {
            Assert.AreEqual(this.credentialCache, args.CredentialCache, "Unexpected cache present in args.");
            this.beforeAccessCalled = true;
        }

        private void BeforeWrite(CredentialCacheNotificationArgs args)
        {
            Assert.AreEqual(this.credentialCache, args.CredentialCache, "Unexpected cache present in args.");
            this.beforeWriteCalled = true;
        }
    }
}
