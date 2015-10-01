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
    public partial class OneDriveClient
    {
        /// <summary>
        /// Creates a OneDrive client for use against OneDrive consumer.
        /// </summary>
        /// <param name="appId">The application ID for Microsoft Account authentication.</param>
        /// <param name="returnUrl">The application return URL for Microsoft Account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft Account authentication.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="webAuthenticationUi">The <see cref="IWebAuthenticationUi"/> for displaying authentication UI to the user.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetMicrosoftAccountClient(
            string appId,
            string returnUrl,
            string[] scopes,
            CredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IWebAuthenticationUi webAuthenticationUi = null)
        {
            return OneDriveClient.GetMicrosoftAccountClient(
                appId,
                returnUrl,
                scopes,
                /* clientSecret */ null,
                credentialCache,
                httpProvider,
                new ServiceInfoProvider(webAuthenticationUi));
        }

        /// <summary>
        /// Creates a OneDrive client for use against OneDrive consumer.
        /// </summary>
        /// <param name="appId">The application ID for Microsoft Account authentication.</param>
        /// <param name="returnUrl">The application return URL for Microsoft Account authentication.</param>
        /// <param name="scopes">The requested scopes for Microsoft Account authentication.</param>
        /// <param name="clientSecret">The client secret for Microsoft Account authentication.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetMicrosoftAccountClient(
            string appId,
            string returnUrl,
            string[] scopes,
            string clientSecret,
            CredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IServiceInfoProvider serviceInfoProvider = null)
        {
            var appConfig = new AppConfig
            {
                MicrosoftAccountAppId = appId,
                MicrosoftAccountReturnUrl = returnUrl,
                MicrosoftAccountScopes = scopes,
            };

            return new OneDriveClient(appConfig, credentialCache, httpProvider, serviceInfoProvider);
        }

        /// <summary>
        /// Gets the default drive.
        /// </summary>
        public IDriveRequestBuilder Drive
        {
            get
            {
                return new DriveRequestBuilder(string.Format("{0}/{1}", this.BaseUrl, Constants.Url.Drive), this);
            }
        }

        /// <summary>
        /// Gets item request builder for the specified item path.
        /// <returns>The item request builder.</returns>
        /// </summary>
        public IItemRequestBuilder ItemWithPath(string path)
        {
            return new ItemRequestBuilder(
                string.Format("{0}{1}:", this.BaseUrl, path),
                this);
        }
    }
}