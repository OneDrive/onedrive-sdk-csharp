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
    using System.Threading.Tasks;

    public static class OneDriveClientExtensions
    {
        /// <summary>
        /// Creates an authenticated client that uses the OnlineIdAuthenticator API for authentication.
        /// </summary>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClientUsingOnlineIdAuthenticator(
            string[] scopes,
            IHttpProvider httpProvider = null)
        {
            var client = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(
                scopes,
                httpProvider: httpProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client that uses the WebAuthenticationBroker API in SSO mode for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClientUsingWebAuthenticationBroker(
            string appId,
            string[] scopes,
            IHttpProvider httpProvider = null)
        {
            return OneDriveClientExtensions.GetAuthenticatedClientUsingWebAuthenticationBroker(
                appId,
                /* returnUrl */ null,
                scopes,
                httpProvider);
        }

        /// <summary>
        /// Creates an authenticated client that uses the WebAuthenticationBroker API in non-SSO mode for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="returnUrl">The application return URL for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClientUsingWebAuthenticationBroker(
            string appId,
            string returnUrl,
            string[] scopes,
            IHttpProvider httpProvider = null)
        {
            var client = OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(
                appId,
                returnUrl,
                scopes,
                httpProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client for use in Store apps that uses the OnlineIdAuthenticator API for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedUniversalClient(
            string[] scopes,
            string returnUrl = null,
            IHttpProvider httpProvider = null)
        {
            var client = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(scopes, returnUrl, httpProvider);
            await client.AuthenticateAsync();
            return client;
        }

        /// <summary>
        /// Creates an unauthenticated client that uses the OnlineIdAuthenticator API for authentication.
        /// </summary>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetClientUsingOnlineIdAuthenticator(
            string[] scopes,
            string returnUrl = null,
            IHttpProvider httpProvider = null)
        {
            return new OneDriveClient(
                new AppConfig { MicrosoftAccountScopes = scopes },
                httpProvider: httpProvider ?? new HttpProvider(),
                serviceInfoProvider: new OnlineIdServiceInfoProvider());
        }

        /// <summary>
        /// Creates an unauthenticated client that uses the WebAuthenticationBroker API in SSO mode for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetClientUsingWebAuthenticationBroker(
            string appId,
            string[] scopes,
            IHttpProvider httpProvider = null)
        {
            return OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(appId, /* returnUrl */ null, scopes, httpProvider);
        }

        /// <summary>
        /// Creates an unauthenticated client that uses the WebAuthenticationBroker API in non-SSO mode for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="returnUrl">The application return URL for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetClientUsingWebAuthenticationBroker(
            string appId,
            string returnUrl,
            string[] scopes,
            IHttpProvider httpProvider = null)
        {
            return new OneDriveClient(
                new AppConfig
                {
                    MicrosoftAccountAppId = appId,
                    MicrosoftAccountReturnUrl = returnUrl,
                    MicrosoftAccountScopes = scopes
                },
                httpProvider: httpProvider ?? new HttpProvider(),
                serviceInfoProvider: new WebAuthenticationBrokerServiceInfoProvider());
        }

        /// <summary>
        /// Creates an unauthenticated client for use in Store apps that uses the OnlineIdAuthenticator API for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetUniversalClient(
            string[] scopes,
            string returnUrl = null,
            IHttpProvider httpProvider = null)
        {
            return OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(scopes, returnUrl, httpProvider);
        }
    }
}
