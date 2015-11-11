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
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class TokenCacheWrapper : ITokenCache
    {
        /// <summary>
        /// Instantiates a new <see cref="TokenCacheWrapper"/>.
        /// </summary>
        /// <param name="tokenCache">The <see cref="TokenCache"/> to use as the inner cache.</param>
        public TokenCacheWrapper(TokenCache tokenCache = null)
        {
            this.InnerTokenCache = tokenCache ?? new TokenCache();
        }

        /// <summary>
        /// Gets or sets the notification delegate for aftere accessing the cache.
        /// </summary>
        public TokenCacheNotification AfterAccess
        {
            get
            {
                return this.InnerTokenCache.AfterAccess;
            }

            set
            {
                this.InnerTokenCache.AfterAccess = value;
            }
        }

        /// <summary>
        /// Gets or sets the notification delegate for before accessing the cache.
        /// </summary>
        public TokenCacheNotification BeforeAccess
        {
            get
            {
                return this.InnerTokenCache.BeforeAccess;
            }

            set
            {
                this.InnerTokenCache.BeforeAccess = value;
            }
        }

        /// <summary>
        /// Gets or sets the notification delegate for before writing to the cache.
        /// </summary>
        public TokenCacheNotification BeforeWrite
        {
            get
            {
                return this.InnerTokenCache.BeforeWrite;
            }

            set
            {
                this.InnerTokenCache.BeforeWrite = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not the cache state has changed.
        /// </summary>
        public bool HasStateChanged
        {
            get
            {
                return this.InnerTokenCache.HasStateChanged;
            }

            set
            {
                this.InnerTokenCache.HasStateChanged = value;
            }
        }

        /// <summary>
        /// Gets the inner <see cref="TokenCache"/>.
        /// </summary>
        public TokenCache InnerTokenCache { get; private set; }

        /// <summary>
        /// Clears the cache contents.
        /// </summary>
        public void Clear()
        {
            this.InnerTokenCache.Clear();
        }

        /// <summary>
        /// Deletes the specified <see cref="ITokenCacheItem"/> from the cache.
        /// </summary>
        /// <param name="tokenCacheItem">The <see cref="ITokenCacheItem"/> to delete.</param>
        public void DeleteItem(ITokenCacheItem tokenCacheItem)
        {
            this.InnerTokenCache.DeleteItem(tokenCacheItem.InnerCacheItem);
        }

        /// <summary>
        /// Initializes the cache from the specified contents.
        /// </summary>
        /// <param name="blob">The cache contents.</param>
        public void Deserialize(byte[] blob)
        {
            this.InnerTokenCache.Deserialize(blob);
        }

        /// <summary>
        /// Returns the collection of <see cref="ITokenCacheItem"/>s in the cache.
        /// </summary>
        /// <returns>The collection of <see cref="ITokenCacheItem"/>s.</returns>
        public IEnumerable<ITokenCacheItem> ReadItems()
        {
            var cacheItems = this.InnerTokenCache.ReadItems();

            if (cacheItems != null)
            {
                return cacheItems.Select(cacheItem => new TokenCacheItemWrapper(cacheItem));
            }

            return null;
        }

        /// <summary>
        /// Gets the contents of the cache.
        /// </summary>
        /// <returns>The cache contents.</returns>
        public byte[] Serialize()
        {
            return this.InnerTokenCache.Serialize();
        }
    }
}
