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

    public interface ITokenCacheItem
    {
        /// <summary>
        /// Gets the access token.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets the authority.
        /// </summary>
        string Authority { get; }

        /// <summary>
        /// Gets the client ID.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Gets the user's displayable ID.
        /// </summary>
        string DisplayableId { get; }

        /// <summary>
        /// Gets the expiration.
        /// </summary>
        DateTimeOffset ExpiresOn { get; }

        /// <summary>
        /// Gets the family name.
        /// </summary>
        string FamilyName { get; }

        /// <summary>
        /// Gets the given name.
        /// </summary>
        string GivenName { get; }

        /// <summary>
        /// Gets the identity provider name.
        /// </summary>
        string IdentityProvider { get; }

        /// <summary>
        /// Gets the entire ID token if returned by the service or null if no ID token is returned.
        /// </summary>
        string IdToken { get; }

        /// <summary>
        /// Gets the inner <see cref="TokenCacheItem"/>.
        /// </summary>
        TokenCacheItem InnerCacheItem { get; }

        /// <summary>
        /// Gets a value indicating whether or not the refresh token applies to multiple resources.
        /// </summary>
        bool IsMultipleResourceRefreshToken { get; }

        /// <summary>
        /// Gets the refresh token associated with the requested access token. Note: not
        /// all operations will return a refresh token.
        /// </summary>
        string RefreshToken { get; }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        string Resource { get; }

        /// <summary>
        /// Get's the user's tenant ID.
        /// </summary>
        string TenantId { get; }

        /// <summary>
        /// Gets the user's unique ID.
        /// </summary>
        string UniqueId { get; }
    }
}