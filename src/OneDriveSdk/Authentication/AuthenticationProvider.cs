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
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// An <see cref="IAuthenticationProvider"/> implementation using the Live SDK.
    /// </summary>
    public abstract class AuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Constructs an <see cref="AuthenticationProvider"/>.
        /// </summary>
        protected AuthenticationProvider(ServiceInfo serviceInfo)
        {
            this.ServiceInfo = serviceInfo;
        }

        public AccountSession CurrentAccountSession { get; set; }

        public ServiceInfo ServiceInfo { get; private set; }

        /// <summary>
        /// Appends the authentication header to the specified web request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public virtual async Task AppendAuthHeaderAsync(HttpRequestMessage request)
        {
            if (this.CurrentAccountSession == null || string.IsNullOrEmpty(this.CurrentAccountSession.AccessToken))
            {
                await this.AuthenticateAsync();
            }

            if (this.CurrentAccountSession != null && !string.IsNullOrEmpty(this.CurrentAccountSession.AccessToken))
            {
                var tokenTypeString = string.IsNullOrEmpty(this.CurrentAccountSession.AccessTokenType)
                    ? Constants.Headers.Bearer
                    : this.CurrentAccountSession.AccessTokenType;
                request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Headers.Bearer, this.CurrentAccountSession.AccessToken);
            }
        }

        /// <summary>
        /// Retrieves the authentication token.
        /// </summary>
        /// <returns>The authentication token.</returns>
        public virtual async Task<AccountSession> AuthenticateAsync()
        {
            var authResult = await this.ProcessCachedAccountSessionAsync(this.CurrentAccountSession);

            if (authResult != null)
            {
                return authResult;
            }

            var cachedResult = this.GetAuthenticationResultFromCache();
            authResult = await this.ProcessCachedAccountSessionAsync(cachedResult);

            if (authResult != null)
            {
                this.CacheAuthResult(authResult);
                return authResult;
            }

            if (cachedResult != null)
            {
                // If we haven't retrieved a valid auth result using cached values, delete the credentials from the cache.
                this.DeleteUserCredentialsFromCache(cachedResult);
            }

            authResult = await this.GetAuthenticationResultAsync();

            if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Failed to retrieve a valid authentication token for the user."
                    });
            }

            this.CacheAuthResult(authResult);

            return authResult;
        }

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        public abstract Task SignOutAsync();
        
        protected void CacheAuthResult(AccountSession accountSession)
        {
            this.CurrentAccountSession = accountSession;

            if (this.ServiceInfo.CredentialCache != null)
            {
                this.ServiceInfo.CredentialCache.AddToCache(accountSession);
            }
        }

        protected void DeleteUserCredentialsFromCache(AccountSession accountSession)
        {
            if (this.ServiceInfo.CredentialCache != null)
            {
                this.ServiceInfo.CredentialCache.DeleteFromCache(accountSession);
            }
        }

        protected abstract Task<AccountSession> GetAuthenticationResultAsync();

        protected AccountSession GetAuthenticationResultFromCache()
        {
            if (this.ServiceInfo.CredentialCache != null)
            {
                var cacheResult = this.ServiceInfo.CredentialCache.GetResultFromCache(
                    this.ServiceInfo.AccountType,
                    this.ServiceInfo.AppId,
                    this.ServiceInfo.UserId);

                return cacheResult;
            }

            return null;
        }

        protected virtual Task<AccountSession> RefreshAccessTokenAsync(string refreshToken)
        {
            return this.SendTokenRequestAsync(this.GetRefreshTokenRequestBody(refreshToken));
        }

        internal string GetRefreshTokenRequestBody(string refreshToken)
        {
            var requestBodyStringBuilder = new StringBuilder();
            requestBodyStringBuilder.AppendFormat("{0}={1}", Constants.Authentication.RedirectUriKeyName, this.ServiceInfo.ReturnUrl);
            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientIdKeyName, this.ServiceInfo.AppId);
            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ScopeKeyName, string.Join("%20", this.ServiceInfo.Scopes));
            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.RefreshTokenKeyName, refreshToken);
            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.GrantTypeKeyName, Constants.Authentication.RefreshTokenKeyName);

            if (!string.IsNullOrEmpty(this.ServiceInfo.ClientSecret))
            {
                requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientSecretKeyName, this.ServiceInfo.ClientSecret);
            }

            return requestBodyStringBuilder.ToString();
        }

        internal async Task<AccountSession> ProcessCachedAccountSessionAsync(AccountSession accountSession)
        {
            if (accountSession != null)
            {
                // If we have cached credentials and they're not expiring, return them.
                if (!string.IsNullOrEmpty(accountSession.AccessToken) && !accountSession.IsExpiring())
                {
                    return accountSession;
                }

                // If we don't have an access token or it's expiring, see if we can refresh the access token.
                if (!string.IsNullOrEmpty(accountSession.RefreshToken))
                {
                    accountSession = await this.RefreshAccessTokenAsync(accountSession.RefreshToken);

                    if (accountSession != null && !string.IsNullOrEmpty(accountSession.AccessToken))
                    {
                        this.CacheAuthResult(accountSession);
                        return accountSession;
                    }
                }
            }

            return null;
        }
        
        internal async Task<AccountSession> SendTokenRequestAsync(string requestBodyString)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, this.ServiceInfo.TokenServiceUrl);

            httpRequestMessage.Content = new StringContent(requestBodyString, Encoding.UTF8, Constants.Headers.FormUrlEncodedContentType);

            using (var authResponse = await this.ServiceInfo.HttpProvider.SendAsync(httpRequestMessage))
            using (var responseStream = await authResponse.Content.ReadAsStreamAsync())
            {
                var responseValues =
                    this.ServiceInfo.HttpProvider.Serializer.DeserializeObject<IDictionary<string, string>>(
                        responseStream);

                if (responseValues != null)
                {
                    OAuthErrorHandler.ThrowIfError(responseValues);
                    return new AccountSession(responseValues, this.ServiceInfo.AppId, AccountType.MicrosoftAccount);
                }

                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Authentication failed. No response values returned from authentication flow."
                    });
            }
        }
    }
}
