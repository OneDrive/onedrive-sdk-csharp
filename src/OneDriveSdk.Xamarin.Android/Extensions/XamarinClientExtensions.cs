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
    using Android.Content;
    using System.Threading.Tasks;

    public static class XamarinClientExtensions
    {
        /// <summary>
        /// Creates an authenticated client using Web Browser for authentication.
        /// </summary>
        /// <param name="context">The Android content context</param>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="returnUrl">The application return URL for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="clientSecret">The client secret for Microsoft account authentication.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClientAsync(
            Context context,
            string appId,
            string returnUrl,
            string[] scopes,
            string clientSecret)
        {
            return GetAuthenticatedClientAsync(
                            context,
                            appId,
                            returnUrl,
                            scopes,
                            clientSecret,
                            null,
                            null);
        }

        /// <summary>
        /// Creates an authenticated client using Web Browser for authentication.
        /// </summary>
        /// <param name="context">The Android content context</param>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="returnUrl">The application return URL for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="clientSecret">The client secret for Microsoft account authentication.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClientAsync(
            Context context,
            string appId,
            string returnUrl,
            string[] scopes,
            string clientSecret,
            CredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = GetClient(
                            context,
                            appId,
                            returnUrl,
                            scopes,
                            clientSecret,
                            credentialCache,
                            httpProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an unauthenticated client using Web Browser for authentication.
        /// </summary>
        /// <param name="context">The Android content context</param>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="returnUrl">The application return URL for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="clientSecret">The client secret for Microsoft account authentication.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetClient(
            Context context,
            string appId,
            string returnUrl,
            string[] scopes,
            string clientSecret)
        {
            return GetClient(
                            context,
                            appId, 
                            returnUrl, 
                            scopes, 
                            clientSecret, 
                            null,
                            null);
        }

        /// <summary>
        /// Creates an unauthenticated client using Web Browser for authentication.
        /// </summary>
        /// <param name="context">The Android content context</param>
        /// <param name="appId">The application ID for Microsoft account authentication.</param>
        /// <param name="returnUrl">The application return URL for Microsoft account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft account authentication.</param>
        /// <param name="clientSecret">The client secret for Microsoft account authentication.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetClient(
            Context context,
            string appId,
            string returnUrl,
            string[] scopes,
            string clientSecret,
            CredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var config = new AppConfig
            {
                MicrosoftAccountAppId = appId,
                MicrosoftAccountClientSecret = clientSecret,
                MicrosoftAccountReturnUrl = returnUrl,
                MicrosoftAccountScopes = scopes
            };

            var authenticationUi = new AndroidWebAuthenticationUi(context);
            var serviceProvider = new AndroidServiceInfoProvider(authenticationUi);

            return new OneDriveClient(
                        appConfig: config,
                        credentialCache: credentialCache,
                        httpProvider: httpProvider ?? new HttpProvider(),
                        serviceInfoProvider: serviceProvider,
                        clientType: ClientType.Consumer);
        }
    }
}