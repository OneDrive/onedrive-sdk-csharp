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
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="serviceEndpointUrl">
        ///     The service endpoint URL for making API requests. For example, "https://microsoft-my.sharepoint.com/_api/v2.0".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        [Obsolete("Please use the GetClient method instead to retrieve a OneDrive for Business client.", false)]
        public static IOneDriveClient GetActiveDirectoryClient(
            string appId,
            string returnUrl,
            string serviceResource = null,
            string serviceEndpointUrl = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetClientInternal(
                new BusinessAppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryReturnUrl = returnUrl,
                    ActiveDirectoryServiceEndpointUrl = serviceEndpointUrl,
                    ActiveDirectoryServiceResource = serviceResource,
                },
                /* serviceInfoProvider */ null,
                credentialCache,
                httpProvider);
        }

        /// <summary>
        /// Creates an authenticated business client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="serviceEndpointUrl">
        ///     The service endpoint URL for making API requests. For example, "https://microsoft-my.sharepoint.com/_api/v2.0".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        [Obsolete("Please use the GetAuthenticatedClientAsync method instead to retrieve a OneDrive for Business client.", false)]
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
        /// Creates an unauthenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="BusinessAppConfig"/> for the application configuration.
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
            BusinessAppConfig appConfig,
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
        /// <param name="serviceEndpointBaseUrl">
        ///     The endpoint base URL for the service before. For example, "https://resource-my.sharepoint.com/"
        ///     or "https://resource-my.sharepoint.com/personal/site_id".
        /// </param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating requests.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedClientUsingCustomAuthenticationAsync(
            string serviceEndpointBaseUrl,
            IAuthenticationProvider authenticationProvider,
            IHttpProvider httpProvider = null)
        {
            var client = BusinessClientExtensions.GetClientUsingCustomAuthentication(
                serviceEndpointBaseUrl,
                authenticationProvider,
                httpProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL app-only authentication flow.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="BusinessAppConfig"/> for the application configuration.
        ///     Web client app-only authentication requires the following to be initialized:
        ///         - ActiveDirectoryAppId
        ///         - ActiveDirectoryClientCertificate
        ///         - ActiveDirectoryReturnUrl
        ///         - ActiveDirectoryServiceResource
        /// </param>
        /// <param name="serviceEndpointBaseUrl">
        ///     The endpoint base URL for the service before. For example, "https://resource-my.sharepoint.com/"
        ///     or "https://resource-my.sharepoint.com/personal/site_id/".
        /// </param>
        /// <param name="tenantId">The ID of the tenant to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedWebClientUsingAppOnlyAuthenticationAsync(
            BusinessAppConfig appConfig,
            string serviceEndpointBaseUrl,
            string tenantId,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = BusinessClientExtensions.GetWebClientUsingAppOnlyAuthentication(
                appConfig,
                serviceEndpointBaseUrl,
                tenantId,
                credentialCache,
                httpProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="BusinessAppConfig"/> for the application configuration.
        ///     Web client authentication requires the following to be initialized:
        ///         - ActiveDirectoryAppId
        ///         - ActiveDirectoryClientCertificate or ActiveDirectoryClientSecret
        ///         - ActiveDirectoryReturnUrl
        ///         - ActiveDirectoryServiceResource
        /// </param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedWebClientAsync(
            BusinessAppConfig appConfig,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            if (appConfig.ActiveDirectoryClientCertificate == null && string.IsNullOrEmpty(appConfig.ActiveDirectoryClientSecret))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Client certificate or client secret is required for authenticating a business web client.",
                    });
            }

            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appConfig,
                userId,
                credentialCache,
                httpProvider);
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL authentication by code flow.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="BusinessAppConfig"/> for the application configuration.
        ///     Web client authentication by code requires the following to be initialized:
        ///         - ActiveDirectoryAppId
        ///         - ActiveDirectoryClientCertificate or ActiveDirectoryClientSecret
        ///         - ActiveDirectoryReturnUrl
        ///         - ActiveDirectoryServiceResource
        /// </param>
        /// <param name="code">The authorization code to redeem for an authentication token.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedWebClientUsingAuthenticationByCodeAsync(
            BusinessAppConfig appConfig,
            string code,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = BusinessClientExtensions.GetClientUsingAuthenticationByCode(
                appConfig,
                code,
                credentialCache,
                httpProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an unauthenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="BusinessAppConfig"/> for the application configuration.
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
            BusinessAppConfig appConfig,
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

            appConfig.ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl();

            return BusinessClientExtensions.GetClientInternal(
                appConfig,
                new AdalServiceInfoProvider() { UserSignInName = userId },
                credentialCache,
                httpProvider);
        }

        /// <summary>
        /// Creates an unauthenticated client using a custom <see cref="IAuthenticationProvider"/> for authentication.
        /// </summary>
        /// <param name="serviceEndpointBaseUrl">
        ///     The endpoint base URL for the service before. For example, "https://resource-my.sharepoint.com/"
        ///     or "https://resource-my.sharepoint.com/personal/site_id".
        /// </param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating requests.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static IOneDriveClient GetClientUsingCustomAuthentication(
            string serviceEndpointBaseUrl,
            IAuthenticationProvider authenticationProvider,
            IHttpProvider httpProvider = null)
        {
            if (authenticationProvider == null)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "An authentication provider is required for a client using custom authentication.",
                    });
            }

            if (string.IsNullOrEmpty(serviceEndpointBaseUrl))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Service endpoint base URL is required when using custom authentication.",
                    });
            }

            return new OneDriveClient(
                new BusinessAppConfig
                {
                    ActiveDirectoryServiceEndpointUrl = string.Format(
                        Constants.Authentication.OneDriveBusinessBaseUrlFormatString,
                        serviceEndpointBaseUrl.TrimEnd('/'),
                        "v2.0")
                },
                /* credentialCache */ null,
                httpProvider ?? new HttpProvider(),
                new AdalServiceInfoProvider(authenticationProvider),
                ClientType.Business);
        }

        /// <summary>
        /// Creates an authenticated client from a refresh token using ADAL for authentication.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="AppConfig"/> for the application configuration.
        ///     Authentication requires the following to be initialized:
        ///         - ActiveDirectoryAppId
        ///         - ActiveDirectoryServiceResource
        /// </param>
        /// <param name="refreshToken">The refresh token to redeem for an access token.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetSilentlyAuthenticatedClientAsync(
            AppConfig appConfig,
            string refreshToken,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Refresh token is required for silently authenticating a business client.",
                    });
            }

            if (string.IsNullOrEmpty(appConfig.ActiveDirectoryServiceResource))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "ActiveDirectoryServiceResource is required for silently authenticating a business client.",
                    });  
            }

            var serviceInfoProvider = new AdalServiceInfoProvider();

            var client = BusinessClientExtensions.GetClientInternal(
                appConfig,
                serviceInfoProvider,
                credentialCache,
                httpProvider) as OneDriveClient;

            if (client.ServiceInfo == null)
            {
                client.ServiceInfo = await serviceInfoProvider.GetServiceInfo(
                    client.appConfig,
                    client.credentialCache,
                    client.HttpProvider,
                    client.ClientType);
            }

            client.AuthenticationProvider.CurrentAccountSession = new AccountSession { RefreshToken = refreshToken };

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client from a refresh token using ADAL for authentication.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="AppConfig"/> for the application configuration.
        ///     Authentication requires the following to be initialized:
        ///         - ActiveDirectoryAppId
        ///         - ActiveDirectoryClientCertificate or ActiveDirectoryClientSecret
        ///         - ActiveDirectoryServiceResource
        /// </param>
        /// <param name="refreshToken">The refresh token to redeem for an access token.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetSilentlyAuthenticatedWebClientAsync(
            BusinessAppConfig appConfig,
            string refreshToken,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            if (appConfig.ActiveDirectoryClientCertificate == null && string.IsNullOrEmpty(appConfig.ActiveDirectoryClientSecret))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Client certificate or client secret is required for authenticating a business web client.",
                    });
            }

            return BusinessClientExtensions.GetSilentlyAuthenticatedClientAsync(appConfig, refreshToken, credentialCache, httpProvider);
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL authentication by code flow.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="BusinessAppConfig"/> for the application configuration.
        /// </param>
        /// <param name="code">The authorization code to redeem for an authentication token.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        internal static IOneDriveClient GetClientUsingAuthenticationByCode(
            BusinessAppConfig appConfig,
            string code,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            if (string.IsNullOrEmpty(appConfig.ActiveDirectoryServiceResource))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Service resource ID is required for authentication by code.",
                    });
            }

            appConfig.ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl();

            return BusinessClientExtensions.GetClientInternal(
                appConfig,
                new AdalAuthenticationByCodeServiceInfoProvider(code),
                credentialCache,
                httpProvider);
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL app-only authentication flow.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="BusinessAppConfig"/> for the application configuration.
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

        /// <summary>
        /// Creates an unauthenticated client using the ADAL app-only authentication flow.
        /// </summary>
        /// <param name="appConfig">
        ///     The <see cref="BusinessAppConfig"/> for the application configuration.
        /// </param>
        /// <param name="serviceEndpointBaseUrl">
        ///     The endpoint base URL for the service before. For example, "https://resource-my.sharepoint.com/"
        ///     or "https://resource-my.sharepoint.com/personal/site_id".
        /// </param>
        /// <param name="tenantId">The ID of the tenant to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        internal static IOneDriveClient GetWebClientUsingAppOnlyAuthentication(
            BusinessAppConfig appConfig,
            string serviceEndpointBaseUrl,
            string tenantId,
            AdalCredentialCache credentialCache,
            IHttpProvider httpProvider)
        {
            if (appConfig.ActiveDirectoryClientCertificate == null)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "ActiveDirectoryClientCertificate is required for app-only authentication."
                    });
            }

            if (string.IsNullOrEmpty(serviceEndpointBaseUrl))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Service endpoint base URL is required for app-only authentication."
                    });
            }

            if (string.IsNullOrEmpty(appConfig.ActiveDirectoryServiceResource))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "ActiveDirectoryServiceResource is required for app-only authentication."
                    });
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Tenant ID is required for app-only authentication."
                    });
            }

            appConfig.ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl(tenantId);
            appConfig.ActiveDirectoryServiceEndpointUrl = string.Format(
                Constants.Authentication.OneDriveBusinessBaseUrlFormatString,
                serviceEndpointBaseUrl.TrimEnd('/'),
                "v2.0");

            return BusinessClientExtensions.GetClientInternal(
                appConfig,
                new AdalAppOnlyServiceInfoProvider(),
                credentialCache,
                httpProvider);
        }

        /// <summary>
        /// Gets the authentication service URL for authentication. If tenant ID is provided, returns the authentication
        /// service URL for the tenant. If not, returns the common login endpoint URL.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to authenticate.</param>
        /// <returns>The authentication service URL.</returns>
        private static string GetAuthenticationServiceUrl(string tenantId = null)
        {
            return string.IsNullOrEmpty(tenantId)
                ? Constants.Authentication.ActiveDirectoryAuthenticationServiceUrl
                : string.Format(Constants.Authentication.ActiveDirectoryAuthenticationServiceUrlFormatString, tenantId);
        }
    }
}
