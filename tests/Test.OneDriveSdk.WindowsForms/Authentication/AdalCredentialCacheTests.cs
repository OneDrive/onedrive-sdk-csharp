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
    using System.Collections.Generic;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class AdalCredentialCacheTests
    {
        private bool afterAccessCalled;
        private bool beforeAccessCalled;
        private bool beforeWriteCalled;

        private AdalCredentialCache credentialCache;
        private MockTokenCache tokenCache;

        [TestInitialize]
        public void Setup()
        {
            this.tokenCache = new MockTokenCache();
            this.credentialCache = new AdalCredentialCache
            {
                TokenCache = tokenCache.Object,
            };
        }

        [TestMethod]
        public void AddToCache()
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

            // Adding items to the cache is handled by ADAL. Verify the delegates are not called
            // for AddToCache since the method is a no-op for AdalCredentialCache.
            Assert.IsFalse(this.afterAccessCalled, "AfterAccess called.");
            Assert.IsFalse(this.beforeAccessCalled, "BeforeAccess called.");
            Assert.IsFalse(this.beforeWriteCalled, "BeforeWrite called.");

            Assert.IsFalse(this.credentialCache.HasStateChanged, "State changed flag not set.");
        }

        [TestMethod]
        public void ClearCache()
        {
            this.credentialCache.AfterAccess = this.AfterAccess;
            this.credentialCache.BeforeAccess = this.BeforeAccess;
            this.credentialCache.BeforeWrite = this.BeforeWrite;

            this.tokenCache.ResetCalls();
            this.credentialCache.Clear();

            this.tokenCache.Verify(cache => cache.Clear(), Times.Once);

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
                UserId = "id",
            };

            var mockCacheItem1 = new MockTokenCacheItem();
            mockCacheItem1.Setup(item => item.AccessToken).Returns("token 1");
            mockCacheItem1.Setup(item => item.ClientId).Returns(accountSession.ClientId);
            mockCacheItem1.Setup(item => item.UniqueId).Returns(accountSession.UserId);

            var mockCacheItem2 = new MockTokenCacheItem();
            mockCacheItem2.Setup(item => item.AccessToken).Returns("token 2");
            mockCacheItem2.Setup(item => item.ClientId).Returns(accountSession.ClientId);

            var mockCacheItem3 = new MockTokenCacheItem();
            mockCacheItem3.Setup(item => item.AccessToken).Returns("token 3");
            mockCacheItem3.Setup(item => item.UniqueId).Returns(accountSession.UserId);

            this.tokenCache.Setup(cache => cache.ReadItems())
                .Returns(new List<ITokenCacheItem>
                {
                    mockCacheItem1.Object,
                    mockCacheItem2.Object,
                    mockCacheItem3.Object
                });

            this.tokenCache.Setup(cache => cache.DeleteItem(
                It.Is<ITokenCacheItem>(item => string.Equals(accountSession.ClientId, item.ClientId))));

            this.tokenCache.ResetCalls();

            this.credentialCache.DeleteFromCache(accountSession);

            this.tokenCache.Verify(cache => 
                cache.DeleteItem(
                    It.Is<ITokenCacheItem>(item => string.Equals("token 1", item.AccessToken))),
                Times.Once, "DeleteItem not called with expected item.");
        }

        [TestMethod]
        public void GetCacheBlob()
        {
            this.credentialCache.GetCacheBlob();
            
            this.tokenCache.Verify(cache => cache.Serialize(), Times.Once);
        }

        [TestMethod]
        public void GetResultFromCache()
        {
            this.credentialCache.AfterAccess = this.AfterAccess;
            this.credentialCache.BeforeAccess = this.BeforeAccess;
            this.credentialCache.BeforeWrite = this.BeforeWrite;

            this.tokenCache.ResetCalls();
            var cacheResult = this.credentialCache.GetResultFromCache(
                AccountType.None,
                /* clientId */ null,
                /* userId */ null);

            // Adding items to the cache is handled by ADAL. Verify the delegates are not called
            // for AddToCache since the method is a no-op for AdalCredentialCache.
            Assert.IsFalse(this.afterAccessCalled, "AfterAccess called.");
            Assert.IsFalse(this.beforeAccessCalled, "BeforeAccess called.");
            Assert.IsFalse(this.beforeWriteCalled, "BeforeWrite called.");
        }

        [TestMethod]
        public void InitializeCacheFromBlob()
        {
            var blob = new byte[2];
            this.credentialCache.InitializeCacheFromBlob(blob);

            this.tokenCache.Verify(cache => cache.Deserialize(blob), Times.Once);
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
