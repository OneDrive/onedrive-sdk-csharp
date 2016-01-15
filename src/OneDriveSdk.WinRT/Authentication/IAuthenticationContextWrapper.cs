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
    using IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Threading.Tasks;

    public interface IAuthenticationContextWrapper
    {
        /// <summary>
        /// Authenticates the user silently using <see cref="AuthenticationContext.AcquireToken(string, string, UserIdentifier)"/>.
        /// </summary>
        /// <param name="resource">The resource to authenticate against.</param>
        /// <param name="clientId">The client ID of the application.</param>
        /// <param name="userIdentifier">The <see cref="UserIdentifier"/> for authentication.</param>
        /// <returns>The <see cref="IAuthenticationResult"/>.</returns>
        Task<IAuthenticationResult> AcquireTokenSilentAsync(string resource, string clientId, UserIdentifier userIdentifier);

        /// <summary>
        /// Authenticates the user using <see cref="AuthenticationContext.AcquireTokenAsync(string, string, Uri, PromptBehavior, UserIdentifier)"/>.
        /// </summary>
        /// <param name="resource">The resource to authenticate against.</param>
        /// <param name="clientId">The client ID of the application.</param>
        /// <param name="redirectUri">The redirect URI of the application.</param>
        /// <param name="promptBehavior">The <see cref="PromptBehavior"/> for authentication.</param>
        /// <param name="userIdentifier">The <see cref="UserIdentifier"/> for authentication.</param>
        /// <returns>The <see cref="IAuthenticationResult"/>.</returns>
        Task<IAuthenticationResult> AcquireTokenAsync(string resource, string clientId, Uri redirectUri, PromptBehavior promptBehavior, UserIdentifier userIdentifier);
    }
}
