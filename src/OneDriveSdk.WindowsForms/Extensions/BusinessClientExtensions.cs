﻿// ------------------------------------------------------------------------------
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

namespace Microsoft.OneDrive.Sdk.WindowsForms
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
            string returnUrl,
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
            string returnUrl,
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
        /// Creates an authenticated client using the ADAL app-only authentication flow.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="tenantId">The ID of the tenant to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClientUsingAppOnlyAuthentication(
            string appId,
            string returnUrl,
            string clientSecret,
            string tenantId,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Tenant ID is required for app-only authentication.",
                    });
            }

            var client = new OneDriveClient(
                new AppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryClientSecret = clientSecret,
                    ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl(tenantId),
                    ActiveDirectoryReturnUrl = returnUrl
                },
                credentialCache ?? new AdalCredentialCache(),
                new HttpProvider(),
                new AdalAppOnlyServiceInfoProvider(),
                ClientType.Business);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL authentication by code flow.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClientUsingAuthenticationByCode(
            string appId,
            string returnUrl,
            string clientSecret,
            string code,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = new OneDriveClient(
                new AppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryClientSecret = clientSecret,
                    ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl(),
                    ActiveDirectoryReturnUrl = returnUrl
                },
                credentialCache ?? new AdalCredentialCache(),
                new HttpProvider(),
                new AdalAuthenticationByCodeServiceInfoProvider(code),
                ClientType.Business);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClient(
            string appId,
            string returnUrl,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClient(
                appId,
                returnUrl,
                null,
                null,
                credentialCache,
                httpProvider);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClient(
            string appId,
            string returnUrl,
            string clientSecret,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClient(
                appId,
                returnUrl,
                clientSecret,
                null,
                credentialCache,
                httpProvider);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="tenantId">The ID of the tenant to authenticate. If not provided, will be retrieved using the Discovery service.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClient(
            string appId,
            string returnUrl,
            string clientSecret,
            string tenantId,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = new OneDriveClient(
                new AppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryClientSecret = clientSecret,
                    ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl(tenantId),
                    ActiveDirectoryReturnUrl = returnUrl
                },
                credentialCache ?? new AdalCredentialCache(),
                new HttpProvider(),
                new AdalServiceInfoProvider(),
                ClientType.Business);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Gets the authentication service URL for authentication. If tenant ID is provided, returns the authentication
        /// service URL for the tenant. If not, returns the common login endpoint URL.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to authenticate.</param>
        /// <returns>The authentication service URL.</returns>
        public static string GetAuthenticationServiceUrl(string tenantId = null)
        {
            return string.IsNullOrEmpty(tenantId)
                ? Constants.Authentication.ActiveDirectoryAuthenticationServiceUrl
                : string.Format(Constants.Authentication.ActiveDirectoryAuthenticationServiceUrlFormatString, tenantId);
        }
    }
}
