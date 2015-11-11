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

    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class TokenCacheItemWrapper : ITokenCacheItem
    {
        /// <summary>
        /// Instantiates a new <see cref="TokenCacheItemWrapper"/>.
        /// </summary>
        /// <param name="tokenCacheItem">The <see cref="TokenCacheItem"/> to store as the inner cache item.</param>
        public TokenCacheItemWrapper(TokenCacheItem tokenCacheItem)
        {
            this.InnerCacheItem = tokenCacheItem;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        public string AccessToken
        {
            get
            {
                return this.InnerCacheItem.AccessToken;
            }
        }
        
        /// <summary>
        /// Gets the authority.
        /// </summary>
        public string Authority
        {
            get
            {
                return this.InnerCacheItem.Authority;
            }
        }

        /// <summary>
        /// Gets the client ID.
        /// </summary>
        public string ClientId
        {
            get
            {
                return this.InnerCacheItem.ClientId;
            }
        }

        /// <summary>
        /// Gets the user's displayable ID.
        /// </summary>
        public string DisplayableId
        {
            get
            {
                return this.InnerCacheItem.DisplayableId;
            }
        }

        /// <summary>
        /// Gets the expiration.
        /// </summary>
        public DateTimeOffset ExpiresOn
        {
            get
            {
                return this.InnerCacheItem.ExpiresOn;
            }
        }

        /// <summary>
        /// Gets the family name.
        /// </summary>
        public string FamilyName
        {
            get
            {
                return this.InnerCacheItem.FamilyName;
            }
        }

        /// <summary>
        /// Gets the given name.
        /// </summary>
        public string GivenName
        {
            get
            {
                return this.InnerCacheItem.GivenName;
            }
        }

        /// <summary>
        /// Gets the identity provider name.
        /// </summary>
        public string IdentityProvider
        {
            get
            {
                return this.InnerCacheItem.IdentityProvider;
            }
        }

        /// <summary>
        /// Gets the entire ID token if returned by the service or null if no ID token is returned.
        /// </summary>
        public string IdToken
        {
            get
            {
                return this.InnerCacheItem.IdToken;
            }
        }

        /// <summary>
        /// Gets the inner <see cref="TokenCacheItem"/>.
        /// </summary>
        public TokenCacheItem InnerCacheItem { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the refresh token applies to multiple resources.
        /// </summary>
        public bool IsMultipleResourceRefreshToken
        {
            get
            {
                return this.InnerCacheItem.IsMultipleResourceRefreshToken;
            }
        }

        /// <summary>
        /// Gets the refresh token associated with the requested access token. Note: not
        /// all operations will return a refresh token.
        /// </summary>
        public string RefreshToken
        {
            get
            {
                return this.InnerCacheItem.RefreshToken;
            }
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        public string Resource
        {
            get
            {
                return this.InnerCacheItem.Resource;
            }
        }

        /// <summary>
        /// Get's the user's tenant ID.
        /// </summary>
        public string TenantId
        {
            get
            {
                return this.InnerCacheItem.TenantId;
            }
        }

        /// <summary>
        /// Gets the user's unique ID.
        /// </summary>
        public string UniqueId
        {
            get
            {
                return this.InnerCacheItem.UniqueId;
            }
        }
    }
}
