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
    using System.Threading.Tasks;

    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class AuthenticationContextWrapper : IAuthenticationContextWrapper
    {
        private AuthenticationContext authenticationContext;

        /// <summary>
        /// Instantiates a new <see cref="AuthenticationContext"/>.
        /// </summary>
        /// <param name="serviceUrl">The service URL for authentication.</param>
        public AuthenticationContextWrapper(string serviceUrl)
        {
            this.authenticationContext = new AuthenticationContext(serviceUrl);
        }

        /// <summary>
        /// Instantiates a new <see cref="AuthenticationContext"/>.
        /// </summary>
        /// <param name="serviceUrl">The service URL for authentication.</param>
        /// <param name="validateAuthority">Whether or not to enable address validation.</param>
        /// <param name="tokenCache">Cache for storing authentication sessions.</param>
        public AuthenticationContextWrapper(string serviceUrl, bool validateAuthority, TokenCache tokenCache)
        {
            this.authenticationContext = new AuthenticationContext(serviceUrl, validateAuthority, tokenCache);
        }

        /// <summary>
        /// Authenticates the user silently using <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, string)"/>.
        /// </summary>
        /// <param name="resource">The resource to authenticate against.</param>
        /// <param name="clientId">The client ID of the application.</param>
        /// <returns>The <see cref="IAuthenticationResult"/>.</returns>
        public async Task<IAuthenticationResult> AcquireTokenSilentAsync(string resource, string clientId)
        {
            var result = await this.authenticationContext.AcquireTokenSilentAsync(resource, clientId);

            return result == null ? null : new AuthenticationResultWrapper(result);
        }

        /// <summary>
        /// Authenticates the user silently using <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, ClientCredential, UserIdentifier)"/>.
        /// </summary>
        /// <param name="resource">The resource to authenticate against.</param>
        /// <param name="clientCredential">The client credential of the application.</param>
        /// <param name="userIdentifier">The <see cref="UserIdentifier"/> of the user.</param>
        /// <returns>The <see cref="IAuthenticationResult"/>.</returns>
        public async Task<IAuthenticationResult> AcquireTokenSilentAsync(string resource, ClientCredential clientCredential, UserIdentifier userIdentifier)
        {
            var result = await this.authenticationContext.AcquireTokenSilentAsync(resource, clientCredential, userIdentifier);

            return result == null ? null : new AuthenticationResultWrapper(result);
        }

        /// <summary>
        /// Authenticates the user silently using <see cref="AuthenticationContext.AcquireToken(string, string, Uri, PromptBehavior)"/>.
        /// </summary>
        /// <param name="resource">The resource to authenticate against.</param>
        /// <param name="clientId">The client ID of the application.</param>
        /// <param name="redirectUri">The redirect URI of the application.</param>
        /// <param name="promptBehavior">The <see cref="PromptBehavior"/> for authentication.</param>
        /// <returns>The <see cref="IAuthenticationResult"/>.</returns>
        public IAuthenticationResult AcquireToken(string resource, string clientId, Uri redirectUri, PromptBehavior promptBehavior)
        {
            var result = this.authenticationContext.AcquireToken(resource, clientId, redirectUri, promptBehavior);

            return result == null ? null : new AuthenticationResultWrapper(result);
        }

        /// <summary>
        /// Authenticates the user silently using <see cref="AuthenticationContext.AcquireTokenAsync(string, ClientCredential)"/>.
        /// </summary>
        /// <param name="resource">The resource to authenticate against.</param>
        /// <param name="clientCredential">The client credential of the application.</param>
        /// <returns>The <see cref="IAuthenticationResult"/>.</returns>
        public async Task<IAuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential)
        {
            var result = await this.authenticationContext.AcquireTokenAsync(resource, clientCredential);

            return result == null ? null : new AuthenticationResultWrapper(result);
        }
    }
}
