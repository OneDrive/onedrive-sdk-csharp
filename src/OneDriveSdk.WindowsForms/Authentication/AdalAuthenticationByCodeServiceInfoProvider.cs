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

    /// <summary>
    /// A <see cref="ServiceInfoProvider"/> implementation for initializing a <see cref="ServiceInfo"/> to use
    /// an authorization code for authentication.
    /// </summary>
    public class AdalAuthenticationByCodeServiceInfoProvider : ServiceInfoProvider
    {
        private string authorizationCode;

        /// <summary>
        /// Initializes an <see cref="AdalAuthenticationByCodeServiceInfoProvider"/> that uses an
        /// <see cref="AdalAuthenticationByCodeAuthenticationProvider"/> for authentication.
        /// </summary>
        public AdalAuthenticationByCodeServiceInfoProvider(string authenticationCode)
            : this(authenticationCode, null)
        {
        }

        /// <summary>
        /// Initializes an <see cref="AdalAuthenticationByCodeServiceInfoProvider"/> that uses a custom
        /// <see cref="IAuthenticationProvider"/> for authentication.
        /// 
        /// Used for unit testing.
        /// </summary>
        internal AdalAuthenticationByCodeServiceInfoProvider(string authorizationCode, IAuthenticationProvider authenticationProvider)
            : base(authenticationProvider, null)
        {
            this.authorizationCode = authorizationCode;
        }

        /// <summary>
        /// Generates the <see cref="ServiceInfo"/> for the current application configuration.
        /// </summary>
        /// <param name="appConfig">The <see cref="AppConfig"/> for the current application.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="clientType">The <see cref="ClientType"/> to specify the business or consumer service.</param>
        /// <returns>The <see cref="ServiceInfo"/> for the current session.</returns>
        public async override Task<ServiceInfo> GetServiceInfo(
            AppConfig appConfig,
            CredentialCache credentialCache,
            IHttpProvider httpProvider,
            ClientType clientType = ClientType.Business)
        {
            if (clientType == ClientType.Consumer)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "AdalAuthenticationByCodeServiceInfoProvider only supports Active Directory authentication."
                    });
            }

            if (string.IsNullOrEmpty(appConfig.ActiveDirectoryServiceResource))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Service resource ID is required for authentication by code.",
                    });
            }

            var serviceInfo = await base.GetServiceInfo(appConfig, credentialCache, httpProvider, clientType);

            var adalServiceInfo = new AdalServiceInfo();
            adalServiceInfo.CopyFrom(serviceInfo);

            var adalAppConfig = appConfig as BusinessAppConfig;

            if (adalAppConfig != null)
            {
                adalServiceInfo.ClientCertificate = adalAppConfig.ActiveDirectoryClientCertificate;
            }

            if (string.IsNullOrEmpty(adalServiceInfo.BaseUrl) && !string.IsNullOrEmpty(adalServiceInfo.ServiceResource))
            {
                adalServiceInfo.BaseUrl = string.Format(
                    Constants.Authentication.OneDriveBusinessBaseUrlFormatString,
                    adalServiceInfo.ServiceResource,
                    "v2.0");
            }

            if (adalServiceInfo.AuthenticationProvider == null)
            {
                adalServiceInfo.AuthenticationProvider = new AdalAuthenticationByCodeAuthenticationProvider(adalServiceInfo, authorizationCode);
            }

            return adalServiceInfo;
        }
    }
}
