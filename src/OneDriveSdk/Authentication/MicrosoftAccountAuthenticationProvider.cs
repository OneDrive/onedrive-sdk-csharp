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
    using System.Net;
    using System.Threading.Tasks;

    public class MicrosoftAccountAuthenticationProvider : AuthenticationProvider
    {
        private readonly Uri callbackUri;

        public MicrosoftAccountAuthenticationProvider(ServiceInfo serviceInfo)
            : base (serviceInfo)
        {
            this.callbackUri = new Uri(serviceInfo.ReturnUrl);
        }

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        public override async Task SignOutAsync()
        {
            if (this.CurrentAccountSession != null && this.CurrentAccountSession.CanSignOut)
            {
                if (this.ServiceInfo.WebAuthenticationUi != null)
                {
                    var requestUri = new Uri(string.Format(
                    "{0}?client_id={1}&redirect_uri={2}",
                    this.ServiceInfo.SignOutUrl,
                    this.ServiceInfo.AppId,
                    this.ServiceInfo.ReturnUrl));

                    await this.ServiceInfo.WebAuthenticationUi.AuthenticateAsync(requestUri, this.callbackUri);
                }
                
                this.DeleteUserCredentialsFromCache(this.CurrentAccountSession);
                this.CurrentAccountSession = null;
            }
        }

        internal string GetCodeRedemptionRequestBody(string code)
        {
            var requestBodyString = string.Format(
                "{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}=authorization_code",
                Constants.Authentication.RedirectUriKeyName,
                this.ServiceInfo.ReturnUrl,
                Constants.Authentication.ClientIdKeyName,
                this.ServiceInfo.AppId,
                Constants.Authentication.ScopeKeyName,
                WebUtility.UrlEncode(string.Join(" ", this.ServiceInfo.Scopes)),
                Constants.Authentication.CodeKeyName,
                code,
                Constants.Authentication.GrantTypeKeyName);

            if (!string.IsNullOrEmpty(this.ServiceInfo.ClientSecret))
            {
                requestBodyString += "&client_secret=" + this.ServiceInfo.ClientSecret;
            }

            return requestBodyString;
        }

        protected override async Task<AccountSession> GetAuthenticationResultAsync()
        {
            AccountSession authResult = null;

            // Log the user in if we haven't already pulled their credentials from the cache.
            var code = await this.GetAuthorizationCodeAsync();

            if (!string.IsNullOrEmpty(code))
            {
                authResult = await this.RedeemAuthorizationCodeAsync(code);
                authResult.CanSignOut = true;
            }

            if (authResult != null)
            {
                this.CacheAuthResult(authResult);
            }

            return authResult;
        }

        private async Task<string> GetAuthorizationCodeAsync()
        {
            if (this.ServiceInfo.WebAuthenticationUi != null)
            {
                var requestUriString = string.Format(
                    "{0}?{1}={2}&{3}={4}&{5}={6}&{7}={8}",
                    this.ServiceInfo.AuthenticationServiceUrl,
                    Constants.Authentication.RedirectUriKeyName,
                    this.ServiceInfo.ReturnUrl,
                    Constants.Authentication.ClientIdKeyName,
                    this.ServiceInfo.AppId,
                    Constants.Authentication.ScopeKeyName,
                    string.Join("%20", this.ServiceInfo.Scopes),
                    Constants.Authentication.ResponseTypeKeyName,
                    Constants.Authentication.CodeKeyName);

                var requestUri = new Uri(requestUriString);

                var authenticationResponseValues = await this.ServiceInfo.WebAuthenticationUi.AuthenticateAsync(
                    requestUri,
                    this.callbackUri);
                OAuthErrorHandler.ThrowIfError(authenticationResponseValues);

                string code;
                if (authenticationResponseValues != null && authenticationResponseValues.TryGetValue("code", out code))
                {
                    return code;
                }
            }

            return null;
        }
        
        private Task<AccountSession> RedeemAuthorizationCodeAsync(string code)
        {
            return this.SendTokenRequestAsync(this.GetCodeRedemptionRequestBody(code));
        }
    }
}
