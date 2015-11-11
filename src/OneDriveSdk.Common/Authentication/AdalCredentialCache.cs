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
        private ITokenCache tokenCache;

        /// <summary>
        /// Instantiates a new, empty <see cref="AdalCredentialCache"/>.
        /// </summary>
        public AdalCredentialCache()
            : this(null)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="AdalCredentialCache"/>.
        /// </summary>
        /// <param name="blob">The cache contents for initialization.</param>
        public AdalCredentialCache(byte[] blob)
            : base()
        {
            this.TokenCache.Deserialize(blob);
        }

        internal ITokenCache TokenCache
        {
            get
            {
                if (this.tokenCache == null)
                {
                    this.tokenCache = new TokenCacheWrapper();

                    this.tokenCache.AfterAccess = this.AfterAdalAccess;
                    this.tokenCache.BeforeAccess = this.BeforeAdalAccess;
                    this.tokenCache.BeforeWrite = this.BeforeAdalWrite;
                }

                return this.tokenCache;
            }

            set
            {
                this.tokenCache = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not the cache state has changed.
        /// </summary>
        public override bool HasStateChanged
        {
            get
            {
                return this.TokenCache.HasStateChanged;
            }
            set
            {
                this.TokenCache.HasStateChanged = value;
            }
        }

        /// <summary>
        /// Gets the contents of the cache.
        /// </summary>
        /// <returns>The cache contents.</returns>
        public override byte[] GetCacheBlob()
        {
            return this.TokenCache.Serialize();
        }

        /// <summary>
        /// Initializes the cache from the specified contents.
        /// </summary>
        /// <param name="cacheBytes">The cache contents.</param>
        public override void InitializeCacheFromBlob(byte[] cacheBytes)
        {
            this.TokenCache.Deserialize(cacheBytes);
        }

        /// <summary>
        /// Clears the cache contents.
        /// </summary>
        public override void Clear()
        {
            // ADAL caching doesn't notify the delegates of access on Clear(). Call them explicitly
            // for consistency with CredentialCache behavior since the cache is being accessed and written.
            var cacheNotificationArgs = new CredentialCacheNotificationArgs { CredentialCache = this };

            this.OnBeforeAccess(cacheNotificationArgs);
            this.OnBeforeWrite(cacheNotificationArgs);

            this.TokenCache.Clear();

            this.OnAfterAccess(cacheNotificationArgs);
            this.HasStateChanged = true;
        }

        internal override void AddToCache(AccountSession accountSession)
        {
            // Let ADAL handle the caching
        }

        internal override void DeleteFromCache(AccountSession accountSession)
        {
            var cacheItems = this.TokenCache.ReadItems();

            var currentUserItems = cacheItems.Where(
                cacheItem =>
                    string.Equals(cacheItem.ClientId, accountSession.ClientId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(cacheItem.UniqueId, accountSession.UserId, StringComparison.OrdinalIgnoreCase));

            if (currentUserItems != null)
            {
                foreach (var item in currentUserItems)
                {
                    this.TokenCache.DeleteItem(item);
                }
            }
        }

        internal override AccountSession GetResultFromCache(AccountType accountType, string clientId, string userId)
        {
            // Let ADAL handle the caching
            return null;
        }

        private void AfterAdalAccess(TokenCacheNotificationArgs args)
        {
            this.OnAfterAccess(new CredentialCacheNotificationArgs { CredentialCache = this });
        }

        private void BeforeAdalAccess(TokenCacheNotificationArgs args)
        {
            this.OnBeforeAccess(new CredentialCacheNotificationArgs { CredentialCache = this });
        }

        private void BeforeAdalWrite(TokenCacheNotificationArgs args)
        {
            this.OnBeforeWrite(new CredentialCacheNotificationArgs { CredentialCache = this });
        }
    }
}
