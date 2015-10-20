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

namespace Microsoft.OneDrive.Sdk
{
    using System;
    using System.Linq;

    using IdentityModel.Clients.ActiveDirectory;

    public class AdalCredentialCache : CredentialCache
    {
        private TokenCache tokenCache;

        public AdalCredentialCache()
            : this(null)
        {
        }

        public AdalCredentialCache(byte[] blob)
            : base()
        {
            this.TokenCache.Deserialize(blob);
        }

        internal TokenCache TokenCache
        {
            get
            {
                if (this.tokenCache == null)
                {
                    this.tokenCache = new TokenCache();

                    this.tokenCache.AfterAccess = this.AfterAdalAccess;
                    this.tokenCache.BeforeAccess = this.BeforeAdalAccess;
                    this.tokenCache.BeforeWrite = this.BeforeAdalWrite;
                }

                return this.tokenCache;
            }
        }

        public override bool HasStateChanged
        {
            get
            {
                return this.tokenCache.HasStateChanged;
            }
            set
            {
                this.tokenCache.HasStateChanged = value;
            }
        }

        public override byte[] GetCacheBlob()
        {
            if (this.tokenCache != null)
            {
                return this.tokenCache.Serialize();
            }

            return null;
        }

        public override void InitializeCacheFromBlob(byte[] cacheBytes)
        {
            if (this.tokenCache != null)
            {
                this.tokenCache.Deserialize(cacheBytes);
            }
        }

        public override void Clear()
        {
            this.tokenCache.Clear();
        }

        internal override void AddToCache(AccountSession accountSession)
        {
            // Let ADAL handle the caching
        }

        internal override void DeleteFromCache(AccountSession accountSession)
        {
            if (this.tokenCache != null)
            {
                var cacheItems = this.tokenCache.ReadItems();
                var currentUserItems = cacheItems.Where(
                    cacheItem =>
                        string.Equals(cacheItem.ClientId, accountSession.ClientId, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(cacheItem.UniqueId, accountSession.UserId, StringComparison.OrdinalIgnoreCase));

                if (currentUserItems != null)
                {
                    foreach (var item in currentUserItems)
                    {
                        this.tokenCache.DeleteItem(item);
                    }
                }
            }
        }

        internal override AccountSession GetResultFromCache(AccountType accountType, string clientId, string userId)
        {
            // Let ADAL handle the caching
            return null;
        }

        public void AfterAdalAccess(TokenCacheNotificationArgs args)
        {
            this.OnAfterAccess(new CredentialCacheNotificationArgs { CredentialCache = this });
        }

        public void BeforeAdalAccess(TokenCacheNotificationArgs args)
        {
            this.OnBeforeAccess(new CredentialCacheNotificationArgs { CredentialCache = this });
        }

        public void BeforeAdalWrite(TokenCacheNotificationArgs args)
        {
            this.OnBeforeWrite(new CredentialCacheNotificationArgs { CredentialCache = this });
        }
    }
}
