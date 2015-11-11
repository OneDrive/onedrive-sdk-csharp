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
    public class ServiceInfo
    {
        /// <summary>
        /// Gets or sets the type of the current user account.
        /// </summary>
        public AccountType AccountType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the current application.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IAuthenticationProvider"/> for authenticating requests.
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the authentication service.
        /// </summary>
        public string AuthenticationServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the OneDrive service endpoint.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the client secret of the current application.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the cache instance for storing user credentials.
        /// </summary>
        public CredentialCache CredentialCache { get; set; }

        /// <summary>
        /// Gets or sets the discovery service resource for Active Directory authentication.
        /// </summary>
        public string DiscoveryServiceResource { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the discovery service for Active Directory authentication.
        /// </summary>
        public string DiscoveryServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IHttpProvider"/> for sending HTTP requests.
        /// </summary>
        public IHttpProvider HttpProvider { get; set; }

        /// <summary>
        /// Gets or sets the application's return URL for authentication.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets the service resource for OneDrive for Active Directory authentication.
        /// </summary>
        public string ServiceResource { get; set; }

        /// <summary>
        /// Gets or sets the application's sign out URL.
        /// </summary>
        public string SignOutUrl { get; set; }

        /// <summary>
        /// Gets or sets the scopes requested by the current application.
        /// </summary>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the authentication token service.
        /// </summary>
        public string TokenServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the ID of the current user.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the version of the OneDrive service endpoint.
        /// </summary>
        public string OneDriveServiceEndpointVersion { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebAuthenticationUi"/> for displaying authentication UI to the user.
        /// </summary>
        public IWebAuthenticationUi WebAuthenticationUi { get; set; }
    }
}
