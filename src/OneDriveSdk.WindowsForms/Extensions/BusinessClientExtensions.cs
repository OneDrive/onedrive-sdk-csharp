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

namespace Microsoft.OneDrive.Sdk.WindowsForms
{
    using System.Security.Cryptography.X509Certificates;
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
        public static IOneDriveClient GetActiveDirectoryClient(
            string appId,
            string returnUrl,
            string serviceResource = null,
            string serviceEndpointUrl = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetClient(
                new AdalAppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryReturnUrl = returnUrl,
                    ActiveDirectoryServiceEndpointUrl = serviceEndpointUrl,
                    ActiveDirectoryServiceResource = serviceResource,
                },
                credentialCache,
                httpProvider,
                serviceInfoProvider: null);
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
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClientAsync(
            string appId,
            string returnUrl,
            string serviceResource,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IServiceInfoProvider serviceInfoProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                /* clientSecret */ null,
                serviceResource,
                userId,
                credentialCache,
                httpProvider,
                serviceInfoProvider);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClientAsync(
            string appId,
            string returnUrl,
            string serviceResource,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                /* clientSecret */ null,
                serviceResource,
                userId,
                credentialCache,
                httpProvider,
                serviceInfoProvider: null);
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL app-only authentication flow.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        /// </param>
        /// <param name="siteId">The ID of the site to access.</param>
        /// <param name="tenantId">The ID of the tenant to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClientUsingAppOnlyAuthenticationAsync(
            string appId,
            X509Certificate2 clientCertificate,
            string serviceResource,
            string siteId,
            string tenantId,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientUsingAppOnlyAuthenticationAsync(
                appId,
                clientCertificate,
                serviceResource,
                siteId,
                tenantId,
                credentialCache,
                httpProvider,
                serviceInfoProvider: null);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClientUsingDiscoveryServiceAsync(
            string appId,
            string returnUrl,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                /* clientSecret */ null,
                /* serviceResource */ null,
                userId,
                credentialCache,
                httpProvider,
                serviceInfoProvider: null);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedClientUsingDiscoveryServiceAsync(
            string appId,
            string returnUrl,
            IServiceInfoProvider serviceInfoProvider,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                /* clientSecret */ null,
                /* serviceResource */ null,
                userId,
                credentialCache,
                httpProvider,
                serviceInfoProvider);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedWebClientAsync(
            string appId,
            string returnUrl,
            X509Certificate2 clientCertificate,
            string serviceResource,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IServiceInfoProvider serviceInfoProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                clientCertificate,
                /* clientSecret */ null,
                serviceResource,
                userId,
                credentialCache,
                httpProvider,
                serviceInfoProvider);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedWebClientAsync(
            string appId,
            string returnUrl,
            string clientSecret,
            string serviceResource,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IServiceInfoProvider serviceInfoProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                clientSecret,
                serviceResource,
                userId,
                credentialCache,
                httpProvider,
                serviceInfoProvider);
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL authentication by code flow.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        /// </param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedWebClientUsingAuthenticationByCodeAsync(
            string appId,
            string returnUrl,
            string clientSecret,
            string serviceResource,
            string code,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = BusinessClientExtensions.GetClientUsingAuthenticationByCode(
                appId,
                returnUrl,
                clientSecret,
                /* clientCertificate */ null,
                serviceResource,
                code,
                credentialCache,
                httpProvider,
                serviceInfoProvider: null);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL authentication by code flow.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        /// </param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static async Task<IOneDriveClient> GetAuthenticatedWebClientUsingAuthenticationByCodeAsync(
            string appId,
            string returnUrl,
            X509Certificate2 clientCertificate,
            string serviceResource,
            string code,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            var client = BusinessClientExtensions.GetClientUsingAuthenticationByCode(
                appId,
                returnUrl,
                /* clientSecret */ null,
                clientCertificate,
                serviceResource,
                code,
                credentialCache,
                httpProvider,
                serviceInfoProvider: null);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedWebClientUsingDiscoveryServiceAsync(
            string appId,
            string returnUrl,
            string clientSecret,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                clientSecret,
                serviceResource: null,
                userId: userId,
                credentialCache: credentialCache,
                httpProvider: httpProvider,
                serviceInfoProvider: null);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="webAuthenticationUi">The <see cref="IWebAuthenticationUi"/> instance for displaying the Discovery Service login screen to the user.</param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedWebClientUsingDiscoveryServiceAsync(
            string appId,
            string returnUrl,
            string clientSecret,
            IWebAuthenticationUi webAuthenticationUi,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                clientSecret,
                serviceResource: null,
                userId: userId,
                credentialCache: credentialCache,
                httpProvider: httpProvider,
                serviceInfoProvider: new AdalServiceInfoProvider(webAuthenticationUi));
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedWebClientUsingDiscoveryServiceAsync(
            string appId,
            string returnUrl,
            string clientSecret,
            IServiceInfoProvider serviceInfoProvider,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                /* clientCertificate */ null,
                clientSecret,
                serviceResource: null,
                userId: userId,
                credentialCache: credentialCache,
                httpProvider: httpProvider,
                serviceInfoProvider: serviceInfoProvider);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="webAuthenticationUi">The <see cref="IWebAuthenticationUi"/> instance for displaying the Discovery Service login screen to the user.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedWebClientUsingDiscoveryServiceAsync(
            string appId,
            string returnUrl,
            X509Certificate2 clientCertificate,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IWebAuthenticationUi webAuthenticationUi = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                clientCertificate,
                clientSecret: null,
                serviceResource: null,
                userId: userId,
                credentialCache: credentialCache,
                httpProvider: httpProvider,
                serviceInfoProvider: new AdalServiceInfoProvider(webAuthenticationUi));
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        public static Task<IOneDriveClient> GetAuthenticatedWebClientUsingDiscoveryServiceAsync(
            string appId,
            string returnUrl,
            X509Certificate2 clientCertificate,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IServiceInfoProvider serviceInfoProvider = null)
        {
            return BusinessClientExtensions.GetAuthenticatedClientAsync(
                appId,
                returnUrl,
                clientCertificate,
                clientSecret: null,
                serviceResource: null,
                userId: userId,
                credentialCache: credentialCache,
                httpProvider: httpProvider,
                serviceInfoProvider: serviceInfoProvider);
        }

        /// <summary>
        /// Creates an authenticated client using ADAL for authentication.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        ///     If not provided, will be retrieved using the Discovery service.
        /// </param>
        /// <param name="userId">The ID of the user to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        internal static async Task<IOneDriveClient> GetAuthenticatedClientAsync(
            string appId,
            string returnUrl,
            X509Certificate2 clientCertificate,
            string clientSecret,
            string serviceResource,
            string userId = null,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IServiceInfoProvider serviceInfoProvider = null)
        {
            var appConfig = new AdalAppConfig
            {
                ActiveDirectoryAppId = appId,
                ActiveDirectoryClientCertificate = clientCertificate,
                ActiveDirectoryClientSecret = clientSecret,
                ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl(),
                ActiveDirectoryReturnUrl = returnUrl,
            };

            if (!string.IsNullOrEmpty(serviceResource))
            {
                appConfig.ActiveDirectoryServiceEndpointUrl = string.Format(
                    Constants.Authentication.OneDriveBusinessBaseUrlFormatString,
                    serviceResource.TrimEnd('/'),
                    "v2.0");
                appConfig.ActiveDirectoryServiceResource = serviceResource;
            }

            var client = BusinessClientExtensions.GetClient(
                appConfig,
                credentialCache,
                httpProvider,
                serviceInfoProvider ?? new AdalServiceInfoProvider { UserSignInName = userId });

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL app-only authentication flow.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        /// </param>
        /// <param name="siteId">The ID of the site to access. For example, "user_domain_com".</param>
        /// <param name="tenantId">The ID of the tenant to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        internal static async Task<IOneDriveClient> GetAuthenticatedClientUsingAppOnlyAuthenticationAsync(
            string appId,
            X509Certificate2 clientCertificate,
            string serviceResource,
            string siteId,
            string tenantId,
            AdalCredentialCache credentialCache,
            IHttpProvider httpProvider,
            IServiceInfoProvider serviceInfoProvider)
        {
            var client = BusinessClientExtensions.GetClientUsingAppOnlyAuthentication(
                appId,
                clientCertificate,
                serviceResource,
                siteId,
                tenantId,
                credentialCache,
                httpProvider,
                serviceInfoProvider);

            await client.AuthenticateAsync();

            return client;
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL authentication by code flow.
        /// </summary>
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="returnUrl">The application return URL for Azure Active Directory authentication.</param>
        /// <param name="clientSecret">The client secret for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        /// </param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        internal static IOneDriveClient GetClientUsingAuthenticationByCode(
            string appId,
            string returnUrl,
            string clientSecret,
            X509Certificate2 clientCertificate,
            string serviceResource,
            string code,
            AdalCredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IServiceInfoProvider serviceInfoProvider = null)
        {
            return BusinessClientExtensions.GetClient(
                new AdalAppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryClientCertificate = clientCertificate,
                    ActiveDirectoryClientSecret = clientSecret,
                    ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl(),
                    ActiveDirectoryServiceResource = serviceResource,
                    ActiveDirectoryReturnUrl = returnUrl
                },
                credentialCache,
                httpProvider,
                serviceInfoProvider ?? new AdalAuthenticationByCodeServiceInfoProvider(code));
        }

        /// <summary>
        /// Creates an authenticated client using the ADAL app-only authentication flow.
        /// </summary>
        /// <param name="appConfig">The configuration for the application.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        internal static IOneDriveClient GetClient(
            AppConfig appConfig,
            AdalCredentialCache credentialCache,
            IHttpProvider httpProvider,
            IServiceInfoProvider serviceInfoProvider)
        {
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
        /// <param name="appId">The application ID for Azure Active Directory authentication.</param>
        /// <param name="clientCertificate">The client certificate for Azure Active Directory authentication.</param>
        /// <param name="serviceResource">
        ///     The service resource for Azure Active Directory authentication. For example, "https://microsoft-my.sharepoint.com/".
        /// </param>
        /// <param name="siteId">The ID of the site to access. For example, "user_domain_com".</param>
        /// <param name="tenantId">The ID of the tenant to authenticate.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="serviceInfoProvider">The <see cref="IServiceInfoProvider"/> for initializing the <see cref="IServiceInfo"/> for the session.</param>
        /// <returns>The <see cref="IOneDriveClient"/> for the session.</returns>
        internal static IOneDriveClient GetClientUsingAppOnlyAuthentication(
            string appId,
            X509Certificate2 clientCertificate,
            string serviceResource,
            string siteId,
            string tenantId,
            AdalCredentialCache credentialCache,
            IHttpProvider httpProvider,
            IServiceInfoProvider serviceInfoProvider)
        {
            return BusinessClientExtensions.GetClient(
                new AdalAppConfig
                {
                    ActiveDirectoryAppId = appId,
                    ActiveDirectoryClientCertificate = clientCertificate,
                    ActiveDirectoryAuthenticationServiceUrl = BusinessClientExtensions.GetAuthenticationServiceUrl(tenantId),
                    ActiveDirectoryServiceResource = serviceResource,
                    ActiveDirectorySiteId = siteId,
                },
                credentialCache,
                httpProvider,
                serviceInfoProvider ?? new AdalAppOnlyServiceInfoProvider());
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
