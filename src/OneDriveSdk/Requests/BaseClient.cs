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
    /// A default <see cref="IBaseClient"/> implementation.
    /// </summary>
    public class BaseClient : IBaseClient
    {
        private string baseUrl;

        internal readonly AppConfig appConfig;
        internal readonly CredentialCache credentialCache;
        internal readonly IServiceInfoProvider serviceInfoProvider;

        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        public BaseClient(
            AppConfig appConfig,
            CredentialCache credentialCache = null,
            IHttpProvider httpProvider = null,
            IServiceInfoProvider serviceInfoProvider = null,
            ClientType clientType = ClientType.Consumer)
        {
            this.appConfig = appConfig;
            this.ClientType = clientType;
            this.credentialCache = credentialCache ?? new CredentialCache();
            this.HttpProvider = httpProvider ?? new HttpProvider(new Serializer());
            this.serviceInfoProvider = serviceInfoProvider ?? new ServiceInfoProvider();
        }

        public IAuthenticationProvider AuthenticationProvider
        {
            get
            {
                if (this.ServiceInfo != null)
                {
                    return this.ServiceInfo.AuthenticationProvider;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the base URL for requests of the client.
        /// </summary>
        public string BaseUrl
        {
            get { return this.baseUrl; }
            set { this.baseUrl = value.TrimEnd('/'); }
        }

        public ClientType ClientType { get; private set; }

        /// <summary>
        /// Gets the <see cref="IHttpProvider"/> for sending HTTP requests.
        /// </summary>
        public IHttpProvider HttpProvider { get; private set; }

        /// <summary>
        /// Gets whether or not the current client is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return this.ServiceInfo != null
                    && this.ServiceInfo.AuthenticationProvider != null
                    && this.ServiceInfo.AuthenticationProvider.CurrentAccountSession != null
                    && !this.ServiceInfo.AuthenticationProvider.CurrentAccountSession.IsExpiring();
            }
        }

        /// <summary>
        /// Gets the <see cref="ServiceInfo"/> for the current session.
        /// </summary>
        public ServiceInfo ServiceInfo { get; internal set; }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <returns>The current account session.</returns>
        public async Task<AccountSession> AuthenticateAsync()
        {
            if (this.ServiceInfo == null)
            {
                this.ServiceInfo = await this.serviceInfoProvider.GetServiceInfo(
                    this.appConfig,
                    this.credentialCache,
                    this.HttpProvider,
                    this.ClientType);
            }

            var authResult = await this.ServiceInfo.AuthenticationProvider.AuthenticateAsync();

            if (string.IsNullOrEmpty(this.BaseUrl))
            {
                this.BaseUrl = this.ServiceInfo.BaseUrl;
            }

            return authResult;
        }

        /// <summary>
        /// Signs the user out.
        /// </summary>
        /// <returns>The task to await.</returns>
        public Task SignOutAsync()
        {
            if (this.AuthenticationProvider != null)
            {
                return this.AuthenticationProvider.SignOutAsync();
            }

            return Task.FromResult(0);
        }
    }
}
