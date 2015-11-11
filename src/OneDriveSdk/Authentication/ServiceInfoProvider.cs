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

    public class ServiceInfoProvider : IServiceInfoProvider
    {
        private const string discoveryServiceUrl =
            "https://api.office.com/discovery/v2.0/me/FirstSignIn?redirect_uri=https://localhost:777&scope=MyFiles";

        private const string discoveryServiceReturnUrl = "https://localhost:777";

        protected IWebAuthenticationUi webAuthenticationUi;

        /// <summary>
        /// Instantiates a new <see cref="ServiceInfoProvider"/>.
        /// </summary>
        /// <param name="webAuthenticationUi">The <see cref="IWebAuthenticationUi"/> for displaying authentication UI to the user.</param>
        public ServiceInfoProvider(IWebAuthenticationUi webAuthenticationUi)
            : this(null, webAuthenticationUi)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="ServiceInfoProvider"/>.
        /// </summary>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating the user.</param>
        public ServiceInfoProvider(IAuthenticationProvider authenticationProvider)
            : this(authenticationProvider, null)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="ServiceInfoProvider"/>.
        /// </summary>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating the user.</param>
        /// <param name="webAuthenticationUi">The <see cref="IWebAuthenticationUi"/> for displaying authentication UI to the user.</param>
        public ServiceInfoProvider(IAuthenticationProvider authenticationProvider = null, IWebAuthenticationUi webAuthenticationUi = null)
        {
            this.AuthenticationProvider = authenticationProvider;
            this.webAuthenticationUi = webAuthenticationUi;
        }

        public IAuthenticationProvider AuthenticationProvider { get; private set; }

        /// <summary>
        /// Generates the <see cref="ServiceInfo"/> for the current application configuration.
        /// </summary>
        /// <param name="appConfig">The <see cref="AppConfig"/> for the current application.</param>
        /// <param name="credentialCache">The cache instance for storing user credentials.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending HTTP requests.</param>
        /// <param name="clientType">The <see cref="ClientType"/> to specify the business or consumer service.</param>
        /// <returns>The <see cref="ServiceInfo"/> for the current session.</returns>
        public virtual Task<ServiceInfo> GetServiceInfo(
            AppConfig appConfig,
            CredentialCache credentialCache,
            IHttpProvider httpProvider,
            ClientType clientType)
        {
            if (clientType == ClientType.Consumer)
            {
                var microsoftAccountServiceInfo = new MicrosoftAccountServiceInfo
                {
                    AppId = appConfig.MicrosoftAccountAppId,
                    ClientSecret = appConfig.MicrosoftAccountClientSecret,
                    CredentialCache = credentialCache,
                    HttpProvider = httpProvider,
                    ReturnUrl = appConfig.MicrosoftAccountReturnUrl,
                    Scopes = appConfig.MicrosoftAccountScopes,
                    WebAuthenticationUi = this.webAuthenticationUi,
                };

                microsoftAccountServiceInfo.AuthenticationProvider = this.AuthenticationProvider?? new MicrosoftAccountAuthenticationProvider(microsoftAccountServiceInfo);
                return Task.FromResult<ServiceInfo>(microsoftAccountServiceInfo);
            }

            var activeDirectoryServiceInfo = new ActiveDirectoryServiceInfo
            {
                AppId = appConfig.ActiveDirectoryAppId,
                AuthenticationProvider = this.AuthenticationProvider,
                ClientSecret = appConfig.ActiveDirectoryClientSecret,
                CredentialCache = credentialCache,
                HttpProvider = httpProvider,
                ReturnUrl = appConfig.ActiveDirectoryReturnUrl,
            };
            
            return Task.FromResult<ServiceInfo>(activeDirectoryServiceInfo);
        }
    }
}
