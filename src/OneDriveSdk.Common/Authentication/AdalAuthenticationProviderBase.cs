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
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using IdentityModel.Clients.ActiveDirectory;

    public abstract class AdalAuthenticationProviderBase : IAuthenticationProvider
    {
        protected ServiceInfo serviceInfo;
        
        protected AuthenticationContext authenticationContext;

        /// <summary>
        /// Constructs an <see cref="AdalAuthenticationProviderBase"/>.
        /// </summary>
        /// <param name="serviceInfo">The information for authenticating against the service.</param>
        /// <param name="currentAccountSession">The current account session, used for initializing an already logged in user.</param>
        protected AdalAuthenticationProviderBase(ServiceInfo serviceInfo, AccountSession currentAccountSession = null)
        {
            this.CurrentAccountSession = currentAccountSession;
            this.ServiceInfo = serviceInfo;
        }

        internal ServiceInfo ServiceInfo
        {
            get
            {
                return this.serviceInfo;
            }
            set
            {
                this.serviceInfo = value;

                if (value == null || string.IsNullOrEmpty(serviceInfo.AuthenticationServiceUrl))
                {
                    throw new OneDriveException(
                        new Error
                        {
                            Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                            Message = "Invalid service info for authentication.",
                        });
                }

                var adalCredentialCache = this.serviceInfo.CredentialCache as AdalCredentialCache;

                this.authenticationContext = adalCredentialCache == null
                    ? new AuthenticationContext(serviceInfo.AuthenticationServiceUrl)
                    : new AuthenticationContext(serviceInfo.AuthenticationServiceUrl, false, adalCredentialCache.TokenCache);
            }
        }

        public AccountSession CurrentAccountSession { get; private set; }

        protected abstract Task<AuthenticationResult> AuthenticateResourceAsync(string resource);

        public async Task AppendAuthHeaderAsync(HttpRequestMessage request)
        {
            if (this.CurrentAccountSession == null)
            {
                await this.AuthenticateAsync();
            }

            if (this.CurrentAccountSession != null && !string.IsNullOrEmpty(this.CurrentAccountSession.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Headers.Bearer, this.CurrentAccountSession.AccessToken);
            }
        }

        public async Task<AccountSession> AuthenticateAsync()
        {
            if (this.CurrentAccountSession != null && !this.CurrentAccountSession.IsExpiring())
            {
                return this.CurrentAccountSession;
            }

            var discoveryServiceToken = await this.GetAuthenticationTokenForResourceAsync(this.serviceInfo.DiscoveryServiceResource);
            await this.RetrieveServiceResourceAsync(discoveryServiceToken);
            var authenticationResult = await this.AuthenticateResourceAsync(this.ServiceInfo.OneDriveServiceResource);

            this.CurrentAccountSession = new AccountSession
            {
                AccessToken = authenticationResult.AccessToken,
                AccessTokenType = authenticationResult.AccessTokenType,
                AccountType = AccountType.ActiveDirectory,
            };

            return this.CurrentAccountSession;
        }

        public Task SignOutAsync()
        {
            throw new NotImplementedException();
        }
        
        private async Task<string> GetAuthenticationTokenForResourceAsync(string resource)
        {
            var authenticationResult = await this.AuthenticateResourceAsync(resource);

            return authenticationResult.AccessToken;
        }

        private async Task RetrieveServiceResourceAsync(string discoveryServiceToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, this.ServiceInfo.DiscoveryServiceUrl))
            {
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.Headers.Bearer, discoveryServiceToken);
                using (var response = await this.ServiceInfo.HttpProvider.SendAsync(httpRequestMessage))
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        var responseValues = this.ServiceInfo.HttpProvider.Serializer.DeserializeObject<DiscoveryServiceResponse>(responseStream);

                        if (responseValues == null)
                        {
                            throw new OneDriveException(
                                new Error
                                {
                                    Code = OneDriveErrorCode.ItemNotFound.ToString(),
                                    Message = "MyFiles service not found for the current user."
                                });
                        }

                        var service = responseValues.Value.FirstOrDefault(value => value.ServiceApiVersion.Equals(this.ServiceInfo.OneDriveServiceEndpointVersion));

                        if (service == null)
                        {
                            throw new OneDriveException(
                                new Error
                                {
                                    Code = OneDriveErrorCode.ItemNotFound.ToString(),
                                    Message = "MyFiles service not found for the current user."
                                });
                        }

                        this.ServiceInfo.OneDriveServiceResource = service.ServiceResourceId;
                        this.ServiceInfo.BaseUrl = service.ServiceEndpointUri;
                    }
                }
            }
        }
    }
}
