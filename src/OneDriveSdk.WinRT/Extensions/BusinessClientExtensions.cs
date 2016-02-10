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
        [Obsolete("Please use the GetClient method instead to retrieve a OneDrive for Business client.", false)]
        public static IOneDriveClient GetActiveDirectoryClient(
            string appId,
            string returnUrl = null,
            string serviceResource = null,
            string serviceEndpointUrl = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetClientInternal(
                new AppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryReturnUrl = returnUrl,
                    ActiveDirectoryServiceEndpointUrl = serviceEndpointUrl,
                    ActiveDirectoryServiceResource = serviceResource,
                },
                new AdalServiceInfoProvider(),
                credentialCache,
                httpProvider);
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
        [Obsolete("Please use the GetAuthenticatedClientAsync method instead to retrieve a OneDrive for Business client.", false)]
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

        /// <summary>
        /// Creates an unauthenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="AppConfig"/> for the application configuration.
        ///     Authentication requires the following to be initialized:
        ///         - ActiveDirectoryAppId
        ///         - ActiveDirectoryReturnUrl
        ///     To bypass using the Discovery Service for service endpoint lookup ActiveDirectoryServiceResource must also be set.
        /// </param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClientAsync(
            AppConfig appConfig,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = BusinessClientExtensions.GetClient(
                appConfig,
                userId,
                credentialCache,
                httpProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using a custom <see cref="IAuthenticationProvider"/> for authentication.
        /// </summary>
        /// <param name="serviceEndpointUrl">
        ///     The endpoint URL for the service. For example, "https://resource-my.sharepoint.com/".
        /// </param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating requests.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClientUsingCustomAuthenticationAsync(
            string serviceEndpointUrl,
            IAuthenticationProvider authenticationProvider,
            IHttpProvider httpProvider = null)
        {
            var client = BusinessClientExtensions.GetClientUsingCustomAuthentication(
                serviceEndpointUrl,
                authenticationProvider,
                httpProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an unauthenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="AppConfig"/> for the application configuration.
        ///     Authentication requires the following to be initialized:
        ///         - ActiveDirectoryAppId
        ///         - ActiveDirectoryReturnUrl
        ///     To bypass using the Discovery Service for service endpoint lookup ActiveDirectoryServiceResource must also be set.
        /// </param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetClient(
            AppConfig appConfig,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            if (string.IsNullOrEmpty(appConfig.ActiveDirectoryReturnUrl))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "ActiveDirectoryReturnUrl is required for authenticating a business client.",
                    });
            }

            appConfig.ActiveDirectoryAuthenticationServiceUrl = Constants.Authentication.ActiveDirectoryAuthenticationServiceUrl;

            return BusinessClientExtensions.GetClientInternal(
                appConfig,
                new AdalServiceInfoProvider() { UserSignInName = userId },
                credentialCache,
                httpProvider);
        }

        /// <summary>
        /// Creates a client using a custom <see cref="IAuthenticationProvider"/> for authentication.
        /// </summary>
        /// <param name="serviceEndpointUrl">
        ///     The endpoint URL for the service. For example, "https://resource-my.sharepoint.com/".
        /// </param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating requests.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetClientUsingCustomAuthentication(
            string serviceEndpointUrl,
            IAuthenticationProvider authenticationProvider,
            IHttpProvider httpProvider = null)
        {
            if (string.IsNullOrEmpty(serviceEndpointUrl))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Service endpoint URL is required when using custom authentication.",
                    });
            }

            return new OneDriveClient(
                new AppConfig
                {
                    ActiveDirectoryServiceResource = serviceEndpointUrl,
                },
                /* credentialCache */ null,
                httpProvider ?? new HttpProvider(),
                new AdalServiceInfoProvider(authenticationProvider),
                ClientType.Business);
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL app-only authentication flow.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="AppConfig"/> for the application configuration.
        /// </param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        internal static IOneDriveClient GetClientInternal(
            AppConfig appConfig,
            IServiceInfoProvider serviceInfoProvider,
            AdalCredentialCache credentialCache,
            IHttpProvider httpProvider)
        {
            if (string.IsNullOrEmpty(appConfig.ActiveDirectoryAppId))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "ActiveDirectoryAppId is required for authentication."
                    });
            }

            return new OneDriveClient(
                appConfig,
                credentialCache ?? new AdalCredentialCache(),
                httpProvider ?? new HttpProvider(),
                serviceInfoProvider ?? new AdalServiceInfoProvider(),
                ClientType.Business);
        }
    }
}
