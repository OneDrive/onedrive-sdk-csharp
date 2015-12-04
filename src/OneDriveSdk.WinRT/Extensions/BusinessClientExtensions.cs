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

    public static class BusinessClientExtensions
    {
        /// <summary>
        /// Creates an unauthenticated business client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">The service resource for Azure Active Directory authentication. If not provided, will be retrieved using the Discovery service.</param>
        /// <param name="serviceEndpointUrl">The service endpoint URL for making API requests. If not provided, will be retrieved using the Discovery service.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetActiveDirectoryClient(
            string appId,
            string returnUrl = null,
            string serviceResource = null,
            string serviceEndpointUrl = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return new OneDriveClient(
                new AppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryReturnUrl = returnUrl,
                    ActiveDirectoryServiceEndpointUrl = serviceEndpointUrl,
                    ActiveDirectoryServiceResource = serviceResource,
                },
                credentialCache ?? new AdalCredentialCache(),
                new HttpProvider(),
                new AdalServiceInfoProvider(),
                ClientType.Business);
        }

        /// <summary>
        /// Creates an authenticated business client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">The service resource for Azure Active Directory authentication. If not provided, will be retrieved using the Discovery service.</param>
        /// <param name="serviceEndpointUrl">The service endpoint URL for making API requests. If not provided, will be retrieved using the Discovery service.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedActiveDirectoryClient(
            string appId,
            string returnUrl = null,
            string serviceResource = null,
            string serviceEndpointUrl = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = BusinessClientExtensions.GetActiveDirectoryClient(
                appId,
                returnUrl,
                serviceResource,
                serviceEndpointUrl,
                credentialCache,
                httpProvider);

            await client.AuthenticateAsync();

            return client;
        }
    }
}
