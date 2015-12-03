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

    public abstract class AdalAuthenticationProviderBase : IAuthenticationProvider
    {
        protected ServiceInfo serviceInfo;
        
        internal IAuthenticationContextWrapper authenticationContextWrapper;

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

                if (adalCredentialCache == null && this.serviceInfo.CredentialCache != null)
                {
                    throw new OneDriveException(
                        new Error
                        {
                            Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                            Message = "Invalid credential cache type for authentication using ADAL.",
                        });
                }

                this.authenticationContextWrapper = adalCredentialCache == null
                    ? new AuthenticationContextWrapper(serviceInfo.AuthenticationServiceUrl)
                    : new AuthenticationContextWrapper(serviceInfo.AuthenticationServiceUrl, false, adalCredentialCache.TokenCache.InnerTokenCache);
            }
        }

        /// <summary>
        /// Gets the current account session.
        /// </summary>
        public AccountSession CurrentAccountSession { get; set; }

        protected abstract Task<IAuthenticationResult> AuthenticateResourceAsync(string resource);

        /// <summary>
        /// Appends the authentication header to the specified web request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
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

        /// <summary>
        /// Retrieves the authentication token.
        /// </summary>
        /// <returns>The authentication token.</returns>
        public async Task<AccountSession> AuthenticateAsync()
        {
            if (this.CurrentAccountSession != null && !this.CurrentAccountSession.IsExpiring())
            {
                return this.CurrentAccountSession;
            }

            if (string.IsNullOrEmpty(this.ServiceInfo.ServiceResource) || string.IsNullOrEmpty(this.ServiceInfo.BaseUrl))
            {
                var discoveryServiceToken = await this.GetAuthenticationTokenForResourceAsync(this.serviceInfo.DiscoveryServiceResource);
                await this.RetrieveMyFilesServiceResourceAsync(discoveryServiceToken);
            }

            var authenticationResult = await this.AuthenticateResourceAsync(this.ServiceInfo.ServiceResource);

            if (authenticationResult == null)
            {
                this.CurrentAccountSession = null;
                return this.CurrentAccountSession;
            }

            this.CurrentAccountSession = new AdalAccountSession
            {
                AccessToken = authenticationResult.AccessToken,
                AccessTokenType = authenticationResult.AccessTokenType,
                AccountType = AccountType.ActiveDirectory,
                CanSignOut = true,
                ClientId = this.ServiceInfo.AppId,
                ExpiresOnUtc = authenticationResult.ExpiresOn,
                UserId = authenticationResult.UserInfo == null ? null : authenticationResult.UserInfo.UniqueId,
            };

            return this.CurrentAccountSession;
        }

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        public abstract Task SignOutAsync();

        protected void DeleteUserCredentialsFromCache(AccountSession accountSession)
        {
            if (this.ServiceInfo.CredentialCache != null)
            {
                this.ServiceInfo.CredentialCache.DeleteFromCache(accountSession);
            }
        }

        private async Task<string> GetAuthenticationTokenForResourceAsync(string resource)
        {
            var authenticationResult = await this.AuthenticateResourceAsync(resource);

            return authenticationResult.AccessToken;
        }

        private Task RetrieveMyFilesServiceResourceAsync(string discoveryServiceToken)
        {
            return this.RetrieveServiceResourceAsync(discoveryServiceToken, Constants.Authentication.MyFilesCapability);
        }

        private async Task RetrieveServiceResourceAsync(string discoveryServiceToken, string capability)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, this.ServiceInfo.DiscoveryServiceUrl))
            {
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.Headers.Bearer, discoveryServiceToken);
                using (var response = await this.ServiceInfo.HttpProvider.SendAsync(httpRequestMessage))
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        var responseValues = this.ServiceInfo.HttpProvider.Serializer.DeserializeObject<DiscoveryServiceResponse>(responseStream);

                        if (responseValues == null || responseValues.Value == null)
                        {
                            throw new OneDriveException(
                                new Error
                                {
                                    Code = OneDriveErrorCode.MyFilesCapabilityNotFound.ToString(),
                                    Message = "MyFiles capability not found for the current user."
                                });
                        }

                        var service = responseValues.Value.FirstOrDefault(value =>
                            string.Equals(value.ServiceApiVersion, this.ServiceInfo.OneDriveServiceEndpointVersion, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(value.Capability, capability, StringComparison.OrdinalIgnoreCase));

                        if (service == null)
                        {
                            throw new OneDriveException(
                                new Error
                                {
                                    Code = OneDriveErrorCode.MyFilesCapabilityNotFound.ToString(),
                                    Message = string.Format("{0} capability with version {1} not found for the current user.", capability, this.ServiceInfo.OneDriveServiceEndpointVersion),
                                });
                        }

                        this.ServiceInfo.ServiceResource = service.ServiceResourceId;
                        this.ServiceInfo.BaseUrl = service.ServiceEndpointUri;
                    }
                }
            }
        }
    }
}
